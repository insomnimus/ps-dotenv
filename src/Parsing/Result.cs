using System;
using System.Collections.Generic;
using Dotenv;
using Dotenv.Errors;

namespace Dotenv.Parsing;
internal class Result<T> {
	public T Ok { get; set; }
	public ParseError Err { get; set; }
	public bool IsErr => this.Err != null;

	public Result(T ok) => (this.Ok, this.Err) = (ok, null);
	public Result(ParseError err) => (this.Ok, this.Err) = (default(T), err);

	public static implicit operator Result<T>(T ok) => new Result<T>(ok);
	public static implicit operator Result<T>(ParseError e) => new Result<T>(e);
}

public class ParseResult {
	public List<EnvEntry> Entries { get; internal set; }
	public List<ParseError> Errors { get; internal set; }
	public bool HasError => Errors.Count > 0;

	internal void add(Result<EnvEntry> res) {
		if (res.IsErr) this.Errors.Add(res.Err);
		else this.Entries.Add(res.Ok);
	}

	internal ParseResult() => (this.Entries, this.Errors) = (new List<EnvEntry>() { }, new List<ParseError>() { });
}
