using System;
using System.Collections.Generic;
using System.Text;
using Dotenv.Errors;

namespace Dotenv.Parsing;

public enum ExpansionType {
	Normal,
	DefaultValue,
	MustExist,
	ReplaceIfExists,
}

internal class EnvStr {
	List<StrFragment> pieces = new List<StrFragment>() { };

	internal EnvStr(StrFragment s) => this.add(s);
	internal EnvStr() { }

	internal void add(StrFragment s) => this.pieces.Add(s);

	internal string expand() {
		var buf = new StringBuilder();
		foreach (var x in this.pieces) buf.Append(x.expand());
		return buf.ToString();
	}

	public static implicit operator EnvStr(StrFragment s) => new EnvStr(s);
}

internal class StrFragment {
	internal StrFragment(string s) => (this.plain, this.interpolated) = (s, null);
	internal StrFragment(ExpandStr s) => (this.plain, this.interpolated) = (null, s);

	private string plain;
	private ExpandStr interpolated;

	public static implicit operator StrFragment(string s) => new StrFragment(s);
	public static implicit operator StrFragment(ExpandStr s) => new StrFragment(s);

	internal string expand() => this.plain ?? this.interpolated?.expand() ?? "";
}

internal class ExpandStr {
	internal string left;
	internal EnvStr right;
	internal ExpansionType op;

	internal ExpandStr(string left, EnvStr right, ExpansionType op) => (this.left, this.right, this.op) = (left, right, op);

	public static implicit operator ExpandStr(string s) => new ExpandStr(s, null, ExpansionType.Normal);

	internal string expand() {
		var s = Environment.GetEnvironmentVariable(this.left ?? "");

		return this.op switch {
			ExpansionType.Normal => s,
			ExpansionType.DefaultValue => s ?? this.right?.expand(),
			ExpansionType.MustExist when String.IsNullOrEmpty(s) => throw new VarUnsetException(this.left, this.right?.expand()),
			ExpansionType.MustExist => s,
			ExpansionType.ReplaceIfExists when String.IsNullOrEmpty(s) => "",
			ExpansionType.ReplaceIfExists => this.right?.expand(),
			_ => null,
		};
	}
}
