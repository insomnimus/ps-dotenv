using System;
using System.Management.Automation;
using Dotenv.Parser;
using Dotenv.Errors;

namespace Dotenv {
	[Cmdlet(VerbsCommunications.Read, "Dotenv", DefaultParameterSetName = "file")]
	[OutputType(typeof(EnvEntry))]
	public class ReadDotenvCmd: PSCmdlet {
		[Parameter(
		HelpMessage = "Path to a env file.",
			Mandatory = true,
			ParameterSetName = "file",
			Position = 0,
			// ValueFromPipeline = true,
			ValueFromPipelineByPropertyName = true)]
		public string Path { get; set; }
		[Parameter(
		Mandatory = true,
		Position = 0,
		ParameterSetName = "text",
		HelpMessage = "The text input to parse.",
		ValueFromPipeline = true,
		ValueFromPipelineByPropertyName = true
		)]
		public string Text { get; set; }

		[Parameter(HelpMessage = "Skip syntax errors if any are encountered. The default behaviour is to stop parsing.")]
		public SwitchParameter SkipErrors { get; set; }

		private string filepath => this.GetUnresolvedProviderPathFromPSPath(this.Path);
		private string data => this.ParameterSetName switch {
			"text" => this.Text,
			_ => System.IO.File.ReadAllText(this.filepath),
		};

		protected override void ProcessRecord() {
			var res = Parser.Parser.Parse(this.data, this.SkipErrors);
			foreach (var e in res.Entries) this.WriteObject(e);
			foreach (var e in res.Errors) this.WriteError(new ErrorRecord(e, "Dotenv.ParseError", ErrorCategory.ParserError, null));
		}
	}
}
