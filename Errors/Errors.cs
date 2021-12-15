using System;

namespace Dotenv.Errors {
	public abstract class ParseError: Exception {
		public int Line { get; set; }

		public ParseError(int line, string msg)
		: base($"line {line}: {msg}") {
			Line = line;
		}
	}

	public class ErrInvalidName: ParseError {
		public ErrInvalidName(int line)
	: base(line, "invalid variable name; names must start with a letter or a '_'") { }

		public ErrInvalidName(int line, string msg)
		: base(line, msg) { }
	}

	public class ErrMissingEquals: ParseError {
		public ErrMissingEquals(int line)
		: base(line, "missing '=' sign") { }
	}

	public class ErrUnclosedQuote: ParseError {
		public Quote Kind { get; set; }

		internal static string quoteStr(Quote q) => q switch {
			Quote.Double => "double",
			Quote.Single => "single",
			Quote.MultiDouble => "multiline double",
			Quote.MultiSingle => "multiline single",
			_ => throw new LogicException($"{q} is not a recognized Quote")
		};

		public ErrUnclosedQuote(int line, Quote kind)
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
		: base(0, $"internal logic error: {msg}") { }
	}

	public class ErrMissingNewline: ParseError {
		public ErrMissingNewline(int line)
		: base(line, "missing newline at the end of the right hand side of '='") { }
	}

	public class ErrMissingRBrace: ParseError {
		public ErrMissingRBrace(int line)
		: base(line, "the closing brace for the ${} interpolated variable is missing") { }
	}
}
