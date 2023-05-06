using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using CubeAD.CubeIndexSets;
using CubeAD.CubeRepresentation;

namespace CubeAD.IndexCubeSets
{
    /// <summary>
    /// A read-only hash-based set class for CubeIndex with all elements in consecutive memory,
    /// optimized for contains calls and space-efficiency
    /// </summary>
    public class SealedHashset
	{
		const int BUCKET_COUNT = (int)IndexCube.MAX_EDGE_PERMUTATION;
		
		public int DataSizeInBytes => Data.Length * CornerEdgeOrientState.PADDED_SIZE_IN_BYTES;

		public int Count => Data.Length;

		CornerEdgeOrientState[] Data;

		//Start-indices of buckets
		//+1 to store end of last bucket
		int[] StartIndex = new int[BUCKET_COUNT + 1];

		public SealedHashset(IndexCube[] cubes)
		{
			Array.Sort(cubes);

			//Count cubes for each bucket
			for (int i = 0; i < cubes.Length; i++)
				StartIndex[cubes[i].EdgePermation]++;

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

			Data = new CornerEdgeOrientState[cubes.Length];
			for(int i = 0; i < cubes.Length; i++)
			{
				Data[i] = new CornerEdgeOrientState(cubes[i]);
			}
		}

		public SealedHashset(string dataPath, string indexPath)
		{
			byte[] array = File.ReadAllBytes(dataPath);

			Data = new CornerEdgeOrientState[array.Length / CornerEdgeOrientState.PADDED_SIZE_IN_BYTES];

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

			Buffer.BlockCopy(File.ReadAllBytes(indexPath), 0, StartIndex, 0, Buffer.ByteLength(StartIndex));

			Console.WriteLine("Finished creating sealed HS");
		}

		public void SaveToFile(string dataPath, string indexPath)
		{
			byte[] array = new byte[(long)Data.Length * CornerEdgeOrientState.PADDED_SIZE_IN_BYTES];

			//Copy all bytes from Data to array
			unsafe
			{
				fixed (void* source = Data)
				{
					fixed (void* dest = array)
					{
						Buffer.MemoryCopy(source, dest, array.LongLength, array.LongLength);
					}
				}
			}

			File.WriteAllBytes(dataPath, array);

			array = new byte[Buffer.ByteLength(StartIndex)];
			Buffer.BlockCopy(StartIndex, 0, array, 0, array.Length);
			File.WriteAllBytes(indexPath, array);
		}

		public bool Contains(IndexCube index)
		{
			//Find start and end of corresbonding bucket
			int start = StartIndex[index.EdgePermation];
			int end = StartIndex[index.EdgePermation + 1];
			CornerEdgeOrientState search = new CornerEdgeOrientState(index);

			for (int i = start; i < end; i++)
			{
				if (Data[i] == search)
					return true;
			}

			return false;
		}

		public bool TryGetValue(IndexCube index, out IndexCube actual)
		{
			int start = StartIndex[index.EdgePermation];
			int end = StartIndex[index.EdgePermation + 1];
			CornerEdgeOrientState search = new CornerEdgeOrientState(index);

			for (int i = start; i < end; i++)
			{
				if (Data[i] == search)
				{
					actual = index;
					actual.LastMove = Data[i].LastMove;
					return true;
				}
			}

			actual = new IndexCube();
			return false;
		}

		readonly struct CornerEdgeOrientState
		{
			public const int PADDED_SIZE_IN_BYTES = 8;

			public readonly ushort EdgeOrientIndex;
			public readonly ushort CornerPermIndex;
			public readonly ushort CornerOrientIndex;
			public readonly CubeMove LastMove;

			public CornerEdgeOrientState(ushort edgeOrientIndex, ushort cornerPermIndex, ushort cornerOrientIndex, CubeMove lastMove)
			{
				EdgeOrientIndex = edgeOrientIndex;
				CornerPermIndex = cornerPermIndex;
				CornerOrientIndex = cornerOrientIndex;
				LastMove = lastMove;
			}

			public CornerEdgeOrientState(IndexCube src) : this(src.EdgeOrientation, src.CornerPermuation, src.CornerOrientation, src.LastMove)
			{

			}

			public static bool operator ==(CornerEdgeOrientState left, CornerEdgeOrientState right)
			{
				return left.EdgeOrientIndex == right.EdgeOrientIndex && 
					left.CornerPermIndex == right.CornerPermIndex &&
					left.CornerOrientIndex == right.CornerOrientIndex;
			}
			public static bool operator !=(CornerEdgeOrientState left, CornerEdgeOrientState right)
			{
				return !(left == right);
			}
		}
	}
}
