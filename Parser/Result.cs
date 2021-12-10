namespace Dotenv.Parser {
	internal class Result {
		public bool IsError;
		public string Ok;
		public ParseError Error;

		public Result(string val) {
			this.IsError = false;
			this.Ok = val;
		}

		public Result(ParseError err) {
			this.IsError = true;
			this.Error = err;
		}
	}

	internal class StrResult: Result {
		public QuoteKind Kind;

		public StrResult(ParseError e)
		: base(e) { }

		public StrResult(string s)
		: base(s) { }

		public StrResult(string s, QuoteKind q)
		: base(s) {
			Kind = q;
		}
	}
}
