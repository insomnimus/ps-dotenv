using System;
using System.IO;
using System.Collections.Generic;
using Dotenv.MemoryBuffer;
using Dotenv.Parsing;
using Dotenv.Errors;
using Dotenv.Logging;

namespace Dotenv;

public class Daemon {
	// Members that are used in the module but not here.
	public bool Async = true;

	private bool _enabled = true;
	private List<string> _names = new List<string>() { ".env" };
	private List<DotenvFile> _sourced = new List<DotenvFile>(32) { };
	public bool IgnoreExportPrefix = true;
	public bool SkipErrors = false;
	private StringComparison strComparison;
	private Logger log = new Logger();
	private string lastdir = "";
	public LoggingPreference LoggingPreference {
		get => this.log.Preference;
		set => this.log.Preference = value;
	}

	public Daemon() {
		this.strComparison = Environment.OSVersion.Platform switch {
			PlatformID.Unix => StringComparison.Ordinal,
			PlatformID.MacOSX => StringComparison.Ordinal,
			_ => StringComparison.OrdinalIgnoreCase,
		};
	}

	public List<string> Names {
		get => this._names;
		set {
			if (value is null) this._names.Clear();
			else this._names = value;
			this.Clear();
			this.Update(this.lastdir);
		}
	}

	public IList<DotenvFile> Sourced => this._sourced.AsReadOnly();
	public MemBuf<LogEntry> Logs => this.log.Logs;
	public bool Enabled {
		get => this._enabled;
		set {
			if (value) this.Enable();
			else this.Disable();
		}
	}

	private bool endsInSeparator(string p) => this.strComparison switch {
		StringComparison.Ordinal => p.EndsWith('/'),
		_ => p.EndsWith('\\') || p.EndsWith('/'),
	};
	private bool pathIsSourced(string p) => this._sourced.Exists(x => x.FilePath.Equals(p, this.strComparison));

	public void Enable() {
		if (this._enabled) return;
		this._enabled = true;
		this.Update(this.lastdir);
	}

	public void Disable() {
		if (!this._enabled) return;
		this.Clear();
		this._enabled = false;
	}

	public void Clear() => this._sourced.RemoveAll(x => {
		this.log.Info("unsourcing", x.FilePath);
		x.Unsource();
		return true;
	});

	public void Update(string pwd) {
		this.log.Debug($"update called in {pwd}");
		this.lastdir = pwd;
		if (!this._enabled || this._names.Count == 0) {
			this.log.Debug("nothing to do because the module is disabled or there are no names");
			return;
		}

		this._sourced.RemoveAll(x => {
			if (pwd.StartsWith(x.Root, this.strComparison)) return false;
			this.log.Debug($"unsourcing {x.FilePath}");
			x.Unsource();
			return true;
		});

		var files = new List<string>(32) { };

		var dir = pwd;
		while (true) {
			foreach (var name in this._names) {
				var filepath = Path.Join(dir, name);
				if (File.Exists(filepath) && !this.pathIsSourced(filepath)) files.Add(filepath);
			}

			if (string.IsNullOrEmpty(dir) || this.endsInSeparator(dir)) break;
			else dir = Path.GetDirectoryName(dir);
		}

		this.sourceFiles(files);
	}

	private void sourceFiles(List<string> files) {
		if (files is null || files.Count == 0) return;

		foreach (var f in files) {
			try {
				this.log.Info("sourcing file", f);
				var data = File.ReadAllText(f);
				var res = Parser.Parse(data, this.SkipErrors, this.IgnoreExportPrefix);
				if (res.Entries.Count > 0) {
					try {
						this._sourced.Add(new DotenvFile(f, res.Entries));
					} catch (VarUnsetException e) {
						this.log.Error(e.ToString(), f);
					}
				}
				foreach (var e in res.Errors) this.log.Error(e.ToString(), f);
			} catch (Exception e) {
				this.log.Exception(e, f);
			}
		}
	}

	public bool AddName(string name) {
		if (this._names.Exists(x => x.Equals(name, this.strComparison))) return false;
		foreach (var c in Path.GetInvalidFileNameChars()) {
			if (name.Contains(c)) throw new ArgumentException("the name can't contain path separators, drive separators or any other illegal path character", "name");
		}
		this._names.Add(name);
		this.Clear();
		this.Update(this.lastdir);
		return true;
	}

	public bool RemoveName(string name) {
		var n = this._names.RemoveAll(x => x.Equals(name, this.strComparison));
		if (n == 0) return false;
		this.Clear();
		this.Update(this.lastdir);
		return true;
	}
}
