using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Text.RegularExpressions;
using System.Numerics;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Collections;

using Isac.Isql.Collections;

namespace Isac
{

	#region numeric types
	
	
	public struct Bit : IComparable, IComparable<Bit>, IEquatable<Bit>
	{
		public byte MaxValue
		{
			get { return 68; }
		}
		
		public byte MinValue
		{
			get { return 0; }
		}
		
		private byte value;

		internal byte Value
		{
			get
			{
				return this.value;
			}
			set
			{
				string tempVal = value.ToString();
				if (value < MinValue || value > MaxValue)
					throw new Isql.ISqlArguementException($"Error: value can not be lower or higher than MinValue and MaxValue");
				
				this.value = value;
			}
		}

		public Bit(Bit value) :this()
		{
			this = value; 
		}
		
		public int CompareTo(Bit other)
		{
			if (other.GetType() == typeof(Bit))
			{
				if (this.Value > other.Value) return 1;
				if (this.Value < other.Value) return -1;
				else return 0;
			}
			else throw new ArgumentException($"Error: this is not a type of '{other.GetType()}'");
		}

		public int CompareTo(object obj)
		{
			Bit tbit = (Bit)obj;
			if (tbit.GetType() == typeof(Bit))
			{
				if (this.Value > tbit.Value) return 1;
				if (this.Value < tbit.Value) return -1;
				else return 0;
			}
			else throw new ArgumentException($"Error: this is not a type of '{tbit.GetType()}'");
		}

		public bool Equals(Bit other)
		{
			if (other.Value == this.Value)
				return true;
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(Bit))
				return this.Equals(Bit.Parse(obj.ToString()));
			return false;
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}

		public static Bit Parse(string value)
		{
			Bit txt = byte.Parse(value.ToString());
			return txt;
		}

		#region coverting to tinyInt Type
		//implicit conversions
		public static implicit operator Bit(byte value)
		{
			Bit b = new Bit();
			b.Value = (byte)value;
			return b;
		}

		public static implicit operator byte(Bit value)
		{
			return value.Value;
		}

		#endregion

		#region overloading operators
		//operators overloading
		public static Bit operator -(Bit b, Bit value)
		{
			Bit bit = new Bit();
			bit.Value = (byte)(b.Value - value.Value);
			return bit;
		}

		public static Bit operator +(Bit a, Bit b)
		{
			Bit bit = new Bit();
			bit.Value = (byte)(a.Value + b.Value);
			return bit;
		}

		public static Bit operator *(Bit a, Bit b)
		{
			Bit bit = new Bit();
			bit.Value = (byte)(a.Value * b.Value);
			return bit;
		}

		public static Bit operator /(Bit a, Bit b)
		{
			Bit bit = new Bit();
			bit.Value = (byte)(a.Value / b.Value);
			return bit;
		}

		public static Bit operator %(Bit a, Bit b)
		{
			Bit bit = new Bit();
			bit.Value = (byte)(a.Value % b.Value);
			return bit;
		}

		//one side operators overloading
		public static Bit operator ++(Bit b)
		{
			b.Value = b.Value++;
			return b;
		}

