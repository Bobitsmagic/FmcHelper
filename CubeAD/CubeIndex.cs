using System;
using System.Collections.Generic;
using System.Numerics;

namespace CubeAD
{
	//Compressed representation of a cube
	public struct CubeIndex
	{
		public const uint MAX_EDGE_PERMUTATION = 479001600; //12!
		public const ushort MAX_CORNER_PERMUTATION = 40320; //8!
		public const ushort MAX_EDGE_ORIENTATION = 4096;    //2^12
		public const ushort MAX_CORNER_ORIENTATION = 6561;  //3^8

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

		public byte this[int index]
		{
			get
			{
				if (index >= 10) Console.WriteLine("Alarm");

				if (index < 4)
					return (byte)(EdgePermutation >> (index * 8));

				if (index < 6)
					return (byte)(CornerPermutation >> ((index - 4) * 8));

				if (index < 8)
					return (byte)(EdgeOrientation >> ((index - 6) * 8));

				return (byte)(CornerOrientation >> ((index - 8) * 8));
			}
		}
		public BigInteger Index
		{
			get
			{
				BigInteger ret = EdgePermutation;
				ret *= MAX_CORNER_PERMUTATION;
				ret += CornerPermutation;
				ret *= MAX_EDGE_ORIENTATION;
				ret += EdgeOrientation;
				ret *= MAX_CORNER_ORIENTATION;
				ret += CornerOrientation;

				return ret;
			}
		}

		public uint EdgePermutation;
		public ushort CornerPermutation; //8! = 40320    /2?
		public ushort EdgeOrientation; //2^12 = 4096         /2?
		public ushort CornerOrientation; //3^8 = 6561    /3?

		public CubeIndex(uint edgePermutation, ushort cornerPermutation, ushort edgeOrientation, ushort cornerOrientation)
		{
			if (edgePermutation >= MAX_EDGE_PERMUTATION ||
				cornerPermutation >= MAX_CORNER_PERMUTATION ||
				edgeOrientation >= MAX_EDGE_ORIENTATION ||
				cornerOrientation >= MAX_CORNER_ORIENTATION)
				throw new ArgumentException();

			EdgePermutation = edgePermutation;
			CornerPermutation = cornerPermutation;
			EdgeOrientation = edgeOrientation;
			CornerOrientation = cornerOrientation;
		}

		public CubeIndex(Cube c)
		{
			EdgePermutation = FindEdgePermutationIndex(c);
			CornerPermutation = FindCornerPermutationIndex(c);
			EdgeOrientation = FindEdgeOrientationIndex(c);
			CornerOrientation = FindCornerOrientationIndex(c);
		}


		public static uint FindEdgePermutationIndex(Cube c)
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
		public static ushort FindCornerPermutationIndex(Cube c)
		{
			int[] perm = new int[8];

			int counter = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						perm[counter++] = CornerIndices[(int)c.GetCornerColor(x, y, z), (int)c.GetCornerColor(y, x, z), (int)c.GetCornerColor(z, x, y)];
					}
				}
			}

			return (ushort)GetIndex(perm);
		}
		public static ushort FindEdgeOrientationIndex(Cube c)
		{
			int ret = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					ret = (ret << 1) | (c.EdgeIsOriented(x, y) ? 0 : 1);
				}
			}

			return (ushort)ret;
		}
		public static ushort FindCornerOrientationIndex(Cube c)
		{
			int ret = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						ret = (ret * 3) + c.CornerOrientaion(x, y, z);
					}
				}
			}

			return (ushort)ret;
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
			return obj is CubeIndex other &&
				   EdgePermutation == other.EdgePermutation &&
				   CornerPermutation == other.CornerPermutation &&
				   EdgeOrientation == other.EdgeOrientation &&
				   CornerOrientation == other.CornerOrientation;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(EdgePermutation, CornerPermutation, EdgeOrientation, CornerOrientation);
		}
	}
}
