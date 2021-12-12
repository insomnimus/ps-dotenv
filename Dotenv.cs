using System;

namespace Dotenv {
	public enum Quote {
		Single,
		Double,
		MultiSingle,
		MultiDouble
	}

	public class EnvEntry {
		public string Name { get; set; }
		public string Value { get; set; }

		public EnvEntry(string name, string value) => (this.Name, this.Value) = (name, value);

		public override string ToString() => $"{this.Name} = {this.Value}";
	}
}
