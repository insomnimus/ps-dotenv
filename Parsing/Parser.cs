using System.Text;
using System.Collections;

namespace Dotenv.Parsing;

public class Parser: IEnumerable<Result<Entry>> {
	public Parser(string input) {
		this.input = input.Replace("\r\n", "\n");
		this.ch = this.input.Length > 0 ? this.input[0] : EOF;
	}

	private const char EOF = char.MinValue;
	private string input;
	private int line = 1;
	private int pos = 0;
	private char ch;

	private int readpos => this.pos + 1;
	private char peek => this.readpos < this.input.Length ? this.input[this.readpos] : EOF;

	private void read() {
		this.pos++;

		if (this.pos >= this.input.Length) {
			this.ch = EOF;
		} else {
			this.ch = this.input[this.pos];
			if (this.ch == '\n') this.line++;
		}
	}

	private static char escaped(char c) => c switch {
		'n' => '\n',
		'r' => '\r',
		'f' => '\f',
		't' => '\t',
		'v' => '\v',
		'b' => '\b',
		_ => c,
	};

	private static bool isSpecial(char c) => c switch {
		'$' or '\\' or '"' or '\'' => true,
		_ => false,
	};

	private void skipLine() => this.skipWhile(c => c != EOF && c != '\n');
	private void skipSpace() => this.skipWhile(c => c != '\n' && char.IsWhiteSpace(c));
	private void skipWhile(Func<char, bool> fn) {
		while (this.ch != EOF && fn(this.ch)) this.read();
	}

	private string readWhile(Func<char, bool> fn) {
		var start = this.pos;
		while (this.ch != EOF && fn(this.ch)) this.read();
		return this.input.Substring(start, this.pos - start);
	}

	private bool aheadIs(string s) => this.input.Substring(this.pos).StartsWith(s, StringComparison.Ordinal);

	private void expect(char c) {
		if (this.ch != c) throw new AssertionException($"expect({c})");
		this.read();
	}

	private void expect(string s) {
		if (!this.tryConsume(s)) throw new AssertionException($"expect({s})");
	}

	private bool tryConsume(string first, params string[] rest) {
		if (this.aheadIs(first)) {
			for (int i = 0; i < first.Length; i++) this.read();
			return true;
		}

		foreach (var s in rest) {
			if (this.aheadIs(s)) {
				for (int i = 0; i < s.Length; i++) this.read();
				return true;
			}
		}

		return false;
	}

	private bool tryConsume(char first, params char[] rest) {
		if (this.ch == first) {
			this.read();
			return true;
		} else {
			foreach (var c in rest) {
				if (c == this.ch) {
					this.read();
					return true;
				}
			}
		}
		return false;
	}

	private Result<string> readName() {
		if (this.ch != '_' && !char.IsLetter(this.ch)) return new ErrInvalidName(this.line);
		var start = this.pos;
		this.read();
		while (this.ch == '_' || char.IsLetterOrDigit(this.ch)) this.read();
		return this.input.Substring(start, this.pos - start);
	}

	private Result<Expansion> parseNormalExpand() {
		var res = this.readName();
		if (res.IsErr) return res.Err;
		else return new Expansion(res.Ok);
	}

	private Result<Expansion> parseBracedExpand() {
		var ln = this.line;
		this.expect('{');
		string left;
		var leftRes = this.readName();
		if (leftRes.IsErr) return leftRes.Err;
		else if (string.IsNullOrEmpty(leftRes.Ok)) return new ErrBadSubstitution(ln);
		else left = leftRes.Ok;

		// Check for any expansion operator.
		var op = ExpansionOp.OrNothing;
		if (this.tryConsume("-", ":-"))
			op = ExpansionOp.OrDefault;
		else if (this.tryConsume("+", ":+"))
			op = ExpansionOp.AndReplace;
		else if (this.tryConsume("?", ":?"))
			op = ExpansionOp.OrError;
		else if (this.tryConsume('}'))
			return new Expansion(left, op, new Value());
		// If no operator is present, it must end here with '}'
		else
			return new ErrMissingRBrace(ln);
		if (this.tryConsume('}')) return new Expansion(left, op, new Value());

		var res = this.parseExpansionValue();
		if (res.IsErr) return res.Err;

		this.expect('}');
		return new Expansion(left, op, res.Ok);
	}

	private Result<Expansion> parseExpand() {
		this.expect('$');
		if (this.ch == '{') return this.parseBracedExpand();
		else return this.parseNormalExpand();
	}

