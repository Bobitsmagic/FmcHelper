using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CubeAD.PreCompTables;
using static CubeAD.Permutation;
using System.Buffers;

namespace CubeAD.CubeRepresentation
{
	public class SeperatedCube
	{
		public const uint MAX_EDGE_PERMUTATION = 479_001_600;   //12!
		public const ushort MAX_CORNER_PERMUTATION = 40320;     //8!
		public const ushort MAX_EDGE_ORIENTATION = 4096;        //2^12
		public const ushort MAX_CORNER_ORIENTATION = 6561;      //3^8

		static byte[][] EdgeMovePerm = new byte[18][];
		static byte[] Buffer = new byte[12];
		static byte[] IdenityPerm = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

		static ArrayPool<byte> ArrayPool = ArrayPool<byte>.Create();

		static SeperatedCube()
		{
			for (int i = 0; i < 18; i++)
			{
				TileCube ac = TileCube.GetSolved();
				ac.MakeMove((CubeMove)i);
	
				EdgeMovePerm[i] = ac.GetEdgePerm().Select(x => (byte)x).ToArray();
			}
		}

		public ushort CornerPermutationIndex;
		ushort CornerOrientationIndex;
		ushort EdgeOrientationIndex;
		byte[] EdgePerm = ArrayPool.Rent(12);
		public SeperatedCube()
		{
			CornerPermutationIndex = 0;
			CornerOrientationIndex = 0;
			EdgeOrientationIndex = 0;

			Array.Copy(IdenityPerm, EdgePerm, 12);
		}
		~SeperatedCube()
		{
			ArrayPool.Return(EdgePerm);
        }

		public void MakeMove(CubeMove m)
		{
			int move = (int)m;

			CornerPermutationIndex = NextCornerPerm[CornerPermutationIndex, move];
			CornerOrientationIndex = NextCornerOrient[CornerOrientationIndex, move];
			EdgeOrientationIndex = NextEdgeOrient[EdgeOrientationIndex, move];

			Array.Copy(EdgePerm, Buffer, 12);
			byte[] perm = EdgeMovePerm[move];

			for (int i = 0; i < 12; i++)
				EdgePerm[perm[i]] = Buffer[i];
		}
	}
}
