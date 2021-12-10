using System;
using System.Text;
using System.Collections.Generic;

namespace Dotenv.Parser {
	public class Parser {
		private string input;
		private string chars => input;
		private int pos = 0;
		private int readpos = 0;
		private char ch;
		private int line = 0;
		private const char EOF = char.MinValue;

		public Parser(string content) {
			this.input = content;
			this.read();
		}

		private char peek {
			get {
				if (readpos >= chars.Length) {
					return EOF;
				}
				return chars[readpos];
			}
		}

		private void read() {
			if (readpos >= chars.Length) {
				ch = EOF;
			} else {
				ch = chars[readpos];
			}
			pos = readpos;
			readpos++;
			if (ch == '\n') {
				line++;
			}
		}

		private char escape(char c) {
			switch (c) {
				case 'n': return '\n';
				case 'r': return '\r';
				case 't': return '\t';
				default: return c;
			};
		}

		private void skipLine() {
			while (ch != '\n' && ch != EOF) {
				read();
			}
			read();
		}

		private void skipAny(string anyof) {
			while (containsChar(anyof, ch)) {
				read();
			}
		}

		private bool aheadIs(string s) {
			if (pos + s.Length >= chars.Length) {
				return false;
			}
			return input.Substring(pos, s.Length) == s;
		}

		private Result readKey() {
			// Must start with `_` or a letter.
			if (ch != '_' && !char.IsLetter(ch)) {
				var err = new ErrInvalidName(line);
				return new Result(err);
			}
			var start = pos;
			read();
			while (char.IsLetter(ch) || char.IsDigit(ch)) {
				read();
			}
			return new Result(input.Substring(start, pos - start));
		}

		private StrResult readValue() {
			QuoteKind q;
			Result res;
			switch (ch) {
				case '\'':
					if (aheadIs("'''")) {
						res = readMultiSingleQuote();
						q = QuoteKind.MultiSingle;
					} else {
						res = readSingleQuote();
						q = QuoteKind.Single;
					}
					break;
				case '"':
					if (aheadIs("\"\"\"")) {
						res = readMultiDoubleQuote();
						q = QuoteKind.MultiDouble;
					} else {
						res = readDoubleQuote();
						q = QuoteKind.Double;
					}
					break;
				default:
					res = readBare();
					q = QuoteKind.Bare;
					break;
			};
			if (res.IsError) {
				return new StrResult(res.Error);
			}
			return new StrResult(res.Ok, q);
		}

		private Result readBare() {
			var start = pos;
			while (ch != EOF && !char.IsWhiteSpace(ch)) {
				read();
			}

			return checkEOL(input.Substring(start, pos - start));
		}

		private Result readSingleQuote() {
			read();
			var ln = line;
			var buf = new StringBuilder();

			while (ch != '\'') {
				switch (ch) {
					case EOF:
					case '\n':
						return new Result(new ErrUnclosedQuote(ln, QuoteKind.Single));
					case '\\':
						if (peek == '\'') {
							buf.Append('\'');
							read();
						} else {
							buf.Append('\\');
						}
						break;
					default:
						buf.Append(ch);
						break;
				};
				read();
			}

			read();
			return checkEOL(buf.ToString());
		}

		private Result readDoubleQuote() {
			var ln = line;
			read();
			var buf = new StringBuilder();

			while (ch != '"') {
				switch (ch) {
					case EOF:
						return new Result(new ErrUnclosedQuote(line, QuoteKind.Double));
					case '\\':
						read();
						if (ch == EOF) {
							return new Result(new ErrUnclosedQuote(ln, QuoteKind.Double));
						}
						buf.Append(escape(ch));
						break;
					default:
						buf.Append(ch);
						break;
				};
				read();
			}

			read();
			return checkEOL(buf.ToString());
		}

