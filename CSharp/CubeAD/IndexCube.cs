using CubeAD.IndexCubeSets;
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
	public struct IndexCube : IComparable<IndexCube>
	{
		public const int SIZE_IN_BYTES = 11;
		public const int PADDED_SIZE_IN_BYTES = 12;

		public const uint MAX_EDGE_PERMUTATION = 479_001_600;	//12!
		public const ushort MAX_CORNER_PERMUTATION = 40320;		//8!
		public const ushort MAX_EDGE_ORIENTATION = 4096;		//2^12
		public const ushort MAX_CORNER_ORIENTATION = 6561;		//3^8

		public bool IsSovled => EdgeOrientationIndex == 0 && EdgePermutationIndex == 0 && CornerOrientationIndex == 0 && CornerPermutationIndex == 0;


		/// <summary>
		/// Generates a unique integer for every possible <see cref="IndexCube"/>
		/// </summary>
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

		#region Constructors
		public IndexCube(uint edgePermutation, ushort cornerPermutation, ushort edgeOrientation, ushort cornerOrientation, CubeMove move = CubeMove.None)
		{
			if (edgePermutation >= MAX_EDGE_PERMUTATION ||
				cornerPermutation >= MAX_CORNER_PERMUTATION ||
				edgeOrientation >= MAX_EDGE_ORIENTATION ||
				cornerOrientation >= MAX_CORNER_ORIENTATION)
				throw new ArgumentException("Indices must be smaller than their respected maximum value");

			EdgePermutationIndex = edgePermutation;
			CornerPermutationIndex = cornerPermutation;
			EdgeOrientationIndex = edgeOrientation;
			CornerOrientationIndex = cornerOrientation;
			LastMove = move;
		}
		public IndexCube(Random rnd)
		{
			EdgePermutationIndex = (uint)rnd.Next((int)MAX_EDGE_PERMUTATION);
			CornerPermutationIndex = (ushort)rnd.Next(MAX_CORNER_PERMUTATION);
			EdgeOrientationIndex = (ushort)rnd.Next(MAX_EDGE_ORIENTATION);
			CornerOrientationIndex = (ushort)rnd.Next(MAX_CORNER_ORIENTATION);
			LastMove = (CubeMove)rnd.Next(18);
		}

		/// <summary>
		/// Creates copy of a <see cref="IndexCube"/> and makes a <see cref="CubeMove"/> afterwards
		/// </summary>
		public IndexCube(IndexCube old, CubeMove m)
		{
			EdgePermutationIndex = old.EdgePermutationIndex;
			CornerPermutationIndex = old.CornerPermutationIndex;
			EdgeOrientationIndex = old.EdgeOrientationIndex;
			CornerOrientationIndex = old.CornerOrientationIndex;
			LastMove = CubeMove.None;

			MakeMove(m);
		}
		/// <summary>
		/// Creates a <see cref="IndexCube"/> from a <see cref="StickerCube"/>
		/// </summary>
		public IndexCube(StickerCube c)
		{
			EdgePermutationIndex = c.FindEdgePermutationIndex();
			CornerPermutationIndex = c.FindCornerPermutationIndex();
			EdgeOrientationIndex = c.FindEdgeOrientationIndex();
			CornerOrientationIndex = c.FindCornerOrientationIndex();

			LastMove = CubeMove.None;
		}

		/// <summary>
		/// Resets this <see cref="IndexCube"/> to the solved state
		/// </summary>
		public void Reset()
		{
			EdgePermutationIndex = 0;
			EdgeOrientationIndex = 0;
			CornerPermutationIndex = 0;
			CornerOrientationIndex = 0;
			LastMove= CubeMove.None;
		}

		/// <summary>
		/// Creates a solved <see cref="IndexCube"/> and applies a <see cref="MoveSequenz"/> afterwards
		/// </summary>
		/// <param name="ms"></param>
		public IndexCube(MoveSequenz ms)
		{
			EdgePermutationIndex = 0;
			CornerPermutationIndex = 0;
			EdgeOrientationIndex = 0;
			CornerOrientationIndex = 0;
			LastMove = CubeMove.None;

			ApplyMoveSequenz(ms);
		}

		/// <summary>
		/// Creates a <see cref="IndexCube"/> from a binary stream
		/// </summary>
		/// <param name="br"></param>
		public IndexCube(BinaryReader br)
		{
			EdgePermutationIndex = br.ReadUInt32();
			CornerPermutationIndex = br.ReadUInt16();
			EdgeOrientationIndex = br.ReadUInt16();
			CornerOrientationIndex = br.ReadUInt16();
			LastMove = (CubeMove)br.ReadByte();
		}
		#endregion


		#region Functions

		/// <summary>
		/// Writes all bytes of this <see cref="IndexCube"/> to a binary stream
		/// </summary>
		public void Write(BinaryWriter bw)
		{
			bw.Write(EdgePermutationIndex);
			bw.Write(CornerPermutationIndex);
			bw.Write(EdgeOrientationIndex);
			bw.Write(CornerOrientationIndex);
			bw.Write((byte)LastMove);
		}

		public void CopyValuesFrom(IndexCube index)
		{
			EdgePermutationIndex = index.EdgePermutationIndex;
			CornerPermutationIndex = index.CornerPermutationIndex;
			EdgeOrientationIndex = index.EdgeOrientationIndex;
			CornerOrientationIndex = index.CornerOrientationIndex;
		}

		/// <returns>The byte at the corresbonding <paramref name="index"/></returns>
		public byte GetByte(int index)
		{
			if (index >= 10 || index < 0)
				throw new ArgumentOutOfRangeException("CubeIndex[" + index + "] out of range [0, 10)");

			if (index < 2)
				return (byte)(CornerOrientationIndex >> (index * 8));

			if (index < 4)
				return (byte)(EdgeOrientationIndex >> ((index - 2) * 8));

			if (index < 6)
				return (byte)(CornerPermutationIndex >> ((index - 4) * 8));

			return (byte)(EdgePermutationIndex >> ((index - 6) * 8));
		}

		/// <returns>An array representing the permutation of edges</returns>
		public int[] GetEdgePermutation()
		{
			return GetIndexedPerm(12, (int)EdgePermutationIndex);
		}
		/// <returns>An array representing the orientation of edges</returns>
		public int[] GetEdgeOrientation()
		{
			int[] buffer = new int[12];

			for (int i = 0; i < 12; i++)
			{
				buffer[i] = (EdgeOrientationIndex >> i) & 1;
			}

			return buffer;
		}
		/// <returns>An array representing the permutation of corner</returns>
		public int[] GetCornerPermuation()
		{
			return GetIndexedPerm(8, CornerPermutationIndex);
		}
		/// <returns>An array representing the orientation of corners</returns>
		public int[] GetCornerOrientation()
		{
			int buffer = CornerOrientationIndex;
			int[] ret = new int[8];
			for (int i = 0; i < 8; i++)
			{
				ret[i] = buffer % 3;
				buffer /= 3;
			}

			return ret;
		}


		//[TODO] Rework to fix inverse perm
		/// <summary>
		/// Places the stickers on <paramref name="c"/> such that they represent the same Cube
		/// </summary>
		public void ApplyToCube(StickerCube c)
		{
			c.Reset();

			int[] edgePerm = GetEdgePermutation();
			int[] edgeOrient = GetEdgeOrientation();
			int counter = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					//When swapping from main side (0, 1) to other sub sides (2, 3)
					int flip = (counter / 8) ^ (edgePerm[counter] / 8);

					c.SetEdgeColor(x, y, CubeEdgeColors[edgePerm[counter], edgeOrient[counter] ^ flip]);
					c.SetEdgeColor(y, x, CubeEdgeColors[edgePerm[counter], edgeOrient[counter] ^ 1 ^ flip]);

					counter++;
				}
			}

			int[] cornerPerm = GetCornerPermuation();
			int[] cornerOrient = GetCornerOrientation();
			counter = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						int dif = counter ^ cornerPerm[counter];
						bool flip = dif == 1 || dif == 2 || dif == 4 || dif == 7;

						//Flips non dominant colors when "corner has been moves twice"

						c.SetCornerColor(x, y, z, CornerColors[cornerPerm[counter], ((flip ? 2 : 1) * (0 + 2 * cornerOrient[counter])) % 3]);
						c.SetCornerColor(y, x, z, CornerColors[cornerPerm[counter], ((flip ? 2 : 1) * (1 + 2 * cornerOrient[counter])) % 3]);
						c.SetCornerColor(z, x, y, CornerColors[cornerPerm[counter], ((flip ? 2 : 1) * (2 + 2 * cornerOrient[counter])) % 3]);

						counter++;
					}
				}
			}
		}
		/// <summary>
		/// Creates a new <see cref="StickerCube"/> equal to this <see cref="IndexCube"/>
		/// </summary>
		public StickerCube GetCube()
		{
			StickerCube c = new StickerCube();
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

			uint[] array = NextEdgePermDif[side];
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

		//[TODO] Solve orientation issue, Unit tests (Cube1 * Cube2 = MoveSequenz1 + MoveSequenz2)
		/// <summary> 
		/// Uses <paramref name="cube"/> as a transformation such that every move to scramble <paramref name="cube"/> is applied on this <see cref="IndexCube"/>
		/// </summary>
		public void Transform(IndexCube cube)
		{
			int[] edgePerm = GetEdgePermutation();
			int[] edgeOrient = GetEdgeOrientation();

			int[] transformEdgePerm = cube.GetEdgePermutation();
			int[] transformEdgeOrient = cube.GetEdgeOrientation();


			int[] resultEdgePerm = new int[edgePerm.Length];
			int[] resultEdgeOrient = new int[edgePerm.Length];

			for (int i = 0; i < edgePerm.Length; i++)
			{
				resultEdgePerm[i] = edgePerm[transformEdgePerm[i]];
				resultEdgeOrient[resultEdgePerm[i]] = (edgeOrient[edgePerm[i]] + transformEdgeOrient[transformEdgePerm[i]]) % 2;
			}

			int[] cornerPerm = GetCornerPermuation();
			int[] cornerOrient = GetCornerOrientation();

			int[] transformCornerPerm = cube.GetCornerPermuation();
			int[] transformCornerOrient = cube.GetCornerOrientation();


			int[] resultCornerPerm = new int[cornerPerm.Length];
			int[] resultCornerOrient = new int[cornerPerm.Length];

			for (int i = 0; i < cornerPerm.Length; i++)
			{
				resultCornerPerm[i] = cornerPerm[transformCornerPerm[i]];
				resultCornerOrient[resultCornerPerm[i]] = (cornerOrient[cornerPerm[i]] + transformCornerOrient[transformCornerPerm[i]]) % 3;
			}
		}
		public void ApplyMoveSequenz(MoveSequenz ms)
		{
			for (int i = 0; i < ms.Count; i++)
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

		/// <returns>The orientation of the corner at position <paramref name="index"/></returns>
		public int GetSingleCornerOrientation(int index)
		{
			return (CornerOrientationIndex / Power(3, index)) % 3;

			int Power(int x, int y)
			{
				int ret = 1;
				for (int i = 0; i < y; i++)
					ret *= x;
				return ret;
			}
		}

		/// <returns>The orientation of the edge at position <paramref name="index"/></returns>
		public int GetSingleEdgeOrientation(int index)
		{
			return (EdgeOrientationIndex >> index) & 1;
		}
		#endregion

		#region Symmetry

		//[TODO] Tests
		/// <returns>A BitVector containing a 1 for each symmetry that applies to this cube</returns>
		public BitMap64 GetSymmetryMask()
		{
			if (Symmetry.EdgePermMask.ContainsKey(EdgePermutationIndex))
			{
				BitMap64 mask = Symmetry.EdgePermMask[EdgePermutationIndex];

				mask &= Symmetry.CornerPermMask[CornerPermutationIndex];
				mask &= Symmetry.EdgePermMask[EdgeOrientationIndex];

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
			int[] perm = GetCornerPermuation();
			int[] inversePerm = GetInverse(perm);
			int[] orient = GetCornerOrientation();

			byte[] symTransfrom = Symmetry.CornerPermTranform[se.Index];
			byte[,,] symCornerOrient = Symmetry.CornerOrientTransform[se.Index];

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
			int[] perm = GetEdgePermutation();
			int[] inversePerm = GetInverse(perm);
			int[] orient = GetEdgeOrientation();

			byte[] symTransfrom = Symmetry.EdgePermTranform[se.Index];
			byte[] symOrientation = Symmetry.EdgePermTranform[se.Index];

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

		public bool IsEqualWithSymmetry(IndexCube other, SymmetryElement se)
		{
			return IsEqualWithSymmetry(other, se.Index);
		}
		public bool IsEqualWithSymmetry(IndexCube other, int se)
		{
			int[] perm = GetCornerPermuation();
			int[] inversePerm = GetInverse(perm);
			int[] orient = GetCornerOrientation();

			int[] otherInversePerm = GetInverse(other.GetCornerPermuation());
			int[] otherOrient = other.GetCornerOrientation();

			byte[] symTransfrom = Symmetry.CornerPermTranform[se];
			byte[,,] symCornerOrient = Symmetry.CornerOrientTransform[se];

			for (int i = 0; i < 8; i++)
			{
				if (symTransfrom[inversePerm[i]] != otherInversePerm[symTransfrom[i]] ||
 					symCornerOrient[i, inversePerm[i], orient[inversePerm[i]]] != otherOrient[symTransfrom[inversePerm[i]]])
				{
					return false;
				}
			}

			//edges
			perm = GetEdgePermutation();
			inversePerm = GetInverse(perm);
			orient = GetEdgeOrientation();

			otherInversePerm = GetInverse(other.GetEdgePermutation());
			otherOrient = other.GetEdgeOrientation();

			symTransfrom = Symmetry.EdgePermTranform[se];
			byte[] symOrientation = Symmetry.EdgeOrientTransform[se];

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
		public bool IsEqualWithSymmetry(IndexCube other)
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
		#endregion
	
		//Sorting
		public static void RadixSortCubeIndices(IndexCube[] data, IndexCube[] CubeBuffer)
		{
			const int BIT_COUNT = 8;
			const int RADIX = 1 << BIT_COUNT;

			int[] indices = new int[RADIX];

			if (CubeBuffer.Length != data.Length)
			{
				CubeBuffer = new IndexCube[data.Length];
				Console.WriteLine("Reallocated");
			}

			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < RADIX; j++)
					indices[j] = 0;
				for (int j = 0; j < data.Length; j++)
					indices[data[j].GetByte(i)]++;

				for (int j = 1; j < RADIX; j++)
					indices[j] += indices[j - 1];
				for (int j = data.Length - 1; j >= 0; j--)
					CubeBuffer[--indices[data[j].GetByte(i)]] = data[j];

				IndexCube[] swap = CubeBuffer;
				CubeBuffer = data;
				data = swap;
			}
		}
		public static void RadixSortCubeIndices(List<IndexCube> data, List<IndexCube> CubeBuffer)
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
					indices[data[j].GetByte(i)]++;

				for (int j = 1; j < RADIX; j++)
					indices[j] += indices[j - 1];
				for (int j = N - 1; j >= 0; j--)
					CubeBuffer[--indices[data[j].GetByte(i)]] = data[j];

				var swap = CubeBuffer;
				CubeBuffer = data;
				data = swap;
			}
		}

		//Operator
		public static bool operator ==(IndexCube a, IndexCube b)
		{
			return a.EdgePermutationIndex == b.EdgePermutationIndex &&
				a.EdgeOrientationIndex == b.EdgeOrientationIndex &&
				a.CornerPermutationIndex == b.CornerPermutationIndex &&
				a.CornerOrientationIndex == b.CornerOrientationIndex;
		}
		public static bool operator !=(IndexCube a, IndexCube b)
		{
			return !(a == b);
		}
		public static bool operator <(IndexCube a, IndexCube b)
		{
			if (a.EdgePermutationIndex != b.EdgePermutationIndex) return a.EdgePermutationIndex < b.EdgePermutationIndex;
			if (a.CornerPermutationIndex != b.CornerPermutationIndex) return a.CornerPermutationIndex < b.CornerPermutationIndex;
			if (a.EdgeOrientationIndex != b.EdgeOrientationIndex) return a.EdgeOrientationIndex < b.EdgeOrientationIndex;

			return a.CornerOrientationIndex < b.CornerOrientationIndex;
		}
		public static bool operator >(IndexCube a, IndexCube b)
		{
			if (a.EdgePermutationIndex != b.EdgePermutationIndex) return a.EdgePermutationIndex > b.EdgePermutationIndex;
			if (a.CornerPermutationIndex != b.CornerPermutationIndex) return a.CornerPermutationIndex > b.CornerPermutationIndex;
			if (a.EdgeOrientationIndex != b.EdgeOrientationIndex) return a.EdgeOrientationIndex > b.EdgeOrientationIndex;

			return a.CornerOrientationIndex > b.CornerOrientationIndex;
		}

		public static IndexCube operator *(IndexCube a, IndexCube b)
		{
			a.Transform(b);

			return a;
		}

		//Overrides
		public override string ToString()
		{
			return Index.ToString("00 000 000 000 000 000 000");
		}
		public override bool Equals(object obj)
		{
			return obj is IndexCube other &&
					this == other;
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(EdgePermutationIndex, CornerPermutationIndex, EdgeOrientationIndex, CornerOrientationIndex);
		}

		public int CompareTo(IndexCube other)
		{
			if(this < other) return -1;
			if(this > other) return 1;

			return 0;
		}
	}
}