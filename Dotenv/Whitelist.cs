using DotNet.Globbing;
using Dotenv.OSSpecific;

namespace Dotenv;

internal class Whitelist {
	public Whitelist(string[]? globs = null) {
		this._patterns = new(Platform.StrComparer);
		if (globs is not null) {
			foreach (var x in globs) this.Add(x);
		}
	}

	private Dictionary<string, Glob> _patterns;
	public ICollection<string> Patterns => this._patterns.Keys;

	public bool Add(string pattern) {
		if (this._patterns.ContainsKey(pattern)) return false;
		var opts = new GlobOptions();

#if Windows
		opts.Evaluation.CaseInsensitive = true;
#else
			opts.Evaluation.CaseInsensitive = false;
#endif

		this._patterns[pattern] = Glob.Parse(pattern, opts);
		return true;
	}

	public bool AddDir(string dir) {
		if (dir.EndsInSeparator())
			return this.Add($"{dir}**");
		else return this.Add($"{dir}{Path.DirectorySeparatorChar}**");
	}

	public bool IsMatch(string path) =>
	this._patterns.Values.Any(glob => glob.IsMatch(path));

	public bool Remove(string pattern) => this._patterns.Remove(pattern);
}
