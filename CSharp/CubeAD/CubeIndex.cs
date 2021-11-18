using CubeAD.CubeIndexSets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static CubeAD.PreCompTables;
using static CubeAD.Permutation;

namespace CubeAD
{
	//Compressed representation of a cube
	public struct CubeIndex : IComparable<CubeIndex>
	{
		public const int SIZE_IN_BYTES = 11;
		public const int PADDED_SIZE_IN_BYTES = 12;

		public const uint MAX_EDGE_PERMUTATION = 479_001_600;	//12!
		public const ushort MAX_CORNER_PERMUTATION = 40320;		//8!
		public const ushort MAX_EDGE_ORIENTATION = 4096;		//2^12
		public const ushort MAX_CORNER_ORIENTATION = 6561;		//3^8

		public bool IsSovled => EdgeOrientation == 0 && EdgePermutation == 0 && CornerOrientation == 0 && CornerPermutation == 0;

		public byte this[int index]
		{
			get
			{
				if (index >= 10) Console.WriteLine("CubeIndex[i] out of range");

				if (index < 2)
					return (byte)(CornerOrientation >> (index * 8));

				if (index < 4)
					return (byte)(EdgeOrientation >> ((index - 2) * 8));

				if (index < 6)
					return (byte)(CornerPermutation >> ((index - 4) * 8));

				return (byte)(EdgePermutation >> ((index - 6) * 8));
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
		public ushort CornerPermutation;
		public ushort EdgeOrientation;
		public ushort CornerOrientation;
		public CubeMove LastMove;

		//Constructor
		public CubeIndex(uint edgePermutation, ushort cornerPermutation, ushort edgeOrientation, ushort cornerOrientation, CubeMove move = CubeMove.None)
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
			LastMove = move;

		}
		public CubeIndex(Random rnd)
		{
			EdgePermutation = (uint)rnd.Next((int)MAX_EDGE_PERMUTATION);
			CornerPermutation = (ushort)rnd.Next(MAX_CORNER_PERMUTATION);
			EdgeOrientation = (ushort)rnd.Next(MAX_EDGE_ORIENTATION);
			CornerOrientation = (ushort)rnd.Next(MAX_CORNER_ORIENTATION);
			LastMove = (CubeMove)rnd.Next(18);
		}

		public CubeIndex(CubeIndex old, CubeMove m)
		{
			EdgePermutation = old.EdgePermutation;
			CornerPermutation = old.CornerPermutation;
			EdgeOrientation = old.EdgeOrientation;
			CornerOrientation = old.CornerOrientation;
			LastMove = CubeMove.None;
			MakeMove(m);
		}

		public CubeIndex(Cube c)
		{
			EdgePermutation = FindEdgePermutationIndex(c);
			CornerPermutation = FindCornerPermutationIndex(c);
			EdgeOrientation = FindEdgeOrientationIndex(c);
			CornerOrientation = FindCornerOrientationIndex(c);

			LastMove = CubeMove.None;
		}

		public CubeIndex(MoveSequenz ms)
		{
			EdgePermutation = 0;
			CornerPermutation = 0;
			EdgeOrientation = 0;
			CornerOrientation = 0;
			LastMove = CubeMove.None;

			ApplyMoveSequenz(ms);
		}
		public CubeIndex(BinaryReader br)
		{
			EdgePermutation = br.ReadUInt32();
			CornerPermutation = br.ReadUInt16();
			EdgeOrientation = br.ReadUInt16();
			CornerOrientation = br.ReadUInt16();
			LastMove = (CubeMove)br.ReadByte();
		}

		//Functions
		public void Write(BinaryWriter bw)
		{
			bw.Write(EdgePermutation);
			bw.Write(CornerPermutation);
			bw.Write(EdgeOrientation);
			bw.Write(CornerOrientation);
			bw.Write((byte)LastMove);
		}

		public void CopyValuesFrom(CubeIndex index)
		{
			EdgePermutation = index.EdgePermutation;
			CornerPermutation = index.CornerPermutation;
			EdgeOrientation = index.EdgeOrientation;
			CornerOrientation = index.CornerOrientation;
		}
		public int[] GetEdgePerm()
		{
			return GetIndexedPerm(12, (int)EdgePermutation);
		}
		public int[] GetEdgeOrient()
		{
			int[] buffer = new int[12];

			for (int i = 0; i < 12; i++)
			{
				buffer[i] = (EdgeOrientation >> (11 - i)) & 1;
			}

			return buffer;
		}
		public int[] GetCornerPerm()
		{
			return GetIndexedPerm(8, CornerPermutation);
		}
		public int[] GetCornerOrient()
		{
			int buffer = CornerOrientation;
			int[] ret = new int[8];
			for (int i = 8 - 1; i >= 0; i--)
			{
				ret[i] = buffer % 3;
				buffer /= 3;
			}

			return ret;
		}

		public void ApplyToCube(Cube c)
		{
			int[] edgePerm = GetEdgePerm();
			int[] edgeOrient = GetEdgeOrient();

			c.Reset();

			int counter = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					//when swapping from main side (0, 1) to other sub sides (2, 3)
					int flip = (counter / 8) ^ (edgePerm[counter] / 8);

					c.SetEdgeColor(x, y, CubeEdgeColors[edgePerm[counter], edgeOrient[counter] ^ flip]);
					c.SetEdgeColor(y, x, CubeEdgeColors[edgePerm[counter], edgeOrient[counter] ^ 1 ^ flip]);

					counter++;
				}
			}

