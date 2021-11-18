using CubeAD.CubeIndexSets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static CubeAD.CubeIndex;
using static CubeAD.SearchingAlgorithms;
using static CubeAD.Permutation;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace CubeAD
{
	public static class PreCompTables
	{	
		public const int MAX_EO_MOVE_DEPTH = 8;
		public const int MAX_MOVE_DEPTH = 7;

		public static readonly string PRE_COMP_PATH = Path.Combine(Directory.GetCurrentDirectory(), "PreCompFiles");
		public const string MOVE_TREE_PREFIX = "solved_tree_";

		public static int[,] EdgeIndices = new int[6, 6];
		public static int[,,] CornerIndices = new int[6, 6, 6];

		public static int[,] CubeEdgeColors = new int[12, 2]
		{
			{0, 2},
			{0, 3},
			{0, 4},
			{0, 5},

			{1, 2},
			{1, 3},
			{1, 4},
			{1, 5},

			{2, 4 },
			{2, 5 },
			{3, 4 },
			{3, 5 }
		};
		public static int[,] CornerColors = new int[8, 3]
		{
			{ 0, 2, 4 },
			{ 0, 2, 5 },
			{ 0, 3, 4 },
			{ 0, 3, 5 },

			{ 1, 2, 4 },
			{ 1, 2, 5 },
			{ 1, 3, 4 },
			{ 1, 3, 5 }
		};

		public static int[] EdgeTrackSizes = new int[6] { 40320, 24, 2, 1, 1, 1 };
		public static int[] NextEdgeSizes = new int[6] { 5940, 840, 119750400, 19958400, 1814400, 181440 }; //[TODO] fill in right numbers
		public static uint[][] NextEdgePerm = new uint[6][]
		{
			new uint[NextEdgeSizes[0]],
			new uint[NextEdgeSizes[1]],
			new uint[NextEdgeSizes[2]],
			new uint[NextEdgeSizes[3]],
			new uint[NextEdgeSizes[4]],
			new uint[NextEdgeSizes[5]]
		};

		public static ushort[,] NextCornerPerm = new ushort[MAX_CORNER_PERMUTATION, 18];
		public static ushort[,] NextEdgeOrient = new ushort[MAX_EDGE_ORIENTATION, 18];
		public static ushort[,] NextCornerOrient = new ushort[MAX_CORNER_ORIENTATION, 18];

		public static byte[] OrientedEdgesCount = new byte[MAX_EDGE_ORIENTATION];
		public static byte[] OrientedCornersCount = new byte[MAX_CORNER_ORIENTATION];
		public static byte[] PositionedEdgesCount = new byte[MAX_EDGE_PERMUTATION];
		public static byte[] PositionedCornersCount = new byte[MAX_CORNER_PERMUTATION];

		private static SealedHashset _EOMoveTree = null;
		private static SealedHashset _MoveTree = null;
		public static SealedHashset GetEdgeOrientedMoveTree
		{
			get
			{
				if (_EOMoveTree is null)
				{
					string path = Path.Combine(Directory.GetCurrentDirectory(), "PreCompFiles", "solved_eo_tree_" + MAX_EO_MOVE_DEPTH + ".bin");

					if (File.Exists(path))
					{
						_EOMoveTree = new SealedHashset(path);
					}
					else
					{
						_EOMoveTree = new SealedHashset(SolvedTreeOrientedEdges(MAX_EO_MOVE_DEPTH).GetArray());
						_EOMoveTree.SaveToFile(path);
						Console.WriteLine("Created: " + path);
					}
				}

				return _EOMoveTree;
			}
		}
		public static SealedHashset GetMoveTree
		{
			get
			{
				if (_MoveTree is null)
				{
					string path = Path.Combine(Directory.GetCurrentDirectory(), "PreCompFiles", "solved_tree_" + MAX_MOVE_DEPTH + ".bin");

					if (File.Exists(path))
					{
						_MoveTree = new SealedHashset(path);
					}
					else
					{
						_MoveTree = new SealedHashset(GenerateSolvedTree(MAX_MOVE_DEPTH).GetArray());
						_MoveTree.SaveToFile(path);
						Console.WriteLine("Created: " + path);
					}
				}

				return _MoveTree;
			}
		}

		static PreCompTables()
		{
			Stopwatch sw = Stopwatch.StartNew();

			int counter = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					EdgeIndices[y, x] = counter;
					EdgeIndices[x, y] = counter++;
				}
			}

			counter = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						CornerIndices[x, y, z] = counter;
						CornerIndices[x, z, y] = counter;

						CornerIndices[y, x, z] = counter;
						CornerIndices[y, z, x] = counter;

						CornerIndices[z, x, y] = counter;
						CornerIndices[z, y, x] = counter++;
					}
				}
			}

			if (!Directory.Exists(PRE_COMP_PATH))
			{
				Directory.CreateDirectory(PRE_COMP_PATH);
				Console.WriteLine("Created directory: " + PRE_COMP_PATH);
			}

			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "next_corner_perm.bin"), NextCornerPerm, (ret) =>
			{
				Cube c = new Cube();
				Cube buffer = new Cube();
				CubeIndex index = new CubeIndex();
				int[] permBuffer = new int[8];

				ushort[,] cast = (ushort[,])ret;
				for (int i = 0; i < MAX_CORNER_PERMUTATION; i++)
				{
					index.CornerPermutation = (ushort)i;
					index.ApplyToCube(c);

					for (int j = 0; j < 18; j++)
					{
						buffer.CopyValuesFrom(c);
						buffer.MakeMove((CubeMove)j);
						cast[i, j] = FindCornerPermutationIndex(buffer, permBuffer);
					}
				}
			});

			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "next_corner_orient.bin"), NextCornerOrient, (ret) =>
			{
				Cube c = new Cube();
				Cube buffer = new Cube();
				CubeIndex index = new CubeIndex();
				ushort[,] cast = (ushort[,])ret;
				for (int i = 0; i < MAX_CORNER_ORIENTATION; i++)
				{
					index.CornerOrientation = (ushort)i;
					index.ApplyToCube(c);

					for (int j = 0; j < 18; j++)
					{
						buffer.CopyValuesFrom(c);
						buffer.MakeMove((CubeMove)j);

						cast[i, j] = FindCornerOrientationIndex(buffer);
					}
				}
			});

			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "next_edge_orient.bin"), NextEdgeOrient, (ret) =>
			{

				Cube c = new Cube();
				Cube buffer = new Cube();
				CubeIndex index = new CubeIndex();
				ushort[,] cast = (ushort[,])ret;
				for (int i = 0; i < MAX_EDGE_ORIENTATION; i++)
				{
					index.EdgeOrientation = (ushort)i;
					index.ApplyToCube(c);

					for (int j = 0; j < 18; j++)
					{
						buffer.CopyValuesFrom(c);
						buffer.MakeMove((CubeMove)j);
						cast[i, j] = FindEdgeOrientationIndex(buffer);
					}
				}
			});

			//Egde permutation
			for (int i = 0; i < 18; i += 3)
			{
				CubeMove m = (CubeMove)i;
				string path = Path.Combine(PRE_COMP_PATH, "next_edge_perm_" + m.ToString() + ".bin");

				ReadFromFileOrCreate(path, NextEdgePerm[i / 3], (ret) => {
					Array gen = GenerateShortendEdgePermArrays(m);
					Console.WriteLine("Gen: " + gen.Length + " ret: " + ret.Length);
					Array.Copy(gen, ret, gen.Length);
				});
			}

			//Oriented edges count
			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "oriented_edges_count.bin"), OrientedEdgesCount, (ret) =>
			{
				byte[] cast = ret as byte[];
				for (int i = 0; i < MAX_EDGE_ORIENTATION; i++)
					cast[i] = (byte)NumberOfSetBits(i);

				int NumberOfSetBits(int value)
				{
					value = value - ((value >> 1) & 0x55555555);
					value = (value & 0x33333333) + ((value >> 2) & 0x33333333);
					return (((value + (value >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
				}
			});

			//Oriented corners count
			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "oriented_corners_count.bin"), OrientedCornersCount, (ret) =>
			{
				byte[] cast = ret as byte[];
				int[] cornerDigits = new int[8];
				for (int i = 0; i < MAX_CORNER_ORIENTATION; i++)
				{
					int count = 0;
					for (int j = 0; j < 8; j++)
					{
						if (cornerDigits[j] == 0)
							count++;
					}


					cast[i] = (byte)count;

					//increment number in base 3
					for (int j = 0; j < cornerDigits.Length; j++)
					{
						if (cornerDigits[j] < 2)
						{
							cornerDigits[j]++;

							for (int k = j - 1; k >= 0; k--)
							{
								cornerDigits[k] = 0;
							}

							break;
						}
					}
				}
			});

			//Positioned corners count
			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "positioned_corners_count.bin"), PositionedCornersCount, (ret) =>
			{
				byte[] cast = ret as byte[];
				int[] permBuffer = new int[8];
				for (int i = 0; i < MAX_CORNER_PERMUTATION; i++)
				{
					GetIndexedPerm(permBuffer, i);

					int count = 0;
					for (int j = 0; j < 8; j++)
					{
						if (permBuffer[j] == j)
							count++;
					}

					cast[i] = (byte)count;
				}
			});

			//Positioned edges
			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "positioned_edges_count.bin"), PositionedEdgesCount, (ret) =>
			{
				byte[] cast = ret as byte[];
				int[] permBuffer = new int[12];
				for (int i = 0; i < MAX_EDGE_PERMUTATION; i++)
				{
					GetIndexedPerm(permBuffer, i);

					int count = 0;
					for (int j = 0; j < 12; j++)
					{
						if (permBuffer[j] == j)
							count++;

					}
					if (i % 10_000_000 == 0)
						Console.WriteLine(i.ToString("000 000 000"));

					PositionedEdgesCount[i] = (byte)count;
				}
			});

			GC.Collect();
			Console.WriteLine("TotalRamUsage: " + GC.GetTotalMemory(true).ToString("0 000 000 000"));

			Console.WriteLine("Initialized CubeIndex class in " + sw.ElapsedMilliseconds + " ms");

			void ReadFromFileOrCreate(string path, Array dest, Action<Array> fillAction)
			{
				if (File.Exists(path))
				{
					byte[] byteBuffer = File.ReadAllBytes(path);
					Buffer.BlockCopy(byteBuffer, 0, dest, 0, byteBuffer.Length);
				}
				else
				{
					fillAction(dest);

					byte[] byteBuffer = new byte[dest.Length * Marshal.SizeOf(dest.GetType().GetElementType())];

					Buffer.BlockCopy(dest, 0, byteBuffer, 0, byteBuffer.Length);

					File.WriteAllBytes(path, byteBuffer);
					Console.WriteLine("Created " + path.Split("\\").Last());
				}
			}
		}

		private static uint[] GenerateShortendEdgePermArrays(CubeMove m)
		{
			//Generate full array
			int[] data = new int[MAX_EDGE_PERMUTATION];
			Cube c = new Cube();
			c.MakeMove(m);
			uint singleIndex = FindEdgePermutationIndex(c);

			Console.WriteLine("Generating: " + m);
			Console.WriteLine(singleIndex);
			int[] move = GetIndexedPerm(12, (int)singleIndex);
			int[] currentPerm = new int[12];
			int[] result = new int[12];
			Console.WriteLine(string.Join(" ", move));

			for (int i = 0; i < data.Length; i++)
			{
				GetIndexedPerm(currentPerm, i);

				for (int j = 0; j < 12; j++)
					result[j] = currentPerm[move[j]];

				data[i] = GetIndex(result);
			}

			//shortened array
			for (int j = 0; j < data.Length; j++)
			{
				data[j] -= j;

				if (data[j] < 0)
					data[j] += data.Length;
			}

			int minCycle = MinCycle(data);
			int trackLength = MaxTrack(data);
			Console.WriteLine(m + " -> Cycle: " + minCycle.ToString("000 000 000") + " TrackLength: " + trackLength);

			bool corret = true;
			for (int j = 0; j < data.Length; j += trackLength)
			{
				for (int k = 0; k < trackLength; k++)
				{
					corret &= data[j] == data[j + k];
				}
			}
			Console.WriteLine("Has blocksize: " + corret);

			corret = true;
			for (int j = 0; j < data.Length; j++)
			{
				corret &= data[j] + data[data.Length - j - 1] == data.Length;
			}
			Console.WriteLine("Has mirror: " + corret);


			NextEdgePerm[(int)m / 3] = new uint[minCycle / trackLength / 2];

			uint[] array = new uint[minCycle / trackLength / 2];

			for (int j = 0; j < array.Length; j++)
			{
				array[j] = (uint)data[j * trackLength];
			}

			//Check values
			for (int j = 0; j < data.Length; j++)
			{
				int value = (j / trackLength) % array.Length;
				uint res = array[value];

				if ((j / trackLength / array.Length) % 2 == 1)
				{
					value = array.Length - value - 1;
					res = (int)MAX_EDGE_PERMUTATION - array[value];
				}

				if (data[j] != res)
				{
					Console.WriteLine("Index was: " + value + " but should have been: " + Array.IndexOf(array, data[j]));
					Console.WriteLine("Alarm");
				}
			}

			return array;

			int MinCycle(int[] data)
			{
				for (int cycleLength = 1; cycleLength < data.Length; cycleLength++)
				{
					if (data.Length % cycleLength != 0)
						continue;

					bool foundMissmatch = false;
					for (int i = 0; i + cycleLength < data.Length; i++)
					{
						if (data[i] != data[i + cycleLength])
						{
							foundMissmatch = true;
							break;
						}
					}

					if (!foundMissmatch)
					{
						return cycleLength;
					}
				}

				return data.Length;
			}

			int MaxTrack(int[] data)
			{
				for (int trackLength = 2; trackLength < data.Length; trackLength++)
				{
					if (data.Length % trackLength != 0)
						continue;

					if (data[0] != data[0 + trackLength])
					{
						for (int i = 0; i < data.Length; i += trackLength)
						{
							for (int j = 1; j < trackLength; j++)
							{
								if (data[i] != data[i + j])
									return 1;
							}
						}

						return trackLength;
					}
				}

				return 1;
			}


		}
	}
}
