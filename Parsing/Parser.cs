using System;
using System.Text;
using System.Collections.Generic;
using Dotenv;
using Dotenv.Errors;
using System.Runtime.InteropServices;

namespace Dotenv.Parsing {
	public class Parser {
		private const char EOF = char.MinValue;

		private string input;
		private int pos = 0;
		private int readpos = 0;
		private char ch;
		private int line = 1;
		private char peek => readpos >= input.Length ? EOF : input[readpos];
		private bool ignoreExport = false;

		public Parser(string input, bool ignoreExport = true) {
			this.input = input;
			this.ignoreExport = ignoreExport;
			this.read();
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

		internal Result<EnvStr> readBare() {
			var ln = line;
			var buf = new StringBuilder();
			var pieces = new EnvStr();

			while (ch != EOF && !char.IsWhiteSpace(ch)) {
				if (ch == '$') {
					var res = this.parseExpand();
					if (res.IsErr) return res.Err;

					if (buf.Length > 0) pieces.add(buf.ToString());
					buf.Clear();
					pieces.add(res.Ok);
				} else {
					buf.Append(ch);
					read();
				}
			}

			if (buf.Length > 0) pieces.add(buf.ToString());

			return pieces;
		}

		internal Result<EnvStr> readSingleQuote() {
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
			return new EnvStr(buf.ToString());
		}

		internal Result<EnvStr> readDoubleQuote() {
			var ln = line;
			this.expect('"');
			var buf = new StringBuilder();
			var pieces = new EnvStr();

			while (ch != '"') {
				var mustRead = true;
				switch (ch) {
					case '$':
						mustRead = false;
						var res = this.parseExpand();
						if (res.IsErr) return res.Err;

						if (buf.Length > 0) pieces.add(buf.ToString());
						buf.Clear();
						pieces.add(res.Ok);
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

			if (buf.Length > 0) pieces.add(buf.ToString());
			return pieces;
		}

		internal Result<EnvStr> readMultiSingle() {
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
							return new EnvStr(trimLF(buf.ToString()));
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

		internal Result<EnvStr> readMultiDouble() {
			var ln = line;
			this.expect("\"\"\"");
			while (ch != '\n') {
				if (ch == EOF || !char.IsWhiteSpace(ch)) return new ErrInvalidMultiline(ln);
				read();
			}
			this.expect('\n');

			var buf = new StringBuilder();
			var pieces = new EnvStr();

			while (true) {
				var mustRead = true;
				switch (ch) {
					case EOF: return new ErrUnclosedQuote(ln, Quote.MultiDouble);
					case '"' when this.aheadIs("\"\"\""):
						for (var i = 0; i < 4; i++) read();
						if (buf.Length > 0) pieces.add(trimLF(buf.ToString()));
						return pieces;
					case '\\':
						read();
						if (ch == EOF) return new ErrUnclosedQuote(ln, Quote.MultiDouble);
						buf.Append(escaped(ch));
						break;
					case '$':
						mustRead = false;
						var res = this.parseExpand();
						if (res.IsErr) return res.Err;

						if (buf.Length > 0) pieces.add(buf.ToString());
						buf.Clear();
						pieces.add(res.Ok);
						break;
					default:
						buf.Append(ch);
						break;
				};
				if (mustRead) read();
			}
		}

		internal Result<EnvStr> parseRHS() => ch switch {
			'\n' => new EnvStr(""),
			'\r' when peek == '\n' => new EnvStr(""),
			'"' when this.aheadIs("\"\"\"") => this.readMultiDouble(),
			'"' => this.readDoubleQuote(),
			'\'' when this.aheadIs("'''") => this.readMultiSingle(),
			'\'' => this.readSingleQuote(),
			_ => this.readBare(),
		};

		internal Result<StrFragment> parseExpand() {
			this.expect('$');
			Result<string> res;
			if (ch == '{') res = readBracedExpand();
			else if (ch == '_' || char.IsLetter(ch)) res = this.readNormalExpand();
			else {
				read();
				return new StrFragment("$", false);
			}
			if (res.IsErr) return res.Err;
			else if (res.Ok.Length == 0) return new ErrInvalidName(line, "the environment variable name can't be empty");
			else return new StrFragment(res.Ok, true);
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
			if (ch != '=') {
				// Ignore the `export` prefix if the option is set.
				if (!this.ignoreExport || key.Ok != "export" || (ch != '_' && !char.IsLetter(ch))) {
					return new ErrMissingEquals(ln);
				} else {
					key = this.parseLHS();
					if (key.IsErr) return key.Err;
					this.skipAny(" \t");
					if (ch != '=') return new ErrMissingEquals(ln);
				}
			}
			this.expect('=');
			this.skipAny(" \t");

			var rhs = this.parseRHS();
			if (rhs.IsErr) return rhs.Err;

			// Check for EOF or LF.
			while (!(ch == EOF || ch == '\n' || ch == '#')) {
				if (!char.IsWhiteSpace(ch)) return new ErrMissingNewline(line);
				read();
			}

			return new EnvEntry(key.Ok, rhs.Ok);
		}

		public ParseResult Parse(bool skipErrors = false) {
			var items = new ParseResult();

			for (var res = this.parseEntry(); res != null; res = this.parseEntry()) {
				items.add(res);
				if (res.IsErr && skipErrors) this.skipLine();
				else if (res.IsErr) return items;
			}

			return items;
		}

		public static ParseResult Parse(string text, bool skipErrors = false, bool ignoreExport = true) {
			var p = new Parser(text, ignoreExport);
			return p.Parse(skipErrors);
		}
	}
}
