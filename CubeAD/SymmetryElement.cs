using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CubeAD
{
	public struct SymmetryElement : IComparable<SymmetryElement>
	{
		public const int ORDER = 48;
		public static SymmetryElement[] Elements = new SymmetryElement[ORDER];
		public static SymmetryElement[] Generators = new SymmetryElement[33]
		{
			//Center pieces
			//X-Axis
			new SymmetryElement(CubeColor.Orange, CubeColor.Green, CubeColor.White),
			new SymmetryElement(CubeColor.Orange, CubeColor.White, CubeColor.Blue),

			//Y-Axis
			new SymmetryElement(CubeColor.Blue, CubeColor.Yellow, CubeColor.Orange),
			new SymmetryElement(CubeColor.Red, CubeColor.Yellow, CubeColor.Blue),
			
			//Z-Axis
			new SymmetryElement(CubeColor.Yellow, CubeColor.Red, CubeColor.Green),
			new SymmetryElement(CubeColor.Red, CubeColor.White, CubeColor.Green),

			//Edge pieces 
			//Orange-Yellow (0 2) LD
			new SymmetryElement(CubeColor.Yellow, CubeColor.Orange, CubeColor.Blue),
			//Orange-White (0 3) LU
			new SymmetryElement(CubeColor.White, CubeColor.Red, CubeColor.Blue),

			//Yellow-Green (2 4) DF
			new SymmetryElement(CubeColor.Red, CubeColor.Green, CubeColor.Yellow),
			//Yellow-Blue (2 5) DB
			new SymmetryElement(CubeColor.Red, CubeColor.Blue, CubeColor.White),

			//Green-Orange (4 0) FL
			new SymmetryElement(CubeColor.Green, CubeColor.White, CubeColor.Orange),
			//Green-Red (4 1) FR
			new SymmetryElement(CubeColor.Blue, CubeColor.White, CubeColor.Red),

			//Corner Pieces
			//Orange-Yellow-Green
			new SymmetryElement(CubeColor.Yellow, CubeColor.Green, CubeColor.Orange),
			//Orange-Yellow-Blue
			new SymmetryElement(CubeColor.Blue, CubeColor.Orange, CubeColor.White),
			//Orange-White-Green
			new SymmetryElement(CubeColor.Green, CubeColor.Red, CubeColor.White),
			//Orange-White-Blue
			new SymmetryElement(CubeColor.White, CubeColor.Green, CubeColor.Red),

			//Reflections with axis normal
			//X-Axis
			new SymmetryElement(CubeColor.Red, CubeColor.Yellow, CubeColor.Green),
			//Y-Axis
			new SymmetryElement(CubeColor.Orange, CubeColor.White, CubeColor.Green),
			//Z-Axis
			new SymmetryElement(CubeColor.Orange, CubeColor.Yellow, CubeColor.Blue),

			//Mirros through cube edges
			//Orange-Yellow (0 2) LD
			new SymmetryElement(CubeColor.Yellow, CubeColor.Orange, CubeColor.Green),
			//Orange-White (0 3) LU
			new SymmetryElement(CubeColor.White, CubeColor.Red, CubeColor.Green),

			//Yellow-Green (2 4) DF
			new SymmetryElement(CubeColor.Orange, CubeColor.Green, CubeColor.Yellow),
			//Yellow-Blue (2 5) DB
			new SymmetryElement(CubeColor.Orange, CubeColor.Blue, CubeColor.White),

			//Green-Orange (4 0) FL
			new SymmetryElement(CubeColor.Green, CubeColor.Yellow, CubeColor.Orange),
			//Green-Red (4 1) FR
			new SymmetryElement(CubeColor.Blue, CubeColor.Yellow, CubeColor.Red),

			//PunktSym Zentrum
			new SymmetryElement(CubeColor.Red, CubeColor.White, CubeColor.Blue),

			//Rotate like "Orange-Yellow-Green" and reflect like "X-Axis"
			new SymmetryElement(CubeColor.White, CubeColor.Green, CubeColor.Orange),

			//Rotate like "Orange-Yellow-Green" and reflect like "Y-Axis"
			new SymmetryElement(CubeColor.Yellow, CubeColor.Blue, CubeColor.Orange),

			//Rotate like "Orange-Yellow-Green" and reflect like "Z-Axis"
			new SymmetryElement(CubeColor.Yellow, CubeColor.Green, CubeColor.Red),

			//Rotate like "Yellow-Green" and reflect like "Z-Axis"
			new SymmetryElement(CubeColor.Red, CubeColor.Green, CubeColor.White),

			//Rotate around Z-Axis and reflect like "Z-Axis"
			new SymmetryElement(CubeColor.Yellow, CubeColor.Red, CubeColor.Blue),

			//Rotate like "Orange-White-Green" and reflect like "Z-Axis"
			new SymmetryElement(CubeColor.White, CubeColor.Blue, CubeColor.Red),

			//Rotate like "Green-Red (4 1) FR" and reflect like "Z-Axis"
			new SymmetryElement(CubeColor.Blue, CubeColor.White, CubeColor.Orange),
		};
		public static int[,] GroupIndexTable = new int[ORDER, ORDER];
		public static int[][] MultGroupIndices = new int[ORDER][];
		public static BitArray FullSymmetry = new BitArray((1UL << ORDER) - 1);

		static SymmetryElement()
		{
			int index = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					for (int z = 0; z < 6; z++)
					{
						if (x / 2 == z / 2 || y / 2 == z / 2) continue;

						Elements[index++] = new SymmetryElement((CubeColor)x, (CubeColor)y, (CubeColor)z);
					}
				}
			}

			for (int i = 0; i < ORDER; i++)
			{
				for (int j = 0; j < ORDER; j++)
				{
					GroupIndexTable[i, j] = (Elements[i] * Elements[j]).Index;
				}
			}

			for (int i = 0; i < ORDER; i++)
			{
				MultGroupIndices[i] = Elements[i].GenerateMultGroup().Select(x => x.Index).ToArray();
			}
		}

		public CubeColor this[int i]
		{
			get
			{
				return i switch
				{
					0 => Orange,
					1 => Yellow,
					2 => Green,
					_ => CubeColor.None,
				};
			}
			set
			{
				switch (i)
				{
					case 0: Orange = value; break;
					case 1: Yellow = value; break;
					case 2: Green = value; break;
				}
			}
		}

		public bool IsIdentity => Orange == CubeColor.Orange && Yellow == CubeColor.Yellow && Green == CubeColor.Green;

		public int Index
		{
			get
			{
				return (int)Orange * 8 +
					((int)Orange < 2
						? ((int)Yellow - 2)
						: ((int)Orange < 4
							? ((int)Yellow >> 1) | ((int)Yellow & 1)
							: (int)Yellow)) * 2 +
					((int)Green & 1);
			}
		}

		public bool HasReflection
		{
			get
			{
				//Dont even ask me
				int index = Index;

				return ((index ^ (index >> 1) ^ (index >> 2) ^ (index >> 3) ^ (index >> 4)) & 1) != 0;
			}
		}

		public CubeColor Orange, Yellow, Green;


		public SymmetryElement(CubeColor orange, CubeColor yellow, CubeColor green)
		{
			Orange = orange;
			Yellow = yellow;
			Green = green;
		}

		public CubeColor TransformColor(CubeColor c)
		{
			if ((((int)c) & 1) == 0)
				return this[(int)c >> 1];
			else
				//flip color twice
				return (CubeColor)((int)this[(((int)c) ^ 1) >> 1] ^ 1);
		}
		public int TransformColor(int c)
		{
			if ((c & 1) == 0)
				return (int)this[c >> 1];
			else
				//flip color twice
				return ((int)this[(c ^ 1) >> 1] ^ 1);
		}

		public CubeMove TransformMove(CubeMove m)
		{
			int val = (int)m;

			val += (TransformColor(val / 3) - (val / 3)) * 3;

			if (HasReflection)
				return (CubeMove)(val +
					((val % 3) == 0 ? 2 :
					(val % 3) == 1 ? 0 : -2));
			else
				return (CubeMove)val;
		}

		public List<SymmetryElement> GenerateMultGroup()
		{
			SymmetryElement state = this;

			List<SymmetryElement> subGroup = new List<SymmetryElement>();
			subGroup.Add(state);
			while (!state.IsIdentity)
			{
				state *= this;
				subGroup.Add(state);
			}

			subGroup.Sort();

			return subGroup;
		}
		public void GenerateMultGroup(List<SymmetryElement> subGroup)
		{
			SymmetryElement state = this;

			subGroup.Clear();
			subGroup.Add(state);
			while (!state.IsIdentity)
			{
				state *= this;
				subGroup.Add(state);
			}

			subGroup.Sort();
		}

		public static SymmetryElement operator *(SymmetryElement a, SymmetryElement b)
		{
			return new SymmetryElement(b.TransformColor(a.Orange), b.TransformColor(a.Yellow), b.TransformColor(a.Green));
		}

		public override bool Equals(object obj)
		{
			return obj is SymmetryElement element &&
				   Orange == element.Orange &&
				   Yellow == element.Yellow &&
				   Green == element.Green;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Orange, Yellow, Green);
		}

		public int CompareTo([AllowNull] SymmetryElement other)
		{
			if (Orange != other.Orange)
				return Orange.CompareTo(other.Orange);

			if (Yellow != other.Yellow)
				return Yellow.CompareTo(other.Yellow);

			return Green.CompareTo(other.Green);
		}

		public override string ToString()
		{
			return Orange + " " + Yellow + " " + Green;
		}
	}
}
