string usage = @"Parse a .env file and print the variables.

USAGE: dotenv <FILE...>

FILE...:
  One or more files to parse.
  The files will be sourced in the order they are provided.";

if (args.Length == 0) {
	Console.WriteLine(usage);
	return 0;
}

foreach (var arg in args) if (arg == "-h" || arg == "--help") {
		Console.WriteLine(usage);
		return 0;
	}

foreach (var file in args) {
	try {
		var data = File.ReadAllText(file);
		try {
			foreach (var res in new Dotenv.Parsing.Parser(data)) {
				if (res.IsErr) {
					Console.Error.WriteLine($"{file}:{res.Err?.Line}: parse error: {res.Err}");
					return 1;
				}
				var val = res.Ok.ExpandValue() ?? "";
				Console.WriteLine($"{res.Ok.Name} = {val}");
				System.Environment.SetEnvironmentVariable(res.Ok.Name, val);
			}
		} catch (Dotenv.Parsing.VarUnsetException) {
			Console.Error.WriteLine("{file}: {e.VariableName}: {e.Msg}");
			return 2;
		}
	} catch (Exception e) {
		Console.Error.WriteLine($"error: {e.Message}");
		return 1;
	}
}

return 0;
