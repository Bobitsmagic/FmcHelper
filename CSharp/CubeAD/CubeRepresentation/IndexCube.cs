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

namespace CubeAD.CubeRepresentation
{
    //Compressed representation of a cube
    public struct IndexCube : IComparable<IndexCube>
    {
        public const int SIZE_IN_BYTES = 11;
        public const int PADDED_SIZE_IN_BYTES = 12;

        public const uint MAX_EDGE_PERMUTATION = 479_001_600;   //12!
        public const ushort MAX_CORNER_PERMUTATION = 40320;     //8!
        public const ushort MAX_EDGE_ORIENTATION = 4096;        //2^12
        public const ushort MAX_CORNER_ORIENTATION = 6561;      //3^8

        public bool IsSovled => EdgeOrientationIndex == 0 && InverseEdgePermutationIndex == 0 && CornerOrientationIndex == 0 && CornerPermutationIndex == 0;


        /// <summary>
        /// Generates a unique integer for every possible <see cref="IndexCube"/>
        /// </summary>
        public BigInteger Index
        {
            get
            {
                BigInteger ret = InverseEdgePermutationIndex;
                ret *= MAX_CORNER_PERMUTATION;
                ret += CornerPermutationIndex;
                ret *= MAX_EDGE_ORIENTATION;
                ret += EdgeOrientationIndex;
                ret *= MAX_CORNER_ORIENTATION;
                ret += CornerOrientationIndex;

                return ret;
            }
        }

        public uint InverseEdgePermutationIndex;
        public ushort CornerPermutationIndex;
        public ushort EdgeOrientationIndex;
        public ushort CornerOrientationIndex;
        public CubeMove LastMove;

        #region Constructors
        public IndexCube(uint edgePermutation, ushort cornerPermutation, ushort edgeOrientation, ushort cornerOrientation, CubeMove lastMove = CubeMove.None)
        {
            if (edgePermutation >= MAX_EDGE_PERMUTATION ||
                cornerPermutation >= MAX_CORNER_PERMUTATION ||
                edgeOrientation >= MAX_EDGE_ORIENTATION ||
                cornerOrientation >= MAX_CORNER_ORIENTATION)
                throw new ArgumentException("Indices must be smaller than their respected maximum value");

            InverseEdgePermutationIndex = edgePermutation;
            CornerPermutationIndex = cornerPermutation;
            EdgeOrientationIndex = edgeOrientation;
            CornerOrientationIndex = cornerOrientation;
            LastMove = lastMove;
        }
        public IndexCube(Random rnd)
        {
            InverseEdgePermutationIndex = (uint)rnd.Next((int)MAX_EDGE_PERMUTATION);
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
            InverseEdgePermutationIndex = old.InverseEdgePermutationIndex;
            CornerPermutationIndex = old.CornerPermutationIndex;
            EdgeOrientationIndex = old.EdgeOrientationIndex;
            CornerOrientationIndex = old.CornerOrientationIndex;
            LastMove = CubeMove.None;

            MakeMove(m);
        }

        /// <summary>
        /// Resets this <see cref="IndexCube"/> to the solved state
        /// </summary>
        public void Reset()
        {
            InverseEdgePermutationIndex = 0;
            EdgeOrientationIndex = 0;
            CornerPermutationIndex = 0;
            CornerOrientationIndex = 0;
            LastMove = CubeMove.None;
        }

        /// <summary>
        /// Creates a solved <see cref="IndexCube"/> and applies a <see cref="MoveSequenz"/> afterwards
        /// </summary>
        /// <param name="ms"></param>
        public IndexCube(MoveSequenz ms)
        {
            InverseEdgePermutationIndex = 0;
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
            InverseEdgePermutationIndex = br.ReadUInt32();
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
            bw.Write(InverseEdgePermutationIndex);
            bw.Write(CornerPermutationIndex);
            bw.Write(EdgeOrientationIndex);
            bw.Write(CornerOrientationIndex);
            bw.Write((byte)LastMove);
        }

