using System;

namespace Dotenv.Parser {
	public abstract class ParseError: Exception {
		public int Line { get; set; }
		// public string Message {get; private set;}

		public ParseError(int line, string msg)
		: base($"line {line}: {msg}") {
			Line = line;
		}
	}

	public class ErrInvalidName: ParseError {
		public ErrInvalidName(int line)
	: base(line, "invalid variable name; names must start with a letter or a '_'") { }
	}

	public class ErrMissingEquals: ParseError {
		public ErrMissingEquals(int line)
		: base(line, "missing '=' sign") { }
	}

	public class ErrUnclosedQuote: ParseError {
		public QuoteKind Kind { get; private set; }

		internal static string quoteStr(QuoteKind q) {
			switch (q) {
				case QuoteKind.Single:
					return "single";
				case QuoteKind.Double:
					return "double";
				case QuoteKind.MultiDouble:
					return "multiline double";
				case QuoteKind.MultiSingle:
					return "multiline single";
				case QuoteKind.Bare:
					return "bare";
				default:
					return "";
			};
		}

		public ErrUnclosedQuote(int line, QuoteKind kind)
		: base(line, $"unclosed {quoteStr(kind)} quote") {
			Kind = kind;
		}
	}

	public class ErrInvalidMultiline: ParseError {
		public ErrInvalidMultiline(int line)
		: base(line, "a triple quoted string opening can only be followed by newline") { }
	}

	public class LogicException: ParseError {
		public LogicException(string msg)
		: base(0, msg) { }
	}

	public class ErrMissingNewline: ParseError {
		public ErrMissingNewline(int line)
		: base(line, "missing newline at the end of the value text") { }
	}
}
