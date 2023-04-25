using CubeAD.CubeRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeAD.CubeIndexSets
{
	public readonly struct EdgeCornerOrientState : IComparable<EdgeCornerOrientState>
	{
		public const int SIZE_IN_BYTES = 8;

		public readonly uint EdgePermIndex;
		public readonly ushort EdgeOrientIndex;
		public readonly ushort CornerOrientIndex;

		public EdgeCornerOrientState(uint egdePermIndex, ushort edgeOrientIndex, ushort cornerOrientIndex)
		{
			EdgePermIndex = egdePermIndex;
			EdgeOrientIndex = edgeOrientIndex;
			CornerOrientIndex = cornerOrientIndex;
		}

		public static bool operator ==(EdgeCornerOrientState left, EdgeCornerOrientState right)
		{
			return left.EdgePermIndex == right.EdgePermIndex && 
				left.EdgeOrientIndex == right.EdgeOrientIndex &&
				left.CornerOrientIndex == right.CornerOrientIndex;
		}
		public static bool operator !=(EdgeCornerOrientState left, EdgeCornerOrientState right)
		{
			return !(left == right);
		}

		public int CompareTo(EdgeCornerOrientState other)
		{
			if(EdgePermIndex != other.EdgePermIndex) 
				return EdgePermIndex.CompareTo(other.EdgePermIndex);

			if (EdgeOrientIndex != other.EdgeOrientIndex) 
				return EdgeOrientIndex.CompareTo(other.EdgeOrientIndex);

			return CornerOrientIndex.CompareTo(other.CornerOrientIndex);
		}

		public byte GetByte(int index)
		{
#if DEBUG
			if (index >= 8 || index < 0) 
				throw new ArgumentOutOfRangeException("CubeIndex[" + index + "] out of range [0, 10)");
#endif
			if (index < 4)
				return (byte)(EdgePermIndex >> index * 8);

			if (index < 6)
				return (byte)(EdgeOrientIndex >> (index - 4) * 8);

			return (byte)(CornerOrientIndex >> (index - 6) * 8);
		}
	}
}
