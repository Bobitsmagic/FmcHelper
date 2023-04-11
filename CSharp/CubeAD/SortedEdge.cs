using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeAD
{
	public struct SortedEdge
	{
		public CubeColor A, B;

		public static SortedEdge[] Edges = new SortedEdge[12]
		{
			new SortedEdge(CubeColor.Orange, CubeColor.Yellow),
			new SortedEdge(CubeColor.Orange, CubeColor.White),
			new SortedEdge(CubeColor.Orange, CubeColor.Blue),
			new SortedEdge(CubeColor.Orange, CubeColor.Green),

			new SortedEdge(CubeColor.Red, CubeColor.Yellow),
			new SortedEdge(CubeColor.Red, CubeColor.White),
			new SortedEdge(CubeColor.Red, CubeColor.Blue),
			new SortedEdge(CubeColor.Red, CubeColor.Green),

			new SortedEdge(CubeColor.Yellow, CubeColor.Blue),
			new SortedEdge(CubeColor.Yellow, CubeColor.Green),
			new SortedEdge(CubeColor.White, CubeColor.Blue),
			new SortedEdge(CubeColor.White, CubeColor.Green),
		};
		public int GetIndex
		{
			get
			{
				return Array.IndexOf(Edges, this);	
			}
		}

		public SortedEdge(CubeColor a, CubeColor b)
		{
			A = (CubeColor)Math.Min((int)a, (int)b);
			B = (CubeColor)Math.Max((int)a, (int)b);
		}

		public static bool operator ==(SortedEdge left, SortedEdge right)
		{
			return left.A == right.A && left.B == right.B;
		}
		public static bool operator !=(SortedEdge left, SortedEdge right)
		{
			return !(left == right);
		}

		public override string ToString()
		{
			return "(" + A + ", " + B + ")";
		}
	}
}
