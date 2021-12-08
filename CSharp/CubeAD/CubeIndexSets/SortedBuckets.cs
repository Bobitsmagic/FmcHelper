using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CubeAD.CubeIndexSets
{
	public class SortedBuckets
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

		SortedCubeIndexSet[] Data = new SortedCubeIndexSet[CubeIndex.MAX_CORNER_PERMUTATION];

		bool IsDirty = false;

		public SortedBuckets(int bucketCapacity = 0)
		{
			for (int i = 0; i < Data.Length; i++)
			{
				Data[i] = new SortedCubeIndexSet(bucketCapacity);
			}
		}

		public SortedBuckets(string path)
		{
			MemoryStream ms = new MemoryStream(File.ReadAllBytes(path));
			BinaryReader br = new BinaryReader(ms);

			for (int i = 0; i < Data.Length; i++)
			{
				Data[i] = new SortedCubeIndexSet(br);
			}

			Console.WriteLine("Done reading");
			RemoveDuplicates();
			Console.WriteLine("Done sorting");
		}

		public void Add(CubeIndex element)
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

		//public void RemoveDuplicatesParallel()
		//{
		//	if (IsDirty)
		//	{
		//		Parallel.ForEach(Data, x =>
		//		{
		//			x.RemoveDuplicates();
		//		});

		//		IsDirty = false;
		//	}
		//}

		public void Clear()
		{
			for (int i = 0; i < Data.Length; i++)
			{
				Data[i].Clear();
			}

			IsDirty = false;
		}

		public HashSet<CubeIndex> GetHashSet()
		{
			RemoveDuplicates();
			HashSet<CubeIndex> ret = new HashSet<CubeIndex>(Count);

			for (int i = 0; i < Data.Length; i++)
			{
				List<CubeIndex> list = Data[i].Data;

				for (int j = 0; j < list.Count; j++)
				{
					if (!ret.Add(list[j]))
					{
						Console.WriteLine("Alarm");
					}
				}
			}

			if (ret.Count != Count)
				Console.WriteLine("This should not happen kek");

			return ret;
		}
		public CubeIndex[] GetArray()
		{
			RemoveDuplicates();
			CubeIndex[] cubeIndices = new CubeIndex[Count];

			int counter = 0;
			for (int i = 0; i < Data.Length; i++)
			{
				List<CubeIndex> list = Data[i].Data;

				for (int j = 0; j < list.Count; j++)
				{
					cubeIndices[counter++] = list[j];
				}
			}

			return cubeIndices;
		}


		public SetBuckets GetSetBuckets()
		{
			RemoveDuplicates();
			SetBuckets ret = new SetBuckets(Count / Data.Length);

			for (int i = 0; i < Data.Length; i++)
			{
				List<CubeIndex> list = Data[i].Data;

				for (int j = 0; j < list.Count; j++)
				{
					if (!ret.Add(list[j]))
					{
						Console.WriteLine("Alarm");
					}
				}
			}

			if (ret.Count != Count)
				Console.WriteLine("This should not happen kek");

			return ret;
		}

		public void SaveDistribution(string path)
		{
			File.WriteAllLines(path, Data.Select(x => x.Count.ToString()));
		}

		public void SaveData(string path)
		{
			RemoveDuplicates();

			FileStream fs = File.Create(path);

			MemoryStream ms = new MemoryStream(Data[0].Count * CubeIndex.SIZE_IN_BYTES);
			BinaryWriter bw = new BinaryWriter(ms);

			foreach (var list in Data)
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

			return Data[cube.CornerPermutationIndex].Contains(cube);
		}

		public override string ToString()
		{
			if (IsDirty)
				throw new Exception("Count call on dirty list");

			return "Count: " + Count;
		}
	}
}
