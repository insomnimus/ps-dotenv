using System;

namespace Dotenv.Parser {
	public class ParseResult {
		public bool IsError { get; set; }
		public EnvEntry Entry;
		public ParseError Error;
	}

	public enum QuoteKind {
		Bare,
		Single,
		Double,
		MultiSingle,
		MultiDouble
	}

	internal class Str {
		public string value;
		public QuoteKind quote;

		internal Str(string s, QuoteKind q) {
			value = s;
			quote = q;
		}
	}

	public class EnvEntry {
		public string Name { get; set; }
		public QuoteKind Quote { get; set; }
		public string Value { get; set; }
	}
}
