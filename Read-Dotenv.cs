using System;
using System.Management.Automation;
using Dotenv.Parser;

namespace Dotenv {
	[Cmdlet(VerbsCommunications.Read, "Dotenv", DefaultParameterSetName = "file")]
	[OutputType(typeof(Parser.EnvEntry))]
	public class ReadDotenvCmd: PSCmdlet {
		[Parameter(
		HelpMessage = "Path to a .env file.",
			Mandatory = true,
			ParameterSetName = "file",
			Position = 0,
			ValueFromPipeline = true,
			ValueFromPipelineByPropertyName = true)]
		public string Path { get; set; }
		[Parameter(
		HelpMessage = "Literal path to a .env file.",
			Mandatory = true,
			ParameterSetName = "literal",
			ValueFromPipeline = true,
			ValueFromPipelineByPropertyName = true)]
		public string LiteralPath { get; set; }
		[Parameter(
		Mandatory = true,
		Position = 0,
		ParameterSetName = "text",
		HelpMessage = "The text input to parse.",
		ValueFromPipeline = true,
		ValueFromPipelineByPropertyName = true
		)]
		public string Text { get; set; }

		[Parameter(HelpMessage = "Stop parsing if any line is invalid. The default behaviour is to continue parsing.")]
		public SwitchParameter StopOnError { get; set; }
		[Parameter(HelpMessage = "Ignore every error; only return entries.")]
		public SwitchParameter IgnoreErrors { get; set; }

		private ErrorAction pref => StopOnError ? ErrorAction.Stop : ErrorAction.Continue;
		private string filepath {
			get {
				if (this.ParameterSetName == "literal") {
					return this.LiteralPath;
				}
				return System.IO.Path.GetFullPath(this.Path);
			}
		}
		private string data {
			get {
				if (this.ParameterSetName == "text") {
					return this.Text;
				}
				return System.IO.File.ReadAllText(this.filepath);
			}
		}

		protected override void ProcessRecord() {
			var vars = Parser.Parser.Parse(this.data, this.pref);
			foreach (var x in vars) {
				if (!x.IsError) {
					this.WriteObject(x.Entry);
				} else if (!IgnoreErrors) {
					this.WriteError(new ErrorRecord(x.Error, "Dotenv.ParseError", ErrorCategory.ParserError, null));
				}
			}
		}
	}
}
