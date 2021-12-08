using System;

namespace CubeAD
{
	/// <summary>
	/// A Bool array of size 64
	/// </summary>
	public struct BitMap64
	{
		public const int SIZE_IN_BYTES = 8;

		public bool this[int i]
		{
			get
			{
				return ((Data >> i) & 1) != 0;
			}
			set
			{
				if (value)
					Data |= 1UL << i;
				else
					Data &= ~(1UL << i);
			}
		}

		public int BitCount
		{
			get
			{
				ulong buffer = Data;
				buffer = buffer - ((buffer >> 1) & 0x5555555555555555);
				buffer = (buffer & 0x3333333333333333) + ((buffer >> 2) & 0x3333333333333333);
				return (int)((((buffer + (buffer >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56);
			}
		}
		public bool Empty { get { return Data == 0; } }

		ulong Data;
		public BitMap64(ulong data)
		{
			Data = data;
		}

		public static BitMap64 operator &(BitMap64 a, BitMap64 b)
		{
			return new BitMap64(a.Data & b.Data);
		}
		public static BitMap64 operator &(BitMap64 a, ulong b)
		{
			return new BitMap64(a.Data & b);
		}
		public static BitMap64 operator |(BitMap64 a, BitMap64 b)
		{
			return new BitMap64(a.Data | b.Data);
		}
		public static BitMap64 operator |(BitMap64 a, ulong b)
		{
			return new BitMap64(a.Data | b);
		}
		public static BitMap64 operator ^(BitMap64 a, BitMap64 b)
		{
			return new BitMap64(a.Data ^ b.Data);
		}
		public static BitMap64 operator ^(BitMap64 a, ulong b)
		{
			return new BitMap64(a.Data ^ b);
		}

		public static BitMap64 operator ~(BitMap64 a)
		{
			return new BitMap64(~a.Data);
		}
		public static bool operator ==(BitMap64 a, BitMap64 b)
		{
			return a.Data == b.Data;
		}
		public static bool operator !=(BitMap64 a, BitMap64 b)
		{
			return a.Data != b.Data;
		}

		public override bool Equals(object obj)
		{
			return obj is BitMap64 board &&
				   Data == board.Data;
		}

		public override int GetHashCode()
		{
			return Data.GetHashCode();
		}
		public override string ToString()
		{
			string s = "";
			for (int i = 0; i < 64; i++)
			{
				s += this[i] ? "1" : "0";
			}

			return s;
		}
	}
}
