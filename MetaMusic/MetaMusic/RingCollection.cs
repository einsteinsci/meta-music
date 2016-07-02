using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UltimateUtil;

namespace MetaMusic
{
	public class RingCollection<T> : IEnumerable<T>, ICollection<T>
	{
		private class Node
		{
			public T Data
			{ get; private set; }

			public Node Next
			{ get; set; }

			public Node Prev
			{ get; set; }

			public Node(T data)
			{
				Data = data;
			}

			public override string ToString()
			{
				return "[NODE] " + Data;
			}
		}

		private class Enumerator : IEnumerator<T>
		{
			public readonly RingCollection<T> Ring;

			public Node CurrentNode
			{ get; private set; }

			public T Current
			{
				get
				{
					if (CurrentNode == null)
					{
						return default(T);
					}

					return CurrentNode.Data;
				}
			}

			object IEnumerator.Current => Current;

			bool _passedInitial;

			public Enumerator(RingCollection<T> ring)
			{
				Ring = ring;
				Reset();
			}

			public void Reset()
			{
				if (Ring.Count == 0)
				{
					CurrentNode = null;
				}
				else
				{
					CurrentNode = new Node(default(T)) { Next = Ring._active };
					_passedInitial = false;
				}
			}

			public bool MoveNext()
			{
				if (CurrentNode == null)
				{
					return false;
				}

				CurrentNode = CurrentNode.Next;

				if (CurrentNode == Ring._active)
				{
					if (!_passedInitial)
					{
						_passedInitial = true;
						return true;
					}

					return false;
				}

				return true;
			}

			public void Dispose()
			{ }
		}

		public int Count
		{ get; private set; }

		public T CurrentValue
		{
			get
			{
				if (_active == null)
				{
					return default(T);
				}

				return _active.Data;
			}
		}

		public bool IsReadOnly => false;

		private Node _active;

		public RingCollection()
		{ }

		public RingCollection(IEnumerable<T> start)
		{
			foreach (T t in start)
			{
				Add(t);
			}
		}

		public RingCollection(params T[] start) : this((IEnumerable<T>)start)
		{ }

		private void _insertBefore(Node added, Node existing)
		{
			added.Prev = existing.Prev;
			added.Next = existing;

			if (existing.Prev != null)
			{
				existing.Prev.Next = added;
			}
			existing.Prev = added;

			Count++;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void MoveNext()
		{
			if (_active?.Next != null)
			{
				_active = _active.Next;
			}
		}

		public void MovePrevious()
		{
			if (_active?.Prev != null)
			{
				_active = _active.Prev;
			}
		}

		public bool MoveTo(Predicate<T> pred)
		{
			if (_active == null)
			{
				return false;
			}

			Node start = _active;

			while (!pred(CurrentValue))
			{
				MoveNext();

				if (_active == start)
				{
					return false;
				}
			}

			return true;
		}

		public bool MoveTo(T t)
		{
			if (t == null)
			{
				throw new ArgumentNullException(nameof(t));
			}

			return MoveTo(_t => _t.Equals(t));
		}

		public void Add(T item)
		{
			Node node = new Node(item);
			if (_active == null)
			{
				_active = node;
				_active.Next = _active;
				_active.Prev = _active;
				Count++;
				return;
			}

			_insertBefore(node, _active);
		}

		public void Clear()
		{
			_active = null;
		}

		public bool Remove(T item)
		{
			if (_active == null)
			{
				return false;
			}

			if (_active.Data.Equals(item))
			{
				RemoveCurrent();
				return true;
			}

			Node n = _active;
			while (n.Next != _active)
			{
				if (n.Next.Data.Equals(item))
				{
					n.Next.Next.Prev = n;
					n.Next = n.Next.Next;

					Count--;
					return true;
				}

				n = n.Next;
			}

			return false;
		}

		public void RemoveCurrent()
		{
			if (_active != null)
			{
				if (Count == 1)
				{
					Count--;
					_active = null;
					return;
				}

				_active.Next.Prev = _active.Prev;
				_active.Prev.Next = _active.Next;

				_active = _active.Next;
				Count--;
			}
		}

		public bool Contains(T item)
		{
			if (_active == null)
			{
				return false;
			}

			if (_active.Data.Equals(item))
			{
				return true;
			}

			Node n = _active;
			while (n.Next != _active)
			{
				if (n.Next.Data.Equals(item))
				{
					return true;
				}

				n = n.Next;
			}

			return false;
		}

		public void CopyTo(T[] arr, int start)
		{
			if (arr == null)
			{
				throw new ArgumentNullException(nameof(arr));
			}

			if (start < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(start));
			}

			if (start + Count > arr.Length)
			{
				throw new ArgumentException($"Array of size {arr.Length} does not have enough room" +
					$" for {Count} elements starting at {start}.");
			}

			Node n = _active;
			int i = start;
			do
			{
				arr[i] = n.Data;

				n = n.Next;
				i++;
			} while (n != _active);
		}

		public RingCollection<T> Shuffle(Random rand = null)
		{
			if (rand == null)
			{
				rand = new Random();
			}

			List<T> list = this.ToList();

			RingCollection<T> res = new RingCollection<T>();
			while (!list.IsEmpty())
			{
				int i = rand.Next(list.Count);
				T t = list[i];
				list.RemoveAt(i);
				res.Add(t);
			}

			return res;
		}

		public override string ToString()
		{
			return $"Count: {Count} items";
		}

		public string ToContentsString()
		{
			string res = "";
			foreach (T t in this)
			{
				res += $"[{t}] <-> ";
			}
			return res + "...";
		}
	}
}