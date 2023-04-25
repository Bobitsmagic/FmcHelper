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
    public struct IndexCube
    {
        public const int SIZE_IN_BYTES = 11;
        public const int PADDED_SIZE_IN_BYTES = 12;

        public const uint MAX_EDGE_PERMUTATION = 479_001_600;   //12!
        public const ushort MAX_CORNER_PERMUTATION = 40320;     //8!
        public const ushort MAX_EDGE_ORIENTATION = 4096;        //2^12
        public const ushort MAX_CORNER_ORIENTATION = 6561;      //3^8

        public bool IsSovled => EdgeOrientation == 0 && EdgePermation == 0 && CornerOrientation == 0 && CornerPermuation == 0;


        /// <summary>
        /// Generates a unique integer for every possible <see cref="IndexCube"/>
        /// </summary>
        public BigInteger Index
        {
            get
            {
                BigInteger ret = EdgePermation;
                ret *= MAX_CORNER_PERMUTATION;
                ret += CornerPermuation;
                ret *= MAX_EDGE_ORIENTATION;
                ret += EdgeOrientation;
                ret *= MAX_CORNER_ORIENTATION;
                ret += CornerOrientation;

                return ret;
            }
        }

        public uint EdgePermation;
        public ushort CornerPermuation;
        public ushort EdgeOrientation;
        public ushort CornerOrientation;
        public CubeMove LastMove;

        #region Constructors
        public IndexCube(uint edgePermutation, ushort cornerPermutation, ushort edgeOrientation, ushort cornerOrientation, CubeMove lastMove = CubeMove.None)
        {
#if DEBUG
            if (edgePermutation >= MAX_EDGE_PERMUTATION ||
                cornerPermutation >= MAX_CORNER_PERMUTATION ||
                edgeOrientation >= MAX_EDGE_ORIENTATION ||
                cornerOrientation >= MAX_CORNER_ORIENTATION)
                throw new ArgumentException("Indices must be smaller than their respected maximum value");
#endif
            EdgePermation = edgePermutation;
            CornerPermuation = cornerPermutation;
            EdgeOrientation = edgeOrientation;
            CornerOrientation = cornerOrientation;
            LastMove = lastMove;
        }
        public IndexCube(int edgePerm, int cornerPerm, int edgeOrient, int cornerOrient, CubeMove lastMove = CubeMove.None) : 
            this((uint)edgePerm, (ushort)cornerPerm, (ushort)edgeOrient, (ushort)cornerOrient)
        {

        }

        public IndexCube(Random rnd) : this(rnd.Next((int)MAX_EDGE_PERMUTATION), rnd.Next(MAX_CORNER_PERMUTATION), rnd.Next(MAX_EDGE_ORIENTATION), rnd.Next(MAX_CORNER_ORIENTATION))
        {
            
        }
    
        public void Reset()
        {
            EdgePermation = 0;
            EdgeOrientation = 0;
            CornerPermuation = 0;
            CornerOrientation = 0;
            LastMove = CubeMove.None;
        }

        public IndexCube(BinaryReader br)
        {
            EdgePermation = br.ReadUInt32();
            CornerPermuation = br.ReadUInt16();
            EdgeOrientation = br.ReadUInt16();
            CornerOrientation = br.ReadUInt16();
            LastMove = (CubeMove)br.ReadByte();
        }
#endregion

        public void Write(BinaryWriter bw)
        {
            bw.Write(EdgePermation);
            bw.Write(CornerPermuation);
            bw.Write(EdgeOrientation);
            bw.Write(CornerOrientation);
            bw.Write((byte)LastMove);
        }

        /// <returns>The byte at the corresbonding <paramref name="index"/></returns>
        public byte GetByte(int index)
        {
#if DEBUG
            if (index >= 10 || index < 0)
                throw new ArgumentOutOfRangeException("CubeIndex[" + index + "] out of range [0, 10)");
#endif
            //unsafe
            //{
            //    fixed(void* ptr = &this)
            //    {
            //        return ptr[index];
            //    }
            //}

            if (index < 2)
                return (byte)(CornerOrientation >> index * 8);

            if (index < 4)
                return (byte)(EdgeOrientation >> (index - 2) * 8);

            if (index < 6)
                return (byte)(CornerPermuation >> (index - 4) * 8);

            return (byte)(EdgePermation >> (index - 6) * 8);
        }

        /// <returns>An array representing the permutation of edges</returns>
        public int[] GetEdgePermutation()
        {
            return GetIndexedPerm(12, (int)EdgePermation);
        }
        /// <returns>An array representing the orientation of edges</returns>
        public int[] GetEdgeOrientation()
        {
            int[] buffer = new int[12];

            for (int i = 0; i < 12; i++)
            {
                buffer[i] = (EdgeOrientation >> (11 - i)) & 1;
            }

            return buffer;
        }
        /// <returns>An array representing the permutation of corner</returns>
        public int[] GetCornerPermuation()
        {
            return GetIndexedPerm(8, CornerPermuation);
        }
        /// <returns>An array representing the orientation of corners</returns>
        public int[] GetCornerOrientation()
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

		//Operator
		public static bool operator ==(IndexCube a, IndexCube b)
        {
            return a.EdgePermation == b.EdgePermation &&
                a.EdgeOrientation == b.EdgeOrientation &&
                a.CornerPermuation == b.CornerPermuation &&
                a.CornerOrientation == b.CornerOrientation;
        }
        public static bool operator !=(IndexCube a, IndexCube b)
        {
            return !(a == b);
        }
        
        
        //public override string ToString()
        //{
        //    return Index.ToString("00 000 000 000 000 000 000");
        //}
		public override string ToString()
		{
			return EdgePermation + " " + CornerPermuation + " " + EdgeOrientation + " " + CornerOrientation;
		}

		public override bool Equals(object obj)
        {
            return obj is IndexCube other &&
                    this == other;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(EdgePermation, CornerPermuation, EdgeOrientation, CornerOrientation);
        }
    }
}