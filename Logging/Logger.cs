using System;
using System.Collections.Generic;

namespace Dotenv.Logging {
	public class Logger {
		public LoggingPreference Preference = new LoggingPreference { };
		private List<LogEntry> _logs = new List<LogEntry>() { };
		public List<LogEntry> Logs {
			get => this._logs;
			set {
				if (value is null) this._logs.Clear();
				else this._logs = value;
			}
		}

		public Logger() { }

		public void Clear() => this._logs.Clear();

		public void Log(LogEntry e) {
			if (this.Preference.Filter(e.Type)) this._logs.Add(e);
		}

		public void Error(string msg, string src = null) => this.Log(LogEntry.Error(msg, src));
		public void Info(string msg, string src = null) => this.Log(LogEntry.Info(msg, src));
		public void Warn(string msg, string src = null) => this.Log(LogEntry.Warn(msg, src));
		public void Debug(string msg, string src = null) => this.Log(LogEntry.Debug(msg, src));
		public void Exception(Exception e, string src = null) => this.Log(LogEntry.Exception(e, src));
	}
}
