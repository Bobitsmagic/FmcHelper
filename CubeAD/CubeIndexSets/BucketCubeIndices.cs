using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CubeAD.CubeIndexSets
{
	public class BucketCubeIndices
	{
		public int Count { get
			{
				int sum = 0;
				for(int i = 0; i < Data.Length; i++)
					sum += Data[i].Count;

				return sum;
			} }

		SortedCubeIndices[] Data = new SortedCubeIndices[CubeIndex.MAX_CORNER_PERMUTATION];
		
		bool IsDirty = false;

		public BucketCubeIndices(int bucketCapacity = 0)
		{
			for (int i = 0; i < Data.Length; i++)
			{
				Data[i] = new SortedCubeIndices(bucketCapacity);
			}
		}

		public void Add(CubeIndex element)
		{
			Data[element.CornerPermutation].Add(element);
			IsDirty = true;
		}

		public void RemoveDuplicates()
		{
			if (IsDirty)
			{
				for(int i = 0; i < Data.Length; i++)
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

		public void SaveDistribution(string path)
		{
			File.WriteAllLines(path, Data.Select(x => x.Count.ToString()));
		}

		public void SaveData(string path)
		{
			FileStream fs = File.Create(path);

			MemoryStream ms = new MemoryStream(Data[0].Count * CubeIndex.SIZE_IN_BYTES);
			BinaryWriter bw = new BinaryWriter(ms);

			foreach(var list in Data)
			{
				ms.Position = 0;
				list.Write(bw);
				fs.Write(ms.ToArray(), 0, list.Count * CubeIndex.SIZE_IN_BYTES + 4);
			}

			bw.Close();
			ms.Close();

			fs.Close();
		}

		public bool Contains(CubeIndex cube)
		{
			if (IsDirty)
				Console.WriteLine("Possible dirty call of contains");

			return Data[cube.CornerPermutation].Contains(cube);
		}

		public override string ToString()
		{
			if (IsDirty)
				throw new Exception("Count call on dirty list");

			return "Count: " + Count;
		}
	}
}
