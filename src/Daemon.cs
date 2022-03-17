using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Immutable;

using Dotenv.MemoryBuffer;
using Dotenv.Parsing;
using Dotenv.Errors;
using Dotenv.Logging;
using Dotenv.OSSpecific;

namespace Dotenv;

public class Daemon {
	public Daemon(string[] whitelist = null, bool enabled = false, bool safe = true, bool quiet = false) {
		this.Quiet = quiet;
		this._enabled = enabled;
		this._safe = safe;
		this._warned = new HashSet<string>(Platform.StrComparer);
		this.auth = new Whitelist(whitelist);
	}

	// Members that are used in the module but not here.
	public bool Async = true;

	private bool _enabled = true;
	private List<string> _names = new List<string>() { ".env" };
	private List<DotenvFile> _sourced = new List<DotenvFile>(32) { };
	public bool IgnoreExportPrefix = true;
	public bool SkipErrors = false;
	private Logger log = new Logger();
	private string lastdir = "";
	public LoggingPreference LoggingPreference {
		get => this.log.Preference;
		set => this.log.Preference = value;
	}
	private Whitelist auth;
	public ICollection<string> AuthorizedPatterns => this.auth.Patterns;
	private bool _safe = true;
	public bool SafeMode {
		get => this._safe;
		set {
			if (value != this._safe) {
				this._safe = value;
				this.Update(this.lastdir);
			}
		}
	}
	public bool Quiet = false;
	private HashSet<string> _warned;

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

	private bool pathIsSourced(string p) => this._sourced.Exists(x => x.FilePath.Equals(p, Platform.StrComparison));

	public void Enable(string pwd = null) {
		pwd ??= this.lastdir;
		if (this._enabled) return;
		this._enabled = true;
		this.Update(pwd);
	}

	public void Disable() {
		if (!this._enabled) return;
		this.Clear();
		this._enabled = false;
		this._warned.Clear();
	}

	public void Clear() {
		this._sourced.RemoveAll(x => {
			this.log.Info("unsourcing", x.FilePath);
			x.Unsource();
			return true;
		});
		this._warned.Clear();
	}

	public void Update(string pwd) {
		this.log.Debug($"update called in {pwd}");
		this.lastdir = pwd;
		if (!this._enabled || this._names.Count == 0) {
			this.log.Debug("nothing to do because the module is disabled or there are no names");
			return;
		}

		this._sourced.RemoveAll(x => {
			if (pwd.StartsWith(x.Root, Platform.StrComparison)) return false;
			this.log.Debug($"unsourcing {x.FilePath}");
			x.Unsource();
			return true;
		});

		this._warned.RemoveWhere(x => {
			var parent = Path.GetDirectoryName(x);
			return !pwd.StartsWith(parent, Platform.StrComparison);
		});

		var files = this.findEnvFiles(pwd, true);
		this.sourceFiles(files);
	}

	private List<string> findEnvFiles(string pwd, bool ignoreSourced) {
		var files = new List<string>(32) { };
		var dir = pwd;

		while (true) {
			foreach (var name in this._names) {
				var filepath = Path.Join(dir, name);
				if (File.Exists(filepath) && (!ignoreSourced || !this.pathIsSourced(filepath)))
					files.Add(filepath);
			}

			if (string.IsNullOrEmpty(dir) || dir.EndsInSeparator()) break;
			else dir = Path.GetDirectoryName(dir);
		}

		return files;
	}

	public List<string> FindEnvFiles(string pwd) => this.findEnvFiles(pwd, false);

	private void sourceFiles(List<string> files) {
		if (files is null || files.Count == 0) return;
		var warned = false;

		foreach (var f in files) {
			if (this._safe && !this.auth.IsMatch(f)) {
				if (!this.Quiet && this._warned.Add(f)) {
					this.log.Warn("unauthorized file not sourced while safe mode is on", f);
					warned = true;
					System.Console.WriteLine($"dotenv info: {f} is not authorized, authorize it with `Approve-DotenvFile` or disable the safe mode");
				}
				continue;
			}
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

		if (warned) System.Console.WriteLine("You can turn this message off by setting `$Dotenv.Quiet = $true`");
	}

	public bool AddName(string name) {
		if (this._names.Exists(x => x.Equals(name, Platform.StrComparison))) return false;
		foreach (var c in Path.GetInvalidFileNameChars()) {
			if (name.Contains(c)) throw new ArgumentException("the name can't contain path separators, drive separators or any other illegal path character", "name");
		}
		this._names.Add(name);
		this.Clear();
		this.Update(this.lastdir);
		return true;
	}

	public bool RemoveName(string name) {
		var n = this._names.RemoveAll(x => x.Equals(name, Platform.StrComparison));
		if (n == 0) return false;
		this.Clear();
		this.Update(this.lastdir);
		return true;
	}

	public bool AuthorizePattern(string path, bool update = false) {
		var fullpath = Path.GetFullPath(path);
		var ok = this.auth.Add(fullpath);
		this._warned.Remove(fullpath);
		if (ok && this.SafeMode) {
			this.Update(this.lastdir);
		}
		return ok;
	}

	public bool UnauthorizePattern(string path, bool update = false) {
		var fullpath = Path.GetFullPath(path);
		var ok = this.auth.Remove(fullpath);
		if (ok && this.SafeMode && update) {
			this.Update(this.lastdir);
		}
		return ok;
	}

	public bool AuthorizeDirectory(string dir, bool update = false) {
		var fullpath = Path.GetFullPath(dir);
		var ok = this.auth.AddDir(fullpath);
		if (ok && update) this.Update(this.lastdir);
		return ok;
	}
}
