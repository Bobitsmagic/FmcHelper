using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeAD
{
	public struct SortedCorner
	{
		public CubeColor A, B, C;

		public static SortedCorner[] Corners = new SortedCorner[8]
		{
			new SortedCorner(CubeColor.Orange, CubeColor.Yellow, CubeColor.Blue),
			new SortedCorner(CubeColor.Orange, CubeColor.Yellow, CubeColor.Green),

			new SortedCorner(CubeColor.Orange, CubeColor.White, CubeColor.Blue),
			new SortedCorner(CubeColor.Orange, CubeColor.White, CubeColor.Green),

			new SortedCorner(CubeColor.Red, CubeColor.Yellow, CubeColor.Blue),
			new SortedCorner(CubeColor.Red, CubeColor.Yellow, CubeColor.Green),

			new SortedCorner(CubeColor.Red, CubeColor.White, CubeColor.Blue),
			new SortedCorner(CubeColor.Red, CubeColor.White, CubeColor.Green),
		};
		public int GetIndex
		{
			get
			{
				return Array.IndexOf(Corners, this);
			}
		}

		public SortedCorner(CubeColor a, CubeColor b, CubeColor c)
		{
			int[] array = new int[3] { (int)a, (int)b, (int)c };
			Array.Sort(array);

			A = (CubeColor)array[0];
			B = (CubeColor)array[1];
			C = (CubeColor)array[2];
		}

		public static bool operator ==(SortedCorner left, SortedCorner right)
		{
			return left.A == right.A && left.B == right.B;
		}
		public static bool operator !=(SortedCorner left, SortedCorner right)
		{
			return !(left == right);
		}

		public override string ToString()
		{
			return "(" + A + ", " + B + ")";
		}
	}
}
