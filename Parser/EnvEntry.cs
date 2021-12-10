using System;
using Dotenv;

namespace Dotenv.Parser {
	public class ParseResult {
		public bool IsError { get; set; }
		public EnvEntry Entry;
		public ParseError Error;
	}

	internal class Str {
		public string value;
		public QuoteKind quote;

		internal Str(string s, QuoteKind q) {
			value = s;
			quote = q;
		}
	}
}
