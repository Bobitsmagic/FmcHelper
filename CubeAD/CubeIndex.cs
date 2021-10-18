using System;
using System.Collections.Generic;
using System.Text;

namespace CubeAD
{
	public struct CubeIndex
	{
		const uint MAX_EDGE_POSITIONS = 479001600;
		const ushort MAX_CORNER_POSITIONS = 40320;
		const ushort MAX_EDGE_FLIPS = 2048;
		const ushort MAX_CORNER_ROTATIONS = 2187;

		public static int[,] EdgeIndices = new int[6, 6];
		public static int[,,] CornerIndices = new int[6, 6, 6];

		static CubeIndex()
		{
			int counter = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					EdgeIndices[y, x] = counter;
					EdgeIndices[x, y] = counter++;
				}
			}

			counter = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						CornerIndices[x, y, z] = counter;
						CornerIndices[x, z, y] = counter;

						CornerIndices[y, x, z] = counter;
						CornerIndices[y, z, x] = counter;

						CornerIndices[z, x, y] = counter;
						CornerIndices[z, y, x] = counter++;
					}
				}
			}
		}
			
		public uint EdgePositions; //12! = 479001600   /2?
		public ushort CornerPositions; //8! = 40320    /2?
		public ushort EdgeFlips; //2^11 = 2048
		public ushort CornerRotations; //3^7 = 2187

		public CubeIndex(uint edgePositions, ushort cornerPositions, ushort edgeFlips, ushort cornerRotations)
		{
			if (edgePositions >= MAX_EDGE_POSITIONS ||
				cornerPositions >= MAX_CORNER_POSITIONS ||
				edgeFlips >= MAX_EDGE_FLIPS ||
				cornerRotations >= MAX_CORNER_ROTATIONS)
				throw new ArgumentException();

			EdgePositions = edgePositions;
			CornerPositions = cornerPositions;
			EdgeFlips = edgeFlips;
			CornerRotations = cornerRotations;
		}

		public CubeIndex(Cube c)
		{
			EdgePositions = FindEdgePositionIndex(c);
			CornerPositions = FindCornerPositionIndex(c);
			EdgeFlips = FindEdgeOrientationIndex(c);
			CornerRotations = FindCornerOrientationIndex(c);
		}


		public static uint FindEdgePositionIndex(Cube c)
		{
			int[] perm = new int[12];

			int counter = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					perm[counter++] = EdgeIndices[(int)c.GetEdgeColor(x, y), (int)c.GetEdgeColor(y, x)];
				}
			}

			return (uint)GetIndex(perm);
		}
		public static ushort FindCornerPositionIndex(Cube c)
		{
			int[] perm = new int[8];

			int counter = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						perm[counter++] = EdgeIndices[(int)c.GetEdgeColor(x, y), (int)c.GetEdgeColor(y, x)];
					}
				}
			}

			return (ushort)GetIndex(perm);
		}
		public static ushort FindEdgeOrientationIndex(Cube c)
		{
			ushort ret = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					ret = (ushort)((ret << 1) | (c.EdgeIsOriented(x, y) ? 0 : 1));
				}
			}

			return ret;
		}
		public static ushort FindCornerOrientationIndex(Cube c)
		{
			ushort ret = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						ret = (ushort)((ret * 3) + c.CornerOrientaion(x,y,z));
					}
				}
			}

			return ret;
		}


		public static int Factorial(int n)
		{
			int ret = 1;
			for (int i = 2; i <= n; i++)
			{
				ret *= i;
			}

			return ret;
		}

		public static int[] GetIndexedPerm(int n, int index)
		{
			List<int> list = new List<int>(n);
			for (int i = 0; i < n; i++) list.Add(i);

			int[] indices = new int[n];
			for (int i = n - 1; i >= 0; i--)
			{
				indices[i] = index / Factorial(i);
				index -= indices[i] * Factorial(i);
			}

			int[] ret = new int[n];

			for (int i = ret.Length - 1; i >= 0; i--)
			{
				ret[n - i - 1] = list[indices[i]];
				list.RemoveAt(indices[i]);
			}

			return ret;
		}
		private static readonly List<int> bufferList = new List<int>();

		public static int[] GetIndexedPerm(int[] ret, int index)
		{
			int n = ret.Length;

			bufferList.Clear();
			for (int i = 0; i < n; i++) bufferList.Add(i);

			int[] indices = new int[n];
			for (int i = n - 1; i >= 0; i--)
			{
				indices[i] = index / Factorial(i);
				index -= indices[i] * Factorial(i);
			}

			for (int i = ret.Length - 1; i >= 0; i--)
			{
				ret[n - i - 1] = bufferList[indices[i]];
				bufferList.RemoveAt(indices[i]);
			}

			return ret;
		}
		public static int GetIndex(int[] perm)
		{
			int n = perm.Length;
			int index = 0;
			int[] indices = new int[n];
			SortedList<int, int> list = new SortedList<int, int>(n);

			for (int i = 0; i < n; i++)
			{
				list.Add(perm[n - i - 1], i);
				indices[i] = list.IndexOfKey(perm[n - i - 1]);
			}

			for (int i = 0; i < n; i++)
			{
				index += indices[i] * Factorial(i);
			}

			return index;
		}

		public override string ToString()
		{
			return base.ToString();
		}

		public override bool Equals(object obj)
		{
			return obj is CubeIndex index &&
				   EdgePositions == index.EdgePositions &&
				   CornerPositions == index.CornerPositions &&
				   EdgeFlips == index.EdgeFlips &&
				   CornerRotations == index.CornerRotations;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(EdgePositions, CornerPositions, EdgeFlips, CornerRotations);
		}
	}
}
