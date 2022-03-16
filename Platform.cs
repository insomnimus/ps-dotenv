using System;

namespace Dotenv.OSSpecific;

public static class Platform {
#if Windows
	public const StringComparison StrComparison = StringComparison.OrdinalIgnoreCase;
	public static StringComparer StrComparer = StringComparer.OrdinalIgnoreCase;
#else
	public const StringComparison StrComparison = StringComparison.Ordinal;
public static StringComparer StrComparer = StringComparer.Ordinal;
#endif

	public static bool EndsInSeparator(this string s) {
#if Windows
		return s.Length > 0 && (s[s.Length - 1] == '\\' || s[s.Length - 1] == '/');
#else
		return s.EndsWith('/');
#endif
	}
}