			int[] cornerPerm = GetCornerPerm();
			int[] cornerOrient = GetCornerOrient();
			counter = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						int dif = counter ^ cornerPerm[counter];
						bool flip = dif == 1 || dif == 2 || dif == 4 || dif == 7;

						c.SetCornerColor(x, y, z, CornerColors[cornerPerm[counter], ((flip ? 2 : 1) * (0 + 2 * cornerOrient[counter])) % 3]);
						c.SetCornerColor(y, x, z, CornerColors[cornerPerm[counter], ((flip ? 2 : 1) * (1 + 2 * cornerOrient[counter])) % 3]);
						c.SetCornerColor(z, x, y, CornerColors[cornerPerm[counter], ((flip ? 2 : 1) * (2 + 2 * cornerOrient[counter])) % 3]);

						counter++;
					}
				}
			}

			//U -> XZ
		}

		public Cube GetCube()
		{
			Cube c = new Cube();
			ApplyToCube(c);
			return c;
		}

		public void MakeMove(CubeMove m)
		{
			int move = (int)m;

			CornerPermutation = NextCornerPerm[CornerPermutation, move];
			CornerOrientation = NextCornerOrient[CornerOrientation, move];
			EdgeOrientation = NextEdgeOrient[EdgeOrientation, move];

			int count = (move % 3) + 1;
			int side = move / 3;

			uint[] array = NextEdgePerm[side];
			int trackLength = EdgeTrackSizes[side];

			for (int i = 0; i < count; i++)
			{
				int index = ((int)EdgePermutation / trackLength) % array.Length;

				if (((int)EdgePermutation / trackLength / array.Length) % 2 == 0)
				{
					EdgePermutation = (EdgePermutation + array[index]) % MAX_EDGE_PERMUTATION;
				}
				else
				{
					index = array.Length - index - 1;
					EdgePermutation = (EdgePermutation + MAX_EDGE_PERMUTATION - array[index]) % MAX_EDGE_PERMUTATION;
				}
			}

			LastMove = m;
		}

		public void ApplyMoveSequenz(MoveSequenz ms)
		{
			for (int i = 0; i < ms.Length; i++)
			{
				MakeMove(ms.Moves[i]);
			}
		}
		public void ApplyMoveSequenz(List<CubeMove> ms)
		{
			for (int i = 0; i < ms.Count; i++)
			{
				MakeMove(ms[i]);
			}
		}

		//Statics
		public static CubeIndex[] ReadCubeIndicesFromFile(string path)
		{
			MemoryStream ms = new MemoryStream(File.ReadAllBytes(path));
			BinaryReader br = new BinaryReader(ms);

			CubeIndex[] ret = new CubeIndex[br.ReadInt32()];

			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = new CubeIndex(br);
			}

			return ret;
		}
		public static void WriteCubeIndicesToFile(CubeIndex[] array, string path)
		{
			MemoryStream ms = new MemoryStream(array.Length * SIZE_IN_BYTES);

			BinaryWriter bw = new BinaryWriter(ms);

			bw.Write(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Write(bw);
			}

			File.WriteAllBytes(path, ms.ToArray());

			bw.Close();
			ms.Close();
		}

		//CubeIndex generation
		public static uint FindEdgePermutationIndex(Cube c, int[] permBuffer)
		{
			if (permBuffer.Length != 12)
				throw new ArgumentException("Array must be of length 12");

			int counter = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					permBuffer[counter++] = EdgeIndices[(int)c.GetEdgeColor(x, y), (int)c.GetEdgeColor(y, x)];
				}
			}
			return (uint)GetIndex(permBuffer);
		}
		public static uint FindEdgePermutationIndex(Cube c)
		{
			return FindEdgePermutationIndex(c, new int[12]);
		}
		public static ushort FindCornerPermutationIndex(Cube c, int[] permBuffer)
		{
			if (permBuffer.Length != 8)
				throw new ArgumentException("Array must be of length 8");

			int counter = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						permBuffer[counter++] = CornerIndices[(int)c.GetCornerColor(x, y, z), (int)c.GetCornerColor(y, x, z), (int)c.GetCornerColor(z, x, y)];
					}
				}
			}

			return (ushort)GetIndex(permBuffer);
		}
		public static ushort FindCornerPermutationIndex(Cube c)
		{
			return FindCornerPermutationIndex(c, new int[8]);
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
						//Console.WriteLine(c.CornerOrientaion(x, y, z));
						ret = (ret * 3) + c.CornerOrientaion(x, y, z);
					}
				}
			}

			return (ushort)ret;
		}
		
		//Sorting
		public static void RadixSortCubeIndices(CubeIndex[] data, CubeIndex[] CubeBuffer)
		{
			const int BIT_COUNT = 8;
			const int RADIX = 1 << BIT_COUNT;

			int[] indices = new int[RADIX];

			if (CubeBuffer.Length != data.Length)
			{
				CubeBuffer = new CubeIndex[data.Length];
				Console.WriteLine("Reallocated");
			}

			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < RADIX; j++)
					indices[j] = 0;
				for (int j = 0; j < data.Length; j++)
					indices[data[j][i]]++;

				for (int j = 1; j < RADIX; j++)
					indices[j] += indices[j - 1];
				for (int j = data.Length - 1; j >= 0; j--)
					CubeBuffer[--indices[data[j][i]]] = data[j];

				CubeIndex[] swap = CubeBuffer;
				CubeBuffer = data;
				data = swap;
			}
		}
		public static void RadixSortCubeIndices(List<CubeIndex> data, List<CubeIndex> CubeBuffer)
		{
			const int BIT_COUNT = 8;
			const int RADIX = 1 << BIT_COUNT;
			int N = data.Count;

			int[] indices = new int[RADIX];

			if (CubeBuffer.Count < N)
			{
				CubeBuffer.Clear();
				CubeBuffer.AddRange(data);
				//Console.WriteLine("Reallocated");
			}

			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < RADIX; j++)
					indices[j] = 0;
				for (int j = 0; j < N; j++)
					indices[data[j][i]]++;

				for (int j = 1; j < RADIX; j++)
					indices[j] += indices[j - 1];
				for (int j = N - 1; j >= 0; j--)
					CubeBuffer[--indices[data[j][i]]] = data[j];

				var swap = CubeBuffer;
				CubeBuffer = data;
				data = swap;
			}
		}

		//Operator
		public static bool operator ==(CubeIndex a, CubeIndex b)
		{
			return a.EdgePermutation == b.EdgePermutation &&
				a.EdgeOrientation == b.EdgeOrientation &&
				a.CornerPermutation == b.CornerPermutation &&
				a.CornerOrientation == b.CornerOrientation;
		}
		public static bool operator !=(CubeIndex a, CubeIndex b)
		{
			return !(a == b);
		}
		public static bool operator <(CubeIndex a, CubeIndex b)
		{
			if (a.EdgePermutation != b.EdgePermutation) return a.EdgePermutation < b.EdgePermutation;
			if (a.CornerPermutation != b.CornerPermutation) return a.CornerPermutation < b.CornerPermutation;
			if (a.EdgeOrientation != b.EdgeOrientation) return a.EdgeOrientation < b.EdgeOrientation;

			return a.CornerOrientation < b.CornerOrientation;
		}
		public static bool operator >(CubeIndex a, CubeIndex b)
		{
			if (a.EdgePermutation != b.EdgePermutation) return a.EdgePermutation > b.EdgePermutation;
			if (a.CornerPermutation != b.CornerPermutation) return a.CornerPermutation > b.CornerPermutation;
			if (a.EdgeOrientation != b.EdgeOrientation) return a.EdgeOrientation > b.EdgeOrientation;

			return a.CornerOrientation > b.CornerOrientation;
		}

		//Overrides
		public override string ToString()
		{
			return Index.ToString("000 000 000 000 000 000 000");
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
		public int CompareTo(CubeIndex other)
		{
			if (EdgePermutation != other.EdgePermutation) return EdgePermutation.CompareTo(other.EdgePermutation);
			if (CornerPermutation != other.CornerPermutation) return CornerPermutation.CompareTo(other.CornerPermutation);
			if (EdgeOrientation != other.EdgeOrientation) return EdgeOrientation.CompareTo(other.EdgeOrientation);

			return CornerOrientation.CompareTo(other.CornerOrientation);
		}
	}
}