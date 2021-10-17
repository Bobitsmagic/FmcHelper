namespace CubeAD
{
	public struct Side
	{
		public const int FACE_COUNT = 8;
		public const int BIT_COUNT = FACE_COUNT * 3;
		public const int BIT_MASK = (1 << 24) - 1;

		public static uint[] STRIPE_MASK =
		{
			511U,
			511U << 6,
			511U << 12,
			7U | (63U << 18)
		};

		public static uint[] SQUARE_MASK =
{
			511U << 3,
			511U << 9,
			511U << 15,
			63U | (7U << 21)
		};

		uint Data;

		public uint this[int i]
		{
			get
			{
				return (Data >> (i * 3)) & 7;
			}
			set
			{
				Data &= ~(7U << i * 3);
				Data |= value << (i * 3);
			}
		}

		public CubeColor this[int x, int y]
		{
			get
			{
				if (y == 0) return (CubeColor)this[x];
				if (y == 2) return (CubeColor)this[6 - x];

				return (CubeColor)this[x == 0 ? 7 : 3];
			}
		}

		public Side(uint val)
		{
			Data = val;
		}

		public Side(CubeColor color)
		{
			uint val = (uint)color;
			Data = 0;
			for (int i = 0; i < 8; i++)
			{
				this[i] = val;
			}
		}

		public bool FitsPattern(Side color, uint bitmask)
		{
			return ((Data ^ color.Data) & bitmask) == 0;
		}
		public bool IsPair(int index)
		{
			return this[index] == this[(index + 1) % 8];
		}

		public static uint GenerateBitmask(uint compressed)
		{
			return ((compressed & 1) * 7) |
				((compressed & 2) * (7 << 2)) |
				((compressed & 4) * (7 << 4)) |
				((compressed & 8) * (7 << 6)) |
				((compressed & 16) * (7 << 8)) |
				((compressed & 32) * (7 << 10)) |
				((compressed & 64) * (7 << 12)) |
				((compressed & 128) * (7 << 14));
		}

		public void Rotate(int count)
		{
			Data = ((Data << (count * 6)) & BIT_MASK) | (Data >> (BIT_COUNT - count * 6));
		}
		public static uint RotateVal(uint val, int count)
		{
			return ((val << (count * 6))) & BIT_MASK | (val >> (BIT_COUNT - count * 6));
		}

		public uint GetStripe(int stripe)
		{
			return RotateVal(Data & STRIPE_MASK[stripe], (4 - stripe) & 3);
		}
		public void SetStripe(int stripe, uint val)
		{
			Data &= ~STRIPE_MASK[stripe];
			Data |= RotateVal(val, stripe);
		}

		public static bool operator ==(Side a, Side b)
		{
			return a.Data == b.Data;
		}
		public static bool operator !=(Side a, Side b)
		{
			return a.Data != b.Data;
		}

		public string WriteAsSide(char center)
		{
			string s = "";
			for (int i = 0; i < 3; i++)
				s += ((CubeColor)this[i]).ToString()[0] + " ";

			s += "\n";
			s += ((CubeColor)this[7]).ToString()[0] + " ";
			s += center + " ";
			s += ((CubeColor)this[3]).ToString()[0] + "\n";

			for (int i = 3 - 1; i >= 0; i--)
				s += ((CubeColor)this[i + 4]).ToString()[0] + " ";

			return s;
		}

		public override string ToString()
		{
			string s = "";
			for (int i = 0; i < 8; i++)
			{
				s += this[i] + " ";
			}

			return s;
		}

		public override bool Equals(object obj)
		{
			return this == (Side)obj;
		}

		public override int GetHashCode()
		{
			return (int)Data;
		}
	}
}
