using System;
using System.Text;
using System.Collections.Generic;
using Dotenv;
using Dotenv.Errors;
using System.Runtime.InteropServices;

namespace Dotenv.Parser {
	public class Parser {
		private const char EOF = char.MinValue;

		private string input;
		private int pos = 0;
		private int readpos = 0;
		private char ch;
		private int line = 0;
		private char peek => readpos >= input.Length ? EOF : input[readpos];
		private List<EnvEntry> parsedVars = new List<EnvEntry>() { };
		private StringComparison comparisonType;

		public Parser(string input) {
			this.input = input;
			this.read();
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				this.comparisonType = StringComparison.OrdinalIgnoreCase;
			} else {
				this.comparisonType = StringComparison.Ordinal;
			}
		}

		internal string getenv(string key) {
			// Check if we parsed the var.
			var entry = this.parsedVars.FindLast(e => e.Name.Equals(key, this.comparisonType));
			return entry?.Value ?? Environment.GetEnvironmentVariable(key);
		}

		internal static string trimLF(string s) {
			if (s.Length > 1 && s.Substring(s.Length - 2) == "\r\n") return s.Substring(0, s.Length - 2);
			else if (s.Length > 0 && s[s.Length - 1] == '\n') return s.Substring(0, s.Length - 1);
			else return s;
		}

		internal static char escaped(char c) => c switch {
			'n' => '\n',
			'r' => '\r',
			'f' => '\f',
			't' => '\t',
			'b' => '\b',
			_ => c,
		};

		bool aheadIs(string s) => input.Substring(pos).StartsWith(s, StringComparison.Ordinal);

		internal void read() {
			if (readpos >= input.Length) {
				this.ch = EOF;
				pos = input.Length;
				readpos = input.Length + 1;
				return;
			} else {
				this.ch = input[readpos];
			}
			this.pos = this.readpos;
			this.readpos++;
			if (ch == '\n') this.line++;
		}

		internal void expect(string s) {
			if (!this.aheadIs(s)) {
				var len = Math.Min(s.Length, input.Length - pos);
				var got = input.Substring(pos, len);
				throw new LogicException($"assertion failed: expected '{s}' ahead; got '{got}'");
			}
			foreach (var _ in s) read();
		}

		internal void expect(char c) {
			if (ch != c) throw new LogicException($"assertion failed: expected {c}, got {ch}");
			else read();
		}

		internal void skipLine() {
			while (ch != '\n' && ch != EOF) read();
			read();
		}

		internal void skipAny(string anyof) {
			while (anyof.Contains(ch)) read();
		}

		internal Result<string> parseLHS() {
			if (ch != '_' && !char.IsLetter(ch)) {
				return new ErrInvalidName(line);
			}
			var start = pos;
			while (ch == '_' || char.IsLetter(ch) || char.IsDigit(ch)) read();
			return input.Substring(start, pos - start);
		}

		internal Result<string> readBare() {
			var ln = line;
			Result<string> res;

			var buf = new StringBuilder();
			while (ch != EOF && !char.IsWhiteSpace(ch)) {
				if (ch == '$') {
					res = this.parseExpand();
					if (res.IsErr) return res;
					else buf.Append(res.Ok);
				} else {
					buf.Append(ch);
					read();
				}
			}

			return buf.ToString();
		}

		internal Result<string> readSingleQuote() {
			var ln = line;
			this.expect('\'');
			var buf = new StringBuilder();

			while (ch != '\'') {
				switch (ch) {
					case EOF:
					case '\n':
						return new ErrUnclosedQuote(ln, Quote.Single);
					case '\\':
						if (peek == '\'') read();
						buf.Append(ch);
						break;
					default:
						buf.Append(ch);
						break;
				};
				read();
			}

			// Consume the quote.
			this.expect("'");
			return buf.ToString();
		}

		internal Result<string> readDoubleQuote() {
			var ln = line;
			this.expect('"');
			var buf = new StringBuilder();

			while (ch != '"') {
				var mustRead = true;
				switch (ch) {
					case '$':
						var res = this.parseExpand();
						if (res.IsErr) return res;
						else buf.Append(res.Ok);
						mustRead = false;
						break;
					case EOF:
					case '\n':
						return new ErrUnclosedQuote(ln, Quote.Double);
					case '\\':
						read();
						if (ch == EOF) return new ErrUnclosedQuote(ln, Quote.Double);
						buf.Append(escaped(ch));
						break;
					default:
						buf.Append(ch);
						break;
				};

				if (mustRead) read();
			}

			// Consume the quote.
			this.expect('"');
			return buf.ToString();
		}