		private Result readMultiDoubleQuote() {
			var ln = line;
			var buf = new StringBuilder();
			expect("\"\"\"");
			if (ch == '\n') {
				read();
			} else if (ch == '\r' && peek == '\n') {
				read();
				read();
			} else {
				return new Result(new ErrInvalidMultiline(ln));
			}

			while (true) {
				switch (ch) {
					case EOF: return new Result(new ErrUnclosedQuote(ln, QuoteKind.MultiDouble));
					case '\\':
						read();
						if (ch == EOF) {
							return new Result(new ErrUnclosedQuote(ln, QuoteKind.MultiDouble));
						}
						buf.Append(escape(ch));
						break;
					case '"':
						if (aheadIs("\"\"\"")) {
							for (var i = 0; i < 4; i++) read();
							return checkEOL(buf.ToString());
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

		private Result readMultiSingleQuote() {
			var ln = line;
			var buf = new StringBuilder();
			expect("'''");
			if (ch == '\n') {
				read();
			} else if (ch == '\r' && peek == '\n') {
				read();
				read();
			} else {
				return new Result(new ErrInvalidMultiline(ln));
			}

			while (true) {
				switch (ch) {
					case EOF: return new Result(new ErrUnclosedQuote(ln, QuoteKind.MultiSingle));
					case '\\':
						if (peek == '\'') {
							read();
							buf.Append('\'');
						} else {
							buf.Append('\\');
						}
						break;
					case '\'':
						if (aheadIs("'''")) {
							for (var i = 0; i < 4; i++) read();
							return checkEOL(buf.ToString());
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

		private ParseResult parseEntry() {
			// skip whitespace.
			skipAny(" \r\n\t");
			if (ch == '#') {
				skipLine();
				return parseEntry();
			}

			var ln = line;
			var key = readKey();
			if (key.IsError) {
				return new ParseResult {
					IsError = true,
					Error = key.Error,
					Entry = null
				};
			}
			skipAny(" \t");
			if (ch != '=') {
				return new ParseResult {
					IsError = true,
					Error = new ErrMissingEquals(ln),
					Entry = null
				};
			}
			read();
			skipAny(" \t");
			if (ch == '\n') {
				read();
				return new ParseResult {
					IsError = false,
					Entry = new EnvEntry { Name = key.Ok, Quote = QuoteKind.Bare, Value = "" },
					Error = null
				};
			} else if (ch == '\r' && peek == '\n') {
				read();
				read();
				return new ParseResult {
					IsError = false,
					Entry = new EnvEntry { Name = key.Ok, Quote = QuoteKind.Bare, Value = "" },
					Error = null
				};
			}

			var value = readValue();
			if (value.IsError) {
				return new ParseResult {
					IsError = true,
					Error = value.Error,
					Entry = null
				};
			}
			var entry = new EnvEntry {
				Name = key.Ok,
				Quote = value.Kind,
				Value = value.Ok
			};
			return new ParseResult {
				IsError = false,
				Error = null,
				Entry = entry
			};
		}

		private void expect(string s) {
			if (!aheadIs(s)) {
				var len = Math.Min(s.Length, input.Length - pos);
				var got = input.Substring(pos, len);
				throw new LogicException($"assertion failed: expected '{s}' ahead; got '{got}'");
			}
			foreach (var _ in s) read();
		}

		private bool containsChar(string s, char c) {
			foreach (var x in s) {
				if (c == x) {
					return true;
				}
			}
			return false;
		}

		private Result checkEOL(string parsed) {
			if (!(ch == EOF || ch == '\n' || (ch == '\r' && peek == '\n'))) {
				return new Result(new ErrMissingNewline(line));
			}
			return new Result(parsed);
		}

		public List<ParseResult> Parse(ErrorAction action = ErrorAction.Stop) {
			var items = new List<ParseResult>() { };
			while (ch != EOF) {
				var res = this.parseEntry();
				items.Add(res);
				if (res.IsError) {
					switch (action) {
						case ErrorAction.Stop:
							return items;
						case ErrorAction.Continue:
							this.skipLine();
							break;
						case ErrorAction.Panic:
							throw res.Error;
					};
				}
			}
			return items;
		}

		public List<EnvEntry> ParseEntries() {
			var items = new List<EnvEntry>() { };
			ParseResult res;
			while (ch != EOF) {
				res = this.parseEntry();
				if (res.IsError) {
					throw res.Error;
				}
				items.Add(res.Entry);
			}
			return items;
		}

		public static List<EnvEntry> ParseEntries(string data) {
			var p = new Parser(data);
			return p.ParseEntries();
		}

		public static List<ParseResult> Parse(string data, ErrorAction action = ErrorAction.Stop) {
			var p = new Parser(data);
			return p.Parse(action);
		}
	}
}
