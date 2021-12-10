using System;

namespace Dotenv {

	public enum QuoteKind {
		Bare,
		Single,
		Double,
		MultiSingle,
		MultiDouble
	}


	public class EnvEntry {
		public string Name { get; set; }
		public QuoteKind Quote { get; set; }
		public string Value { get; set; }
	}
}
