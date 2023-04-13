using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeAD.Pieces
{
    public struct Corner
    {
        public CubeColor A, B, C;

        public Corner(CubeColor a, CubeColor b, CubeColor c)
        {
            A = a;
            B = b;
            C = c;
        }

        public static bool operator ==(Corner left, Corner right)
        {
            return left.A == right.A && left.B == right.B && left.C == right.C;
        }
        public static bool operator !=(Corner left, Corner right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return "(" + A + ", " + B + ", " + C + ")";
        }
    }
}
