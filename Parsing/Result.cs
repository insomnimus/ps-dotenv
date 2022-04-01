namespace Dotenv.Parsing;

public class Result<T> {
	public T? Ok { get; set; }
	public ParseError? Err { get; set; }
	public bool IsErr => this.Err is not null;

	public Result(T ok) => (this.Ok, this.Err) = (ok, null);
	public Result(ParseError err) => (this.Ok, this.Err) = (default(T), err);

	public static implicit operator Result<T>(T ok) => new Result<T>(ok);
	public static implicit operator Result<T>(ParseError e) => new Result<T>(e);
}
