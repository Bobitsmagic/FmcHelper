using System;
using System.Collections.Generic;
using System.IO;

namespace CubeAD.CubeIndexSets
{
	/// <summary>
	/// An add-only set class for <see cref="CubeIndex"/>, optimized for add operations and space efficiency
	/// </summary>
	public class SortedCubeIndexSet
	{
		public static List<CubeIndex> CubeIndexBuffer = new List<CubeIndex>();

		public int Count => Data.Count;

		public List<CubeIndex> Data;

		//Flag whether this instance can contain duplicates
		bool IsDirty = false;

		public SortedCubeIndexSet(int capacity = 0)
		{
			Data = new List<CubeIndex>(capacity);
		}

		public void Add(CubeIndex element)
		{
			Data.Add(element);
			IsDirty = true;
		}

		//Sorts the CubeIndexBuffer and removes all duplicates afterwards
		public void RemoveDuplicates()
		{
			if (Data.Count > 1 && IsDirty)
			{
				CubeIndex.RadixSortCubeIndices(Data, CubeIndexBuffer);


				//Copy all unique elements at the first duplicate and count duplicates
				CubeIndex current = Data[0];
				int deleteCount = 0;
				for (int i = 1; i < Data.Count; i++)
				{
					CubeIndex cube = Data[i];
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
		public bool Contains(CubeIndex cube)
		{
			if (IsDirty)
				throw new Exception("Contains call on dirty list");

			return Data.BinarySearch(cube) >= 0;
		}
	}
}
