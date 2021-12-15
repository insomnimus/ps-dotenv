using System;
using Dotenv.Parsing;

namespace Dotenv {
	public enum Quote {
		Single,
		Double,
		MultiSingle,
		MultiDouble
	}

	public class EnvEntry {
		public string Name { get; set; }
		internal EnvStr value;

		public bool HasExpandableVar => this.value.hasExpandableVar;


		public string ExpandValue() => this.value.expand();

		public void SetEnv() => System.Environment.SetEnvironmentVariable(this.Name, this.ExpandValue());

		internal EnvEntry(string name, EnvStr value) => (this.Name, this.value) = (name, value);

		public override string ToString() => $"{this.Name} = {this.ExpandValue()}";
	}
}
