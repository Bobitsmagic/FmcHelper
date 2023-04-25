using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
		SortedListSet[] Data = new SortedListSet[IndexCube.MAX_CORNER_PERMUTATION];

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
				Data[i] = new SortedListSet(bucketCapacity);
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
			Data[cube.CornerPermuation].Add(new EdgeCornerOrientState(cube.EdgePermation, cube.EdgeOrientation, cube.CornerOrientation));
			IsDirty++;

			if(IsDirty > 100_000_000 || GC.GetTotalMemory(false) > 10_000_000_000)
			{
				long c = Count;
				RemoveDuplicates();
				
                Console.WriteLine("Removed duplicates: " + c + " -> " + Count + " " + (((double)c - Count) / c).ToString("0.0000"));
            }
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

		public void Clear()
		{
			for (int i = 0; i < Data.Length; i++)
			{
				Data[i].Clear();
			}

			IsDirty = 0;
		}

		/// <returns>An array containing all unique elements</returns>
		public EdgeCornerOrientState[] GetArray()
		{
			if(IsDirty > 0) 
				RemoveDuplicates();

			EdgeCornerOrientState[] cubeIndices = new EdgeCornerOrientState[Count];

			int counter = 0;
			for (int i = 0; i < Data.Length; i++)
			{
				List<EdgeCornerOrientState> list = Data[i].Data;

				for (int j = 0; j < list.Count; j++)
				{
					cubeIndices[counter++] = list[j];
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
	}
}