        public void CopyValuesFrom(IndexCube index)
        {
            InverseEdgePermutationIndex = index.InverseEdgePermutationIndex;
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
                return (byte)(CornerOrientationIndex >> index * 8);

            if (index < 4)
                return (byte)(EdgeOrientationIndex >> (index - 2) * 8);

            if (index < 6)
                return (byte)(CornerPermutationIndex >> (index - 4) * 8);

            return (byte)(InverseEdgePermutationIndex >> (index - 6) * 8);
        }

        /// <returns>An array representing the permutation of edges</returns>
        public int[] GetEdgePermutation()
        {
            return GetIndexedPerm(12, (int)InverseEdgePermutationIndex);
        }
        /// <returns>An array representing the orientation of edges</returns>
        public int[] GetEdgeOrientation()
        {
            int[] buffer = new int[12];

            for (int i = 0; i < 12; i++)
            {
                buffer[i] = EdgeOrientationIndex >> i & 1;
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
        public void MakeMove(CubeMove m)
        {
            int move = (int)m;

            CornerPermutationIndex = NextCornerPerm[CornerPermutationIndex, move];
            CornerOrientationIndex = NextCornerOrient[CornerOrientationIndex, move];
            EdgeOrientationIndex = NextEdgeOrient[EdgeOrientationIndex, move];

            int count = move % 3 + 1;
            int side = move / 3;

            uint[] array = NextEdgePermDif[side];
            int trackLength = EdgeTrackSizes[side];

            for (int i = 0; i < count; i++)
            {
                int index = (int)InverseEdgePermutationIndex / trackLength % array.Length;

                if ((int)InverseEdgePermutationIndex / trackLength / array.Length % 2 == 0)
                {
                    InverseEdgePermutationIndex = (InverseEdgePermutationIndex + array[index]) % MAX_EDGE_PERMUTATION;
                }
                else
                {
                    index = array.Length - index - 1;
                    InverseEdgePermutationIndex = (InverseEdgePermutationIndex + MAX_EDGE_PERMUTATION - array[index]) % MAX_EDGE_PERMUTATION;
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
            return CornerOrientationIndex / Power(3, index) % 3;

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
            return EdgeOrientationIndex >> index & 1;
        }
        #endregion

        #region Symmetry
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

            for (int i = 1; i < 48; i++)
            {
                if (IsEqualWithSymmetry(other, i))
                {
                    return true;
                }
            }

            return false;
        }

        //[TODO] Tests
        /// <returns>A BitVector containing a 1 for each symmetry that applies to this cube</returns>
        public BitMap64 GetSymmetryMask()
        {
            BitMap64 ret = new BitMap64();
            for (int i = 0; i < SymmetryElement.Elements.Length; i++)
            {
                ret[i] = HasSymmetry(SymmetryElement.Elements[i]);
            }

            //Identity sym
            return ret;
        }
        public bool HasSymmetry(SymmetryElement se)
        {
            return IsEqualWithSymmetry(this, se);
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
            return a.InverseEdgePermutationIndex == b.InverseEdgePermutationIndex &&
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
            if (a.InverseEdgePermutationIndex != b.InverseEdgePermutationIndex) return a.InverseEdgePermutationIndex < b.InverseEdgePermutationIndex;
            if (a.CornerPermutationIndex != b.CornerPermutationIndex) return a.CornerPermutationIndex < b.CornerPermutationIndex;
            if (a.EdgeOrientationIndex != b.EdgeOrientationIndex) return a.EdgeOrientationIndex < b.EdgeOrientationIndex;

            return a.CornerOrientationIndex < b.CornerOrientationIndex;
        }
        public static bool operator >(IndexCube a, IndexCube b)
        {
            if (a.InverseEdgePermutationIndex != b.InverseEdgePermutationIndex) return a.InverseEdgePermutationIndex > b.InverseEdgePermutationIndex;
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
            return HashCode.Combine(InverseEdgePermutationIndex, CornerPermutationIndex, EdgeOrientationIndex, CornerOrientationIndex);
        }

        public int CompareTo(IndexCube other)
        {
            if (this < other) return -1;
            if (this > other) return 1;

            return 0;
        }
    }
}