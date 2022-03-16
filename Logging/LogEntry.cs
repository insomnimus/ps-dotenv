using System;

namespace Dotenv.Logging;

public enum LogType {
	Exception,
	Error,
	Warning,
	Info,
	Debug,
}

public readonly struct LogEntry {
	public string Message { get; }
	public LogType Type { get; }
	public string Source { get; }

	public LogEntry(string msg, LogType t, string src = null) => (this.Message, this.Type, this.Source) = (msg, t, src);

	public static LogEntry Error(string msg, string src = null) => new LogEntry(msg, LogType.Error, src);
	public static LogEntry Warn(string msg, string src = null) => new LogEntry(msg, LogType.Warning, src);
	public static LogEntry Info(string msg, string src = null) => new LogEntry(msg, LogType.Info, src);
	public static LogEntry Debug(string msg, string src = null) => new LogEntry(msg, LogType.Debug, src);
	public static LogEntry Exception(Exception e, string src = null) => new LogEntry(e.ToString(), LogType.Exception, src);

	public override string ToString() {
		var kind = this.Type switch {
			LogType.Exception => "exception",
			LogType.Error => "error",
			LogType.Warning => "warning",
			LogType.Info => "info",
			LogType.Debug => "debug",
			_ => "log",
		};

		if (string.IsNullOrEmpty(this.Source)) return $"[{kind}] {this.Message}";
		else return $"[{kind}] {this.Message}\nsource: {this.Source}";

	}
}

public class LoggingPreference {
	public bool Error;
	public bool Warning;
	public bool Info;
	public bool Debug;
	public bool Exception;

	public static LoggingPreference Default() => new LoggingPreference {
		Error = true,
		Warning = true,
		Info = false,
		Debug = false,
		Exception = true
	};

	public bool Filter(LogType t) {
		switch (t) {
			case LogType.Error when this.Error:
			case LogType.Warning when this.Warning:
			case LogType.Info when this.Info:
			case LogType.Debug when this.Debug:
			case LogType.Exception when this.Exception:
				return true;
			default:
				return false;
		}
	}
}