		internal Result<string> readMultiSingle() {
			var ln = line;
			this.expect("'''");
			while (ch != '\n') {
				if (ch == EOF || !char.IsWhiteSpace(ch)) return new ErrInvalidMultiline(ln);
				read();
			}
			this.expect('\n');

			var buf = new StringBuilder();
			while (true) {
				switch (ch) {
					case EOF: return new ErrUnclosedQuote(ln, Quote.MultiSingle);
					case '\\':
						if (peek == '\'') read();
						buf.Append(ch);
						break;
					case '\'':
						if (aheadIs("'''")) {
							for (int i = 0; i < 4; i++) read();
							return trimLF(buf.ToString());
						}
						buf.Append(ch);
						break;
					default:
						buf.Append(ch);
						break;
				};
				read();
			}
		}

		internal Result<string> readMultiDouble() {
			var ln = line;
			this.expect("\"\"\"");
			while (ch != '\n') {
				if (ch == EOF || !char.IsWhiteSpace(ch)) return new ErrInvalidMultiline(ln);
				read();
			}
			this.expect('\n');

			var buf = new StringBuilder();
			while (true) {
				var mustRead = true;
				switch (ch) {
					case EOF: return new ErrUnclosedQuote(ln, Quote.MultiDouble);
					case '"' when this.aheadIs("\"\"\""):
						for (var i = 0; i < 4; i++) read();
						return trimLF(buf.ToString());
					case '\\':
						read();
						if (ch == EOF) return new ErrUnclosedQuote(ln, Quote.MultiDouble);
						buf.Append(escaped(ch));
						break;
					case '$':
						mustRead = false;
						var res = this.parseExpand();
						if (res.IsErr) return res;
						else buf.Append(res.Ok);
						break;
					default:
						buf.Append(ch);
						break;
				};
				if (mustRead) read();
			}
		}

		internal Result<string> parseRHS() => ch switch {
			'\n' => "",
			'\r' when peek == '\n' => "",
			'"' when this.aheadIs("\"\"\"") => this.readMultiDouble(),
			'"' => this.readDoubleQuote(),
			'\'' when this.aheadIs("'''") => this.readMultiSingle(),
			'\'' => this.readSingleQuote(),
			_ => this.readBare(),
		};

		internal Result<string> parseExpand() {
			this.expect('$');
			Result<string> res;
			if (ch == '{') res = readBracedExpand();
			else if (ch == '_' || char.IsLetter(ch)) res = this.readNormalExpand();
			else {
				read();
				return "$";
			}
			if (res.IsErr) return res.Err;
			else return this.getenv(res.Ok);
		}

		internal string readNormalExpand() {
			var start = pos;
			while (ch == '_' || char.IsLetter(ch) || char.IsDigit(ch)) read();
			return input.Substring(start, pos - start);
		}

		internal Result<string> readBracedExpand() {
			var ln = line;
			this.expect('{');
			var start = pos;
			while (ch != '}') {
				if (ch == '\n' || (ch == '\r' && peek == '\n')) return new ErrMissingRBrace(ln);
				read();
			}
			this.expect('}');
			return input.Substring(start, pos - 1 - start);
		}

		internal Result<EnvEntry> parseEntry() {
			// Skip whitespace.
			while (ch != EOF && char.IsWhiteSpace(ch)) read();
			if (ch == '#') {
				this.skipLine();
				return this.parseEntry();
			}
			if (ch == EOF) return null;

			var ln = line;
			var key = this.parseLHS();
			if (key.IsErr) return key.Err;

			// Ignore whitespace as a separator.
			this.skipAny(" \t");
			if (ch != '=') return new ErrMissingEquals(ln);
			this.expect('=');
			this.skipAny(" \t");

			var rhs = this.parseRHS();
			if (rhs.IsErr) return rhs.Err;

			// Check for EOF or LF.
			while (!(ch == EOF || ch == '\n' || ch == '#')) {
				if (!char.IsWhiteSpace(ch)) return new ErrMissingNewline(line);
				read();
			}

			var entry = new EnvEntry(key.Ok, rhs.Ok);
			this.parsedVars.Add(entry);
			return entry;
		}

		public ParseResult Parse(bool skipErrors = false) {
			var items = new ParseResult();

			while (ch != EOF) {
				var res = this.parseEntry();
				if (res == null) break;
				items.add(res);
				if (res.IsErr && skipErrors) this.skipLine();
				else if (res.IsErr) return items;
			}

			return items;
		}

		public static ParseResult Parse(string text, bool skipErrors = false) {
			var p = new Parser(text);
			return p.Parse(skipErrors);
		}
	}
}
