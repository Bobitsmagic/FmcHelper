using System;
using System.IO;
using System.Linq;

namespace CubeAD.CubeIndexSets
{
	public class SealedHashset
	{
		const int LENGTH = (int)CubeIndex.MAX_EDGE_PERMUTATION;
		public int DataSizeInBytes => Data.Length * CubeIndex.SIZE_IN_BYTES + 4;
		public int Count => Data.Length;

		CubeIndex[] Data;
		int[] StartIndex = new int[LENGTH + 1];

		//Input array is not preserved!
		public SealedHashset(CubeIndex[] cubes)
		{
			Data = new CubeIndex[cubes.Length];
			cubes.CopyTo(Data, 0);

			CubeIndex.RadixSortCubeIndices(Data, cubes);

			//count cubes
			for (int i = 0; i < cubes.Length; i++)
				StartIndex[cubes[i].EdgePermutationIndex]++;

			double Mean = (double)cubes.Length / LENGTH;
			double Std = 0;
			for (int i = 0; i < StartIndex.Length; i++)
				Std += (StartIndex[i] - Mean) * (StartIndex[i] - Mean);

			Std /= LENGTH;

			Console.WriteLine("Maximum bucket size: " + StartIndex.Max());
			Console.WriteLine("Minimum bucket size: " + StartIndex.Min());
			Console.WriteLine("Mean: " + Mean);
			Console.WriteLine("Std: " + Std);

			//Sum up the first i cubes
			for (int i = 1; i < StartIndex.Length; i++)
				StartIndex[i] += StartIndex[i - 1];

			//calculated indices
			for (int i = StartIndex.Length - 1; i >= 1; i--)
				StartIndex[i] = StartIndex[i - 1];

			StartIndex[0] = 0;
		}

		public SealedHashset(string file)
		{
			byte[] array = File.ReadAllBytes(file);
			Data = new CubeIndex[array.Length / CubeIndex.PADDED_SIZE_IN_BYTES];
			unsafe
			{
				fixed (void* source = array)
				{
					fixed (void* dest = Data)
					{
						Buffer.MemoryCopy(source, dest, array.Length, array.Length);
					}
				}
			}
			
			Console.WriteLine("Done reading " + Data.Length + " cubes");

			//count cubes
			for (int i = 0; i < Data.Length; i++)
				StartIndex[Data[i].EdgePermutationIndex]++;

			//Sum up the first i cubes
			for (int i = 1; i < StartIndex.Length; i++)
				StartIndex[i] += StartIndex[i - 1];
			//calculated indices
			for (int i = StartIndex.Length - 1; i >= 1; i--)
				StartIndex[i] = StartIndex[i - 1];

			StartIndex[0] = 0;

			Console.WriteLine("Finished creating sealed HS");
		}

		public void SaveToFile(string file)
		{
			byte[] array = new byte[Data.Length * CubeIndex.PADDED_SIZE_IN_BYTES];
			unsafe
			{
				fixed (void* source = Data)
				{
					fixed (void* dest = array)
					{
						Buffer.MemoryCopy(source, dest, array.Length, array.Length);
					}
				}
			}

			File.WriteAllBytes(file, array);
		}

		public byte[] ToByteArray()
		{
			byte[] array = new byte[Data.Length * CubeIndex.SIZE_IN_BYTES];
			MemoryStream ms = new MemoryStream(array);
			BinaryWriter bw = new BinaryWriter(ms);

			for (int i = 0; i < Data.Length; i++)
			{
				Data[i].Write(bw);
			}

			return array;
		}

		public byte[] ToByteArrayUnsafe()
		{
			byte[] array = new byte[Data.Length * CubeIndex.PADDED_SIZE_IN_BYTES];
			unsafe
			{
				fixed (void* source = Data)
				{
					fixed (void* dest = array)
					{
						Buffer.MemoryCopy(source, dest, array.Length, array.Length);
					}
				}
			}

			return array;
		}

		public bool Contains(CubeIndex index)
		{
			int start = StartIndex[index.EdgePermutationIndex];
			int end = StartIndex[index.EdgePermutationIndex + 1];

			for (int i = start; i < end; i++)
			{
				if (Data[i] == index)
					return true;
			}

			return false;
		}

		public bool TryGetValue(CubeIndex index, out CubeIndex actual)
		{
			int start = StartIndex[index.EdgePermutationIndex];
			int end = StartIndex[index.EdgePermutationIndex + 1];

			for (int i = start; i < end; i++)
			{
				if (Data[i] == index)
				{
					actual = Data[i];
					return true;
				}
			}

			actual = new CubeIndex();
			return false;
		}
	}
}
