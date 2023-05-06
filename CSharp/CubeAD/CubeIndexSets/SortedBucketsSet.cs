using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Xml.Linq;
using CubeAD.CubeIndexSets;
using CubeAD.CubeRepresentation;
using CubeAD.IndexCubeSets;

namespace CubeAD.CubeIndexSets
{
    /// <summary>
    /// An add-only set class for <see cref="EdgeCornerOrientState"/>, optimized for add operations and space efficiency
    /// </summary>
    public class SortedBucketsSet
	{
		public long Count
		{
			get
			{
				long sum = 0;
				for (int i = 0; i < Data.Length; i++)
					sum += Data[i].Count;

				return sum;
			}
		}

		//Each bucket is a SortedCubeIndexSet
		//All buckets are indexed by the corner permutation index
		SortedEdgeCornerOrientStateList[] Data = new SortedEdgeCornerOrientStateList[IndexCube.MAX_CORNER_PERMUTATION];

		//Flag whether this instance can contain duplicates
		ulong IsDirty = 0;

		/// <summary>
		/// Creates a new <see cref="SortedBucketsSet"/>.
		/// </summary>
		/// <param name="bucketCapacity">The capcity of each bucket</param>
		public SortedBucketsSet(int bucketCapacity = 0)
		{
			for (int i = 0; i < Data.Length; i++)
			{
				Data[i] = new SortedEdgeCornerOrientStateList(bucketCapacity);
			}
		}
		
		public void Foreach(Action<IndexCube> action)
		{
			Stopwatch sw = Stopwatch.StartNew();

			int[] lengths = new int[Data.Length];

			for(int i = 0; i < Data.Length; i++)
				lengths[i] = Data[i].Count;


			long preCount = Count;
			long stepSize = preCount / 100;
			long stepCount = 0;
			long sum = 0;
			for(int b = 0; b < Data.Length; b++)
			{
				for(int i = 0; i < lengths[b]; i++)
				{
					EdgeCornerOrientState state = Data[b].Data[i];
					action(new IndexCube(state.EdgePermIndex, (ushort)b, state.EdgeOrientIndex, state.CornerOrientIndex));
				}

				stepCount  += lengths[b];
				sum += lengths[b];

				if(stepCount  > stepSize && preCount > 30_000_000)
				{
					stepCount -= stepSize;

                    Console.WriteLine("Completion: " + ((double)sum / preCount).ToString("0.000") + " time: " + sw.ElapsedMilliseconds.ToString("000 000 000"));
                }
			}
		}

		public void Add(IndexCube cube)
		{
			Data[cube.CornerPermuation].Add(new EdgeCornerOrientState(cube.EdgePermation, cube.EdgeOrientation, cube.CornerOrientation, cube.LastMove));
			IsDirty++;

			//if(IsDirty > 100_000_000)
			//{
			//	long c = Count;
			//	RemoveDuplicates();
				
   //             Console.WriteLine("Removed duplicates: " + c + " -> " + Count + " " + (((double)c - Count) / c).ToString("0.0000"));
   //         }
		}

		public void RemoveDuplicates()
		{
			if (IsDirty > 0)
			{
				for (int i = 0; i < Data.Length; i++)
				{
					Data[i].RemoveDuplicates();
				}

				IsDirty = 0;
			}
		}

		public void PrintStats()
		{
			int min = int.MaxValue;
			int max = 0;

			for(int i = 0; i < Data.Length; i++)
			{
				min = Math.Min(Data[i].Count, min);
				max = Math.Max(Data[i].Count, max);
			}

            Console.WriteLine("Min: " + min + " max: " + max + " avg: " + ((double)Count / Data.Length));
        }

		public void Clear()
		{
			for (int i = 0; i < Data.Length; i++)
			{
				Data[i].Clear();
			}

			IsDirty = 0;
		}

