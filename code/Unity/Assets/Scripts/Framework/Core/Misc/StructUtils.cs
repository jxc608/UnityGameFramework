using System.Collections.Generic;
using System;

namespace StructUtils
{
	// 双值的元组，可理解为两个成员的结构体
	public class Tuple<T1, T2>
	{
		public T1 _item1;
		public T2 _item2;

		public Tuple(T1 item1, T2 item2)
		{
			_item1 = item1;
			_item2 = item2;
		}

		public override bool Equals(object obj)
		{
			if (obj is Tuple<T1, T2>) {
				return ((Tuple<T1, T2>)obj)._item1.Equals(_item1) && ((Tuple<T1, T2>)obj)._item2.Equals(_item2);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return _item1.GetHashCode() + _item2.GetHashCode();
		}
	}

	// 有顺序的字典容器，使用List容器来实现
	public class OrderedDictionary<TKey, TValue>
	{
		public List<Tuple<TKey, TValue>> _list = null;

		public OrderedDictionary()
		{
			_list = new List<Tuple<TKey, TValue>>();
		}

		public int Count {
			get { return _list.Count; }
		}

		public TValue this[TKey key] {  
			get {
				for (int i = 0; i < _list.Count; ++i) {
					if (_list[i]._item1.Equals(key)) {
						return _list[i]._item2;
					}
				}
				return default (TValue);
			}  
			set {
				for (int i = 0; i < _list.Count; ++i) {
					if (_list[i]._item1.Equals(key)) {
						_list[i]._item2 = value;
						return;
					}
				}
				Add(key, value);
			}  
		}

		public List<TKey> Keys {
			get {
				var keys = new List<TKey>();
				for (int i = 0; i < _list.Count; ++i) {
					keys.Add(_list[i]._item1);
				}
				return keys;
			}
		}

		public List<TValue> Values {
			get {
				var values = new List<TValue>();
				for (int i = 0; i < _list.Count; ++i) {
					values.Add(_list[i]._item2);
				}
				return values;
			}
		}

		public TKey KeyAt(int i)
		{
			if (i < 0 || i > Count)
				throw new Exception("Array index out of range: " + i);

			return _list[i]._item1;
		}

		public Tuple<TKey, TValue> PairAt(int i)
		{
			if (i < 0 || i > Count)
				throw new Exception("Array index out of range: " + i);

			return _list[i];
		}

		public void Add(TKey key, TValue value)
		{
			_list.Add(new Tuple<TKey, TValue>(key, value));
		}

		public int IndexOf(TKey key)
		{
			for (int i = 0; i < _list.Count; ++i) {
				if (_list[i]._item1.Equals(key)) {
					return i;
				}
			}

			return -1;
		}

		public bool ContainsKey(TKey key)
		{
			return IndexOf(key) >= 0;
		}

		public void Remove(TKey key)
		{
			for (int i = 0; i < _list.Count; ++i) {
				if (_list[i]._item1.Equals(key)) {
					_list.RemoveAt(i);
					return;
				}
			}
		}

		public void RemoveRange(int index, int count)
		{
			_list.RemoveRange(index, count);
		}

		public void Sort(IComparer<Tuple<TKey, TValue>> comparer)
		{
			_list.Sort(comparer);
		}

		public void Sort(Comparison<Tuple<TKey, TValue>> comparison)
		{
			_list.Sort(comparison);
		}
	}
}
