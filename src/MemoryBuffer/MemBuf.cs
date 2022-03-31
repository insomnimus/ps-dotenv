namespace Dotenv.MemoryBuffer;

public class MemBuf<T>: IEnumerable<T> {
	public MemBuf(uint capacity, uint prealloc = 0) {
		this._cap = (int)capacity;
		this.buf = new List<T>((int)prealloc);
	}

	private List<T> buf;
	private int _cap;
	private int cursor = 0;

	public int Length => this.buf.Count;
	public bool IsFull => this._cap <= this.Length;
	private int tail => this.IsFull ? this.cursor : 0;

	public uint Capacity {
		get => (uint)this._cap;
		set => this.SetCapacity(value);
	}

	public T this[int index] {
		get => this.buf[this.realindex(index)];
		set => this.buf[this.realindex(index)] = value;
	}

	private int realindex(int n) {
		if (n >= this.Length) return n;
		else if (n < 0) {
			n += this.Length;
			if (n < 0) throw new IndexOutOfRangeException($"negative index too small: length is {this.Length} and the wrapped index is {n}");
			return this.realindex(n);
		} else return (this.tail + n) % this._cap;
	}

	public void Add(T item) {
		if (this.IsFull) this.buf[this.cursor] = item;
		else this.buf.Add(item);
		this.cursor++;
		if (this.cursor >= this._cap) this.cursor = 0;
	}

	public void SetCapacity(uint capacity) {
		int n = (int)capacity;
		if (!this.IsFull && n >= this.Length) {
			// We're not full and we're decreasing the capacity but there's no need to trim.
		} else if (!this.IsFull) {
			// We're not full and we're decreasing the capacity but we also have to trim current items.
			// Shift left, keep last n items.
			int shift = (int)(this.buf.Count - n);
			Util.ShiftLeft(this.buf, shift);
			this.cursor = 0;
		} else if (this.Length > n) {
			// We're full and we're reducing the capacity and we need to trim.
			var temp = new List<T>((int)n);
			for (int i = this.Length - n; i < this.Length; i++) {
				temp.Add(this[i]);
			}
			this.buf = temp;
			this.cursor = 0;
		} else {
			// We're full and we're increasing the capacity.
			// All we do here is to order current items.
			var left = this.buf.GetRange(0, this.cursor);
			this.buf.AddRange(left);
			// Shift left.
			Util.ShiftLeft(this.buf, left.Count);
			this.cursor = this.Length;
		}
		this._cap = (int)n;
	}

	public IEnumerator<T> GetEnumerator() {
		for (int i = 0; i < this.Length; i++) {
			yield return this[i];
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
		return this.GetEnumerator();
	}

	public void Clear() {
		this.buf.Clear();
		this.cursor = 0;
	}

	public void AddRange(IEnumerable<T> items) {
		foreach (var x in items) this.Add(x);
	}
}
