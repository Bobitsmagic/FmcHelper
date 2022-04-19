using CubeAD.IndexCubeSets;
using System;
using System.Collections.Generic;
using System.IO;
using static CubeAD.IndexCube;
using static CubeAD.SearchingAlgorithms;
using static CubeAD.Permutation;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace CubeAD
{
	/// <summary>
	/// A class holding pre-computed data either loaded from files or generated
	/// </summary>
	public static class PreCompTables
	{	
		public const int MAX_EO_MOVE_DEPTH = 8;
		public const int MAX_MOVE_DEPTH = 7;

		public static readonly string PRE_COMP_PATH = @"C:\Users\Martin\source\repos\FmcHelper\Files";
		public const string MOVE_TREE_PREFIX = "solved_tree_";

		//A unique index for each Edge addressed by its 2 colores/sides
		public static int[,] EdgeIndices = new int[6, 6];
		//A unique index for each Corner addressed by its 3 colores/sides
		public static int[,,] CornerIndices = new int[6, 6, 6];

		//The colors of the stickers of each edge[i, ] indexed from lower dimension to higher dimension [ , j]
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
		//The colors of the stickers of each corner[i, ] looking towards the X,Y,Z-Dimension [ , j]
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
		//The sizes of each sub array
		public static int[] NextEdgeSizes = new int[6] { 5940, 840, 119750400, 19958400, 1814400, 181440 };
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

			//The representing element of the group generated with all symmetries (Element with the lowest index)
			public static ushort[] CornerPermutationRepresentative = new ushort[MAX_CORNER_PERMUTATION];
			public static uint[] EdgePermutationRepresentative = new uint[MAX_EDGE_PERMUTATION];

			//The symmetry used to get to this element from the representative
			public static byte[] CornerPermutationSymmetry = new byte[MAX_CORNER_PERMUTATION];
			public static byte[] EdgePermutationSymmetry = new byte[MAX_EDGE_PERMUTATION];
			
			//A bitmask containing information about which self-symmetries apply to a specific permutation index
			public static BitMap64[] CornerPermMask = new BitMap64[MAX_CORNER_PERMUTATION];
			public static Dictionary<uint, BitMap64> EdgePermMask = new Dictionary<uint, BitMap64>(); //containing only elements with non-trivial symmetries
		} 

		//The hamilton path through all states of corners (ignoering orientation and impossible cases), LDB corner always stays
		public static CubeMove[] CornerHamilton = new CubeMove[3_674_160]; //3^8 * 8! / 3 / 24

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

			//Edge indices
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

			//Corner indices
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
		
			//Hamilton
			ReadFromFileOrCreateUnsafeCubeMove(Path.Combine(PRE_COMP_PATH, "hamilton.bin"), CornerHamilton , (ret) =>
			{
				//File from https://bruce.cubing.net/index.html
				string s = File.ReadAllText(Path.Combine(PRE_COMP_PATH, "Hamilton222.txt")).Replace("\r", "").Replace("\n", "");
				Console.WriteLine("Loaded 222Hamilton.txt");

				if(s.Length != ret.Length)
					Console.WriteLine("Lengths dont match");

				for (int i = 0; i < s.Length; i++)
				{
					switch (s[i])
					{
						case 'R': ret[i] = CubeMove.R; break;
						case 'S': ret[i] = CubeMove.RP; break;
						case 'U': ret[i] = CubeMove.U; break;
						case 'V': ret[i] = CubeMove.UP; break;
						case 'F': ret[i] = CubeMove.F; break;
						case 'G': ret[i] = CubeMove.FP; break;
						default: Console.WriteLine("Found unknon charachter: (" + s[i] + ")"); break;
					}
				}

			});

			//Next corner perm
			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "next_corner_perm.bin"), NextCornerPerm, (ret) =>
			{
				StickerCube c = new StickerCube();
				StickerCube buffer = new StickerCube();
				IndexCube index = new IndexCube();
				int[] permBuffer = new int[8];

				ushort[,] cast = (ushort[,])ret;
				for (int i = 0; i < MAX_CORNER_PERMUTATION; i++)
				{
					index.CornerPermutationIndex = (ushort)i;
					index.ApplyToCube(c);

					for (int j = 0; j < 18; j++)
					{
						buffer.CopyValuesFrom(c);
						buffer.MakeMove((CubeMove)j);
						cast[i, j] = buffer.FindCornerPermutationIndex(permBuffer);
					}
				}
			});

			//Next corner orient
			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "next_corner_orient.bin"), NextCornerOrient, (ret) =>
			{
				StickerCube c = new StickerCube();
				StickerCube buffer = new StickerCube();
				IndexCube index = new IndexCube();
				ushort[,] cast = (ushort[,])ret;
				for (int i = 0; i < MAX_CORNER_ORIENTATION; i++)
				{
					index.CornerOrientationIndex = (ushort)i;
					index.ApplyToCube(c);

					for (int j = 0; j < 18; j++)
					{
						buffer.CopyValuesFrom(c);
						buffer.MakeMove((CubeMove)j);

						cast[i, j] = buffer.FindCornerOrientationIndex();
					}
				}
			});

			//Next edge orientation
			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "next_edge_orient.bin"), NextEdgeOrient, (ret) =>
			{

				StickerCube c = new StickerCube();
				StickerCube buffer = new StickerCube();
				IndexCube index = new IndexCube();
				ushort[,] cast = (ushort[,])ret;
				for (int i = 0; i < MAX_EDGE_ORIENTATION; i++)
				{
					index.EdgeOrientationIndex = (ushort)i;
					index.ApplyToCube(c);

					for (int j = 0; j < 18; j++)
					{
						buffer.CopyValuesFrom(c);
						buffer.MakeMove((CubeMove)j);
						cast[i, j] = buffer.FindEdgeOrientationIndex();
					}
				}
			});

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

			//Symmetry
			uint[] permsWithSymmetry = new uint[304224];
			//Symmetry edge permuation mask
			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "symmetry_edgde_perm_mask.bin"), permsWithSymmetry, (ret) =>
			{
				int[] currentPerm = new int[12];
				for (int i = 0; i < 12; i++)
					currentPerm[i] = i;
				int N = 12;

				uint[] cast = (uint[])ret;

				int count = 0;
				for (uint i = 0; i < MAX_EDGE_PERMUTATION; i++)
				{
					int symCounter = 0;
					for (int j = 0; j < SymmetryElement.Elements.Length; j++)
					{
						byte[] symTransfrom = Symmetry.EdgePermTranform[j];
						bool res = true;
						for (int k = 0; k < 12; k++)
						{
							if (currentPerm[symTransfrom[k]] != symTransfrom[currentPerm[k]])
							{
								res = false;
								break;
							}
						}

						if (res) symCounter++;

						if (symCounter > 1)
						{
							cast[count++] = i;
							break;
						}
					}

					if (i % 10_000_000 == 0)
						Console.WriteLine(i.ToString("000 000 000"));

					//Next lexico
					int index = N - 1;

					while (index >= 0 && currentPerm[index - 1] >= currentPerm[index])
					{

						if (--index < 1)
						{
							return;
						}
					}
					if (index < 0)
					{
						Console.WriteLine("Alarm");
					}
					else
					{
						int j = N;
						while (j > index && currentPerm[j - 1] <= currentPerm[index - 1])
							j--;

						Swap(j - 1, index - 1);

						index++;
						j = N;

						while (index < j)
						{
							Swap(index - 1, j - 1);
							index++;
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

				Console.WriteLine("Size: " + count);
			});

			BitMap64[] edgePermMasks = new BitMap64[permsWithSymmetry.Length];
			ReadFromFileOrCreateUnsafeBitArray(Path.Combine(PRE_COMP_PATH, "symmetry_edgde_perm_bitarray.bin"), edgePermMasks, (ret) =>
			{
				for (int i = 0; i < permsWithSymmetry.Length; i++)
				{
					uint val = permsWithSymmetry[i];
					IndexCube cubeIndex = new IndexCube();
					cubeIndex.EdgePermutationIndex = val;
					BitMap64 array = new BitMap64();
					for (int j = 0; j < SymmetryElement.Elements.Length; j++)
					{
						SymmetryElement se = SymmetryElement.Elements[j];
						array[j] = cubeIndex.HasSymmetry(se);
					}

					ret[i] = array;
				}
			});

			Symmetry.EdgePermMask = new Dictionary<uint, BitMap64>(permsWithSymmetry.Length);
			for (int i = 0; i < permsWithSymmetry.Length; i++)
			{
				Symmetry.EdgePermMask.Add(permsWithSymmetry[i], edgePermMasks[i]);
			}

			//Symmetry corner permutation mask
			ReadFromFileOrCreateUnsafeBitArray(Path.Combine(PRE_COMP_PATH, "symmetry_corner_perm_mask.bin"), Symmetry.CornerPermMask, (ret) =>
			{
				int min = int.MaxValue, max = int.MinValue, sum = 0, zeroCount = 0;
				IndexCube cubeIndex = new IndexCube();
				for (ushort i = 0; i < MAX_CORNER_PERMUTATION; i++)
				{
					cubeIndex.CornerPermutationIndex = i;
					BitMap64 array = new BitMap64();
					for (int j = 0; j < SymmetryElement.Elements.Length; j++)
					{
						SymmetryElement se = SymmetryElement.Elements[j];
						array[j] = cubeIndex.HasSymmetry(se);
					}

					min = Math.Min(min, array.BitCount);
					max = Math.Max(max, array.BitCount);
					sum += array.BitCount;
					if (array.BitCount == 0) zeroCount++;

					ret[i] = array;
				}

				Console.WriteLine("Min: " + min);
				Console.WriteLine("Max: " + max);
				Console.WriteLine("Sum: " + sum);
				Console.WriteLine("ZeroCount: " + zeroCount);
			});

			//Calculating the transformation performed by a symmetry
			int[] orderTransfrom = new int[6] { 0, 1, 4, 5, 2, 3 };
			for (int i = 0; i < SymmetryElement.ORDER; i++)
			{
				SymmetryElement se = SymmetryElement.Elements[i];
				Symmetry.EdgePermTranform[i] = new byte[12];
				Symmetry.CornerPermTranform[i] = new byte[8];
				Symmetry.EdgeOrientTransform[i] = new byte[12];

				counter = 0;
				for (int x = 0; x < 6; x++)
				{
					for (int y = x + 1; y < 6; y++)
					{
						if (x / 2 == y / 2) continue;

						//If the color change results in a change of order colorindices this edge will be flipped
						Symmetry.EdgeOrientTransform[i][counter] = (byte)((orderTransfrom[x] < orderTransfrom[y] !=
							orderTransfrom[se.TransformColor(x)] < orderTransfrom[se.TransformColor(y)]) ? 1 : 0);
						Symmetry.EdgePermTranform[i][counter++] = (byte)EdgeIndices[se.TransformColor(x), se.TransformColor(y)];
					}
				}

				counter = 0;
				for (int x = 0; x < 2; x++)
				{
					for (int y = 2; y < 4; y++)
					{
						for (int z = 4; z < 6; z++)
						{
							Symmetry.CornerPermTranform[i][counter++] = (byte)CornerIndices[se.TransformColor(x), se.TransformColor(y), se.TransformColor(z)];
						}
					}
				}
			}

			//Symmetry Corner Orientation
			//Generates for all corner positions and orientations and symmetries the resulting orientation after a symmetry transformation
			for (int i = 0; i < 48; i++)
			{
				SymmetryElement se = SymmetryElement.Elements[i];
				bool[,,] seen = new bool[8, 8, 3];
				int sCounter = 0;
				byte[] symTranfrom = Symmetry.CornerPermTranform[i];
				IndexCube c1 = new IndexCube();
				IndexCube c2 = new IndexCube();

				var array = new byte[8, 8, 3];
				Symmetry.CornerOrientTransform[i] = array;

				ushort[] permutations = new ushort[8];
				for(int j = 0; j < 8; j++)
				{
					int[] perm = new int[8];
					for (int k = 0; k < 8; k++)
						perm[k] = (k + j) % 8;

					permutations[j] = (ushort)GetIndex(perm);
				}
				ushort[] orientaions = new ushort[3] { 0, 3280, 6560 };


				StickerCube cube = new StickerCube();
				for (int pos = 0; pos < 8; pos++)
				{
					c1.CornerPermutationIndex = permutations[pos];
					for (int rot = 0; rot < 3; rot++)
					{
						c1.CornerOrientationIndex = orientaions[rot];

						int[] perm = GetInverse(c1.GetCornerPermuation());
						int[] orient = c1.GetCornerOrientation();

						c1.GetCube().InsertSymmetryTransformation(se, cube);
						c2 = new IndexCube(cube);

						int[] otherOrient = c2.GetCornerOrientation();

						for (int k = 0; k < 8; k++)
						{
							if (!seen[k, perm[k], orient[perm[k]]])
							{
								sCounter++;
								seen[k, perm[k], orient[perm[k]]] = true;
								array[k, perm[k], orient[perm[k]]] = (byte)otherOrient[symTranfrom[perm[k]]];
							}
							else
							{
								if (array[k, perm[k], orient[perm[k]]] != otherOrient[symTranfrom[perm[k]]])
								{
									Console.WriteLine("Ambiguous value detected S:" + i + " M: " + pos + " C: " + k);
								}
							}
						}
					}
				}
			}

			//Symmetry Corner representator
			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "symmetry_corner_matrix.bin"), Symmetry.CornerPermutationRepresentative, (ret) =>
			{
				ushort[] cast = ret as ushort[];

				Array.Fill(cast, ushort.MaxValue);

				int fac = Factorial(8);
				int[] inverse = new int[8];
				int[] perm = new int[8];
				int[] transformed = new int[8];

				for (int i = 0; i < fac; i++)
				{
					if(cast[i] == ushort.MaxValue)
					{
						cast[i] = (ushort)i;

						GetIndexedPerm(inverse, i);
						perm = GetInverse(inverse);

						//Skip identity
						for(int j = 1; j < 48; j++)
						{
							byte[] symmetryTranform = Symmetry.CornerPermTranform[j];
							
							for(int k = 0; k < 8; k++)
							{
								//symTransfrom[inversePerm[i]] == otherInversePerm[symTransfrom[i]]
								transformed[symmetryTranform[k]] = symmetryTranform[inverse[k]]; 
							}

							cast[GetIndex(GetInverse(transformed))] = (ushort)i;							
						}
					}
				}
			});

			//Symmetry Edge representator
			ReadFromFileOrCreate(Path.Combine(PRE_COMP_PATH, "symmetry_edge_matrix.bin"), Symmetry.EdgePermutationRepresentative, (ret) =>
			{
				uint[] cast = ret as uint[];

				Array.Fill(cast, uint.MaxValue);

				int fac = Factorial(12);
				int[] inverse = new int[12];
				int[] perm = new int[12];
				int[] transformed = new int[12];

				for (int i = 0; i < fac; i++)
				{
					if (cast[i] == uint.MaxValue)
					{
						cast[i] = (uint)i;

						GetIndexedPerm(inverse, i);
						perm = GetInverse(inverse);

						//Skip identity
						for (int j = 1; j < 48; j++)
						{
							byte[] symmetryTranform = Symmetry.EdgePermTranform[j];

							for (int k = 0; k < 12; k++)
							{
								//symTransfrom[inversePerm[i]] == otherInversePerm[symTransfrom[i]]
								transformed[symmetryTranform[k]] = symmetryTranform[inverse[k]];
							}

							cast[GetIndex(GetInverse(transformed))] = (uint)i;
						}
					}
				}
			});

			GC.Collect();
			Console.WriteLine("TotalRamUsage: " + GC.GetTotalMemory(true).ToString("0 000 000 000"));
			Console.WriteLine("Initialized CubeIndex class in " + sw.ElapsedMilliseconds + " ms");


			//Loads an array of value types from a file or generates it if necessary
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

			//Loads an array of BitArray from a file or generates it if necessary
			void ReadFromFileOrCreateUnsafeBitArray(string path, BitMap64[] dest, Action<BitMap64[]> fillAction)
			{
				if (File.Exists(path))
				{
					byte[] byteBuffer = File.ReadAllBytes(path);
					
					unsafe
					{
						fixed (void* source = byteBuffer)
						{
							fixed (void* destination = dest)
							{
								Buffer.MemoryCopy(source, destination, byteBuffer.Length, byteBuffer.Length);
							}
						}
					}
				}
				else
				{
					Console.WriteLine("Generating: " + path.Split("\\").Last());
					fillAction(dest);

					byte[] byteBuffer = new byte[dest.Length * Marshal.SizeOf(dest.GetType().GetElementType())];

					unsafe
					{
						fixed (void* source = dest)
						{
							fixed (void* destination = byteBuffer)
							{
								Buffer.MemoryCopy(source, destination, byteBuffer.Length, byteBuffer.Length);
							}
						}
					}

					File.WriteAllBytes(path, byteBuffer);
					Console.WriteLine("Created " + path.Split("\\").Last());
				}
			}

			//Loads an array of CubeMove from a file or generates it if necessary
			void ReadFromFileOrCreateUnsafeCubeMove(string path, CubeMove[] dest, Action<CubeMove[]> fillAction)
			{
				if (File.Exists(path))
				{
					byte[] byteBuffer = File.ReadAllBytes(path);

					unsafe
					{
						fixed (void* source = byteBuffer)
						{
							fixed (void* destination = dest)
							{
								Buffer.MemoryCopy(source, destination, byteBuffer.Length, byteBuffer.Length);
							}
						}
					}
				}
				else
				{
					Console.WriteLine("Generating: " + path.Split("\\").Last());
					fillAction(dest);

					byte[] byteBuffer = new byte[dest.Length];

					unsafe
					{
						fixed (void* source = dest)
						{
							fixed (void* destination = byteBuffer)
							{
								Buffer.MemoryCopy(source, destination, byteBuffer.Length, byteBuffer.Length);
							}
						}
					}

					File.WriteAllBytes(path, byteBuffer);
					Console.WriteLine("Created " + path.Split("\\").Last());
				}
			}
		}

		
		//[TODO]
		private static uint[] GenerateShortendNextEdgePermArrays(CubeMove m)
		{
			int N = 12;
			//Generate full array
			int[] data = new int[MAX_EDGE_PERMUTATION];
			StickerCube c = new StickerCube();
			c.MakeMove(m);
			uint singleIndex = c.FindEdgePermutationIndex();

			Console.WriteLine("Generating: " + m);
			Console.WriteLine(singleIndex);
			int[] move = GetIndexedPerm(12, (int)singleIndex);
			int[] currentPerm = new int[12];
			for (int i = 0; i < currentPerm.Length; i++)
				currentPerm[i] = i;

			int[] result = new int[12];
			Console.WriteLine(string.Join(" ", move));

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
						System.Console.WriteLine("Alarm");
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

			//Checks whether this data set fullfills the mirror proberty (f(i) + f(i - n) = n)
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