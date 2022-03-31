using System.Management.Automation;
using Dotenv.Parsing;

namespace Dotenv;

[Cmdlet(VerbsCommunications.Read, "Dotenv", DefaultParameterSetName = "file")]
[OutputType(typeof(Entry))]
public class ReadDotenvCmd: PSCmdlet {
	[Parameter(
	HelpMessage = "Path to a env file.",
		Mandatory = true,
		ParameterSetName = "file",
		Position = 0,
		// ValueFromPipeline = true,
		ValueFromPipelineByPropertyName = true)]
	public string? Path { get; set; }
	[Parameter(
	Mandatory = true,
	Position = 0,
	ParameterSetName = "text",
	HelpMessage = "The text input to parse.",
	ValueFromPipeline = true,
	ValueFromPipelineByPropertyName = true
	)]
	public string? Text { get; set; }

	private string filepath => this.GetUnresolvedProviderPathFromPSPath(this.Path);
	private string? data => this.ParameterSetName switch {
		"text" => this.Text,
		_ => System.IO.File.ReadAllText(this.filepath),
	};

	protected override void ProcessRecord() {
		foreach (var res in new Parser(this.data)) {
			if (res.IsErr)
				this.WriteError(new ErrorRecord(res.Err, "Dotenv.ParseError", ErrorCategory.ParserError, null));
			else
				this.WriteObject(res.Ok);
		}
	}
}