		public IndexCube[] GetArray()
		{
			if(IsDirty > 0) 
				RemoveDuplicates();

			IndexCube[] cubeIndices = new IndexCube[Count];

			int counter = 0;
			for (int i = 0; i < Data.Length; i++)
			{
				List<EdgeCornerOrientState> list = Data[i].Data;

				foreach (EdgeCornerOrientState state in list)
				{

					cubeIndices[counter++] = new IndexCube(state.EdgePermIndex, (ushort)i, state.EdgeOrientIndex, state.CornerOrientIndex, state.LastMove);						
				}
			}

			return cubeIndices;
		}

		//For testing purposes
		public bool Contains(IndexCube cube)
		{

#if DEBUG
			if (IsDirty > 0)
				Console.WriteLine("Possible dirty call of contains");
#endif

			return Data[cube.CornerPermuation].Contains(new EdgeCornerOrientState(cube.EdgePermation, cube.EdgeOrientation, cube.CornerOrientation));
		}

		class SortedEdgeCornerOrientStateList
		{
			public int Count => Data.Count;

			public List<EdgeCornerOrientState> Data;

			//Flag whether this instance can contain duplicates
			bool IsDirty = false;

			public SortedEdgeCornerOrientStateList(int capacity = 0)
			{
				Data = new List<EdgeCornerOrientState>(capacity);
			}

			public void Add(EdgeCornerOrientState element)
			{
				Data.Add(element);
				IsDirty = true;
			}

			//Sorts the CubeIndexBuffer and removes all duplicates afterwards
			public void RemoveDuplicates()
			{
				if (Data.Count > 1 && IsDirty)
				{
					//IndexCube.RadixSortCubeIndices(Data, CubeIndexBuffer);

					Data.Sort();

					//Copy all unique elements at the first duplicate and count duplicates
					EdgeCornerOrientState current = Data[0];
					int deleteCount = 0;
					for (int i = 1; i < Data.Count; i++)
					{
						EdgeCornerOrientState cube = Data[i];
						if (cube != current)
						{
							current = cube;
							Data[i - deleteCount] = cube;
						}
						else
						{
							deleteCount++;
						}
					}

					//Remove all duplicates at the end of the list
					Data.RemoveRange(Data.Count - deleteCount, deleteCount);
				}

				IsDirty = false;
			}

			public void Clear()
			{
				Data.Clear();
				Data.Capacity = 0;
				IsDirty = false;
			}

			//For testing purposes
			public bool Contains(EdgeCornerOrientState cube)
			{
#if DEBUG
			if (IsDirty)
				throw new Exception("Contains call on dirty list");
#endif

				return Data.BinarySearch(cube) >= 0;
			}
		}

		//No corner perm
		public readonly struct EdgeCornerOrientState : IComparable<EdgeCornerOrientState>
		{
			public const int PADDED_SIZE_IN_BYTES = 8;

			public readonly uint EdgePermIndex;
			public readonly ushort EdgeOrientIndex;
			public readonly ushort CornerOrientIndex;
			public readonly CubeMove LastMove;

			public EdgeCornerOrientState(uint egdePermIndex, ushort edgeOrientIndex, ushort cornerOrientIndex, CubeMove lastMove = CubeMove.None)
			{
				EdgePermIndex = egdePermIndex;
				EdgeOrientIndex = edgeOrientIndex;
				CornerOrientIndex = cornerOrientIndex;
				LastMove = lastMove;
			}

			public static bool operator ==(EdgeCornerOrientState left, EdgeCornerOrientState right)
			{
				return left.EdgePermIndex == right.EdgePermIndex &&
					left.EdgeOrientIndex == right.EdgeOrientIndex &&
					left.CornerOrientIndex == right.CornerOrientIndex;
			}
			public static bool operator !=(EdgeCornerOrientState left, EdgeCornerOrientState right)
			{
				return !(left == right);
			}

			public int CompareTo(EdgeCornerOrientState other)
			{
				if (EdgePermIndex != other.EdgePermIndex)
					return EdgePermIndex.CompareTo(other.EdgePermIndex);

				if (EdgeOrientIndex != other.EdgeOrientIndex)
					return EdgeOrientIndex.CompareTo(other.EdgeOrientIndex);

				return CornerOrientIndex.CompareTo(other.CornerOrientIndex);
			}
		}
	}
}
