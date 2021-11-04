using CubeAD.CubeIndexSets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace CubeAD
{
	//Compressed representation of a cube
	public struct CubeIndex : IComparable<CubeIndex>
	{
		public const int SIZE_IN_BYTES = 10;

		public const uint MAX_EDGE_PERMUTATION = 479_001_600; //12!
		public const ushort MAX_CORNER_PERMUTATION = 40320; //8!
		public const ushort MAX_EDGE_ORIENTATION = 4096;    //2^12
		public const ushort MAX_CORNER_ORIENTATION = 6561;  //3^8


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

		public static uint[][] NextEdgePerm = new uint[6][];
		public static int[] EdgeTrackSizes = new int[6] { 40320, 24, 2, 1, 1, 1 };
		public static ushort[,] NextCornerPerm = new ushort[MAX_CORNER_PERMUTATION, 18];
		public static ushort[,] NextEdgeOrient = new ushort[MAX_EDGE_ORIENTATION, 18];
		public static ushort[,] NextCornerOrient = new ushort[MAX_CORNER_ORIENTATION, 18];

		public static HashSet<CubeIndex> EOTree;

		static CubeIndex()
		{
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

			Cube c = new Cube();
			Cube buffer = new Cube();
			CubeIndex index = new CubeIndex();
			int[] permBuffer = new int[8];

			for (int i = 0; i < MAX_CORNER_PERMUTATION; i++)
			{
				index.CornerPermutation = (ushort)i;
				index.ApplyToCube(c);

				for (int j = 0; j < 18; j++)
				{
					buffer.CopyValuesFrom(c);
					buffer.MakeMove((CubeMove)j);
					NextCornerPerm[i, j] = FindCornerPermutationIndex(buffer, permBuffer);
				}
			}

			index.CornerPermutation = 0;

			for (int i = 0; i < MAX_CORNER_ORIENTATION; i++)
			{
				index.CornerOrientation = (ushort)i;
				index.ApplyToCube(c);

				for (int j = 0; j < 18; j++)
				{
					if(i == 2460 && j == (int)CubeMove.F2)
					{
						Console.WriteLine("kek");
					}

					buffer.CopyValuesFrom(c);

					//Cube c2 = new CubeIndex(buffer).GetCube();
					//Console.WriteLine(buffer == c2);
					//buffer.PrintSideView();

					buffer.MakeMove((CubeMove)j);

					//c2 = new CubeIndex(buffer).GetCube();
					//Console.WriteLine(buffer == c2);
					//buffer.PrintSideView();

					NextCornerOrient[i, j] = FindCornerOrientationIndex(buffer);
				}
			}

			index.CornerOrientation = 0;
			for (int i = 0; i < MAX_EDGE_ORIENTATION; i++)
			{
				index.EdgeOrientation = (ushort)i;
				index.ApplyToCube(c);

				for (int j = 0; j < 18; j++)
				{
					buffer.CopyValuesFrom(c);
					buffer.MakeMove((CubeMove)j);
					NextEdgeOrient[i, j] = FindEdgeOrientationIndex(buffer);
				}
			}

			for (int i = 0; i < 18; i += 3)
			{
				CubeMove m = (CubeMove)i;
				byte[] byteArray = File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\PreCompFiles\\" + m + ".bin");
				NextEdgePerm[i / 3] = new uint[byteArray.Length / 4];

				Buffer.BlockCopy(byteArray, 0, NextEdgePerm[i / 3], 0, byteArray.Length);
			}


			EOTree = new BucketCubeIndices(Directory.GetCurrentDirectory() + "\\solvedEOset.bin").GetHashSet();

			GC.Collect();

			Console.WriteLine("Initialized CubeIndex class");
		}

		public static void GenerateShortendEdgePermArrays()
		{
			long sum = 0;
			int[] data = new int[MAX_EDGE_PERMUTATION];
			for (int i = 0; i < 18; i += 3)
			{
				CubeMove m = (CubeMove)i;

				byte[] byteArray = File.ReadAllBytes(@"D:\BigFiles\" + m + ".bin");
				Buffer.BlockCopy(byteArray, 0, data, 0, byteArray.Length);


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


				NextEdgePerm[i / 3] = new uint[minCycle / trackLength / 2];

				int[] array = new int[minCycle / trackLength / 2];
				sum += array.Length;

				for (int j = 0; j < array.Length; j++)
				{
					array[j] = data[j * trackLength];
				}

				for (int j = 0; j < data.Length; j++)
				{
					int value = (j / trackLength) % array.Length;
					int result = array[value];

					if ((j / trackLength / array.Length) % 2 == 1)
					{
						value = array.Length - value - 1;
						result = (int)MAX_EDGE_PERMUTATION - array[value];
					}

					if (data[j] != result)
					{
						Console.WriteLine("Index was: " + value + " but should have been: " + Array.IndexOf(array, data[j]));
						Console.WriteLine("Alarm");
					}
				}

				byte[] kek = new byte[array.Length * 4];
				Buffer.BlockCopy(array, 0, kek, 0, kek.Length);

				File.WriteAllBytes(Directory.GetCurrentDirectory() + "\\" + m + ".bin", kek);


				GC.Collect();

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

			Console.WriteLine("Sum: " + sum);
		}

		public static void GenerateFullEdgePermArray()
		{
			int N = Factorial(12);
			Cube c = new Cube();
			int[] data = new int[N];
			byte[] byteArray = new byte[data.Length * 4];

			for(int k = 0; k < 18; k++)
			{
				CubeMove m = (CubeMove)k;
				c.MakeMove(m);
				uint singleIndex = FindEdgePermutationIndex(c);

				Console.WriteLine(m);
				Console.WriteLine(singleIndex);
				int[] move = GetIndexedPerm(12, (int)singleIndex);
				int[] currentPerm = new int[12];
				int[] result = new int[12];
				Console.WriteLine(string.Join(" ", move));


				for(int i = 0; i < data.Length; i++)
				{
					GetIndexedPerm(currentPerm, i);

					for(int j = 0; j < 12; j++)
					{
						result[j] = currentPerm[move[j]];
					}

					data[i] = GetIndex(result);
				}

				Buffer.BlockCopy(data, 0, byteArray, 0, byteArray.Length);
				File.WriteAllBytes(Directory.GetCurrentDirectory() + "\\" + m + ".bin", byteArray);
			}
		}

		public static void GenerateSolvedEdgeSet(int depth)
		{
			var set = SolvedTreeOrientedEdges(depth);

			set.SaveData(Directory.GetCurrentDirectory() + "\\solvedEOset.bin");
		}


		public bool IsSovled => EdgeOrientation == 0 && EdgePermutation == 0 && CornerOrientation == 0 && CornerPermutation == 0;

		public byte this[int index]
		{
			get
			{
				if (index >= 10) Console.WriteLine("CubeIndex[i] out of range");

				if (index < 2)
					return (byte)(CornerOrientation >> (index * 8));

				if (index < 4)
					return (byte)(EdgeOrientation >> ((index - 2) * 8));

				if (index < 6)
					return (byte)(CornerPermutation >> ((index - 4) * 8));

				return (byte)(EdgePermutation >> ((index - 6) * 8));
			}
		}
		public BigInteger Index
		{
			get
			{
				BigInteger ret = EdgePermutation;
				ret *= MAX_CORNER_PERMUTATION;
				ret += CornerPermutation;
				ret *= MAX_EDGE_ORIENTATION;
				ret += EdgeOrientation;
				ret *= MAX_CORNER_ORIENTATION;
				ret += CornerOrientation;

				return ret;
			}
		}

		public uint EdgePermutation;
		public ushort CornerPermutation;
		public ushort EdgeOrientation;
		public ushort CornerOrientation;

		public CubeIndex(uint edgePermutation, ushort cornerPermutation, ushort edgeOrientation, ushort cornerOrientation)
		{
			if (edgePermutation >= MAX_EDGE_PERMUTATION ||
				cornerPermutation >= MAX_CORNER_PERMUTATION ||
				edgeOrientation >= MAX_EDGE_ORIENTATION ||
				cornerOrientation >= MAX_CORNER_ORIENTATION)
				throw new ArgumentException();

			EdgePermutation = edgePermutation;
			CornerPermutation = cornerPermutation;
			EdgeOrientation = edgeOrientation;
			CornerOrientation = cornerOrientation;
		}
		public CubeIndex(CubeIndex old, CubeMove m)
		{
			EdgePermutation = old.EdgePermutation;
			CornerPermutation = old.CornerPermutation;
			EdgeOrientation = old.EdgeOrientation;
			CornerOrientation = old.CornerOrientation;

			MakeMove(m);
		}

		public CubeIndex(Cube c)
		{
			EdgePermutation = FindEdgePermutationIndex(c);
			CornerPermutation = FindCornerPermutationIndex(c);
			EdgeOrientation = FindEdgeOrientationIndex(c);
			CornerOrientation = FindCornerOrientationIndex(c);
		}

		public CubeIndex(MoveSequenz ms)
		{
			EdgePermutation = 0;
			CornerPermutation = 0;
			EdgeOrientation = 0;
			CornerOrientation = 0;

			ApplyMoveSequenz(ms);
		}

		public void CopyValuesFrom(CubeIndex index)
		{
			EdgePermutation = index.EdgePermutation;
			CornerPermutation = index.CornerPermutation;
			EdgeOrientation = index.EdgeOrientation;
			CornerOrientation = index.CornerOrientation;
		}

		public CubeIndex(BinaryReader br)
		{
			EdgePermutation = br.ReadUInt32();
			CornerPermutation = br.ReadUInt16();
			EdgeOrientation = br.ReadUInt16();
			CornerOrientation = br.ReadUInt16();
		}
		public void Write(BinaryWriter bw)
		{
			bw.Write(EdgePermutation);
			bw.Write(CornerPermutation);
			bw.Write(EdgeOrientation);
			bw.Write(CornerOrientation);
		}
		public int[] GetEdgePerm()
		{
			return GetIndexedPerm(12, (int)EdgePermutation);
		}
		public int[] GetEdgeOrient()
		{
			int[] buffer = new int[12];

			for (int i = 0; i < 12; i++)
			{
				buffer[i] = (EdgeOrientation >> (11 - i)) & 1;
			}

			return buffer;
		}

		public int[] GetCornerPerm()
		{
			return GetIndexedPerm(8, CornerPermutation);
		}

		public int[] GetCornerOrient()
		{
			int buffer = CornerOrientation;
			int[] ret = new int[8];
			for (int i = 8 - 1; i >= 0; i--)
			{
				ret[i] = buffer % 3;
				buffer /= 3;
			}

			return ret;
		}

		public void ApplyToCube(Cube c)
		{
			int[] edgePerm = GetEdgePerm();
			int[] edgeOrient = GetEdgeOrient();

			c.Reset();

			int counter = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					//when swapping from main side (0, 1) to other sub sides (2, 3)
					int flip = (counter / 8) ^ (edgePerm[counter] / 8);

					c.SetEdgeColor(x, y, CubeEdgeColors[edgePerm[counter], edgeOrient[counter] ^ flip]);
					c.SetEdgeColor(y, x, CubeEdgeColors[edgePerm[counter], edgeOrient[counter] ^ 1 ^ flip]);

					counter++;
				}
			}

			int[] cornerPerm = GetCornerPerm();
			int[] cornerOrient = GetCornerOrient();
			counter = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						int dif = counter ^ cornerPerm[counter];
						bool flip = dif == 1 || dif == 2 || dif == 4 || dif == 7;

						c.SetCornerColor(x, y, z, CornerColors[cornerPerm[counter],  ((flip ? 2 : 1) * (0 + 2 * cornerOrient[counter])) % 3]);
						c.SetCornerColor(y, x, z, CornerColors[cornerPerm[counter],  ((flip ? 2 : 1) * (1 + 2 * cornerOrient[counter])) % 3]);
						c.SetCornerColor(z, x, y, CornerColors[cornerPerm[counter],  ((flip ? 2 : 1) * (2 + 2 * cornerOrient[counter])) % 3]);

						counter++;
					}
				}
			}

			//U -> XZ
		}

		public Cube GetCube()
		{
			Cube c = new Cube();
			ApplyToCube(c);
			return c;
		}

		public void MakeMove(CubeMove m)
		{
			int move = (int)m;

			CornerPermutation = NextCornerPerm[CornerPermutation, move];
			CornerOrientation = NextCornerOrient[CornerOrientation, move];
			EdgeOrientation = NextEdgeOrient[EdgeOrientation, move];

			int count = (move % 3) + 1;
			int side = move / 3;

			uint[] array = NextEdgePerm[side];
			int trackLength = EdgeTrackSizes[side];

			for (int i = 0; i < count; i++)
			{
				int index = ((int)EdgePermutation / trackLength) % array.Length;
				
				if (((int)EdgePermutation / trackLength / array.Length) % 2 == 0)
				{
					EdgePermutation = (EdgePermutation + array[index]) % MAX_EDGE_PERMUTATION;
				}
				else
				{
					index = array.Length - index - 1;
					EdgePermutation = (EdgePermutation + MAX_EDGE_PERMUTATION - array[index]) % MAX_EDGE_PERMUTATION;
				}
			}
		}

		public void ApplyMoveSequenz(MoveSequenz ms)
		{
			for (int i = 0; i < ms.Length; i++)
			{
				MakeMove(ms.Moves[i]);
			}
		}

		//Statics

		public static CubeIndex[] ReadCubeIndicesFromFile(string path)
		{
			MemoryStream ms = new MemoryStream(File.ReadAllBytes(path));
			BinaryReader br = new BinaryReader(ms);

			CubeIndex[] ret = new CubeIndex[br.ReadInt32()];

			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = new CubeIndex(br);
			}

			return ret;
		}
		public static void WriteCubeIndicesToFile(CubeIndex[] array, string path)
		{
			MemoryStream ms = new MemoryStream(array.Length * SIZE_IN_BYTES);

			BinaryWriter bw = new BinaryWriter(ms);

			bw.Write(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Write(bw);
			}

			File.WriteAllBytes(path, ms.ToArray());

			bw.Close();
			ms.Close();
		}

		public static uint FindEdgePermutationIndex(Cube c, int[] permBuffer)
		{
			if (permBuffer.Length != 12)
				throw new ArgumentException("Array must be of length 12");

			int counter = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					permBuffer[counter++] = EdgeIndices[(int)c.GetEdgeColor(x, y), (int)c.GetEdgeColor(y, x)];
				}
			}
			return (uint)GetIndex(permBuffer);
		}
		public static uint FindEdgePermutationIndex(Cube c)
		{
			return FindEdgePermutationIndex(c, new int[12]);
		}
		public static ushort FindCornerPermutationIndex(Cube c, int[] permBuffer)
		{
			if (permBuffer.Length != 8)
				throw new ArgumentException("Array must be of length 8");

			int counter = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						permBuffer[counter++] = CornerIndices[(int)c.GetCornerColor(x, y, z), (int)c.GetCornerColor(y, x, z), (int)c.GetCornerColor(z, x, y)];
					}
				}
			}

			return (ushort)GetIndex(permBuffer);
		}
		public static ushort FindCornerPermutationIndex(Cube c)
		{
			return FindCornerPermutationIndex(c, new int[8]);
		}
		public static ushort FindEdgeOrientationIndex(Cube c)
		{
			int ret = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					ret = (ret << 1) | (c.EdgeIsOriented(x, y) ? 0 : 1);
				}
			}

			return (ushort)ret;
		}
		public static ushort FindCornerOrientationIndex(Cube c)
		{
			int ret = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						//Console.WriteLine(c.CornerOrientaion(x, y, z));
						ret = (ret * 3) + c.CornerOrientaion(x, y, z);
					}
				}
			}

			return (ushort)ret;
		}
		public static void RadixSortCubeIndices(CubeIndex[] data, CubeIndex[] CubeBuffer)
		{
			const int BIT_COUNT = 8;
			const int RADIX = 1 << BIT_COUNT;

			int[] indices = new int[RADIX];

			if (CubeBuffer.Length != data.Length)
			{
				CubeBuffer = new CubeIndex[data.Length];
				Console.WriteLine("Reallocated");
			}

			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < RADIX; j++)
					indices[j] = 0;
				for (int j = 0; j < data.Length; j++)
					indices[data[j][i]]++;

				for (int j = 1; j < RADIX; j++)
					indices[j] += indices[j - 1];
				for (int j = data.Length - 1; j >= 0; j--)
					CubeBuffer[--indices[data[j][i]]] = data[j];

				CubeIndex[] swap = CubeBuffer;
				CubeBuffer = data;
				data = swap;
			}
		}

		public static List<MoveSequenz> FindEdgeOrientationMoves(ushort edgeOrientation)
		{
			const int MAX_SEARCH_DEPTH = 7;
			List<MoveSequenz> ret = new List<MoveSequenz>();
			
			int maxDepth;
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(MAX_SEARCH_DEPTH);
			MoveBlocker[] blocked = new MoveBlocker[MAX_SEARCH_DEPTH + 1];

			for(int i = 0; i <= MAX_SEARCH_DEPTH && ret.Count == 0; i++)
			{
				maxDepth = i;
				DFS(0, edgeOrientation);
			}

			return ret;

			void DFS(int depth, ushort orientation)
			{
				if (orientation == 0)
				{
					if (depth != maxDepth) Console.WriteLine("This should not happen");

					ret.Add(new MoveSequenz(currentMoves.Reverse()));
				}

				if (depth < maxDepth)
				{
					for(int i = 0; i < 18; i++)
					{
						currentMoves.Push((CubeMove)i);
						DFS(depth + 1, NextEdgeOrient[orientation, i]);
						currentMoves.Pop();
					}
				}
			}
		}

		public static BucketCubeIndices SolvedTree(int maxDepth)
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(maxDepth);
			BucketCubeIndices set = new BucketCubeIndices(20_000);

			//HashSet<CubeIndex> set = new HashSet<CubeIndex>();

			ulong counter = 0;
			DFS(0, new CubeIndex(), new MoveBlocker());

			set.RemoveDuplicatesParallel();
			return set;

			void DFS(int depth, CubeIndex cube, MoveBlocker blocker)
			{
				set.Add(cube);
				if (counter++ % 100_000_000 == 0)
				{
					set.RemoveDuplicatesParallel();
					//Console.WriteLine("Sorted: " + set.Count.ToString("000 000 000 000"));
				}
				
				if (depth < maxDepth)
				{
					for (int i = 0; i < 18; i++)
					{
						if (blocker[i / 3]) continue;

						currentMoves.Push((CubeMove)i);
						DFS(depth + 1, new CubeIndex(cube, (CubeMove)i), new MoveBlocker(blocker, i / 3));
						currentMoves.Pop();
					}
				}
			}
		}
		public static BucketCubeIndices SolvedTreeOrientedEdges(int maxDepth)
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(maxDepth);
			BucketCubeIndices set = new BucketCubeIndices(20_000);

			//HashSet<CubeIndex> set = new HashSet<CubeIndex>();

			ulong counter = 0;
			DFS(0, new CubeIndex(), new MoveBlocker());

			set.RemoveDuplicatesParallel();
			return set;

			void DFS(int depth, CubeIndex cube, MoveBlocker blocker)
			{
				set.Add(cube);
				if (counter++ % 100_000_000 == 0)
				{
					set.RemoveDuplicatesParallel();
					//Console.WriteLine("Sorted: " + set.Count.ToString("000 000 000 000"));
				}

				if (depth < maxDepth)
				{
					for (int i = 0; i < 18; i++)
					{
						if (blocker[i / 3]) continue;
						if (i / 3 > 3 && i % 3 != 1) continue;

						currentMoves.Push((CubeMove)i);
						DFS(depth + 1, new CubeIndex(cube, (CubeMove)i), new MoveBlocker(blocker, i / 3));
						currentMoves.Pop();
					}
				}
			}
		}

		public static List<CubeMove> FindSolution(CubeIndex cube)
		{
			const int MAX_DEPTH = 10;

			List<MoveSequenz> eoMoves = FindEdgeOrientationMoves(cube.EdgeOrientation);
			Console.WriteLine("Eo Length: " + eoMoves[0].Length + " count: " + eoMoves.Count);
			List<CubeMove> solution = new List<CubeMove>();

			bool foundSolution = false;

			foreach (MoveSequenz ms in eoMoves)
			{
				CubeIndex copy = cube;

				copy.ApplyMoveSequenz(ms);

				if (FindEOSolution(copy))
				{
					solution.InsertRange(0, ms.Moves);
					break;
				}
			}

			//Parallel.ForEach(eoMoves, ms =>
			//{
			//	CubeIndex copy = cube;

			//	copy.ApplyMoveSequenz(ms);

			//	if (FindEOSolution(copy))
			//	{
			//		solution.InsertRange(0, ms.Moves);
			//	}
			//});

			return solution;


			bool FindEOSolution(CubeIndex cube)
			{

				Stack<CubeMove> currentMoves = new Stack<CubeMove>(20);

				return DFS(0, cube, new MoveBlocker());

				bool DFS(int depth, CubeIndex cube, MoveBlocker blocker)
				{
					if (foundSolution) return false;

					if (EOTree.Contains(cube))
					{
						if (!foundSolution)
						{
							foundSolution = true;

							Console.WriteLine("Found solution!");

							solution.AddRange(currentMoves.Reverse());
							return true;
						}

						return false;
					}

					if (depth < MAX_DEPTH)
					{
						for (int i = 0; i < 18; i++)
						{
							if (blocker[i / 3]) continue;
							if (i / 3 > 3 && i % 3 != 1) continue;

							currentMoves.Push((CubeMove)i);
							DFS(depth + 1, new CubeIndex(cube, (CubeMove)i), new MoveBlocker(blocker, i / 3));
							currentMoves.Pop();
						}
					}

					return false;
				}


			}
		}

		public static int Factorial(int n)
		{
			int ret = 1;
			for (int i = 2; i <= n; i++)
			{
				ret *= i;
			}

			return ret;
		}

		public static int[] GetIndexedPerm(int n, int index)
		{
			List<int> list = new List<int>(n);
			for (int i = 0; i < n; i++) list.Add(i);

			int[] indices = new int[n];
			for (int i = n - 1; i >= 0; i--)
			{
				indices[i] = index / Factorial(i);
				index -= indices[i] * Factorial(i);
			}

			int[] ret = new int[n];

			for (int i = ret.Length - 1; i >= 0; i--)
			{
				ret[n - i - 1] = list[indices[i]];
				list.RemoveAt(indices[i]);
			}

			return ret;
		}
		private static readonly List<int> bufferList = new List<int>();

		public static int[] GetIndexedPerm(int[] ret, int index)
		{
			int n = ret.Length;

			bufferList.Clear();
			for (int i = 0; i < n; i++) bufferList.Add(i);

			int[] indices = new int[n];
			for (int i = n - 1; i >= 0; i--)
			{
				indices[i] = index / Factorial(i);
				index -= indices[i] * Factorial(i);
			}

			for (int i = ret.Length - 1; i >= 0; i--)
			{
				ret[n - i - 1] = bufferList[indices[i]];
				bufferList.RemoveAt(indices[i]);
			}

			return ret;
		}
		public static int GetIndex(int[] perm)
		{
			int ret = 0;
			for (int i = 0; i < perm.Length; i++)
			{
				int counter = 0;
				for (int j = i + 1; j < perm.Length; j++)
				{
					if (perm[i] > perm[j])
						counter++;
				}

				ret += Factorial(perm.Length - 1 - i) * counter;
			}
			return ret;
		}

		public static int GetInversIndex(int[] perm)
		{
			int ret = 0;
			for (int i = 0; i < perm.Length; i++)
			{
				int counter = 0;
				for (int j = i - 1; j >= 0; j--)
				{
					if (perm[i] > perm[j])
						counter++;
				}

				ret += Factorial(i) * counter;
			}
			return ret;
		}

		public static int TransfromToFacNumber(int N, int value)
		{
			int ret = 0;
			int factor = 1;
			for (int i = 2; i < N; i++)
			{
				int dec = value % i;
				ret += dec * factor;

				value /= i;
				factor *= 10;
			}
			return ret;
		}

		//Operator
		public static bool operator ==(CubeIndex a, CubeIndex b)
		{
			return a.EdgePermutation == b.EdgePermutation &&
				a.EdgeOrientation == b.EdgeOrientation &&
				a.CornerPermutation == b.CornerPermutation &&
				a.CornerOrientation == b.CornerOrientation;
		}

		public static bool operator !=(CubeIndex a, CubeIndex b)
		{
			return !(a == b);
		}

		public static bool operator <(CubeIndex a, CubeIndex b)
		{
			if (a.EdgePermutation != b.EdgePermutation) return a.EdgePermutation < b.EdgePermutation;
			if (a.CornerPermutation != b.CornerPermutation) return a.CornerPermutation < b.CornerPermutation;
			if (a.EdgeOrientation != b.EdgeOrientation) return a.EdgeOrientation < b.EdgeOrientation;

			return a.CornerOrientation < b.CornerOrientation;
		}
		public static bool operator >(CubeIndex a, CubeIndex b)
		{
			if (a.EdgePermutation != b.EdgePermutation) return a.EdgePermutation > b.EdgePermutation;
			if (a.CornerPermutation != b.CornerPermutation) return a.CornerPermutation > b.CornerPermutation;
			if (a.EdgeOrientation != b.EdgeOrientation) return a.EdgeOrientation > b.EdgeOrientation;

			return a.CornerOrientation > b.CornerOrientation;
		}

		//overrides
		public override string ToString()
		{
			return Index.ToString("000 000 000 000 000 000 000");
		}

		public override bool Equals(object obj)
		{
			return obj is CubeIndex other &&
				   EdgePermutation == other.EdgePermutation &&
				   CornerPermutation == other.CornerPermutation &&
				   EdgeOrientation == other.EdgeOrientation &&
				   CornerOrientation == other.CornerOrientation;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(EdgePermutation, CornerPermutation, EdgeOrientation, CornerOrientation);
		}

		public int CompareTo(CubeIndex other)
		{
			if (EdgePermutation != other.EdgePermutation) return EdgePermutation.CompareTo(other.EdgePermutation);
			if (CornerPermutation != other.CornerPermutation) return CornerPermutation.CompareTo(other.CornerPermutation);
			if (EdgeOrientation != other.EdgeOrientation) return EdgeOrientation.CompareTo(other.EdgeOrientation);

			return CornerOrientation.CompareTo(other.CornerOrientation);
		}
	}
}