		public static Bit operator --(Bit b)
		{
			b.Value = b.Value--;
			return b;
		}
		#endregion
		
	}

	public struct TinyInt : IComparable, IComparable<TinyInt>, IEquatable<TinyInt>
	{
		public const sbyte MaxValue = 127;
		public const sbyte MinValue = -128;
	
		private sbyte value;
		internal sbyte Value
		{
			get
			{
				return value;
			}
			set
			{
				if (!(value < MinValue) && !(value > MaxValue))
					this.value = (sbyte)value;
				else throw new ArgumentOutOfRangeException($"'{value}'\nRange is '{MinValue}' to '{MaxValue}' ");
			}
		}
		

		public TinyInt(TinyInt value)
		{
			this = value; 
		}
		
		public int CompareTo(TinyInt other)
		{
			if (other.GetType() == typeof(TinyInt))
			{
				if (this.Value > other.Value) return 1;
				if (this.Value < other.Value) return -1;
				else return 0;
			}
			else throw new ArgumentException("Error: this is not a type of tinyInt");
		}

		public int CompareTo(object obj)
		{
			TinyInt tbit = (TinyInt)obj;
			if (tbit.GetType() == typeof(TinyInt))
			{
				if (this.Value > tbit.Value) return 1;
				if (this.Value < tbit.Value) return -1;
				else return 0;
			}
			else throw new ArgumentException("Error: this is not a type of tinyInt");
		}

		public bool Equals(TinyInt other)
		{
			if (other.Value == this.Value)
				return true;
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(TinyInt))
				return this.Equals(TinyInt.Parse(obj.ToString()));
			return false;
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}

		public static TinyInt Parse(string value)
		{
			TinyInt txt = sbyte.Parse(value.ToString());
			return txt;
		}

		//my type conversions

		#region coverting to tinyInt Type
		//implicit conversions
		public static implicit operator TinyInt(SByte value)
		{
			TinyInt b = new TinyInt();
			b.Value = value;
			return b;
		}

		public static implicit operator TinyInt(Int32 value)
		{
			TinyInt b = new TinyInt();
			b.Value = (SByte)value;
			return b;
		}
		#endregion

		#region overloading operators
		//operators overloading
		public static TinyInt operator -(TinyInt b, TinyInt value)
		{
			TinyInt m = TinyInt.Parse((b.Value - value.Value).ToString());
			return m;
		}

		public static TinyInt operator +(TinyInt a, TinyInt b)
		{
			TinyInt m = TinyInt.Parse((a.Value + b.Value).ToString());
			return m;
		}

		public static TinyInt operator *(TinyInt a, TinyInt b)
		{
			TinyInt m = TinyInt.Parse((a.Value * b.Value).ToString());
			return m;
		}

		public static TinyInt operator /(TinyInt a, TinyInt b)
		{
			TinyInt m = TinyInt.Parse((a.Value / b.Value).ToString());
			return m;
		}

		public static TinyInt operator %(TinyInt a, TinyInt b)
		{
			TinyInt m = TinyInt.Parse((a.Value % b.Value).ToString());
			return m;
		}

		public static TinyInt operator ^(TinyInt a, TinyInt b)
		{
			TinyInt m = TinyInt.Parse((a.Value ^ b.Value).ToString());
			return m;
		}

		public static TinyInt operator >>(TinyInt b, int value)
		{
			TinyInt m = TinyInt.Parse((b.Value >> value).ToString());
			return m;
		}

		public static TinyInt operator <<(TinyInt b, int value)
		{
			TinyInt m = TinyInt.Parse((b.Value >> value).ToString());
			return m;
		}

		//one side operators overloading
		public static TinyInt operator ++(TinyInt b)
		{
			TinyInt m = b.Value + 1;
			return m;
		}
		public static TinyInt operator --(TinyInt b)
		{
			TinyInt m = b.Value - 1;
			return m;
		}
		#endregion

		#region converting back to other types
		//converting back to other types implicitly
		public static implicit operator SByte(TinyInt v)
		{
			return (SByte)v.Value;
		}

		public static implicit operator Int32(TinyInt v)
		{
			return (Int32)v.Value;
		}
		#endregion
	}

	public struct Integer : IComparable, IComparable<Integer>, IEquatable<Integer>
	{
		public const int MaxValue = int.MaxValue;
		public const int MinValue = int.MinValue;

		private int value;
		internal int Value
		{
			get
			{
				return value;
			}
			set
			{
				if (!(value < MinValue) && !(value > MaxValue))
					this.value = (int)value;
				else throw new ArgumentOutOfRangeException($"'{value}'\nRange is '{MinValue}' to '{MaxValue}' ");
			}
		}


		public Integer(Integer value) :this()
		{
			this = value; 
		}
		
		public int CompareTo(Integer other)
		{
			if (other.GetType() == typeof(Integer))
			{
				if (this.Value > other.Value) return 1;
				if (this.Value < other.Value) return -1;
				else return 0;
			}
			else throw new ArgumentException($"Error: this is not a type of '{other.GetType()}'");
		}

		public int CompareTo(object obj)
		{
			Integer tbit = (Integer)obj;
			if (tbit.GetType() == typeof(Integer))
			{
				if (this.Value > tbit.Value) return 1;
				if (this.Value < tbit.Value) return -1;
				else return 0;
			}
			else throw new ArgumentException($"Error: this is not a type of '{tbit.GetType()}'");
		}

		public bool Equals(Integer other)
		{
			if (other.Value == this.Value)
				return true;
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(Integer))
				return this.Equals(Integer.Parse(obj.ToString()));
			return false;
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}

		public static Integer Parse(string value)
		{
			Integer integer = int.Parse(value);
			return integer;
		}

		//my type conversions

		#region coverting to integer Type
		//implicit conversions

		public static implicit operator Integer(Int16 value)
		{
			Integer b = new Integer();
			b.Value = value;
			return b;
		}

		public static implicit operator Integer(Int32 value)
		{
			Integer b = new Integer();
			b.Value = value;
			return b;
		}

		//explicit conversions
		public static explicit operator Integer(Int64 value)
		{
			Integer b = new Integer();
			b.Value = (int)value;
			return b;
		}

		public static explicit operator Integer(Decimal value)
		{
			Integer b = new Integer();
			b.Value = (int)value;
			return b;
		}

		public static explicit operator Integer(UInt16 value)
		{
			Integer b = new Integer();
			b.Value = (int)value;
			return b;
		}

		public static explicit operator Integer(UInt32 value)
		{
			Integer b = new Integer();
			b.Value = (int)value;
			return b;
		}

		public static explicit operator Integer(UInt64 value)
		{
			Integer b = new Integer();
			b.Value = (int)value;
			return b;
		}

		#endregion

		#region overloading operators
		//operators overloading
		public static Integer operator -(Integer b, Integer value)
		{
			return b.Value - value.Value;
		}

		public static Integer operator +(Integer a, Integer b)
		{
			return a.Value + b.Value;
		}

		public static Integer operator *(Integer a, Integer b)
		{
			return a.Value * b.Value;
		}

		public static Integer operator /(Integer a, Integer b)
		{
			return a.Value / b.Value;
		}

		public static Integer operator %(Integer a, Integer b)
		{
			return a.Value % b.Value;
		}

		public static Integer operator ^(Integer a, Integer b)
		{
			return a.Value ^ b.Value;
		}

		public static Integer operator >>(Integer b, int value)
		{
			return b.Value >> value;
		}

		public static Integer operator <<(Integer b, int value)
		{
			return b.Value << value;
		}

		//one side operators overloading
		public static Integer operator +(Integer a, int value)
		{
			return a.Value + value;
		}
		public static Integer operator -(Integer b, int value)
		{
			return b.Value - value;
		}
		public static Integer operator ++(Integer b)
		{
			return b.Value + 1;
		}
		public static Integer operator --(Integer b)
		{
			return b.Value - 1;
		}
		#endregion

		#region converting back to other types
		//converting back to other types implicitly
		public static implicit operator SByte(Integer v)
		{
			return (SByte)v.Value;
		}
		public static implicit operator Int16(Integer v)
		{
			return (Int16)v.Value;
		}
		public static implicit operator Int32(Integer v)
		{
			return (Int32)v.Value;
		}
		//converting back to other types explicitly
		public static explicit operator Int64(Integer v)
		{
			return (Int64)v.Value;
		}
		public static explicit operator Decimal(Integer v)
		{
			return (Decimal)v.Value;
		}
		public static explicit operator UInt16(Integer v)
		{
			return (UInt16)v.Value;
		}
		public static explicit operator UInt32(Integer v)
		{
			return (UInt32)v.Value;
		}
		public static explicit operator UInt64(Integer v)
		{
			return (UInt64)v.Value;
		}
		#endregion
	}

	public struct Point : IComparable, IComparable<Point>, IEquatable<Point>
	{
		public const double MaxValue = double.MaxValue;
		public const double MinValue = double.MinValue;

		private double value;
		internal double Value
		{
			get
			{
				return value;
			}
			set
			{
				if (!(value < MinValue) && !(value > MaxValue))
					this.value = (double)value;
				else throw new ArgumentOutOfRangeException($"'{value}'\nRange is '{MinValue}' to '{MaxValue}' ");
			}
		}
		
		public Point(Point value) :this()
		{
			this = value; 
		}
		
		public int CompareTo(Point other)
		{
			if (other.GetType() == typeof(Point))
			{
				if (this.Value > other.Value) return 1;
				if (this.Value < other.Value) return -1;
				else return 0;
			}
			else throw new ArgumentException($"Error: this is not a type of '{other.GetType()}'");
		}

		public int CompareTo(object obj)
		{
			Point tbit = (Point)obj;
			if (tbit.GetType() == typeof(Point))
			{
				if (this.Value > tbit.Value) return 1;
				if (this.Value < tbit.Value) return -1;
				else return 0;
			}
			else throw new ArgumentException($"Error: this is not a type of '{tbit.GetType()}'");
		}

		public bool Equals(Point other)
		{
			if (other.Value == this.Value)
				return true;
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(Point))
				return this.Equals(Point.Parse(obj.ToString()));
			return false;
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}

		public static Point Parse(string value)
		{
			Point p = double.Parse(value);
			return p;
		}

		//my type conversions
		#region coverting to Point Type
		//implicit conversions
		public static implicit operator Point(Double value)
		{
			Point b = new Point();
			b.Value = value;
			return b;
		}

		public static implicit operator Point(SByte value)
		{
			Point b = new Point();
			b.Value = value;
			return b;
		}

		public static implicit operator Point(Int16 value)
		{
			Point b = new Point();
			b.Value = value;
			return b;
		}

		public static implicit operator Point(Int32 value)
		{
			Point b = new Point();
			b.Value = value;
			return b;
		}

		public static implicit operator Point(Int64 value)
		{
			Point b = new Point();
			b.Value = value;
			return b;
		}

		public static implicit operator Point(UInt16 value)
		{
			Point b = new Point();
			b.Value = value;
			return b;
		}

		public static implicit operator Point(UInt32 value)
		{
			Point b = new Point();
			b.Value = value;
			return b;
		}

		public static implicit operator Point(UInt64 value)
		{
			Point b = new Point();
			b.Value = value;
			return b;
		}

		//explicit conversions
		public static explicit operator Point(Decimal value)
		{
			Point b = new Point();
			b.Value = (double)value;
			return b;
		}

		#endregion

		#region overloading operators
		//operators overloading
		public static Point operator -(Point b, Point value)
		{
			Point x = new Point();
			x.Value = b.Value - value.Value;
			return x;
		}

		public static Point operator +(Point a, Point b)
		{
			Point x = new Point();
			x.Value = a.Value + b.Value;
			return x;
		}

		public static Point operator *(Point a, Point b)
		{
			Point x = new Point();
			x.Value = a.Value * b.Value;
			return x;
		}

		public static Point operator /(Point a, Point b)
		{
			Point x = new Point();
			x.Value = a.Value / b.Value;
			return x;
		}

		public static Point operator %(Point a, Point b)
		{
			Point x = new Point();
			x.Value = a.Value % b.Value;
			return x;
		}

		//one side operators overloading
		public static Point operator ++(Point b)
		{
			Point x = new Point();
			x.Value = b.Value + 1;
			return x;
		}

		public static Point operator --(Point b)
		{
			Point x = new Point();
			x.Value = b.Value - 1;
			return x;
		}

		#endregion

		#region converting back to other types
		//converting back to other types implicitly
		public static implicit operator SByte(Point v)
		{
			return (SByte)v.Value;
		}

		public static implicit operator Int16(Point v)
		{
			return (Int16)v.Value;
		}

		public static implicit operator Int32(Point v)
		{
			return (Int32)v.Value;
		}

		//converting back to other types explicitly
		public static explicit operator Int64(Point v)
		{
			return (Int64)v.Value;
		}

		public static explicit operator Decimal(Point v)
		{
			return (Decimal)v.Value;
		}

		public static explicit operator UInt16(Point v)
		{
			return (UInt16)v.Value;
		}

		public static explicit operator UInt32(Point v)
		{
			return (UInt32)v.Value;
		}

		public static explicit operator UInt64(Point v)
		{
			return (UInt64)v.Value;
		}

		#endregion
	}

	public struct FloatPoint : IComparable, IComparable<FloatPoint>, IEquatable<FloatPoint>
	{
		public const float MaxValue = float.MaxValue;
		public const float MinValue = float.MinValue;

		private float value;
		internal float Value
		{
			get
			{
				return value;
			}
			set
			{
				if (!(value < MinValue) && !(value > MaxValue))
					this.value = (float)value;
				else throw new ArgumentOutOfRangeException($"'{value}'\nRange is '{MinValue}' to '{MaxValue}' ");
			}
		}

		public FloatPoint(FloatPoint value) :this()
		{
			this = value; 
		}
		
		public int CompareTo(FloatPoint other)
		{
			if (other.GetType() == typeof(FloatPoint))
			{
				if (this.Value > other.Value) return 1;
				if (this.Value < other.Value) return -1;
				else return 0;
			}
			else throw new ArgumentException($"Error: this is not a type of '{other.GetType()}'");
		}

		public int CompareTo(object obj)
		{
			FloatPoint tbit = (FloatPoint)obj;
			if (tbit.GetType() == typeof(FloatPoint))
			{
				if (this.Value > tbit.Value) return 1;
				if (this.Value < tbit.Value) return -1;
				else return 0;
			}
			else throw new ArgumentException($"Error: this is not a type of '{tbit.GetType()}'");
		}

		public bool Equals(FloatPoint other)
		{
			if (other.Value == this.Value)
				return true;
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(FloatPoint))
				return this.Equals(FloatPoint.Parse(obj.ToString()));
			return false;
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}

		public static FloatPoint Parse(string value)
		{
			FloatPoint fp = float.Parse(value);
			return fp;
		}

		//my type conversions
		public static implicit operator FloatPoint(Bit v)
		{
			return (int)v.Value;
		}

		public static implicit operator Point(FloatPoint value)
		{
			return value.Value;
		}

		#region coverting to tinyInt Type
		//implicit conversions
		public static implicit operator FloatPoint(Single value)
		{
			FloatPoint b = new FloatPoint();
			b.Value = value;
			return b;
		}

		public static implicit operator Single(FloatPoint value)
		{
			return value.Value;
		}

		public static implicit operator double(FloatPoint value)
		{
			return value.Value;
		}

		public static implicit operator FloatPoint(SByte value)
		{
			FloatPoint b = new FloatPoint();
			b.Value = value;
			return b;
		}

		public static implicit operator FloatPoint(Int16 value)
		{
			FloatPoint b = new FloatPoint();
			b.Value = value;
			return b;
		}

		public static implicit operator FloatPoint(Int32 value)
		{
			FloatPoint b = new FloatPoint();
			b.Value = value;
			return b;
		}

		//explicit conversions
		public static explicit operator FloatPoint(Int64 value)
		{
			FloatPoint b = new FloatPoint();
			b.Value = (int)value;
			return b;
		}

		public static explicit operator FloatPoint(Decimal value)
		{
			FloatPoint b = new FloatPoint();
			b.Value = (int)value;
			return b;
		}

		public static explicit operator FloatPoint(UInt16 value)
		{
			FloatPoint b = new FloatPoint();
			b.Value = (int)value;
			return b;
		}

		public static explicit operator FloatPoint(UInt32 value)
		{
			FloatPoint b = new FloatPoint();
			b.Value = (int)value;
			return b;
		}

		public static explicit operator FloatPoint(UInt64 value)
		{
			FloatPoint b = new FloatPoint();
			b.Value = (int)value;
			return b;
		}

		#endregion

		#region overloading operators
		//operators overloading
		public static FloatPoint operator -(FloatPoint b, FloatPoint value)
		{
			return b.Value - value.Value;
		}

		public static FloatPoint operator +(FloatPoint a, FloatPoint b)
		{
			return a.Value + b.Value;
		}

		public static FloatPoint operator *(FloatPoint a, FloatPoint b)
		{
			return a.Value * b.Value;
		}

		public static FloatPoint operator /(FloatPoint a, FloatPoint b)
		{
			return a.Value / b.Value;
		}

		public static FloatPoint operator %(FloatPoint a, FloatPoint b)
		{
			return a.Value % b.Value;
		}

		//one side operators overloading
		public static FloatPoint operator ++(FloatPoint b)
		{
			return b.Value + 1;
		}

		public static FloatPoint operator --(FloatPoint b)
		{
			return b.Value - 1;
		}

		#endregion

		#region converting back to other types
		//converting back to other types implicitly
		public static implicit operator SByte(FloatPoint v)
		{
			return (SByte)v.Value;
		}

		public static implicit operator Int16(FloatPoint v)
		{
			return (Int16)v.Value;
		}

		public static implicit operator Int32(FloatPoint v)
		{
			return (Int32)v.Value;
		}

		//converting back to other types explicitly
		public static explicit operator Int64(FloatPoint v)
		{
			return (Int64)v.Value;
		}

		public static explicit operator Decimal(FloatPoint v)
		{
			return (Decimal)v.Value;
		}

		public static explicit operator UInt16(FloatPoint v)
		{
			return (UInt16)v.Value;
		}

		public static explicit operator UInt32(FloatPoint v)
		{
			return (UInt32)v.Value;
		}

		public static explicit operator UInt64(FloatPoint v)
		{
			return (UInt64)v.Value;
		}

		#endregion
	}

	public struct BigInt : IComparable, IComparable<BigInt>, IEquatable<BigInt>
	{
		public const long MaxValue = long.MaxValue;
		public const long MinValue = long.MinValue;

		private long value;
		internal long Value
		{
			get
			{
				return value;
			}
			set
			{
				if (!(value < MinValue) && !(value > MaxValue))
					this.value = (long)value;
				else throw new ArgumentOutOfRangeException($"'{value}'\nRange is '{MinValue}' to '{MaxValue}' ");
			}
		}
		
		public BigInt(BigInt value) :this()
		{
			this = value; 
		}
		
		public int CompareTo(BigInt other)
		{
			if (other.GetType() == typeof(BigInt))
			{
				if (this.Value > other.Value) return 1;
				if (this.Value < other.Value) return -1;
				else return 0;
			}
			else throw new ArgumentException($"Error: this is not a type of '{other.GetType()}'");
		}

		public int CompareTo(object obj)
		{
			BigInt tbit = (BigInt)obj;
			if (tbit.GetType() == typeof(BigInt))
			{
				if (this.Value > tbit.Value) return 1;
				if (this.Value < tbit.Value) return -1;
				else return 0;
			}
			else throw new ArgumentException($"Error: this is not a type of '{tbit.GetType()}'");
		}

		public bool Equals(BigInt other)
		{
			if (other.Value == this.Value)
				return true;
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(BigInt))
				return this.Equals(BigInt.Parse(obj.ToString()));
			return false;
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}

		public static BigInt Parse(string value)
		{
			BigInt bi = long.Parse(value);
			return bi;
		}

		//my type conversions

		#region coverting to tinyInt Type
		//implicit conversions
		public static implicit operator BigInt(UInt32 value)
		{
			BigInt b = new BigInt();
			b.Value = value;
			return b;
		}
		
		public static implicit operator BigInt(Int64 value)
		{
			BigInt bi = new BigInt();
			bi.Value = value;
			return bi;
		}

		public static implicit operator BigInt(UInt16 value)
		{
			BigInt b = new BigInt();
			b.Value = value;
			return b;
		}
		//explicit conversions
		public static explicit operator BigInt(Decimal value)
		{
			BigInt b = new BigInt();
			b.Value = (long)value;
			return b;
		}

		public static explicit operator BigInt(UInt64 value)
		{
			BigInt b = new BigInt();
			b.Value = (Int64)value;
			return b;
		}
		#endregion

		#region overloading operators
		//operators overloading
		public static BigInt operator -(BigInt b, BigInt value)
		{
			BigInt big = new BigInt();
			big.Value = b.Value - value.Value;
			return big;
		}
		public static BigInt operator +(BigInt a, BigInt b)
		{
			BigInt big = new BigInt();
			big.Value = a.Value + b.Value;
			return big;
		}
		public static BigInt operator *(BigInt a, BigInt b)
		{
			BigInt big = new BigInt();
			big.Value = a.Value * b.Value;
			return big;
		}
		public static BigInt operator /(BigInt a, BigInt b)
		{
			BigInt big = new BigInt();
			big.Value = a.Value / b.Value;
			return big;
		}
		public static BigInt operator %(BigInt a, BigInt b)
		{
			BigInt big = new BigInt();
			big.Value = a.Value % b.Value;
			return big;
		}
		public static BigInt operator ^(BigInt a, BigInt b)
		{
			BigInt big = new BigInt();
			big.Value = a.Value ^ b.Value;
			return big;
		}
		public static BigInt operator >>(BigInt b, int value)
		{
			BigInt big = new BigInt();
			big.Value = b.Value >> value;
			return big;
		}
		public static BigInt operator <<(BigInt b, int value)
		{
			BigInt big = new BigInt();
			big.Value = b.Value << value;
			return big;
		}

		//one side operators overloading
		public static BigInt operator ++(BigInt b)
		{
			BigInt big = new BigInt();
			big.Value = b.Value + 1;
			return big;
		}
		public static BigInt operator --(BigInt b)
		{
			BigInt big = new BigInt();
			big.Value = b.Value - 1;
			return big;
		}
		#endregion

		#region converting back to other types
		//converting back to other types implicitly
		public static implicit operator Int64(BigInt v)
		{
			return v.Value;
		}
		//converting back to other types explicitly
		public static explicit operator SByte(BigInt v)
		{
			return (SByte)v.Value;
		}
		public static explicit operator Int16(BigInt v)
		{
			return (Int16)v.Value;
		}
		public static explicit operator Int32(BigInt v)
		{
			return (Int32)v.Value;
		}
		public static explicit operator UInt64(BigInt v)
		{
			return (UInt64)v.Value;
		}
		public static explicit operator Double(BigInt v)
		{
			return (Double)v.Value;
		}
		public static explicit operator Decimal(BigInt v)
		{
			return (Decimal)v.Value;
		}
		public static explicit operator UInt16(BigInt v)
		{
			return (UInt16)v.Value;
		}
		public static explicit operator UInt32(BigInt v)
		{
			return (UInt32)v.Value;
		}
		#endregion
	}

	#endregion

	#region string types
	public struct Character : IComparable, IComparable<Character>, IEquatable<Character>
	{
		
		public Character this[int index]
		{
			get
			{
				if (index < 0 || index >= this.Length)
					throw new Isql.ISqlArguementException($"Error: index out of bounds");

				return Character.Parse(this.Value[index].ToString());
			}
		}

		private string value;
		internal string Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}
		
		public int Length
		{
			get
			{
				return this.Value.Length;
			}
		}

		public Character(Character character) : this()
		{
			this.Value = character.Value;
		}

		public int CompareTo(Character other)
		{
			if (other.GetType() == typeof(Character))
				return this.Value.CompareTo(other.Value);

			else throw new ArgumentException($"Error: this is not a type of '{other.GetType()}'");
		}

		public int CompareTo(object obj)
		{
			Character tbit = obj.ToString();
			if (tbit.GetType() == typeof(Character))
				return this.Value.CompareTo(tbit.Value);

			else throw new ArgumentException($"Error: this is not a type of '{tbit.GetType()}'");
		}

		public bool Equals(Character other)
		{
			if (other.Value == this.Value)
				return true;
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(Character))
				return this.Equals(Character.Parse(obj.ToString()));
			return false;
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}

		public static Character Parse(string value)
		{
			Character c = value;
			return c;
		}
		
		internal static Character Parse(string value, int length)
		{
			Character c = new Character();
			if(value.Length > length)
				throw new Isql.ISqlException($"Error: values length to long\nValue Length:'{value.Length}'\nLength: '{length}'");
			
			else if(value.Length == length)
				c.Value = value;
				
				
			else
			{
				c.Value = value;
				int repeat = length - value.Length;
				for(int i = 0; i < repeat; i++)
					c.Value += " ";
					
				c.Value = value;
			}
				
			return c;
		}

		//my type conversions
		public static implicit operator Character(string value)
		{
			Character c = new Character();
			c.Value = value;
			return c;
		}

		public static implicit operator Character(Varchar value)
		{
			Character c = new Character();
			c.Value = value.Value;
			return c;
		}

		#region coverting to tinyInt Type
		//implicit conversions
		/*public static implicit operator Character(Char value)
		{
			Character c = new Character();
			c.Value = value;
			return c;
		}
		//explicit conversions
		public static explicit operator Character(SByte value)
		{
			Character b = new Character();
			b.Value = (char)value;
			return b;
		}
		public static explicit operator Character(Int16 value)
		{
			Character b = new Character();
			b.Value = (char)value;
			return b;
		}
		public static explicit operator Character(Int32 value)
		{
			Character b = new Character();
			b.Value = (char)value;
			return b;
		}
		public static explicit operator Character(Int64 value)
		{
			Character b = new Character();
			b.Value = (char)value;
			return b;
		}
		public static explicit operator Character(Decimal value)
		{
			Character b = new Character();
			b.Value = (char)value;
			return b;
		}
		public static explicit operator Character(UInt16 value)
		{
			Character b = new Character();
			b.Value = (char)value;
			return b;
		}
		public static explicit operator Character(UInt32 value)
		{
			Character b = new Character();
			b.Value = (char)value;
			return b;
		}
		public static explicit operator Character(UInt64 value)
		{
			Character b = new Character();
			b.Value = (char)value;
			return b;
		}*/
		#endregion

		#region overloading operators
		public static Character operator +(Character a, Character b)
		{
			Character big = new Character();
			big.Value = a.Value + b.Value;
			return big;
		}
		//for same type
		public static string operator +(Character a, string b)
		{
			return a.Value + b;
		}
		
		public static string operator *(Character a, int b)
		{
			return string.Join("",Enumerable.Repeat(a.Value, b));
		}
		
		public static bool operator == (Character a, Character b)
		{
			return a.Value == b.Value;
		}
		
		public static bool operator != (Character a, Character b)
		{
			return a.Value != b.Value;
		}

		//operators overloading
		//none
		//one side operators overloading
		//none
		#endregion

		#region converting back to other types
		//converting back to other types implicitly
		/*public static implicit operator SByte(Character v)
		{
			return (SByte)v.Value;
		}
		public static implicit operator Int16(Character v)
		{
			return (Int16)v.Value;
		}
		public static implicit operator Int32(Character v)
		{
			return (Int32)v.Value;
		}
		//converting back to other types explicitly
		public static explicit operator Int64(Character v)
		{
			return (Int64)v.Value;
		}
		public static explicit operator Decimal(Character v)
		{
			return (Decimal)v.Value;
		}
		public static explicit operator UInt16(Character v)
		{
			return (UInt16)v.Value;
		}
		public static explicit operator UInt32(Character v)
		{
			return (UInt32)v.Value;
		}
		public static explicit operator UInt64(Character v)
		{
			return (UInt64)v.Value;
		}*/
		#endregion

	}

	public struct Text : IComparable, IComparable<Text>, IEquatable<Text>
	{
		public Text this[int index]
		{
			get
			{
				if (index < 0 || index >= this.Length)
					throw new Isql.ISqlArguementException($"Error: index out of bounds");

				return ToText(this.Value[index].ToString());
			}
		}

		public int Length
		{
			get
			{
				return this.Value.Length;
			}
		}

		private string value;
		internal string Value
		{
			get
			{
				return value;
			}
			set
			{
				if (value.StartsWith("`") && value.EndsWith("`") || value.Count(x => x == '`') <= 2)
					value = value.Replace("`", "");

				Fundamentals fun = new Fundamentals();
				fun.TextCheacker(value);
				this.value = value;
			}
		}

		public Text(Text value) :this()
		{
			this = value; 
		}
		
		public int CompareTo(object obj)
		{
			Text tbit = Text.ToText(obj.ToString());
			if (tbit.GetType() == typeof(Text))
			{
				return this.Value.CompareTo(tbit.Value);
			}
			else throw new ArgumentException($"Error: this is not a type of '{tbit.GetType()}'");
		}
		public bool Equals(Text other)
		{
			if (other.Value == this.Value)
				return true;
			return false;
		}
		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(Text) || obj.GetType() == typeof(string))
				return obj.ToString() == this.Value;
			return false;
		}
		public override string ToString()
		{
			return this.Value.ToString();
		}

		public override int GetHashCode()
		{
			return this.Value.GetHashCode();
		}

		public int CompareTo(Text other)
		{
			return this.Value.CompareTo(other.Value);
		}

		//my type conversions

		#region coverting to tinyInt Type
		//original conversion of main types
		public static implicit operator Text(string value)
		{
			Text txt = new Text();
			txt.Value = value;
			return txt;
		}
		public static implicit operator string(Text value)
		{
			return value.Value;
		}
		public static Text ToText<T>(T value)
		{
			Text txt = new Text();
			return txt.value = value.ToString();
		}
		//implicit conversions
		//none
		//explicit conversions
		//none
		#endregion

		#region overloading operators
		//operators overloading
		public static Text operator +(Text a, Text b)
		{
			Text big = new Text();
			big.Value = a.Value + b.Value;
			return big;
		}//for same type
		public static string operator +(Text a, string b)
		{
			return a.Value + b;
		}
		public static string operator *(Text a, int b)
		{
			return string.Join("",Enumerable.Repeat(a, b));
		}

		//for current type with base
		 //one side operators overloading
		#endregion

		#region converting back to other types
		//converting back to other types explicitly
		//none
		#endregion
	}

	public struct Varchar : IComparable, IComparable<Varchar>, IEquatable<Varchar>
	{
		public Varchar this[int index]
		{
			get
			{
				if (index < 0 || index >= this.Length)
					throw new Isql.ISqlArguementException($"Error: index out of bounds");

				return ToVarchar(this.Value[index].ToString());
			}
		}

		public int Length
		{
			get
			{
				return this.Value.Length;
			}
		}

		private string value;
		internal string Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}

		public Varchar(Varchar value) :this()
		{
			this = value; 
		}
		
		public int CompareTo(object obj)
		{
			Varchar tbit = Varchar.ToVarchar(obj.ToString());
			if (tbit.GetType() == typeof(Varchar))
			{
				return this.Value.CompareTo(tbit.Value);
			}
			else throw new ArgumentException($"Error: this is not a type of '{tbit.GetType()}'");
		}
		
		public int CompareTo(Varchar other)
		{
			return this.Value.CompareTo(other.Value);
		}
		
		public bool Equals(Varchar other)
		{
			if (other.Value == this.Value)
				return true;
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(Varchar) || obj.GetType() == typeof(string))
				return this.Value == obj.ToString();
			return false;
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}
		
		//my type conversions
		public static implicit operator Varchar(Bit v)
		{
			Varchar b = new Varchar();
			b.Value = v.ToString();
			return b;
		}


		#region coverting to tinyInt Type
		//original conversion of main types
		public static implicit operator Varchar(string value)
		{
			Varchar txt = new Varchar();
			txt.Value = value;
			return txt;
		}

		public static implicit operator string(Varchar value)
		{
			return value.Value;
		}

		//Converting any type to Varchar
		public static Varchar ToVarchar<T>(T value)
		{
			Varchar v = new Varchar();
			v.Value = value.ToString();
			return v;
		}
		//implicit conversions
		//none
		//explicit conversions
		#endregion

		#region overloading operators
		//operators overloading
		public static Varchar operator +(Varchar a, Varchar b)//for same type
		{
			Varchar big = new Varchar();
			big.Value = a.Value + b.Value;
			return big;
		}
		public static string operator +(Varchar a, string b)
		{
			return a.Value + b;
		}//for current type
		
		public static string operator *(Varchar a, int b)
		{
			return string.Join("",Enumerable.Repeat(a, b));
		}

		#endregion

		#region converting back to other types
		//converting back to other types explicitly
		//none
		#endregion
	}
	#endregion
	
	#region Complex Types
	public struct Value : IComparable, IComparable<Value>, IEquatable<Value>
	{	
		private dynamic data;
		private string ano;
		public dynamic Data
		{
			get { return data; }
			set 
			{	
				Parser p = new Parser();
				
				string timePattern1 = @"^[0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,3})?$";
				string dateTimePattern1 = @"^[0-9]{1,2}\/[0-9]{1,2}\/[0-9]{2,4} [0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,3})?$";
				
				string timePattern = @"^[0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,3})?(\s?[a-z]{2})?$";
				string datePattern = @"^[0-9]{1,2}\/[0-9]{1,2}\/[0-9]{2,4}$";
				string dateTimePattern = @"^[0-9]{1,2}\/[0-9]{1,2}\/[0-9]{2,4} [0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,3})?(\s?[a-z]{2})?$";
				
				var regexTime = new Regex(timePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
				var regexDate = new Regex(datePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
				var regexDateTime = new Regex(dateTimePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
				
				if(value.GetType() == typeof(string) || Parser.IsStringType(value))
					value = value.ToString().Trim();
				//preparing it if i has the value(`) symbol
				
				if(Parser.IsStringType(value))
				{
					//if(p.GetType(value.ToString().Trim()[0]) != "value" && !(value.ToString().StartsWith("`") || value.ToString().StartsWith("'")) && !(value.ToString().EndsWith("`") || value.ToString().EndsWith("'")))
					//	value = "'" + value + "'";
						
					//for evaluation
					if(value.ToString().Contains("`") && value.ToString().StartsWith("`") && value.ToString().EndsWith("`") && value.ToString().Length > 1)
					{
						value = (value.ToString().Length > 2) ? value.ToString().Remove(value.ToString().Length - 1, 1).Remove(0, 1) : "";
						
						//convert to bool
						if(value.ToString().ToLower() == "true" || value.ToString().ToLower() == "false")
							data = bool.Parse(value.ToString());
						
						//convert to null
						else if (value.ToString() == "" || value.ToString().ToLower() == "null")
						{
							data = "null";
							ano = "STR";
						}
						
						//convert to time
						else if(regexTime.IsMatch(value.ToString().Trim()))
						{
							try { data = Time.Parse(value.ToString()); }
							catch (Exception) { throw new Isql.ISqlFormatException($"Error: invalid Time format"); }
						}
						
						//convert to date
						else if(regexDate.IsMatch(value.ToString().Trim()))
						{
							try { data = Date.Parse(value.ToString()); }
							catch (Exception) { throw new Isql.ISqlFormatException($"Error: invalid Date format"); }
						}
						
						//convert to datetime
						else if(regexDateTime.IsMatch(value.ToString().Trim()))
						{
							try { data = DateTime.Parse(value.ToString()); }
							catch (Exception) { throw new Isql.ISqlFormatException($"Error: invalid DateTime format"); }
						}
						
						//for logics
						else if (p.StrHasLogic(value.ToString()))
						{
							var logic = new Isql.Logistics.LogicExpressionEngine(value.ToString());
							logic.Solve();
							data = logic.RawResult;
							
						}
						
						//for maths
						else if (p.StrHasMath(value.ToString()))
						{
							var exp = new Isql.Logistics.ExpressionEngine(value.ToString());
							exp.Calculate();
							data = exp.RawResult;
						}
						
						//Convert to number (ie double or long)
						else if(Parser.CheckNum(value.ToString()))
						{
							try
							{
								if(value.GetType() == typeof(float) || value.GetType() == typeof(FloatPoint) || value.GetType() == typeof(Point) || value.GetType() == typeof(double) || value.GetType() == typeof(decimal))
									data = double.Parse(value.ToString());
								else
									data = (value.ToString().Contains(".")) ? double.Parse(value.ToString()) : long.Parse(value.ToString());
							}
							catch(FormatException)
							{
								throw new Isql.ISqlFormatException($"Error: invalid Number format");
							}
						}
						
						//default
						else data = value;
						
						if(Parser.IsStringType(data))
						{
							ano = "EVL";
							goto OUT;
						}
					}
					
					//for no-eval
					else if(value.ToString().Contains("'") && value.ToString().StartsWith("'") && value.ToString().EndsWith("'") && value.ToString().Length > 1)
					{
						value = (value.ToString().Length > 2) ? value.ToString().Remove(value.ToString().Length - 1, 1).Remove(0, 1) : "";
						data = value;
						
						if(Parser.IsStringType(data))
							ano = "STR";

					}
					
					else 
					{
						data = value;
						ano = "STR";
					}
				}
				
				else if (!Parser.IsStringType(value))
					data = value;
				
				else throw new Isql.ISqlException($"Error: unknown error occured");
				
				//if(Parser.IsStringType(data))
				//	ano = "STR";
				OUT:; 
			}
		}
		
		public string Anno
		{
			get
			{
				try
				{
				if(Data.GetType() == typeof(DateTime) || Data.GetType() == typeof(Time) || Data.GetType() == typeof(Date))
					return "DAT";
				else if(Data.GetType() == typeof(bool))
					return "BOL";
				else if (Data.GetType() == typeof(long) || Data.GetType() == typeof(double))
					return "NUM";
				else if (Data.GetType() == typeof(string) || Parser.IsStringType(Data))
					return ano;
				else return "OBJ";
				}
				catch(Exception)
				{
					throw new Isql.ISqlException($"Error: no data found");
				}
				throw new Isql.ISqlException($"Error: no data found");
			}
		}
		
		public Value(dynamic value) : this()
		{
			Data = value;
		}
		
		public Value(dynamic value, bool translate) : this()
		{
			if(translate)
				Data = value;
				
			else if (!translate)
			{
				if(Parser.IsStringType(value))
				{
					if ((value.ToString().Contains("`") && value.ToString().StartsWith("`") && value.ToString().EndsWith("`")) ||
					value.ToString().Contains("'") && value.ToString().StartsWith("'") && value.ToString().EndsWith("'"))
					{
						value = value.ToString().Remove(value.ToString().Length - 1, 1);
						value = value.ToString().Remove(0, 1);
					}
					data = value;
				}
					
				else if (!Parser.IsStringType(value))
				{
					data = value;
				}
			}
		}
		
		public dynamic GetAsValue()
		{
			if(Parser.IsStringType(Data))
			{
				if(Data.ToString().StartsWith(" ") || Data.ToString().EndsWith(" "))
					return "'" + Data + "'";
				
				else if(Data.ToString() == "")
					return null;
				
				else return Data;
				
			}
			
			return Data;
		}
		
		public override string ToString()
		{
			return this.Data.ToString();
		}
		
		public int CompareTo(object obj)
		{
			Value value = new Value(obj.ToString());
			if (value.GetType() == typeof(Value))
			{
				return this.Data.CompareTo(value.Data);
			}
			else throw new ArgumentException($"Error: this is not a type of '{value.GetType()}'");
		}
		
		public int CompareTo(Value other)
		{
			return this.CompareTo(other.Data);
		}
		
		public bool Equals(Value other)
		{
			if (other.Data == this.Data)
				return true;
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(Value))
				return this.Data == (Value)obj;
			return false;
		}

		public override int GetHashCode()
		{
			return Data.GetHashCode();
		}
		
		public static implicit operator Value(bool s)
		{
			Value value = new Value(s);
			return value;
		}
		
		public static implicit operator Value(string s)
		{
			Value value = new Value(s);
			return value;
		}
		
		public static implicit operator Value(Point s)
		{
			Value value = new Value(s);
			return value;
		}
		
		public static implicit operator Value(double s)
		{
			Value value = new Value(s);
			return value;
		}
		
		public static implicit operator Value(BigInt s)
		{
			Value value = new Value(s);
			return value;
		}
		
		public static implicit operator Value(long s)
		{
			Value value = new Value(s);
			return value;
		}
		
		public static implicit operator Value(DateTime s)
		{
			Value value = new Value(s);
			return value;
		}
		
		public static implicit operator Value(Date s)
		{
			Value value = new Value(s);
			return value;
		}
		
		public static implicit operator Value(Time s)
		{
			Value value = new Value(s);
			return value;
		}
	}
	
	public struct None : IComparable, IComparable<None>, IEquatable<None>
	{
		private object value;
		
		/* public None():this()
        { value = null; } */
		
		public int CompareTo(object obj)
		{	
			if(obj.GetType() != typeof(None))
				return -1;
			
			return 0;
		}
		
		public int CompareTo(None other)
		{
			return this.CompareTo(other);
		}
		
		public bool Equals(None other)
		{
			if (other.GetType() == this.GetType())
				return true;
				
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(None))
				return this.Equals((None)obj);
				
			return false;
		}

		public override int GetHashCode()
		{
			return 0;
		}
		
		/*
				ALL C# TYPES SUPPORTED BY ISQL
				- DATETIME, TIMESPAN, INT, FLOAT, DOUBLE, BYTE, LONG, STRING, DECIMAL
				  BOOL, CHAR
				
				ALL ISQL TYPES
				- INTEGER, FLOATPOINT, VARCHAR, CHARACTER, TEXT, DATE, TIME, BIT, TINYINT
				  POINT, BIGINT, CHOICE, MEADIA, SET
		*/
		
		public static implicit operator int(None none)
		{
			return 0;
		}
		
	}
	
	public struct Set : IEnumerable, IEnumerable<object>, IComparable, IComparable<Set>, IEquatable<Set>
	{
		private dynamic[] _set;
		private int capacity;
		
		public int Size
		{
			get
			{
				return _set.Length;
			}
		}
		
		public int Count
		{
			get
			{
				int count = 0;
				foreach(var item in _set)
				{
					try
					{
						if(item == null || item.ToString().ToLower() == "null")
							continue;
						count++;
					}
					catch(NullReferenceException)
					{
						continue;
					}
				}
				
				return count;
			}
		}
		
		public int Capacity
		{
			get { return capacity; }
		}
		
		//Constructor
		/*public Set()
		{
			_set = new object[0];
		}*/
		
		public Set(int capacity) : this()
		{
			this.capacity = capacity;
		}
		
		public Set(object[] set) : this()
		{
			_set = new dynamic[set.Length];
			Array.Copy(set, _set, set.Length);
		}
		
		//Indexer
		public object this[int index]
		{
			get
			{
				if(index < 0 || index >= _set.Length)
					throw new Isql.ISqlArguementException($"Error: index is out of range");
				
				return _set[index];
			}
			
			set
			{
				if(index < 0 || index >= _set.Length)
					throw new Isql.ISqlArguementException($"Error: index is out of range");
				
				_set[index] = value;
			}
		}
		
		public void Add(object item)
		{
			if(capacity > -1 && capacity == _set.Length)
				throw new Isql.ISqlArguementException($"Error: set is full '{capacity}' " + string.Join(", ", _set));
				
			List<object> li = _set.ToList<object>();
			li.Add(item);
			_set = li.ToArray<object>();
			
		}
		
		public void Remove(object item)
		{
			List<dynamic> li = _set.ToList<dynamic>();
			li.Remove(item);
			_set = li.ToArray<dynamic>();
		}
		
		public bool Contains(object item)
		{
			foreach(var t in _set)
			{
				try
				{
					if(item.GetType() == t.GetType())
					{
						if(item.Equals(t))
							return true;
					}
				}catch(NullReferenceException) { continue; }
				
			}
			
			return false;
		}

		public static Set operator +(Set set1, Set set2)
		{
			return new Set(set1._set.Concat(set2._set).ToArray<dynamic>());
		}
		
		public static Set operator -(Set set1, Set set2)
		{
			List<dynamic> a = set1.ToList<dynamic>();
			foreach(var set in set2)
			{
				//if (set1.Contains(set))
			}
			return new Set(set1._set.Concat(set2._set).ToArray<dynamic>());
		}
		
		public static Set operator *(Set set1, int rep)
		{
			for(int i = 0; i < rep; i++)
				set1._set.Concat(set1._set);
			
			return set1;
		}
		
		public static bool operator !=(Set set1, Set set2)
		{
			return !set1._set.Equals(set2._set);
		}
		
		public static bool operator ==(Set set1, Set set2)
		{
			return set1._set.Equals(set2._set);
		}
		
		public static implicit operator Set(object[] value)
		{
			return new Set(value);
		}
		
		public static implicit operator Set(Array value)
		{
			Set set =  new Set();
			foreach(var v in value)
				set.Add(v);
			
			return set;
		}
		
		public static implicit operator Set(string[] value)
		{
			return new Set(value);
		}
		
		public int CompareTo(object obj)
		{
			if (obj.GetType() == typeof(Set))
			{
				 Set set = new Set(((Set)obj)._set);
				if(this._set.Length > set._set.Length)
					return 1;
				if(this._set.Length < set._set.Length)
					return -1;
				if(this._set.Length == set._set.Length)
				{
					if(this._set.GetHashCode() > set._set.GetHashCode())
						return 1;
					if(this._set.GetHashCode() < set._set.GetHashCode())
						return -1;
					if(this._set.GetHashCode() == set._set.GetHashCode())
						return 0;
				}
			}
			return -1;
		}
		
		public int CompareTo(Set other)
		{
			return this.CompareTo(other);
		}
		
		public bool Equals(Set other)
		{
			return this._set.Equals(other._set);
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(Set))
				return this.Equals((Set)obj);
				
			return false;
		}

		public override int GetHashCode()
		{
			return this._set.GetHashCode();
		}
		
		public object[] ToArray()
		{
			return _set;
		}
		
		public static object[] ToArray(Set set)
		{
			object[] se = new object[set.Size];
			Array.Copy(set._set, se, set.Size);
			return	se;
		}
		
		public IEnumerator<dynamic> GetEnumerator()
        {
            return ((IEnumerable<dynamic>)_set).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._set.GetEnumerator();
        }
        
        public override string ToString()
        {
        	string objstr = "Set <object> { ";
        	int count = 0;
        	foreach (var item in _set)
        	{
        		try
        		{
        			if(count == _set.Length - 1)
        				objstr += item.GetType().Name + ":" + item;
        			
        			else
        				objstr += item.GetType().Name + ":" + item + ", ";
        		}catch(NullReferenceException)
        		{
        			if(count == _set.Length - 1)
        				objstr += "null" + ":" + "null";
        			
        			else
        				objstr += "null" + ":" + "null" + ", ";
        		}
        		count++;
        	}
        	objstr += " }";
        	return objstr;
        }
	}
	
	public class Media : IComparable, IComparable<Media>, IEquatable<Media>
	{
		private List<byte> buffer = new List<byte>();
		
		private int block_size = 5120;
		private int fixed_buffer_size = -1;
		private int buffer_size;
		private int curr_reading_block;
		private string media_name;
		private string media_path = "";
		
		/*
		public int BlockSize
		{
			get { return block_size; }
			set { block_size = value; }
		}
		
		public int BlockLength
		{
			get { return Math.Abs(buffer.Count / block_size); }
		}
		*/
		
		public string Name
		{
			get { return (media_path != "") ? Path.GetFileNameWithoutExtension(media_path) : ""; }
		}
		
		public string Extension
		{
			get { return (media_path != "") ? Path.GetExtension(media_path) : ""; }
		}
		
		public string Location
		{
			get { return (media_path != "") ? Path.GetFileName(media_path) : ""; }
		}
		
		public byte[] Data
		{
			get { return buffer.ToArray<byte>(); }
		}
		
		public Media()
		{
			buffer = new List<byte>(block_size);
		}
		
		public Media(string mediaPath)
		{
			ReadFromPath(mediaPath);
		}
		
		public Media(Stream mediaStream)
		{
			ReadFromStream(mediaStream);
		}
		
		public void ReadFromPath(string mediaPath)
		{
			using(FileStream fs = new FileStream(mediaPath, FileMode.Open, FileAccess.Read))
			{
				byte[] _buffer = new byte[fs.Length];
				using(BinaryReader br = new BinaryReader(fs))
				{
					media_path = mediaPath;
					br.Read(_buffer, 0, (int)((buffer_size > 0) ? buffer_size : (int)fs.Length));
				}
				
				/*
				block_size = (int)(_buffer.Length / 4);
				bool redo = true; List<int> re = new List<int>();
				while(redo)
				{
					if(block_size <= _buffer.Length / 8 && _buffer.Length / 8 < 1 == false)
					{
						block_size = (int)_buffer.Length; redo = false;
					}
					
					else if (((_buffer.Length / 2) / block_size) <= 2)
					{
						redo = false;
					}
					
					else if (_buffer.Length / block_size <= 2)
					{
						redo = false;
						block_size = _buffer.Length / block_size;
					}
					
					else
						block_size = _buffer.Length / block_size;
					
					if(re.Contains(block_size))
					{
						redo = false; re.Clear();
					}
					
					re.Add(block_size);
				}
				*/
				buffer = _buffer.ToList<byte>();
			}
		}
		
		public void ReadFromStream(Stream mediaStream)
		{
			byte[] _buffer = new byte[mediaStream.Length];
			using(BinaryReader br = new BinaryReader(mediaStream))
			{
				br.Read(_buffer, 0, (int)((buffer_size > 0) ? buffer_size : (int)mediaStream.Length));
			}
			
			buffer = _buffer.ToList<byte>();
		}
		
		//Under Constructions
		public void ReadMediaStream(Stream mediaObjStream)
		{
			
		}
		
		//Under Construction
		public void ReadMediaObj(string mediaobj)
		{
			
		}
		
		/*
		public byte[] GetBlock(int count)
		{
			if(count < 1)
				throw new Isql.ISqlException($"Error: count is lessthan the minimum value\nCount: '{count}'");
				
			int max_range = block_size * count;
			
			
			if(max_range >= buffer.Count)
				max_range = max_range - (max_range - buffer.Count);
			
			int start = max_range;	
			return buffer.GetRange((max_range == buffer.Count) ? 0 : max_range, 
							(max_range == buffer.Count) ? max_range - 1 : max_range ).ToArray<byte>();
		}
		
		//FIX THIS
		public void SetBlock(int count, byte[] blockData)
		{
			if(count < 1)
				throw new Isql.ISqlException($"Error: count is lessthan the minimum value\nCount: '{count}'");
			
			int max_range = block_size + count;
			
			if(max_range >= buffer.Count || (max_range - block_size) >= buffer.Count)
				max_range = max_range - (max_range - buffer.Count);
			
			buffer.RemoveRange(max_range - count, max_range);
			buffer.InsertRange(max_range - count, blockData);
		}
		*/
		
		public int CompareTo(Media other)
		{
			if (other.GetType() == typeof(Media))
			{
				if (this.Data.Length > other.Data.Length) return 1;
				if (this.Data.Length < Data.Length) return -1;
				else return 0;
			}
			return 1;
		}

		public int CompareTo(object obj)
		{
			if (obj.GetType() == typeof(Media))
			{
				Media tbit = (Media)obj;
				if (this.Data.Length > tbit.Data.Length) return 1;
				if (this.Data.Length < tbit.Data.Length) return -1;
				else return 0;
			}
			return -1;
		}

		public bool Equals(Media other)
		{
			if (other.Data.Length == this.Data.Length)
				return true;
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(BigInt))
				return this.Equals((Media)obj);
				
			return false;
		}
		
	}
	
	public class Choice<T> : IEnumerable<T> 
	{
		T[] _choices = new T[0];
		
		public Choice(params T[] choices)
		{
			AddChoice(choices);
		}
		
		public void AddChoice(params T[] choices)
		{
			if (_choices != null && _choices.Length > 0)
			{

                    T[] newChoices = new T[this._choices.Length + choices.Length];
                    Array.Copy(this._choices, newChoices, this._choices.Length);
                    int startPoint = _choices.Length;
                    foreach (T choice in choices)
                    {
                            newChoices[startPoint] = choice;
                            startPoint++;
                    }

                    this._choices = new T[newChoices.Length];
                    Array.Copy(newChoices, this._choices, newChoices.Length);
                }
            else
            {
            	this._choices = new T[choices.Length];

            	for (int i = 0; i < choices.Length; i++)
            		this._choices[i] = choices[i];
            }
		}
		
		public void RemoveChoice(T choice)
		{
			T[] newChoice = new T[_choices.Length - 1];
			
			int c = 0;
			foreach(var ch in _choices)
			{
				if(ch.ToString() != choice.ToString())
				{
					newChoice[c] = ch;
					c++;
				}
			}
			
			_choices = new T[newChoice.Length];
			Array.Copy(newChoice, _choices, newChoice.Length);
		}
		
		public void RemoveChoiceAt(int choiceIndex)
		{
			List<dynamic> newChoices = new List<dynamic>();
			foreach(var c in this._choices)
				newChoices.Add(c);
				
			newChoices.RemoveAt(choiceIndex);
			
			this._choices = new T[newChoices.Count];
            Array.Copy(newChoices.ToArray<object>(), this._choices, newChoices.Count);
		}

		public bool HasChoice(T choice)
		{
			return _choices.Contains(choice);
		}
		
		public T GetChoice(int choiceIndex)
		{
			return _choices[choiceIndex];
		}
		
		public T[] GetChoiceObject()
		{
			return this._choices.ToArray<T>();
		}

		public override string ToString()
		{
			string value = $"Choice<"+Parser.StringTypeConverter(new Type[] { typeof(object) })[0]+$"> [ "; int count = 0;
			foreach(var choiceData in this)
			{
				if(count < this._choices.Length - 1)
					value += Parser.StringTypeConverter(choiceData.GetType())[0] +":" + choiceData+", ";
				else
					value += Parser.StringTypeConverter(choiceData.GetType())[0] +":" + choiceData+" ]";
					
				count++;
			}
			return value;
		}
		
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_choices).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._choices.GetEnumerator();
        }
	}
	
	public class Money
	{
		
	}
	
	public enum Currency
	{
		
	}
	
	internal class Binary
	{
		
	}
	#endregion
	
	#region date and time types
	
    public struct Date : IComparable, IComparable<Date>, IEquatable<Date>
    {
        public static DateTime MaxValue = DateTime.MaxValue;
        public static DateTime MinValue = DateTime.MinValue;
        
        public int Year 
        {
        	get{ return value.Year; }
        }
        
        public int Month 
        {
        	get{ return value.Month; }
        }
        
        public int Day 
        {
        	get{ return value.Day; }
        }
        
        public static Date Current
        {
        	get
        	{
        		Date d = DateTime.Now;
        		return d;
        	}
        }
        
        public long Intervals
        {
        	get
        	{
        		return this.Value.Ticks;;
        	}
        }

        private DateTime value;
        internal DateTime Value
        {
            get
            {
                return value;
            }
            set
            {
                if (!(value < MinValue) && !(value > MaxValue))
                    this.value = value;
                    
                else throw new ArgumentOutOfRangeException($"'{value}'\nRange is '{MinValue}' to '{MaxValue}' ");
            }
        }
        
        public Date AddIntervals(long value)
        {
        	return this.Value.AddTicks(value);
        }
        
        public Date AddYears(int years)
        {
        	return this.Value.AddYears(years);
        }

        public Date AddMonths(int months)
        {
        	return this.Value.AddMonths(months);
        }
        
        public Date AddDays(double days)
        {
        	return this.Value.AddDays(days);
        }
        
        public Date(DateTime date) : this()
        {
        	this.Value = date;
        }
        
        public Date(string date) : this()
        {
        	this.Value = DateTime.Parse(date);
        }
        
        public Date(long intervals) : this()
        {
        	this.Value = new DateTime(intervals);
        }
        public int CompareTo(Date other)
        {
            if (other.GetType() == typeof(Date))
            {
                if (this.Value > other.Value) return 1;
                if (this.Value < other.Value) return -1;
                else return 0;
            }
            else throw new ArgumentException($"Error: this is not a type of '{other.GetType()}'");
        }
        
        public int CompareTo(object obj)
        {
            Date tbit = (Date)obj;
            if (tbit.GetType() == typeof(Date))
            {
                if (this.Value > tbit.Value) return 1;
                if (this.Value < tbit.Value) return -1;
                else return 0;
            }
            else throw new ArgumentException($"Error: this is not a type of '{tbit.GetType()}'");
        }
        
        public bool Equals(Date other)
        {
            if (other.Value == this.Value)
                return true;
            return false;
        }
        public override bool Equals(object obj)
        {
            if(obj.GetType() == typeof(Date) || obj.GetType() == typeof(DateTime)) 
            	return this.Value.Day+"/"+this.Value.Month+"/"+this.Value.Year == ((DateTime)obj).Day+"/"+((DateTime)obj).Month+"/"+((DateTime)obj).Year;
            return false;
        }
        public override int GetHashCode()
        {
        	return value.GetHashCode();
        }
        public override string ToString()
        {
            return this.Year.ToString("0000") + "/" + this.Month.ToString("00") + "/" + this.Day.ToString("00");
        }
        
        public static Date Parse(string value)
        {
        	Date date = DateTime.Parse(value);
        	return date;
        }
        
        public static implicit operator Date(DateTime date)
        {
        	Date d = new Date(date);
        	return d;
        }
        
        public static implicit operator DateTime(Date date)
        {
        	return date.Value;
        }
        
        //my type conversions
        #region overloading operators
        //operators overloading
        
        public static Date operator -(Date b, Date value)
        {
        	var ts = b.Value.Subtract(value.Value);
        	Date d = new Date();
        	d.Value = new DateTime((long)(ts.Ticks - 1000));

            return d;
        }
        
        public static Date operator +(Date a, Date b)
        {
        	var ts = a.Value.Add(TimeSpan.FromTicks(b.Value.Ticks));
        	Date d = new Date();
        	d.Value = new DateTime(ts.Ticks, DateTimeKind.Utc);

            return d;
        }
        
        /*public static Date operator *(Date a, Date b)
        {
            return a.Value * b.Value;
        }
        public static Date operator /(Date a, Date b)
        {
            return a.Value / b.Value;
        }
        public static Date operator %(Date a, Date b)
        {
            return a.Value % b.Value;
        }*/

        //one side operators overloading
        #endregion
    }
    
    public struct Time : IComparable, IComparable<Time>, IEquatable<Time>
    {
        public static DateTime MaxValue = DateTime.MaxValue;
        public static DateTime MinValue = DateTime.MinValue;
        
        public int Hour 
        {
        	get{ return value.Hour; }
        }
        
        public int Minute 
        {
        	get{ return value.Minute; }
        }
        
        public int Second 
        {
        	get{ return value.Second; }
        }
        
        public int Millisecond 
        {
        	get{ return value.Millisecond; }
        }

        public static Time Current
        {
        	get
        	{
        		Time t = DateTime.Now;
        		return t;
        	}
        }

        private DateTime value;
        internal DateTime Value
        {
            get
            {
                return value;
            }
            set
            {
                if (!(value < MinValue) && !(value > MaxValue))
                    this.value = value;
                    
                else throw new ArgumentOutOfRangeException($"'{value}'\nRange is '{MinValue}' to '{MaxValue}' ");
            }
        }
        
        public Time AddHours(int hours)
        {
        	return this.Value.AddHours(hours);
        }
        
        public Time AddMinutes(int minutes)
        {
        	return this.Value.AddMinutes(minutes);
        }
        
        public Time AddSeconds(double seconds)
        {
        	return this.Value.AddSeconds(seconds);
        }
        
        public Time AddMilliseconds(double milli)
        {
        	return this.Value.AddMilliseconds(milli);
        }

        public Time(DateTime time) : this()
        {
        	this.Value = time;
        }
        
        public Time(string time) : this()
        {
        	this.Value = DateTime.Parse(time);
        }

        public int CompareTo(Time other)
        {
            if (other.GetType() == typeof(Time))
            {
                if (this.Value > other.Value) return 1;
                if (this.Value < other.Value) return -1;
                else return 0;
            }
            else throw new ArgumentException($"Error: this is not a type of '{other.GetType()}'");
        }
        
        public int CompareTo(object obj)
        {
            Time tbit = (Time)obj;
            if (tbit.GetType() == typeof(Time))
            {
                if (this.Value > tbit.Value) return 1;
                if (this.Value < tbit.Value) return -1;
                else return 0;
            }
            else throw new ArgumentException($"Error: this is not a type of '{tbit.GetType()}'");
        }
        
        public bool Equals(Time other)
        {
            if (other.Value == this.Value)
                return true;
            return false;
        }
        public override bool Equals(object obj)
        {
            if(obj.GetType() == typeof(Date) || obj.GetType() == typeof(DateTime)) 
            	return this.Value.Hour+":"+this.Value.Minute+":"+this.Value.Second+":"+this.Value.Millisecond == ((DateTime)obj).Hour+":"+((DateTime)obj).Minute+":"+((DateTime)obj).Second+":"+((DateTime)obj).Millisecond;
            return false;
        }
        public override int GetHashCode()
        {
        	return value.GetHashCode();
        }
        public override string ToString()
        {
            return this.Hour.ToString("00") + ":" + this.Minute.ToString("00") + ":" + this.Second.ToString("00");
        }
        
        public static Time Parse(string value)
        {
        	Time time = new Time();
        	time.Value = DateTime.Parse(value);
            return time;
        }
        
        public static implicit operator Time(DateTime time)
        {
        	Time d = new Time(time);
        	return d;
        }
        /*
        public static explicit operator DateTime(Time time)
        {
        	return time.Value;
        }*/

        public static implicit operator DateTime(Time time)
        {
        	return time.Value;
        }
        
        //my type conversions
        #region overloading operators
        //operators overloading
        public static Time operator -(Time b, Time value)
        {
        	TimeSpan ts = new TimeSpan();
            return DateTime.FromOADate(b.Value.Subtract(value.Value).Ticks);
        }
        public static Time operator +(Time a, Time b)
        {
            return new Time(); //a.Value.Add(b.Value);
        }
        /*public static Date operator *(Date a, Date b)
        {
            return a.Value * b.Value;
        }
        public static Date operator /(Date a, Date b)
        {
            return a.Value / b.Value;
        }
        public static Date operator %(Date a, Date b)
        {
            return a.Value % b.Value;
        }*/

        //one side operators overloading
        #endregion
    }
    
    public struct Year
    {
    	
    }
    
	#endregion
	
	#region constraints table
	
	/// <summary>
	///	the base class for all contraints type.
	///	</summary>
	public abstract class Constraint
	{
		private string name = "null";
		private bool key = false;
		
		///	<summary>
		///	serves as the name of the target column. Default is null
		///	</summary>
		public string Name
		{
			get { return name; }
			internal set { name = value; }
		}
		
		///	<summary>
		///	serves as the switch for the constraint. if true, constraint is enabled, else disabled
		///	</summary>
		public bool Key
		{
			get { return key; }
			set { key = value; }
		}
		
	}
	
	//Has Name Has Key
	///	<summary>
	///	use to auto increament a column
	///	</summary>
	public class AutoIncreament : Constraint
	{
		private double current_value = 0;
		private double increament = 1;
		
		
		public AutoIncreament()
		{
			Key = false;
			this.current_value = 0;
			this.increament = 1;
		}
		
		public AutoIncreament(bool key)
		{
			Key = key;
			current_value = 0; increament = 1;
		}
		
		///	<summary>
		///	create an instance of the autoincreament class
		///	</summary>
		public AutoIncreament(double value, double increament, bool key = false)
		{
			this.current_value = value;
			this.increament = increament;
			Key = key;
		}

		public AutoIncreament(string name, double value, double increament, bool key = false)
		{
			Name = name;
			current_value = value;
			increament = increament;
			Key = key;
		}
		
		public static implicit operator AutoIncreament(bool value)
		{
			return new AutoIncreament(value);
		}
		
		public static implicit operator bool(AutoIncreament value)
		{
			return value.Key;
		}
		
		public void Increament()
		{
			this.current_value += this.increament;
		}

		public void Decreament()
		{
			this.current_value -= increament;
		}

		public void ChangeValue(double value)
		{
			this.current_value = value;
		}

		public void ChangeIncreament(double value)
		{
			this.increament = value;
		}

		public double GetValue()
		{
			return this.current_value;
		}

		public double GetIncreament()
		{
			return increament;
		}
		
		public override string ToString()
		{
			return $"AutoIncreament<{Name}> [ {current_value}:{increament} ]";
		}
		
		public static AutoIncreament Parse(string value)
		{
			value = value.Replace(" ", "");
			if(!(value.StartsWith("AutoIncreament<") && value.EndsWith("]") && value.Contains(">")))
				throw new Isql.ISqlFormatException($"Error: value is not in correct format\nValue: {value}");

			string column = value.Substring(value.IndexOf("<") + 1, value.LastIndexOf(">") - value.IndexOf("<") - 1).Trim();

			string[] numbers = value.Substring(value.IndexOf("[") + 1, value.LastIndexOf("]") - value.IndexOf("[") - 1).Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

			AutoIncreament auto = new AutoIncreament(column, double.Parse(numbers[0].Trim()), double.Parse(numbers[1].Trim()));
			return auto;
		}
	}
	
	public class Reference
	{
		//for direct database references
		private string target_table;
		private string[] target_columns;
		
		private OnDelete onDelete = OnDelete.None;
		public OnDelete OnDelete
		{
			get { return onDelete; }
			internal set { onDelete = value; }
		}
		
		//for direct dataTable-dataTable references
		private object target_dataTable;
		
		private bool isVirtual = false;
		
		internal dynamic TargetTable
		{
			get { return (!isVirtual) ? target_table : target_dataTable; }
		}
		
		internal string[] TargetColumns
		{
			get { return target_columns; }
		}
		
		public Reference(string table_ref, string[] columns, OnDelete onDelete = OnDelete.None)
		{
			target_table = table_ref; target_columns = columns;
			this.OnDelete = onDelete;
			isVirtual = false;
		}
		
		public Reference(DataTable dt, string[] columns, OnDelete onDelete = OnDelete.None)
		{
			target_dataTable = dt; target_columns = columns;
			this. OnDelete = onDelete;
			isVirtual = true;
		}
		
		public override string ToString()
		{
			return "";
		}
		
		public static Reference Parse(string objString)
		{
			return new Reference("Not Fixed", new string[] { "Not Fixed" });
		}
	}
	
	//Has Name Has Key
	public class ForeignKey : Constraint
	{
		private string col_name;
		private List<Reference> references = new List<Reference>();
		
		internal Reference[] References
		{
			get { return references.ToArray<Reference>(); }
		}
		
		public ForeignKey(string columnName, params Reference[] references)
		{
			Name = columnName; 
			this.references = references.ToList<Reference>();
		}
		
		public void AddRefrence(Reference reference)
		{
			if(this.Name != null)
				throw new Isql.ISqlException($"Error: column is not well defined");
			
				references.Add(reference);
		}
		
		public override string ToString()
		{
			return $@"ForeignKey<{Name}> Refrences: [ {string.Join(",", references)} ]";
		}
		
		public static ForeignKey Parse(string value)
		{
			value = value.Replace(" ", "");
			if(!(value.StartsWith("ForeignKey<") && value.EndsWith("]") && value.Contains("Refrences:")))
				throw new Isql.ISqlFormatException($"Error: value is not in correct format\nValue: {value}");

			string column = value.Substring(value.IndexOf("<") + 1, value.LastIndexOf(">") - value.IndexOf("<") - 1);

			List<string> key = value.Substring(value.IndexOf("[") + 1, value.LastIndexOf("]") - value.IndexOf("[") - 1).Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
			ForeignKey forKey = new ForeignKey("CODE", new Reference[] { new Reference("Code", new string[] { "baba" }) });
			return forKey;
		}
		
	}
	
	//Consider removing this object
	public class ForeignKeyObject
	{
		private List<ForeignKey> FKs= new List<ForeignKey>();
		
		internal ForeignKey[] ForeignKeys
		{
			get { return FKs.ToArray<ForeignKey>(); }
		}
		
		public ForeignKeyObject(params ForeignKey[] foreignKeys)
		{
			FKs = foreignKeys.ToList<ForeignKey>();
		}
	}
	
	//Has Name No Key
	public class Check : Constraint
	{
		internal Isql.Logistics.LogicExpressionEngine logic_criteria;
		
		public Check()
		{
			Name = "null";
			logic_criteria = new Isql.Logistics.LogicExpressionEngine("`true`");
		}
		
		public Check(string criteria)
		{
			Name = "null";
			logic_criteria = new Isql.Logistics.LogicExpressionEngine(criteria);
		}
		
		public Check(string name, string criteria)
		{
			Name = name;
			logic_criteria = new Isql.Logistics.LogicExpressionEngine(criteria);
		}

		public string Criteria
		{
			get { return string.Join(" ",logic_criteria.GetStringExpression()); }
			set { logic_criteria.LogicArguement(value); }
		}
		
		public bool HasInput
		{
			get { return logic_criteria.HasVariables(); }
		}
		
		public string[] GetInputs()
		{
			return logic_criteria.GetVariables();
		}
		
		public void SetInputs(string[] inputName, object[] inputValue)
		{
			logic_criteria.SetVariables(inputName, inputValue);
		}
		
		public bool CheckResult
		{
			get 
			{ 
				logic_criteria.Solve();
				return logic_criteria.Result;
			}
		}
		
		public override string ToString()
		{
			return $"Check<{Name}> [{Criteria}]";
		}
		
		public static Check Parse(string value)
		{
			value = value.Replace(" ", "");
			if(!(value.StartsWith("Check<") && value.EndsWith("]")))
				throw new Isql.ISqlFormatException($"Error: value is not in correct format\nValue: {value}");
			
			Check c = new Check(value.Substring(value.IndexOf("<") + 1 , value.LastIndexOf(">") - value.IndexOf("<") - 1), value.Substring(value.IndexOf("[") + 1 , value.LastIndexOf("]") - value.IndexOf("[") - 1));
			return c;
		}
		
		public static implicit operator Check(string criteria)
		{
			Check c = new Check(criteria);
			return c;
		}
	}
	
	//No Name No Key
	public class Default : Constraint
	{
		private object defVal = "null";
		
		internal object DefaultValue
		{
			get
			{
				return defVal;
			}
			set
			{
				this.defVal = value;
			}
		}
		
		public Default()
		{
			defVal = "null";
		}
		
		public Default(object value)
		{
			defVal = value;
		}
		
		public override string ToString()
		{
			return $"Default<" + Parser.StringTypeConverter(new Type[] { defVal.GetType() })[0] + $"> [ {defVal} ]".ToString();
		}
		
		public static Default Parse(string value)
		{
			value = value.Replace(" ", "");
			if(!(value.StartsWith("Default<") && value.EndsWith("]")))
				throw new Isql.ISqlFormatException($"Error: value is not in correct format\nValue: {value}");
			
			string type = value.Substring(value.IndexOf("<") + 1, value.LastIndexOf(">") - value.IndexOf("<") - 1);

			 Default def  = new Default(Parser.DataConverter(Parser.TypeConverter(type), 
										value.Substring(value.IndexOf("[") + 1, 
										value.LastIndexOf("]") - value.IndexOf("[") - 1).Trim()));
										
			return def;
		}
		
		public object GetDefault()
		{
			return defVal;
		}
		
		public Type GetUnderLayingType()
		{
			return defVal.GetType();
		}
		
		public Type GetType()
		{
			return defVal.GetType();
		}
		
		public static implicit operator Default(string value)
		{
			Default def = new Default();
			def.defVal = value;
			return def;
		}
		
		public static implicit operator Default(Varchar value)
		{
			Default def = new Default();
			def.defVal = value.Value;
			return def;
		}
		
		public static implicit operator Default(bool value)
		{
			Default def = new Default();
			def.defVal = value;
			return def;
		}

		public static implicit operator Default(Character value)
		{
			Default def = new Default();
			def.defVal = value.Value;
			return def;
		}
		
		public static implicit operator Default(Text value)
		{
			Default def = new Default();
			def.defVal = value.Value;
			return def;
		}
		
		public static implicit operator Default(FloatPoint value)
		{
			Default def = new Default();
			def.defVal = value.Value;
			return def;
		}
		
		public static implicit operator Default(Integer value)
		{
			Default def = new Default();
			def.defVal = value.Value;
			return def;
		}
		
		public static implicit operator Default(Point value)
		{
			Default def = new Default();
			def.defVal = value.Value;
			return def;
		}
		
		public static implicit operator Default(Bit value)
		{
			Default def = new Default();
			def.defVal = value.Value;
			return def;
		}
		
		public static implicit operator Default(TinyInt value)
		{
			Default def = new Default();
			def.defVal = value.Value;
			return def;
		}
		
		public static implicit operator Default(BigInt value)
		{
			Default def = new Default();
			def.defVal = value.Value;
			return def;
		}
		
		public static implicit operator Default(Date value)
		{
			Default def = new Default();
			def.defVal = value.Value;
			return def;
		}
		
		public static implicit operator Default(DateTime value)
		{
			Default def = new Default();
			def.defVal = value;
			return def;
		}
		
		public static implicit operator Default(Time value)
		{
			Default def = new Default();
			def.defVal = value.Value;
			return def;
		}
		
		public static implicit operator Default(TimeSpan value)
		{
			Default def = new Default();
			def.defVal = value;
			return def;
		}
		
		public static implicit operator Default(int value)
		{
			Default def = new Default();
			def.defVal = value;
			return def;
		}
		
		public static implicit operator Default(float value)
		{
			Default def = new Default();
			def.defVal = value;
			return def;
		}
		
		public static implicit operator Default(double value)
		{
			Default def = new Default();
			def.defVal = value;
			return def;
		}
		
		public static implicit operator Default(byte value)
		{
			Default def = new Default();
			def.defVal = value;
			return def;
		}
		
		public static implicit operator Default(long value)
		{
			Default def = new Default();
			def.defVal = value;
			return def;
		}
		
		public static implicit operator Default(decimal value)
		{
			Default def = new Default();
			def.defVal = value;
			return def;
		}
		
		public static implicit operator Default(char value)
		{
			Default def = new Default();
			def.defVal = value;
			return def;
		}
	}
	
	//Has Name Has Key
	public class PrimaryKey : Constraint
	{
		public PrimaryKey()
		{
			Name = "null";
			Key = false;
		}
		
		public PrimaryKey(bool key)
		{
			Key = key; Name = "null";
		}
		
		public PrimaryKey(string name, bool key)
		{
			Name = name; Key = key;
		}
		
		public static implicit operator PrimaryKey(bool value)
		{
			return new PrimaryKey(value);
		}
		
		public static implicit operator bool(PrimaryKey value)
		{
			return value.Key;
		}
		
		public static implicit operator PrimaryKey(string name)
		{
			PrimaryKey pk = new PrimaryKey(name, true);
			return pk;
		}
		
		public override string ToString()
		{
			return $"PrimaryKey<{Name}> [ {Key} ]";
		}
		
		public static PrimaryKey Parse(string value)
		{
			value = value.Replace(" ", "");
			if(!(value.StartsWith("PrimaryKey<") && value.EndsWith("]")))
				throw new Isql.ISqlFormatException($"Error: value is not in correct format\nValue: {value}");

			string column = value.Substring(value.IndexOf("<") + 1, value.LastIndexOf(">") - value.IndexOf("<") - 1);

			bool key = bool.Parse(value.Substring(value.IndexOf("[") + 1, value.LastIndexOf("]") - value.IndexOf("[") - 1).Trim());
			PrimaryKey pry = new PrimaryKey(column, key);
			return pry;
		}
	}
	
	//Has Name Has Key
	public class Unique : Constraint
	{
		public Unique()
		{
			Name = "null";
			Key = false;
		}
		
		public Unique(bool key)
		{
			Key = key;
		}
		
		public Unique(string name, bool key)
		{
			Name = name; Key = key;
		}
		
		public static implicit operator Unique(bool value)
		{
			return new Unique(value);
		}
		
		public static implicit operator bool(Unique value)
		{
			return value.Key;
		}
		
		public override string ToString()
		{
			return "Unique <Constraint> { Name:" + Name + ", " + "IsKey:" + Key + " }";
		}
		
		public static Unique Parse(string value)
		{
			value = value.Replace(" ", "");
			if(!(value.StartsWith("Unique<") && value.EndsWith("]")))
				throw new Isql.ISqlFormatException($"Error: value is not in correct format\nValue: {value}");

			string column = value.Substring(value.IndexOf("<") + 1, value.LastIndexOf(">") - value.IndexOf("<") - 1);

			bool key = bool.Parse(value.Substring(value.IndexOf("[") + 1, value.LastIndexOf("]") - value.IndexOf("[") - 1).Trim());
			Unique pry = new Unique(column, key);
			return pry;
		}
	}
	
	//Has Name and Has Key
	public class Null : Constraint
	{
		public Null(bool key)
		{
			Key = key;
		}
		
		public Null(string name, bool key)
		{
			Name = name; Key = key;
		}
		
		public static implicit operator Null(bool value)
		{
			return new Null(value);
		}
		
		public static implicit operator bool(Null value)
		{
			return value.Key;
		}
		
		public override string ToString()
		{
			return "";
		}
		
		public static Null Parse(string obj)
		{
			return new Null(false);
		}
	}
	
	//Has Name Has Key
	public class Choice : Constraint, IEnumerable<object>
	{
        private List<object> choices;// = new List<object>();
		public Choice(params object[] choices)
		{
            this.choices = new List<object>();
			AddChoice(choices);
		}
		
		public void AddChoice(params object[] choices)
		{
            if (this.choices.Count <= 0)
                this.choices = new List<object>();

			foreach(var choice in choices)
				this.choices.Add(choice);
		}
		
		public void RemoveChoice(object choice)
		{
			choices.Remove(choice);
		}
		
		public void RemoveChoiceAt(int choiceIndex)
		{
			choices.RemoveAt(choiceIndex);
		}
		
		public bool HasChoice(object choice)
		{
			return choices.Contains(choice);
		}
		
		public bool IHasChoice(string choice)
		{
			return choices.Any(x => x.ToString().ToLower() == choice.ToLower());
		}

		public object GetChoice(int choiceIndex)
		{
			return choices[choiceIndex];
		}
		
		public object[] GetChoiceObject()
		{
			return this.choices.ToArray<object>();
		}
		
		public override string ToString()
		{
			string value = $"Choice<object>[ "; int count = 0;
			foreach(var choiceData in choices)
			{
				if(count < this.choices.Count - 1)
					value += Parser.StringTypeConverter(choiceData.GetType())[0]+":" + choiceData+", ";
				else
					value += Parser.StringTypeConverter(choiceData.GetType())[0]+":" + choiceData+" ]";
					
				count++;
			}
			return value;
		}
		
		public static Choice Parse(string value)
		{
			value = value.Replace(" ","");
			Choice choice = new Choice();
			value = value.Trim();
			if(value.ToLower().StartsWith("choice<object>[") && value.ToLower(). EndsWith("]") && value.Contains(":"))
				throw new Isql.ISqlFormatException($"Error: passed data is not in correct format");
			
			string[] data = value.Substring(value.IndexOf("[") + 1, value.LastIndexOf("]") - value.IndexOf("[") - 1).Split(',');
				
			foreach(var d in data)
			{
				string[] da= d.Split(':');
				choice.AddChoice(Parser.DataConverter(
								 Parser.TypeConverter(da[0]), 
								 da[1]));
			}
				
			return choice;
		}

        public IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)choices).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.choices.GetEnumerator();
        }
    }
		
	
	public enum OnDelete
	{
		SETNULL, Cascade, None
	}
	
	#endregion
}







