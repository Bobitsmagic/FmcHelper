using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CubeAD.IndexCubeSets
{
	/// <summary>
	/// A read-only hash-based set class for CubeIndex with all elements in consecutive memory,
	/// optimized for contains calls and space-efficiency
	/// </summary>
	public class SealedHashset
	{
		const int BUCKET_COUNT = (int)IndexCube.MAX_EDGE_PERMUTATION;
		
		public int DataSizeInBytes => Data.Length * IndexCube.SIZE_IN_BYTES + 4;

		public int Count => Data.Length;

		IndexCube[] Data;

		//Start-indices of buckets
		//+1 to store end of last bucket
		int[] StartIndex = new int[BUCKET_COUNT + 1];

		/// <summary>
		/// Creates a <see cref="SealedHashset"/> from an array of  <see cref="IndexCube"/>
		/// </summary>
		/// <param name="cubes">The array of <see cref="IndexCube"/> (not preserved)</param>
		public SealedHashset(IndexCube[] cubes)
		{
			Data = new IndexCube[cubes.Length];
			cubes.CopyTo(Data, 0);

			IndexCube.RadixSortCubeIndices(Data, cubes);

			//Count cubes for each bucket
			for (int i = 0; i < cubes.Length; i++)
				StartIndex[cubes[i].EdgePermutationIndex]++;

			//Calculate distribution metrics
			double Mean = (double)cubes.Length / BUCKET_COUNT;
			double Std = 0;
			for (int i = 0; i < StartIndex.Length; i++)
				Std += (StartIndex[i] - Mean) * (StartIndex[i] - Mean);

			Std /= BUCKET_COUNT;

			Console.WriteLine("Maximum bucket size: " + StartIndex.Max());
			Console.WriteLine("Minimum bucket size: " + StartIndex.Min());
			Console.WriteLine("Mean: " + Mean);
			Console.WriteLine("Std: " + Std);

			//Sum up the first i cubes (prefix sum)
			for (int i = 1; i < StartIndex.Length; i++)
				StartIndex[i] += StartIndex[i - 1];

			//Calculated start-indices 
			for (int i = StartIndex.Length - 1; i >= 1; i--)
				StartIndex[i] = StartIndex[i - 1];

			StartIndex[0] = 0;
		}

		/// <summary>
		/// Loads a <see cref="SealedHashset"/> from a memory dump file
		/// </summary>
		/// <param name="path">The path to the memory dump file</param>
		public SealedHashset(string path)
		{
			byte[] array = File.ReadAllBytes(path);
			Data = new IndexCube[array.Length / IndexCube.PADDED_SIZE_IN_BYTES];

			//Copy all bytes from array to Data
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

			//Count cubes for each bucket
			for (int i = 0; i < Data.Length; i++)
				StartIndex[Data[i].EdgePermutationIndex]++;
			//Sum up the first i cubes (prefix sum)
			for (int i = 1; i < StartIndex.Length; i++)
				StartIndex[i] += StartIndex[i - 1];
			//Calculated start-indices
			for (int i = StartIndex.Length - 1; i >= 1; i--)
				StartIndex[i] = StartIndex[i - 1];

			StartIndex[0] = 0;

			Console.WriteLine("Finished creating sealed HS");
		}

		/// <summary>
		/// Creates a memory dump file of the <see cref="SealedHashset"/>
		/// </summary>
		/// <param name="path">The path and the name of the file to create</param>
		public void SaveToFile(string path)
		{
			byte[] array = new byte[Data.Length * IndexCube.PADDED_SIZE_IN_BYTES];

			//Copy all bytes from Data to array
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

			File.WriteAllBytes(path, array);
		}

		public bool Contains(IndexCube index)
		{
			//Find start and end of corresbonding bucket
			int start = StartIndex[index.EdgePermutationIndex];
			int end = StartIndex[index.EdgePermutationIndex + 1];

			for (int i = start; i < end; i++)
			{
				if (Data[i] == index)
					return true;
			}

			return false;
		}

		public bool TryGetValue(IndexCube index, out IndexCube actual)
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

			actual = new IndexCube();
			return false;
		}

		/// <summary>
		/// Searches a cube in this tree/set
		/// </summary>
		/// <returns> If this cube is contained within the tree then a list of <see cref="CubeMove"/> that solves is this cube otherwise <see cref="null"/> </returns>
		public List<CubeMove> SearchSolutionInTree(IndexCube cube)
        {
			List<CubeMove> list = new List<CubeMove>();

			while (!cube.IsSovled)
			{
				if (TryGetValue(cube, out cube))
				{
					if (!cube.IsSovled)
					{
						list.Add(MoveSequenz.ReverseMove(cube.LastMove));
						cube.MakeMove(MoveSequenz.ReverseMove(cube.LastMove));
					}
				}
				else
				{
					Console.WriteLine("Cube not in set");
					return null;
				}
			}

			return list;
		}
	}
}
