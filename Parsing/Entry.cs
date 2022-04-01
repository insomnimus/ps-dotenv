namespace Dotenv.Parsing;

public readonly struct Entry {
	public readonly string Name { get; init; }
	internal readonly Value val { get; init; }

	internal Entry(string name, Value val) => (this.Name, this.val) = (name, val);

	public string ExpandValue() => this.val.ExpandValue();
	public void SetEnv() => System.Environment.SetEnvironmentVariable(this.Name, this.ExpandValue());
}
