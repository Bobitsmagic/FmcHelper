using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CubeAD.CubeRepresentation
{
	public class PermCubeSingleArray
	{
		static int[][] EdgeMovePerm = new int[18][];
		static int[][] CornerMovePerm = new int[18][];
		static int[] FrontEdges = new int[] { 3, 7, 9, 11 };
		static int[] BackEdges = new int[] { 2, 6, 8, 10 };
		static int[][] SideCorners = new int[][]
		{
			new int[]{0, 1, 2, 3},
			new int[]{4, 5, 6, 7},
			new int[]{0, 1 ,4, 5},
			new int[]{2, 3, 6, 7},
			new int[]{0, 2, 4, 6},
			new int[]{1, 3, 5, 7},
		};

		static byte[] DataBuffer = new byte[12 + 8 + 12 + 8];
		static byte[] SolvedPerm = new byte[12 + 8 + 12 + 8];

		byte[] Data = new byte[12 + 8 + 12 + 8];
		

		static PermCubeSingleArray()
		{
			for (int i = 0; i < 18; i++)
			{
				TileCube ac = TileCube.GetSolved();
				ac.MakeMove((CubeMove)i);
				EdgeMovePerm[i] = ac.GetEdgePerm();
				CornerMovePerm[i] = ac.GetCornerPerm();
			}

			for (int i = 0; i < 12; i++)
			{
				SolvedPerm[i] = (byte)i;
			}
			for (int i = 0; i < 8; i++)
			{
				SolvedPerm[i + 12] = (byte)i;
			}

			//for(int i = 0; i < FrontEdges.Length; i++)
			//{
			//	FrontEdges[i] += 20;
			//	BackEdges[i] += 20;
			//}

			//for (int i = 0; i < 6; i++)
			//{
			//	for(int j = 0; j < 4; j++)
			//	{
			//		SideCorners[i][j] += 32;
			//	}
			//}
		}

		public PermCubeSingleArray()
		{
			Reset();
		}

		public void Reset()
		{
			Array.Copy(SolvedPerm, Data, 40);
		}

		public void MakeMove(CubeMove m)
		{
			int move = (int)m;
			Array.Copy(Data, DataBuffer, 40);

			int[] perm = EdgeMovePerm[move];
			Span<byte> EdgeIndices = new Span<byte>(Data, 0, 12);
			Span<byte> CornerIndices = new Span<byte>(Data, 12, 8);
			Span<byte> EdgeOrientation = new Span<byte>(Data, 20, 12);
			Span<byte> CornerOrientation = new Span<byte>(Data, 32, 8);

			for (int i = 0; i < 12; i++)
			{
				EdgeIndices[perm[i]] = DataBuffer[i];
				EdgeOrientation[perm[i]] = DataBuffer[i + 20];
			}

			//Flip edges on F, FP, B, BP
			if (m == CubeMove.F || m == CubeMove.FP)
			{
				foreach (int index in FrontEdges)
				{
					EdgeOrientation[index] = (byte)(1 - EdgeOrientation[index]);
				}
			}
			if (m == CubeMove.B || m == CubeMove.BP)
			{
				foreach (int index in BackEdges)
				{
					EdgeOrientation[index] = (byte)(1 - EdgeOrientation[index]);
				}
			}
			perm = CornerMovePerm[(int)m];
			for (int i = 0; i < 8; i++)
			{
				CornerIndices[perm[i]] = DataBuffer[i + 12];
				CornerOrientation[perm[i]] = DataBuffer[i + 32];
			}

			if (move % 3 != 1)
			{
				foreach (var index in SideCorners[move / 3])
				{
					//swaps orientation if its not equal to side / 2
					CornerOrientation[index] = (byte)((2 * CornerOrientation[index] - (move / 6) + 3) % 3);
				}
			}
		}
	}
}
