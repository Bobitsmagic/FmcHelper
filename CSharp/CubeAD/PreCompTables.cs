using CubeAD.IndexCubeSets;
using System;
using System.Collections.Generic;
using System.IO;
using static CubeAD.CubeRepresentation.IndexCube;
using static CubeAD.SearchingAlgorithms;
using static CubeAD.Permutation;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using CubeAD.CubeRepresentation;
using System.Security.Cryptography;
using System.Runtime.ExceptionServices;

namespace CubeAD
{
    /// <summary>
    /// A class holding pre-computed data either loaded from files or generated
    /// </summary>
    public static class PreCompTables
	{	
		public const int MAX_EO_MOVE_DEPTH = 8;
		public const int MAX_MOVE_DEPTH = 7;

		//public static readonly string PRE_COMP_PATH = @"C:\Users\hmart\source\repos\FmcHelper\Files";
        public static readonly string PRE_COMP_PATH = @"C:\Users\Martin\source\repos\FmcHelper\Files";

        public const string MOVE_TREE_PREFIX = "solved_tree_";


		//The sizes of each sub array
		public static int[] NextEdgeSizes = new int[6] { 5940, 840, 119_750_400, 19_958_400, 1_814_400, 181_440 };
		//The difference (mod 12!) from the current edge permutation index ([i][]) to the next index after one clockwise move (L, R, D, U, F, B) on a side ([][j]) 
		public static uint[][] NextEdgePermDif = new uint[6][]
		{
			new uint[NextEdgeSizes[0]],
			new uint[NextEdgeSizes[1]],
			new uint[NextEdgeSizes[2]],
			new uint[NextEdgeSizes[3]],
			new uint[NextEdgeSizes[4]],
			new uint[NextEdgeSizes[5]]
		};
		//The length of consecutively equal values in the uncompressed NextEdgePermDif array
		public static int[] EdgeTrackSizes = new int[6] { 40320, 24, 2, 1, 1, 1 };

		//The index after a move [, j] given a current index [i, ]
		public static ushort[,] NextCornerPerm = new ushort[MAX_CORNER_PERMUTATION, 18];
		public static ushort[,] NextEdgeOrient = new ushort[MAX_EDGE_ORIENTATION, 18];
		public static ushort[,] NextCornerOrient = new ushort[MAX_CORNER_ORIENTATION, 18];

		//The amount of correct pieces given an index
		public static byte[] OrientedEdgesCount = new byte[MAX_EDGE_ORIENTATION];
		public static byte[] OrientedCornersCount = new byte[MAX_CORNER_ORIENTATION];
		public static byte[] PositionedEdgesCount = new byte[MAX_EDGE_PERMUTATION];
		public static byte[] PositionedCornersCount = new byte[MAX_CORNER_PERMUTATION];

		public static class Symmetry
		{
			//The permutation that a symmetry transformation performs
			public static byte[][] CornerPermTranform = new byte[SymmetryElement.ORDER][];
			public static byte[][] EdgePermTranform = new byte[SymmetryElement.ORDER][];
			//The orientation a corner has after a symmetry transformation
			public static byte[][,,] CornerOrientTransform = new byte[SymmetryElement.ORDER][,,]; //color index, position, orient
			//Whether a edge get flipped by a symmetry transformation
			public static byte[][] EdgeOrientTransform = new byte[SymmetryElement.ORDER][];
		} 

		//A set containing all cubes (with all edges oriented) that can be solved in MAX_EO_MOVE_DEPTH moves or less
		private static SealedHashset _EOMoveTree = null;
		public static SealedHashset GetEdgeOrientedMoveTree
		{
			get
			{
				if (_EOMoveTree is null)
				{
					string path = Path.Combine(PRE_COMP_PATH, "solved_eo_tree_" + MAX_EO_MOVE_DEPTH + ".bin");

					if (File.Exists(path))
					{
						_EOMoveTree = new SealedHashset(path);
					}
					else
					{
						_EOMoveTree = new SealedHashset(GenerateSolvedTreeOrientedEdges(MAX_EO_MOVE_DEPTH).GetArray());
						_EOMoveTree.SaveToFile(path);
						Console.WriteLine("Created: " + path);
					}
				}

				return _EOMoveTree;
			}
		}

