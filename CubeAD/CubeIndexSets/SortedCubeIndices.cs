using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CubeAD.CubeIndexSets
{
	public class SortedCubeIndices
	{
		public int Count => Data.Count;

		public List<CubeIndex> Data;
		bool IsDirty = false;

		public SortedCubeIndices(int capacity = 0)
		{
			Data = new List<CubeIndex>(capacity);
		}

		public SortedCubeIndices(BinaryReader br)
		{
			int length = br.ReadInt32();
			Data = new List<CubeIndex>(length);

			for(int i = 0; i < Data.Capacity; i++)
			{
				Data.Add(new CubeIndex(br));
			}
		}

		public void Add(CubeIndex element)
		{
			Data.Add(element);
			IsDirty = true;
		}

		public void RemoveDuplicates()
		{
			if (Data.Count > 1 && IsDirty)
			{
				Data.Sort();
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

				Data.RemoveRange(Data.Count - deleteCount, deleteCount);
			}

			IsDirty = false;
		}

		public void Write(BinaryWriter bw)
		{
			bw.Write(Data.Count);
			for (int i = 0; i < Data.Count; i++)
			{
				Data[i].Write(bw);
			}
		}
		public void Clear()
		{
			Data.Clear();
			Data.Capacity = 0;
			IsDirty = false;
		}

		public bool Contains(CubeIndex cube)
		{
			if (IsDirty)
				throw new Exception("Contains call on dirty list");
			return Data.BinarySearch(cube) >= 0;
		}

		public override string ToString()
		{
			if (IsDirty)
				throw new Exception("Count call on dirty list");

			return "Count: " + Count;
		}
	}
}
