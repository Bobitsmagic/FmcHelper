using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeAD.Pieces
{
    public struct Edge
    {
        public CubeColor A, B;

        public Edge(CubeColor a, CubeColor b)
        {
            A = a;
            B = b;
        }
		public Edge Transform(SymmetryElement se)
		{
			return new Edge(se.TransformColor(A), se.TransformColor(B));
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
