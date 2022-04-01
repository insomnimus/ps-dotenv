namespace Dotenv.Parsing;

public abstract class ParseError: Exception {
	public int Line { get; set; }

	public ParseError(int line, string msg)
	: base($"line {line}: {msg}") {
		Line = line;
	}

	public override string ToString() => this.Message;
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
	public ErrUnclosedQuote(int line)
	: base(line, "unclosed quote") { }
}

public class AssertionException: Exception {
	public AssertionException(string msg)
	: base($"internal logic error: {msg}") { }
}

public class ErrMissingNewline: ParseError {
	public ErrMissingNewline(int line)
	: base(line, "missing newline at the end of the right hand side of '='") { }
}

public class ErrMissingRBrace: ParseError {
	public ErrMissingRBrace(int line)
	: base(line, "the closing brace for the parameter expansion is missing") { }
}

public class VarUnsetException: Exception {
	public string VariableName { get; private set; }
	public string Msg { get; private set; }

	public VarUnsetException(string name, string msg)
	: base($"{name}: {msg ?? "the value is not set"}") {
		this.VariableName = name;
		this.Msg = msg ?? "the variable is not set";
	}

	public override string ToString() => base.Message;
}

public class ErrBadSubstitution: ParseError {
	public ErrBadSubstitution(int line) : base(line, "bad substitution") { }
}
