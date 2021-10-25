using System;
using System.Collections.Generic;
using System.Text;

namespace CubeAD
{
	public class SortedCubeIndices
	{
		static List<CubeIndex> ListBuffer = new List<CubeIndex>();

		public int Count => Data.Count;

		List<CubeIndex> Data = new List<CubeIndex>();
		bool IsDirty = false;

		public void Add(CubeIndex element)
		{
			Data.Add(element);
			IsDirty = true;
		}

		public void RemoveDuplicates()
		{
			if(Data.Count > 1)
			{
				Data.Sort();

				ListBuffer.Capacity = Data.Count;

				CubeIndex current = Data[0];
				ListBuffer.Clear();
				ListBuffer.Add(current);
				foreach(CubeIndex cube in Data)
				{
					if(cube != current)
					{
						current = cube;
						ListBuffer.Add(current);
					}
				}

				List<CubeIndex> swap = ListBuffer;
				ListBuffer = Data;
				Data = swap;

			}

			IsDirty = false;
		}

		public bool Contains(CubeIndex cube)
		{
			if(IsDirty)
				Console.WriteLine("Alarm");
			return Data.BinarySearch(cube) >= 0;
		}

		public override string ToString()
		{
			return "Count: " + Count;
		}
	}
}
