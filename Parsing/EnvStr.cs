using System;
using System.Collections.Generic;
using System.Text;

namespace Dotenv.Parsing {
	internal class StrFragment {
		internal bool isVar;
		internal string text;

		internal StrFragment(string text, bool isVar) => (this.text, this.isVar) = (text, isVar);
		public static implicit operator StrFragment(string s) => new StrFragment(s, false);
	}

	internal class EnvStr {
		List<StrFragment> pieces = new List<StrFragment>() { };

		internal bool hasExpandableVar => this.pieces.Exists(x => x.isVar);

		internal EnvStr(StrFragment s) => this.add(s);
		internal EnvStr() { }

		internal void add(StrFragment s) => this.pieces.Add(s);

		internal string expand() {
			var buf = new StringBuilder();
			foreach (var f in this.pieces) {
				if (f.isVar) buf.Append(System.Environment.GetEnvironmentVariable(f.text));
				else buf.Append(f.text);
			}
			return buf.ToString();
		}

		public static implicit operator EnvStr(StrFragment s) => new EnvStr(s);
	}
}
