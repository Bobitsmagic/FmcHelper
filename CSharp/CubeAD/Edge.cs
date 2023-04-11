using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeAD
{
	public struct Edge
	{
		public CubeColor A, B;

		public Edge(CubeColor a, CubeColor b)
		{
			A = a;
			B = b;
		}

		public static bool operator ==(Edge left, Edge right)
		{
			return left.A == right.A && left.B == right.B;
		}
		public static bool operator !=(Edge left, Edge right)
		{
			return !(left == right);
		}

		public override string ToString()
		{
			return "(" + A + ", " + B + ")";	
		}
	}
}
