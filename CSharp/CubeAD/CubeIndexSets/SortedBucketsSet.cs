using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CubeAD.IndexCubeSets
{
	/// <summary>
	/// An add-only set class for <see cref="IndexCube"/>, optimized for add operations and space efficiency
	/// </summary>
	public class SortedBucketsSet
	{
		public int Count
		{
			get
			{
				int sum = 0;
				for (int i = 0; i < Data.Length; i++)
					sum += Data[i].Count;

				return sum;
			}
		}

		//Each bucket is a SortedCubeIndexSet
		//All buckets are indexed by the corner permutation index
		SortedListSet[] Data = new SortedListSet[IndexCube.MAX_CORNER_PERMUTATION];

		//Flag whether this instance can contain duplicates
		bool IsDirty = false;

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

		public void Add(IndexCube element)
		{
			Data[element.CornerPermutationIndex].Add(element);
			IsDirty = true;
		}

		public void RemoveDuplicates()
		{
			if (IsDirty)
			{
				for (int i = 0; i < Data.Length; i++)
				{
					Data[i].RemoveDuplicates();
				}

				IsDirty = false;
			}
		}

		public void Clear()
		{
			for (int i = 0; i < Data.Length; i++)
			{
				Data[i].Clear();
			}

			IsDirty = false;
		}

		/// <returns>An array containing all unique elements</returns>
		public IndexCube[] GetArray()
		{
			if(IsDirty) 
				RemoveDuplicates();

			IndexCube[] cubeIndices = new IndexCube[Count];

			int counter = 0;
			for (int i = 0; i < Data.Length; i++)
			{
				List<IndexCube> list = Data[i].Data;

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
			if (IsDirty)
				Console.WriteLine("Possible dirty call of contains");

			return Data[cube.CornerPermutationIndex].Contains(cube);
		}
	}
}
