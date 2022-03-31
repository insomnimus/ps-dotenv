using Dotenv.MemoryBuffer;

namespace Dotenv.Logging;

public class Logger {
	private MemBuf<LogEntry> _logs = new MemBuf<LogEntry>(64);
	public LoggingPreference Preference = LoggingPreference.Default();
	public MemBuf<LogEntry> Logs {
		get => this._logs;
		set {
			if (value is null) this._logs.Clear();
			else this._logs = value;
		}
	}

	public Logger() { }

	public uint MaxSize {
		get => this._logs.Capacity;
		set => this._logs.Capacity = value;
	}

	public void Clear() => this._logs.Clear();

	public void Log(LogEntry e) {
		if (this._logs.Capacity > 0 && this.Preference.Filter(e.Type)) this._logs.Add(e);
	}

	public void Error(string msg, string? src = null) => this.Log(LogEntry.Error(msg, src));
	public void Info(string msg, string? src = null) => this.Log(LogEntry.Info(msg, src));
	public void Warn(string msg, string? src = null) => this.Log(LogEntry.Warn(msg, src));
	public void Debug(string msg, string? src = null) => this.Log(LogEntry.Debug(msg, src));
	public void Exception(Exception e, string? src = null) => this.Log(LogEntry.Exception(e, src));
}

