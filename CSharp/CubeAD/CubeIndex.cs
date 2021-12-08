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

		public bool IsSovled => EdgeOrientationIndex == 0 && EdgePermutationIndex == 0 && CornerOrientationIndex == 0 && CornerPermutationIndex == 0;

		public byte this[int index]
		{
			get
			{
				if (index >= 10) Console.WriteLine("CubeIndex[i] out of range");

				if (index < 2)
					return (byte)(CornerOrientationIndex >> (index * 8));

				if (index < 4)
					return (byte)(EdgeOrientationIndex >> ((index - 2) * 8));

				if (index < 6)
					return (byte)(CornerPermutationIndex >> ((index - 4) * 8));

				return (byte)(EdgePermutationIndex >> ((index - 6) * 8));
			}
		}
		public BigInteger Index
		{
			get
			{
				BigInteger ret = EdgePermutationIndex;
				ret *= MAX_CORNER_PERMUTATION;
				ret += CornerPermutationIndex;
				ret *= MAX_EDGE_ORIENTATION;
				ret += EdgeOrientationIndex;
				ret *= MAX_CORNER_ORIENTATION;
				ret += CornerOrientationIndex;

				return ret;
			}
		}

		public uint EdgePermutationIndex;
		public ushort CornerPermutationIndex;
		public ushort EdgeOrientationIndex;
		public ushort CornerOrientationIndex;
		public CubeMove LastMove;

		//Constructor
		public CubeIndex(uint edgePermutation, ushort cornerPermutation, ushort edgeOrientation, ushort cornerOrientation, CubeMove move = CubeMove.None)
		{
			if (edgePermutation >= MAX_EDGE_PERMUTATION ||
				cornerPermutation >= MAX_CORNER_PERMUTATION ||
				edgeOrientation >= MAX_EDGE_ORIENTATION ||
				cornerOrientation >= MAX_CORNER_ORIENTATION)
				throw new ArgumentException();

			EdgePermutationIndex = edgePermutation;
			CornerPermutationIndex = cornerPermutation;
			EdgeOrientationIndex = edgeOrientation;
			CornerOrientationIndex = cornerOrientation;
			LastMove = move;
		}
		public CubeIndex(Random rnd)
		{
			EdgePermutationIndex = (uint)rnd.Next((int)MAX_EDGE_PERMUTATION);
			CornerPermutationIndex = (ushort)rnd.Next(MAX_CORNER_PERMUTATION);
			EdgeOrientationIndex = (ushort)rnd.Next(MAX_EDGE_ORIENTATION);
			CornerOrientationIndex = (ushort)rnd.Next(MAX_CORNER_ORIENTATION);
			LastMove = (CubeMove)rnd.Next(18);
		}

		public CubeIndex(CubeIndex old, CubeMove m)
		{
			EdgePermutationIndex = old.EdgePermutationIndex;
			CornerPermutationIndex = old.CornerPermutationIndex;
			EdgeOrientationIndex = old.EdgeOrientationIndex;
			CornerOrientationIndex = old.CornerOrientationIndex;
			LastMove = CubeMove.None;

			MakeMove(m);
		}

		public CubeIndex(Cube c)
		{
			EdgePermutationIndex = FindEdgePermutationIndex(c);
			CornerPermutationIndex = FindCornerPermutationIndex(c);
			EdgeOrientationIndex = FindEdgeOrientationIndex(c);
			CornerOrientationIndex = FindCornerOrientationIndex(c);

			LastMove = CubeMove.None;
		}
		public void Reset()
		{
			EdgePermutationIndex = 0;
			EdgeOrientationIndex = 0;
			CornerPermutationIndex = 0;
			CornerOrientationIndex = 0;
			LastMove= CubeMove.None;
		}

		public CubeIndex(MoveSequenz ms)
		{
			EdgePermutationIndex = 0;
			CornerPermutationIndex = 0;
			EdgeOrientationIndex = 0;
			CornerOrientationIndex = 0;
			LastMove = CubeMove.None;

			ApplyMoveSequenz(ms);
		}
		public CubeIndex(BinaryReader br)
		{
			EdgePermutationIndex = br.ReadUInt32();
			CornerPermutationIndex = br.ReadUInt16();
			EdgeOrientationIndex = br.ReadUInt16();
			CornerOrientationIndex = br.ReadUInt16();
			LastMove = (CubeMove)br.ReadByte();
		}

		//Functions
		public void Write(BinaryWriter bw)
		{
			bw.Write(EdgePermutationIndex);
			bw.Write(CornerPermutationIndex);
			bw.Write(EdgeOrientationIndex);
			bw.Write(CornerOrientationIndex);
			bw.Write((byte)LastMove);
		}

		public void CopyValuesFrom(CubeIndex index)
		{
			EdgePermutationIndex = index.EdgePermutationIndex;
			CornerPermutationIndex = index.CornerPermutationIndex;
			EdgeOrientationIndex = index.EdgeOrientationIndex;
			CornerOrientationIndex = index.CornerOrientationIndex;
		}
		public int[] GetEdgePerm()
		{
			return GetIndexedPerm(12, (int)EdgePermutationIndex);
		}
		public int[] GetEdgeOrient()
		{
			int[] buffer = new int[12];

			for (int i = 0; i < 12; i++)
			{
				buffer[i] = (EdgeOrientationIndex >> (11 - i)) & 1;
			}

			return buffer;
		}
		public int[] GetCornerPerm()
		{
			return GetIndexedPerm(8, CornerPermutationIndex);
		}
		public int[] GetCornerOrient()
		{
			int buffer = CornerOrientationIndex;
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

		public void Transform(CubeIndex cube)
		{
			int[] edgePerm = GetEdgePerm();
			int[] edgeOrient = GetEdgeOrient();

			int[] transformEdgePerm = cube.GetEdgePerm();
			int[] transformEdgeOrient = cube.GetEdgeOrient();


			int[] resultEdgePerm = new int[edgePerm.Length];
			int[] resultEdgeOrient = new int[edgePerm.Length];

			for (int i = 0; i < edgePerm.Length; i++)
			{
				resultEdgePerm[i] = edgePerm[transformEdgePerm[i]];
				resultEdgeOrient[resultEdgePerm[i]] = (edgeOrient[edgePerm[i]] + transformEdgeOrient[transformEdgePerm[i]]) % 2;
			}

			int[] cornerPerm = GetCornerPerm();
			int[] cornerOrient = GetCornerOrient();

			int[] transformCornerPerm = cube.GetCornerPerm();
			int[] transformCornerOrient = cube.GetCornerOrient();


			int[] resultCornerPerm = new int[cornerPerm.Length];
			int[] resultCornerOrient = new int[cornerPerm.Length];

			for (int i = 0; i < cornerPerm.Length; i++)
			{
				resultCornerPerm[i] = cornerPerm[transformCornerPerm[i]];
				resultCornerOrient[resultCornerPerm[i]] = (cornerOrient[cornerPerm[i]] + transformCornerOrient[transformCornerPerm[i]]) % 3;
			}
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

			CornerPermutationIndex = NextCornerPerm[CornerPermutationIndex, move];
			CornerOrientationIndex = NextCornerOrient[CornerOrientationIndex, move];
			EdgeOrientationIndex = NextEdgeOrient[EdgeOrientationIndex, move];

			int count = (move % 3) + 1;
			int side = move / 3;

			uint[] array = NextEdgePerm[side];
			int trackLength = EdgeTrackSizes[side];

			for (int i = 0; i < count; i++)
			{
				int index = ((int)EdgePermutationIndex / trackLength) % array.Length;

				if (((int)EdgePermutationIndex / trackLength / array.Length) % 2 == 0)
				{
					EdgePermutationIndex = (EdgePermutationIndex + array[index]) % MAX_EDGE_PERMUTATION;
				}
				else
				{
					index = array.Length - index - 1;
					EdgePermutationIndex = (EdgePermutationIndex + MAX_EDGE_PERMUTATION - array[index]) % MAX_EDGE_PERMUTATION;
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
		public int GetSingleCornerOrientation(int index)
		{
			return GetCornerOrient()[index];
		}

		public BitMap64 GetSymmetryMask()
		{
			if (SymmetryEdgePermMask.ContainsKey(EdgePermutationIndex))
			{
				BitMap64 mask = SymmetryEdgePermMask[EdgePermutationIndex];

				mask &= SymmetryCornerPermMask[CornerPermutationIndex];
				mask &= SymmetryEdgePermMask[EdgeOrientationIndex];

				return mask;
			}

			//Identity sym
			return new BitMap64(1);
		}

		public bool HasSymmetry(SymmetryElement se)
		{
			return CornersHaveSymmetry(se) && EdgesHaveSymmetry(se);
		}

		//[TODO]
		public bool CornersHaveSymmetry(SymmetryElement se)
		{
			int[] perm = GetCornerPerm();
			int[] inversePerm = GetInverse(perm);
			int[] orient = GetCornerOrient();

			byte[] symTransfrom = SymmetryCornerPermutation[se.Index];
			byte[,,] symCornerOrient = SymmetryCornerOrientation[se.Index];

			for (int i = 0; i < 8; i++)
			{
				if (symTransfrom[inversePerm[i]] != inversePerm[symTransfrom[i]] ||
 					symCornerOrient[i, inversePerm[i], orient[inversePerm[i]]] != orient[symTransfrom[inversePerm[i]]])
				{
					return false;
				}
			}

			return true;
		}
		public bool EdgesHaveSymmetry(SymmetryElement se)
		{
			//edges
			int[] perm = GetEdgePerm();
			int[] inversePerm = GetInverse(perm);
			int[] orient = GetEdgeOrient();

			byte[] symTransfrom = SymmetryEdgePermutation[se.Index];
			byte[] symOrientation = SymmetryEdgeOrientation[se.Index];

			for (int i = 0; i < 8; i++)
			{
				if (symTransfrom[inversePerm[i]] != inversePerm[symTransfrom[i]] ||
					orient[i] != (orient[symTransfrom[i]] ^ symOrientation[i] ^ symOrientation[perm[i]]))
				{
					return false;
				}
			}

			return true;
		}

		public bool IsEqualWithSymmetry(CubeIndex other, SymmetryElement se)
		{
			int[] perm = GetCornerPerm();
			int[] inversePerm = GetInverse(perm);
			int[] orient = GetCornerOrient();

			int[] otherInversePerm = GetInverse(other.GetCornerPerm());
			int[] otherOrient = other.GetCornerOrient();

			byte[] symTransfrom = SymmetryCornerPermutation[se.Index];
			byte[,,] symCornerOrient = SymmetryCornerOrientation[se.Index];

			for (int i = 0; i < 8; i++)
			{
				if (symTransfrom[inversePerm[i]] != otherInversePerm[symTransfrom[i]] ||
 					symCornerOrient[i, inversePerm[i], orient[inversePerm[i]]] != otherOrient[symTransfrom[inversePerm[i]]])
				{
					return false;
				}
			}

			//edges
			perm = GetEdgePerm();
			inversePerm = GetInverse(perm);
			orient = GetEdgeOrient();

			otherInversePerm = GetInverse(other.GetEdgePerm());
			otherOrient = other.GetEdgeOrient();

			symTransfrom = SymmetryEdgePermutation[se.Index];
			byte[] symOrientation = SymmetryEdgeOrientation[se.Index];

			for (int i = 0; i < 8; i++)
			{
				if (symTransfrom[inversePerm[i]] != otherInversePerm[symTransfrom[i]] || 
					orient[i] != (otherOrient[symTransfrom[i]] ^ symOrientation[i] ^ symOrientation[perm[i]]))
				{
					return false;
				}
			}

			return true;
		}
		public bool IsEqualWithSymmetry(CubeIndex other, int se)
		{
			int[] perm = GetCornerPerm();
			int[] inversePerm = GetInverse(perm);
			int[] orient = GetCornerOrient();

			int[] otherInversePerm = GetInverse(other.GetCornerPerm());
			int[] otherOrient = other.GetCornerOrient();

			byte[] symTransfrom = SymmetryCornerPermutation[se];
			byte[,,] symCornerOrient = SymmetryCornerOrientation[se];

			for (int i = 0; i < 8; i++)
			{
				if (symTransfrom[inversePerm[i]] != otherInversePerm[symTransfrom[i]] ||
 					symCornerOrient[i, inversePerm[i], orient[inversePerm[i]]] != otherOrient[symTransfrom[inversePerm[i]]])
				{
					return false;
				}
			}

			//edges
			perm = GetEdgePerm();
			inversePerm = GetInverse(perm);
			orient = GetEdgeOrient();

			otherInversePerm = GetInverse(other.GetEdgePerm());
			otherOrient = other.GetEdgeOrient();

			symTransfrom = SymmetryEdgePermutation[se];
			byte[] symOrientation = SymmetryEdgeOrientation[se];

			for (int i = 0; i < 8; i++)
			{
				if (symTransfrom[inversePerm[i]] != otherInversePerm[symTransfrom[i]] ||
					orient[i] != (otherOrient[symTransfrom[i]] ^ symOrientation[i] ^ symOrientation[perm[i]]))
				{
					return false;
				}
			}

			return true;
		}
		public bool IsEqualWithSymmetry(CubeIndex other)
		{
			if (this == other)
				return true;

			for(int i = 1; i < 48; i++)
			{
				if (IsEqualWithSymmetry(other, i))
				{
					return true;
				}
			}

			return false;
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
			return a.EdgePermutationIndex == b.EdgePermutationIndex &&
				a.EdgeOrientationIndex == b.EdgeOrientationIndex &&
				a.CornerPermutationIndex == b.CornerPermutationIndex &&
				a.CornerOrientationIndex == b.CornerOrientationIndex;
		}
		public static bool operator !=(CubeIndex a, CubeIndex b)
		{
			return !(a == b);
		}
		public static bool operator <(CubeIndex a, CubeIndex b)
		{
			if (a.EdgePermutationIndex != b.EdgePermutationIndex) return a.EdgePermutationIndex < b.EdgePermutationIndex;
			if (a.CornerPermutationIndex != b.CornerPermutationIndex) return a.CornerPermutationIndex < b.CornerPermutationIndex;
			if (a.EdgeOrientationIndex != b.EdgeOrientationIndex) return a.EdgeOrientationIndex < b.EdgeOrientationIndex;

			return a.CornerOrientationIndex < b.CornerOrientationIndex;
		}
		public static bool operator >(CubeIndex a, CubeIndex b)
		{
			if (a.EdgePermutationIndex != b.EdgePermutationIndex) return a.EdgePermutationIndex > b.EdgePermutationIndex;
			if (a.CornerPermutationIndex != b.CornerPermutationIndex) return a.CornerPermutationIndex > b.CornerPermutationIndex;
			if (a.EdgeOrientationIndex != b.EdgeOrientationIndex) return a.EdgeOrientationIndex > b.EdgeOrientationIndex;

			return a.CornerOrientationIndex > b.CornerOrientationIndex;
		}
		public static CubeIndex operator *(CubeIndex a, CubeIndex b)
		{
			CubeIndex result = new CubeIndex();

			return result;
		}

		//Overrides
		public override string ToString()
		{
			return Index.ToString("00 000 000 000 000 000 000");
		}
		public override bool Equals(object obj)
		{
			return obj is CubeIndex other &&
				   EdgePermutationIndex == other.EdgePermutationIndex &&
				   CornerPermutationIndex == other.CornerPermutationIndex &&
				   EdgeOrientationIndex == other.EdgeOrientationIndex &&
				   CornerOrientationIndex == other.CornerOrientationIndex;
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(EdgePermutationIndex, CornerPermutationIndex, EdgeOrientationIndex, CornerOrientationIndex);
		}

		public int GetSymmetryHashCode()
		{
			return HashCode.Combine(SymmetryCornerMatrix[CornerPermutationIndex], SymmetryEdgeMatrix[EdgePermutationIndex]);
		}
		public int CompareTo(CubeIndex other)
		{
			if (EdgePermutationIndex != other.EdgePermutationIndex) return EdgePermutationIndex.CompareTo(other.EdgePermutationIndex);
			if (CornerPermutationIndex != other.CornerPermutationIndex) return CornerPermutationIndex.CompareTo(other.CornerPermutationIndex);
			if (EdgeOrientationIndex != other.EdgeOrientationIndex) return EdgeOrientationIndex.CompareTo(other.EdgeOrientationIndex);

			return CornerOrientationIndex.CompareTo(other.CornerOrientationIndex);
		}
	}
}