using System;
using System.Collections.Generic;
using System.IO;

namespace Dotenv;

public readonly struct EnvVar {
	public string Name { get; }
	public string Value { get; }
	public string Replaced { get; }
	public EnvVar(string name, string val, string replaced) => (this.Name, this.Value, this.Replaced) = (name, val, replaced);

	public override string ToString() => $"{this.Name} = {this.Value}";
	internal void unset() => Environment.SetEnvironmentVariable(this.Name, this.Replaced);
}

public class DotenvFile {
	public string FilePath { get; internal set; }
	public string Name { get; internal set; }
	public string Root { get; internal set; }
	List<EnvVar> _vars;

	public IList<EnvVar> Vars => this._vars.AsReadOnly();

	internal DotenvFile(string path, List<EnvEntry> entries) {
		this.FilePath = path;
		this.Root = Path.GetDirectoryName(path);
		this.Name = Path.GetFileName(path);
		this._vars = new List<EnvVar>(entries.Count) { };

		foreach (var e in entries) {
			var replaced = Environment.GetEnvironmentVariable(e.Name);
			var expanded = e.ExpandValue();
			Environment.SetEnvironmentVariable(e.Name, expanded);
			this._vars.Add(new EnvVar(e.Name, expanded, replaced));
		}
	}

	internal void Unsource() {
		foreach (var v in this._vars) {
			v.unset();
		}
	}
}
