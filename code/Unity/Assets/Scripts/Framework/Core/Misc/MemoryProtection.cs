using UnityEngine;

enum Operation
{
	Add,
	Sub,

	Count
}

public struct Int
{
	int _data;
	Operation _operation;
	int _random;

	public Int(int val)
	{
		_operation = (Operation)Random.Range(0, (int)Operation.Count);
		_random = Random.Range(1000, 10000);
		switch (_operation) {
			case Operation.Add:
				_data = val + _random;
				break;
			case Operation.Sub:
				_data = val - _random;
				break;
			default:
				_data = val;
				break;
		}
	}

	public int ToInt()
	{
		switch (_operation) {
			case Operation.Add:
				return _data - _random;
			case Operation.Sub:
				return _data + _random;
			default:
				return _data;
		}
	}

	public override string ToString()
	{
		return ToInt().ToString();
	}

	public string ToString(string format)
	{
		return ToInt().ToString(format);
	}

	public override bool Equals(object i)
	{
		return (i is Int && (Int)i == this) || (i is int && (int)i == this.ToInt());
	}

	public override int GetHashCode()
	{
		return ToInt().GetHashCode();
	}

	public static Int operator ++(Int i)
	{
		return new Int(i.ToInt() + 1);
	}

	public static Int operator --(Int i)
	{
		return new Int(i.ToInt() - 1);
	}

	public static Int operator +(Int i1, Int i2)
	{
		return new Int(i1.ToInt() + i2.ToInt());
	}

	public static Int operator -(Int i1, Int i2)
	{
		return new Int(i1.ToInt() - i2.ToInt());
	}

	public static Int operator *(Int i1, Int i2)
	{
		return new Int(i1.ToInt() * i2.ToInt());
	}

	public static Int operator /(Int i1, Int i2)
	{
		return new Int(i1.ToInt() / i2.ToInt());
	}

	public static Int operator +(Int i1, int i2)
	{
		return new Int(i1.ToInt() + i2);
	}

	public static Int operator -(Int i1, int i2)
	{
		return new Int(i1.ToInt() - i2);
	}

	public static Int operator *(Int i1, int i2)
	{
		return new Int(i1.ToInt() * i2);
	}

	public static Int operator /(Int i1, int i2)
	{
		return new Int(i1.ToInt() / i2);
	}

	public static bool operator ==(Int i1, Int i2)
	{
		return i1.ToInt() == i2.ToInt();
	}

	public static bool operator !=(Int i1, Int i2)
	{
		return i1.ToInt() != i2.ToInt();
	}

	public static bool operator >(Int i1, Int i2)
	{
		return i1.ToInt() > i2.ToInt();
	}

	public static bool operator <(Int i1, Int i2)
	{
		return i1.ToInt() < i2.ToInt();
	}

	public static bool operator >=(Int i1, Int i2)
	{
		return i1.ToInt() >= i2.ToInt();
	}

	public static bool operator <=(Int i1, Int i2)
	{
		return i1.ToInt() <= i2.ToInt();
	}

	public static bool operator ==(Int i1, int i2)
	{
		return i1.ToInt() == i2;
	}

	public static bool operator !=(Int i1, int i2)
	{
		return i1.ToInt() != i2;
	}

	public static bool operator >(Int i1, int i2)
	{
		return i1.ToInt() > i2;
	}

	public static bool operator <(Int i1, int i2)
	{
		return i1.ToInt() < i2;
	}

	public static bool operator >=(Int i1, int i2)
	{
		return i1.ToInt() >= i2;
	}

	public static bool operator <=(Int i1, int i2)
	{
		return i1.ToInt() <= i2;
	}

	public static bool operator ==(int i1, Int i2)
	{
		return i1 == i2.ToInt();
	}

	public static bool operator !=(int i1, Int i2)
	{
		return i1 != i2.ToInt();
	}

	public static bool operator >(int i1, Int i2)
	{
		return i1 > i2.ToInt();
	}

	public static bool operator <(int i1, Int i2)
	{
		return i1 < i2.ToInt();
	}

	public static bool operator >=(int i1, Int i2)
	{
		return i1 >= i2.ToInt();
	}

	public static bool operator <=(int i1, Int i2)
	{
		return i1 <= i2.ToInt();
	}
}


