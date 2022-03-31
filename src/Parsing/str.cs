using System.Text;
using Dotenv.Errors;

namespace Dotenv.Parsing;

internal enum ExpansionOp {
	OrNothing,
	OrDefault,
	OrError,
	AndReplace,
}

interface Expr {
	string ExpandValue();
}

internal readonly struct StrLiteral: Expr {
	private readonly string val { get; init; }

	public StrLiteral(string s) => this.val = s;
	public static implicit operator StrLiteral(string s) => new StrLiteral(s);
	// public static implicit operator string(StrLiteral s) => s.val;

	public string ExpandValue() => this.val;
	public override string ToString() => this.val;
}

internal class Expansion: Expr {
	internal Expansion(string left, ExpansionOp op, Value right) {
		this.left = left;
		this.right = right;
		this.op = op;
	}
	internal Expansion(string name) {
		this.left = name;
		this.right = new Value();
		this.op = ExpansionOp.OrNothing;
	}
	// ${<left><op><right>}
	private readonly string left;
	private readonly ExpansionOp op;
	private readonly Value right;

	public string ExpandValue() {
		var s = System.Environment.GetEnvironmentVariable(this.left ?? "");
		switch (this.op) {
			case ExpansionOp.OrNothing:
				return s;
			case ExpansionOp.OrDefault:
				if (string.IsNullOrEmpty(s)) return this.right.ExpandValue();
				else return s;
			case ExpansionOp.AndReplace:
				if (string.IsNullOrEmpty(s)) return "";
				else return this.right.ExpandValue();
			case ExpansionOp.OrError:
				if (string.IsNullOrEmpty(s))
					throw new VarUnsetException(this.left ?? "", this.right?.ExpandValue() ?? "the variable is not set");
				else
					return s;
			default:
				throw new AssertionException("unhandled enum case");
		}
	}
}

internal class Value: Expr {
	private List<Expr> fragments = new();
	public int Count => this.fragments.Count;

	internal Value() { }

	public void Add(StrLiteral s) => this.fragments.Add(s);
	public void Add(Expansion e) => this.fragments.Add(e);
	public void Add(Value v) => this.fragments.Add(v);

	public string ExpandValue() {
		if (this.fragments is null || this.fragments.Count == 0) return "";

		var buf = new StringBuilder();
		foreach (var x in this.fragments) buf.Append(x.ExpandValue());
		return buf.ToString();
	}
}