	private Result<Value> parseExpansionValue() {
		var ln = this.line;
		var vals = new Value();

		while (this.ch != '}') {
			switch (this.ch) {
				case EOF:
					return new ErrMissingRBrace(ln);
				case '"':
					var resD = this.readDoubleQuote();
					if (resD.IsErr) return resD.Err;
					else vals.Add(resD.Ok);
					break;
				case '\'':
					var resS = this.readSingleQuote();
					if (resS.IsErr) return resS.Err;
					else vals.Add(resS.Ok);
					break;
				case '$' when char.IsDigit(this.peek):
					this.read();
					this.skipWhile(c => char.IsDigit(c));
					break;
				case '$' when this.peek != '{' && this.peek != '_' && !char.IsLetter(this.peek):
					vals.Add(new StrLiteral("$"));
					this.read();
					break;
				case '$':
					var res = this.parseExpand();
					if (res.IsErr) return res.Err;
					else vals.Add(res.Ok);
					break;
				case '\\':
					this.read();
					if (this.ch != '\n')
						vals.Add(new StrLiteral(this.ch.ToString()));
					this.read();
					break;
				default:
					if (char.IsWhiteSpace(this.ch)) {
						vals.Add(new StrLiteral(" "));
						this.skipWhile(c => char.IsWhiteSpace(c));
					} else {
						vals.Add(new StrLiteral(
				this.readWhile(c => !isSpecial(c) && c != '}' && !char.IsWhiteSpace(c))
						));
					}
					break;
			}
		}

		// Note: Don't expect here since this is done in parseBracedExpand
		// this.expect('}');
		return vals;
	}

	private Result<StrLiteral> readSingleQuote() {
		var ln = this.line;
		this.expect('\'');
		var start = this.pos;
		while (this.ch != '\'') {
			if (this.ch == EOF) return new ErrUnclosedQuote(ln);
			this.read();
		}
		var s = this.input.Substring(start, this.pos - start);
		this.expect('\'');
		return new StrLiteral(s);
	}

	private Result<Value> readDoubleQuote() {
		var ln = this.line;
		this.expect('"');

		var buf = new StringBuilder();
		var vals = new Value();

		while (this.ch != '"') {
			switch (this.ch) {
				case EOF:
					return new ErrUnclosedQuote(ln);
				// Ignore $0, $1 etc
				case '$' when char.IsDigit(this.peek):
					this.read();
					this.skipWhile(c => char.IsDigit(c));
					continue;
				case '$' when this.peek != '{' && this.peek != '_' && !char.IsLetter(this.peek):
					buf.Append('$');
					this.read();
					break;
				case '$':
					var res = this.parseExpand();
					if (res.IsErr) return res.Err;
					if (buf.Length != 0) {
						vals.Add(new StrLiteral(buf.ToString()));
						buf.Clear();
					}
					vals.Add(res.Ok);
					continue;
				case '\\' when this.peek == '\n':
					this.read();
					break;
				case '\\':
					this.read();
					buf.Append(escaped(this.ch));
					break;
				default:
					buf.Append(this.ch);
					break;
			}

			this.read();
		}

		this.expect('"');

		if (buf.Length != 0) vals.Add(new StrLiteral(buf.ToString()));
		return vals;
	}

	private Result<Value> parseValue() {
		// a value is any number of adjacent strings or parameter expansion
		var vals = new Value();

		while (this.ch != EOF && !char.IsWhiteSpace(this.ch)) {
			switch (this.ch) {
				case '"':
					var resD = this.readDoubleQuote();
					if (resD.IsErr) return resD.Err;
					else vals.Add(resD.Ok);
					break;
				case '\'':
					var resS = this.readSingleQuote();
					if (resS.IsErr) return resS.Err;
					else vals.Add(resS.Ok);
					break;
				case '$' when char.IsDigit(this.peek):
					this.read();
					this.skipWhile(c => char.IsDigit(c));
					break;
				case '$' when this.peek != '{' && this.peek != '_' && !char.IsLetter(this.peek):
					vals.Add(new StrLiteral("$"));
					this.read();
					break;
				case '$':
					var res = this.parseExpand();
					if (res.IsErr) return res.Err;
					else vals.Add(res.Ok);
					break;
				case '\\':
					if (this.peek == '\n') {
						this.read();
						this.read();
						if (vals.Count == 0) {
							this.skipSpace();
							continue;
						}
					} else {
						this.read();
						vals.Add(new StrLiteral(this.ch.ToString()));
					}
					break;
				default:
					vals.Add(new StrLiteral(
					this.readWhile(c => !char.IsWhiteSpace(c) && !isSpecial(c))
					));
					break;
			}
		}
		return vals;
	}

	private Result<Entry> parseEntry() {
		var ln = this.line;
		this.tryConsume("export ", "export\t");
		this.skipSpace();
		var name = this.readName();
		if (name.IsErr) return name.Err;

		this.skipSpace();

		if (this.ch != '=') return new ErrMissingEquals(ln);
		this.read();
		this.skipSpace();

		if (this.ch == '\n' || this.ch == EOF) {
			return new Entry(name.Ok, new Value());
		}

		var res = this.parseValue();
		if (res.IsErr) return res.Err;
		this.skipSpace();
		if (this.ch != '\n' && this.ch != '#' && this.ch != EOF) {
			return new ErrMissingNewline(this.line);
		}
		return new Entry(name.Ok, res.Ok);
	}

	public IEnumerator<Result<Entry>> GetEnumerator() {
		while (this.ch != EOF) {
			this.skipWhile(c => char.IsWhiteSpace(c));
			if (this.ch == '#') {
				this.skipLine();
				continue;
			}
			if (this.ch == EOF) break;
			var res = this.parseEntry();
			if (res.IsErr) this.skipLine();
			yield return res;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
