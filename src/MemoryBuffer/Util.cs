using System.Collections.Generic;

namespace Dotenv.MemoryBuffer;

internal class Util {
	public static void ShiftLeft<T>(List<T> buf, int shift) {
		if (shift <= 0) return;
		for (int i = shift; i < buf.Count; i++) {
			buf[i - shift] = buf[i];
		}
		buf.RemoveRange(buf.Count - shift, shift);
	}
}