		//A set containing all cubes that can be solved in MAX_MOVE_DEPTH moves or less
		private static SealedHashset _MoveTree = null;
		public static SealedHashset GetMoveTree
		{
			get
			{
				if (_MoveTree is null)
				{
					string path = Path.Combine(PRE_COMP_PATH, "solved_tree_" + MAX_MOVE_DEPTH + ".bin");

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

			//Egde permutation
			for (int i = 0; i < 18; i += 3)
			{
				CubeMove m = (CubeMove)i;
				string path = Path.Combine(PRE_COMP_PATH, "next_edge_perm_" + m.ToString() + ".bin");

				ReadFromFileOrCreate(path, NextEdgePermDif[i / 3], (ret) => {
					Array gen = GenerateShortendNextEdgePermArrays(m);
					Console.WriteLine("Gen: " + gen.Length + " ret: " + ret.Length);
					Array.Copy(gen, ret, gen.Length);
				});
			}

			//Next corner perm
			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "next_corner_perm.bin"), NextCornerPerm, (ret) =>
			{
				ushort[,] cast = (ushort[,])ret;

				FillNextCornerPermArray(cast);
			});

            //Next edge orient
            ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "next_edge_orient.bin"), NextEdgeOrient, (ret) =>
            {
                ushort[,] cast = (ushort[,])ret;

                FillNextEdgeOrientArray(cast);
            });

