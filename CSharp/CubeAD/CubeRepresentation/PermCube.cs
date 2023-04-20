using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeAD.CubeRepresentation
{
	public class PermCube
	{
		static int[][] EdgeMovePerm = new int[18][];
		static int[][] CornerMovePerm = new int[18][];
		static byte[] PermBufferArray = new byte[12];
		static byte[] OrientBufferArray = new byte[12];
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

		static byte[] SolvedEdgePerm = new byte[12];
		static byte[] SolvedCornerPerm = new byte[8];

		byte[] EdgeIndices = new byte[12];
		byte[] EdgeOrientation = new byte[12];
		byte[] CornerIndices = new byte[8];
		byte[] CornerOrientation = new byte[8];

		static PermCube()
		{
			for(int i = 0; i < 18; i++)
			{
				TileCube ac = TileCube.GetSolved();
				ac.MakeMove((CubeMove)i);

				EdgeMovePerm[i] = ac.GetEdgePerm();
				CornerMovePerm[i] = ac.GetCornerPerm();	
			}

			for(int i = 0; i < 12; i++)
			{
				SolvedEdgePerm[i] = (byte)i;
			}
			for(int i = 0; i < 8; i++)
			{
				SolvedCornerPerm[i] = (byte)i;
			}
		}

		public PermCube()
		{
			Reset();
		}

		public void Reset()
		{
			Array.Copy(SolvedEdgePerm, EdgeIndices, 12);
			Array.Clear(EdgeOrientation);
			Array.Copy(SolvedCornerPerm, CornerIndices, 8);
			Array.Clear(CornerOrientation);
		}

		public void MakeMove(CubeMove m)
		{
			int move = (int)m;
			Array.Copy(EdgeIndices, PermBufferArray, 12);
			Array.Copy(EdgeOrientation, OrientBufferArray, 12);
			int[] perm = EdgeMovePerm[move];

			for(int i = 0; i < 12; i++)
			{
				EdgeIndices[perm[i]] = PermBufferArray[i];
				EdgeOrientation[perm[i]] = OrientBufferArray[i];
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

			Array.Copy(CornerIndices, PermBufferArray, 8);
			Array.Copy(CornerOrientation, OrientBufferArray, 8);
			perm = CornerMovePerm[(int)m];
			for (int i = 0; i < 8; i++)
			{
				CornerIndices[perm[i]] = PermBufferArray[i];
				CornerOrientation[perm[i]] = OrientBufferArray[i];
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
