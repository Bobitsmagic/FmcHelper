using System;

namespace CubeAD
{
	//Bool array of size 64
	public struct BitArray
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

		//public ulong Value { get { return Val; } }
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
		public BitArray(ulong data)
		{
			Data = data;
		}

		public static BitArray operator &(BitArray a, BitArray b)
		{
			return new BitArray(a.Data & b.Data);
		}
		public static BitArray operator &(BitArray a, ulong b)
		{
			return new BitArray(a.Data & b);
		}
		public static BitArray operator |(BitArray a, BitArray b)
		{
			return new BitArray(a.Data | b.Data);
		}
		public static BitArray operator |(BitArray a, ulong b)
		{
			return new BitArray(a.Data | b);
		}
		public static BitArray operator ^(BitArray a, BitArray b)
		{
			return new BitArray(a.Data ^ b.Data);
		}
		public static BitArray operator ^(BitArray a, ulong b)
		{
			return new BitArray(a.Data ^ b);
		}

		public static BitArray operator ~(BitArray a)
		{
			return new BitArray(~a.Data);
		}
		public static bool operator ==(BitArray a, BitArray b)
		{
			return a.Data == b.Data;
		}
		public static bool operator !=(BitArray a, BitArray b)
		{
			return a.Data != b.Data;
		}

		public override bool Equals(object obj)
		{
			return obj is BitArray board &&
				   Data == board.Data;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Data);
		}
		public override string ToString()
		{
			string s = "";
			for (int i = 0; i < 48; i++)
			{
				s += this[i] ? "1" : "0";
			}

			return s;
		}
	}
}