public struct Float
{
	float _data;
	Operation _operation;
	float _random;

	public Float(float val)
	{
		_operation = (Operation)Random.Range(0, (int)Operation.Count);
		_random = Random.Range(100f, 1000f);
		switch (_operation) {
			case Operation.Add:
				_data = val + _random;
				break;
			case Operation.Sub:
				_data = val - _random;
				break;
			default:
				_data = val;
				break;
		}
	}

	public float ToFloat()
	{
		switch (_operation) {
			case Operation.Add:
				return _data - _random;
			case Operation.Sub:
				return _data + _random;
			default:
				return _data;
		}
	}

	public override string ToString()
	{
		return ToFloat().ToString();
	}

	public string ToString(string format)
	{
		return ToFloat().ToString(format);
	}

	public override bool Equals(object f)
	{
		return (f is Float && (Float)f == this) || (f is float && (float)f == this.ToFloat());
	}

	public override int GetHashCode()
	{
		return ToFloat().GetHashCode();
	}

	public static Float operator ++(Float f)
	{
		return new Float(f.ToFloat() + 1);
	}

	public static Float operator --(Float f)
	{
		return new Float(f.ToFloat() - 1);
	}

	public static Float operator +(Float f1, Float f2)
	{
		return new Float(f1.ToFloat() + f2.ToFloat());
	}

	public static Float operator -(Float f1, Float f2)
	{
		return new Float(f1.ToFloat() - f2.ToFloat());
	}

	public static Float operator *(Float f1, Float f2)
	{
		return new Float(f1.ToFloat() * f2.ToFloat());
	}

	public static Float operator /(Float f1, Float f2)
	{
		return new Float(f1.ToFloat() / f2.ToFloat());
	}

	public static Float operator +(Float f1, float f2)
	{
		return new Float(f1.ToFloat() + f2);
	}

	public static Float operator -(Float f1, float f2)
	{
		return new Float(f1.ToFloat() - f2);
	}

	public static Float operator *(Float f1, float f2)
	{
		return new Float(f1.ToFloat() * f2);
	}

	public static Float operator /(Float f1, float f2)
	{
		return new Float(f1.ToFloat() / f2);
	}

	public static bool operator ==(Float f1, Float f2)
	{
		return f1.ToFloat() == f2.ToFloat();
	}

	public static bool operator !=(Float f1, Float f2)
	{
		return f1.ToFloat() != f2.ToFloat();
	}

	public static bool operator >(Float f1, Float f2)
	{
		return f1.ToFloat() > f2.ToFloat();
	}

	public static bool operator <(Float f1, Float f2)
	{
		return f1.ToFloat() < f2.ToFloat();
	}

	public static bool operator >=(Float f1, Float f2)
	{
		return f1.ToFloat() >= f2.ToFloat();
	}

	public static bool operator <=(Float f1, Float f2)
	{
		return f1.ToFloat() <= f2.ToFloat();
	}

	public static bool operator ==(Float f1, float f2)
	{
		return f1.ToFloat() == f2;
	}

	public static bool operator !=(Float f1, float f2)
	{
		return f1.ToFloat() != f2;
	}

	public static bool operator >(Float f1, float f2)
	{
		return f1.ToFloat() > f2;
	}

	public static bool operator <(Float f1, float f2)
	{
		return f1.ToFloat() < f2;
	}

	public static bool operator >=(Float f1, float f2)
	{
		return f1.ToFloat() >= f2;
	}

	public static bool operator <=(Float f1, float f2)
	{
		return f1.ToFloat() <= f2;
	}

	public static bool operator ==(float f1, Float f2)
	{
		return f1 == f2.ToFloat();
	}

	public static bool operator !=(float f1, Float f2)
	{
		return f1 != f2.ToFloat();
	}

	public static bool operator >(float f1, Float f2)
	{
		return f1 > f2.ToFloat();
	}

	public static bool operator <(float f1, Float f2)
	{
		return f1 < f2.ToFloat();
	}

	public static bool operator >=(float f1, Float f2)
	{
		return f1 >= f2.ToFloat();
	}

	public static bool operator <=(float f1, Float f2)
	{
		return f1 <= f2.ToFloat();
	}
}