            //Next corner orient
            ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "next_corner_orient.bin"), NextCornerOrient, (ret) =>
            {
                ushort[,] cast = (ushort[,])ret;

                FillNextCornerOrientArray(cast);
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
					Console.WriteLine("Generating: " + path.Split("\\").Last());
					fillAction(dest);

					byte[] byteBuffer = new byte[dest.Length * Marshal.SizeOf(dest.GetType().GetElementType())];

					Buffer.BlockCopy(dest, 0, byteBuffer, 0, byteBuffer.Length);

					File.WriteAllBytes(path, byteBuffer);
					Console.WriteLine("Created " + path.Split("\\").Last());
				}
			}
		}

		public static uint[] GenerateShortendNextEdgePermArrays(CubeMove m)
		{
			int N = 12;
			//Generate full array
			int[] data = new int[MAX_EDGE_PERMUTATION];
			ArrayCube c = ArrayCube.GetSolved();
			c.MakeMove(m);

			int[] move = Permutation.GetInverse(c.GetEdgePerm());

			Console.WriteLine("Generating: " + m);

			int[] currentPerm = Enumerable.Range(0, 12).ToArray();

			int[] result = new int[12];
			//Console.WriteLine(string.Join("\n", currentPerm.Select(x => x + "\t" + move[x])));

			for (int i = 0; i < data.Length; i++)
			{
				Transform(currentPerm, move, result);

				data[i] = GetIndex(result);

				NextLexico();

				void NextLexico()
				{
					int i = N - 1;

					while (i >= 0 && currentPerm[i - 1] >= currentPerm[i])
					{

						if (--i < 1)
						{
							return;
						}
					}


					if (i < 0)
					{
						Console.WriteLine("Alarm");
					}
					else
					{
						int j = N;
						while (j > i && currentPerm[j - 1] <= currentPerm[i - 1])
							j--;

						Swap(j - 1, i - 1);

						i++;
						j = N;

						while (i < j)
						{
							Swap(i - 1, j - 1);
							i++;
							j--;
						}
					}


					void Swap(int a, int b)
					{
						int c = currentPerm[a];
						currentPerm[a] = currentPerm[b];
						currentPerm[b] = c;
					}
				}
			}

            //Shorten the array
            //Calculate the differences to the current index
            for (int j = 0; j < data.Length; j++)
			{
				data[j] -= j;

				if (data[j] < 0)
					data[j] += data.Length;
			}

            //Find redundancies
            int minCycle = MinCycle(data); //The smallest cycle of distinct elements (ABCABCABC -> 3)
			int trackLength = MaxTrack(data); //The longest sequenz of repeating elements (AAABBBCCC -> 3)

			Console.WriteLine(m + " -> Cycle: " + minCycle.ToString("000 000 000") + " TrackLength: " + trackLength);

			//Checks whether the tracksize is correct
			bool correct = true;
			for (int j = 0; j < data.Length; j += trackLength)
			{
				for (int k = 0; k < trackLength; k++)
				{
					correct &= data[j] == data[j + k];
				}
			}
			Console.WriteLine("Has max track size: " + correct);

			//Checks whether this data set fullfills the mirror proberty (f(i) + f(n - i - 1) = n)
			correct = true;
			for (int j = 0; j < data.Length; j++)
			{
				correct &= data[j] + data[data.Length - j - 1] == data.Length;
			}
			Console.WriteLine("Has mirror: " + correct);

			//Fills in the shortend array
			uint[] array = new uint[minCycle / trackLength / 2];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = (uint)data[j * trackLength];
			}

			//Checks whether the compression works
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
				}
			}

            Console.WriteLine("Final array length: " + array.Length.ToString("000 000 000"));

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

		public static void FillNextCornerPermArray(ushort[,] array)
		{
			const int N = 8;
			for(int move = 0; move < 18; move++)
			{
				ArrayCube c = ArrayCube.GetSolved();
				CubeMove m = (CubeMove)move;
				c.MakeMove(m);

				int[] movePerm = c.GetCornerPerm();

				Console.WriteLine("Generating: " + m);

				int[] currentPerm = Enumerable.Range(0, N).ToArray();

				int[] result = new int[N];
				Console.WriteLine(string.Join("\n", currentPerm.Select(x => x + "\t" + movePerm[x])));

				for (int i = 0; i < MAX_CORNER_PERMUTATION; i++)
				{
					if(i == 11306 && m == CubeMove.U2)
					{
                        Console.WriteLine(	"KEK");
                    }

					Transform(movePerm, currentPerm, result);

					array[i, move] = (ushort)GetIndex(result);

					NextLexico();

					void NextLexico()
					{
						int i = N - 1;

						while (i >= 0 && currentPerm[i - 1] >= currentPerm[i])
						{

							if (--i < 1)
							{
								return;
							}
						}


						if (i < 0)
						{
							Console.WriteLine("Alarm");
						}
						else
						{
							int j = N;
							while (j > i && currentPerm[j - 1] <= currentPerm[i - 1])
								j--;

							Swap(j - 1, i - 1);

							i++;
							j = N;

							while (i < j)
							{
								Swap(i - 1, j - 1);
								i++;
								j--;
							}
						}


						void Swap(int a, int b)
						{
							int c = currentPerm[a];
							currentPerm[a] = currentPerm[b];
							currentPerm[b] = c;
						}
					}
				}
			}

		} 

		public static void FillNextEdgeOrientArray(ushort[,] array)
		{
			int[] frontEdges = new int[] { 3, 7, 9, 11 };
			int[] backEdges = new int[] { 2, 6, 8, 10 };

            const int N = 12;
            for (int move = 0; move < 18; move++)
            {

                ArrayCube c = ArrayCube.GetSolved();
                CubeMove m = (CubeMove)move;
                c.MakeMove(m);

				//inverse because we are working in position space not permuation space
                int[] movePerm = GetInverse(c.GetEdgePerm());

                Console.WriteLine("Generating: " + m);

				int[] currentPerm = new int[N];
                int[] result = new int[N];

                for (int i = 0; i < MAX_EDGE_ORIENTATION; i++)
                {
                    Transform(currentPerm, movePerm, result);

					if(m == CubeMove.F || m == CubeMove.FP)
					{
						foreach(int index in frontEdges)
						{
							result[index] = 1 - result[index];
						}
					}

                    if (m == CubeMove.B || m == CubeMove.BP)
                    {
                        foreach (int index in backEdges)
                        {
                            result[index] = 1 - result[index];
                        }
                    }

                    array[i, move] = OrientationIndex();

                    NextLexico();

                    void NextLexico()
                    {
						for(int i = N - 1; i >= 0; i--)
						{
							if (currentPerm[i] == 1)
								continue;
							
							for(int j = i + 1; j < N; j++)
							{
								currentPerm[j] = 0;
							}

							currentPerm[i] = 1;
							break;
						}
                    }

					ushort OrientationIndex()
					{
						int ret = 0;
						for(int i = 0; i < N; i++)
						{
							ret = (ret << 1) | result[i];
						}
						return (ushort)ret;
					}
                }
            }
        }

        public static void FillNextCornerOrientArray(ushort[,] array)
        {
			int[][] sideCorners = new int[][]
			{
				new int[]{0, 1, 2, 3},
				new int[]{4, 5, 6, 7},
				new int[]{0, 1 ,4, 5},
				new int[]{2, 3, 6, 7},
				new int[]{0, 2, 4, 6},
				new int[]{1, 3, 5, 7},
            };

            const int N = 8;
            for (int move = 0; move < 18; move++)
            {
                ArrayCube c = ArrayCube.GetSolved();
                CubeMove m = (CubeMove)move;
                c.MakeMove(m);

                //inverse because we are working in position space not permuation space
                int[] movePerm = GetInverse(c.GetCornerPerm());

                Console.WriteLine("Generating: " + m);

                int[] currentPerm = new int[N];
                int[] result = new int[N];
				int side = move / 3;

                for (int i = 0; i < MAX_CORNER_ORIENTATION; i++)
                {
                    Transform(currentPerm, movePerm, result);

                    if(move % 3 != 1)
					{
						foreach(var index in sideCorners[side])
						{
							//swaps orientation if its not equal to side / 2
							result[index] = (2 * result[index] - (side / 2) + 3) % 3;
						}
                    }

                    array[i, move] = OrientationIndex();

                    NextLexico();

                    void NextLexico()
                    {
                        for (int i = N - 1; i >= 0; i--)
                        {
                            if (currentPerm[i] == 2)
                                continue;

                            for (int j = i + 1; j < N; j++)
                            {
                                currentPerm[j] = 0;
                            }

                            currentPerm[i]++;
                            break;
                        }
                    }

                    ushort OrientationIndex()
                    {
                        int ret = 0;
                        for (int i = 0; i < N; i++)
                        {
                            ret = (ret * 3) + result[i];
                        }
                        return (ushort)ret;
                    }
                }
            }
        }
    }
}