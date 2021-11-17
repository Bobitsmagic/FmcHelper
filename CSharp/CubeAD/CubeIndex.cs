using CubeAD.CubeIndexSets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace CubeAD
{
	//Compressed representation of a cube
	public struct CubeIndex : IComparable<CubeIndex>
	{
		const int MAX_EO_MOVE_DEPTH = 8;
		const int MAX_MOVE_DEPTH = 6;

		public const int SIZE_IN_BYTES = 11;
		public const int PADDED_SIZE_IN_BYTES = 12;

		public const uint MAX_EDGE_PERMUTATION = 479_001_600; //12!
		public const ushort MAX_CORNER_PERMUTATION = 40320; //8!
		public const ushort MAX_EDGE_ORIENTATION = 4096;    //2^12
		public const ushort MAX_CORNER_ORIENTATION = 6561;  //3^8

		public static string PRE_COMP_PATH = Directory.GetCurrentDirectory() + "\\PreCompFiles";

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

		public static byte[] OrientedEdgeCount;
		public static byte[] OrientedCornersCount;
		public static byte[] PositionedEdgesCount;
		public static byte[] PositionedCornersCount;

		public static SealedHashset GetEdgeOrientedMoveTree 
		{ get
			{
				if (_EOMoveTree is null)
					_EOMoveTree = new SealedHashset(Path.Combine(Directory.GetCurrentDirectory(), "PreCompFiles", "solved_eo_tree_" + MAX_EO_MOVE_DEPTH + ".bin"));

				return _EOMoveTree;
			} 
		}
		public static SealedHashset GetMoveTree
		{
			get
			{
				if (_MoveTree is null)
					_MoveTree = new SealedHashset(Path.Combine(Directory.GetCurrentDirectory(), "PreCompFiles", "solved_tree_" + MAX_MOVE_DEPTH + ".bin"));

				return _MoveTree;
			}
		}

		private static SealedHashset _EOMoveTree = null;
		private static SealedHashset _MoveTree = null;

		static CubeIndex()
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

					buffer.CopyValuesFrom(c);
					buffer.MakeMove((CubeMove)j);

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
			Console.WriteLine("Cpu stuff in " + sw.ElapsedMilliseconds + " ms");


			for (int i = 0; i < 18; i += 3)
			{
				CubeMove m = (CubeMove)i;
				byte[] byteArray = File.ReadAllBytes(PRE_COMP_PATH + "\\" + m + ".bin");
				NextEdgePerm[i / 3] = new uint[byteArray.Length / 4];

				Buffer.BlockCopy(byteArray, 0, NextEdgePerm[i / 3], 0, byteArray.Length);
			}

			OrientedEdgeCount = File.ReadAllBytes(PRE_COMP_PATH + "\\oriented_edge_count.bin");
			OrientedCornersCount = File.ReadAllBytes(PRE_COMP_PATH + "\\oriented_corner_count.bin");
			PositionedEdgesCount = File.ReadAllBytes(PRE_COMP_PATH + "\\positioned_edge_count.bin");
			PositionedCornersCount = File.ReadAllBytes(PRE_COMP_PATH + "\\positioned_corner_count.bin");


			GC.Collect();
			Console.WriteLine("TotalRamUsage: " + GC.GetTotalMemory(true).ToString("0 000 000 000"));

			Console.WriteLine("Initialized CubeIndex class in " + sw.ElapsedMilliseconds + " ms");
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

			for (int k = 0; k < 18; k++)
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


				for (int i = 0; i < data.Length; i++)
				{
					GetIndexedPerm(currentPerm, i);

					for (int j = 0; j < 12; j++)
					{
						result[j] = currentPerm[move[j]];
					}

					data[i] = GetIndex(result);
				}

				Buffer.BlockCopy(data, 0, byteArray, 0, byteArray.Length);
				File.WriteAllBytes(Directory.GetCurrentDirectory() + "\\" + m + ".bin", byteArray);
			}
		}

		public static void GenerateTreeFile(int depth)
		{
			new SealedHashset(SolvedTree(depth).GetArray()).SaveToFile(Path.Combine(Directory.GetCurrentDirectory(), "solved_tree_" + depth + ".bin"));

			Console.WriteLine("Saved file: solvedTree_" + depth + ".bin");
		}
		public static void GenerateEOTreeFile(int depth)
		{
			new SealedHashset(SolvedTreeOrientedEdges(depth).GetArray()).SaveToFile(Path.Combine(Directory.GetCurrentDirectory(), "solved_eo_Tree_" + depth + ".bin"));

			Console.WriteLine("Saved file: solvedEOtree_" + depth + ".bin");
		}

		public static void GenerateSolvedEdgeSet(int depth)
		{
			var set = SolvedTreeOrientedEdges(depth);

			set.SaveData(Directory.GetCurrentDirectory() + "\\solvedEOset_" + depth + ".bin");

			Console.WriteLine("Created set");
		}

		public static void GenerateSolvedCounts()
		{
			OrientedEdgeCount = new byte[MAX_EDGE_ORIENTATION];
			OrientedCornersCount = new byte[MAX_CORNER_ORIENTATION];
			PositionedEdgesCount = new byte[MAX_EDGE_PERMUTATION];
			PositionedCornersCount = new byte[MAX_CORNER_PERMUTATION];


			for (int i = 0; i < MAX_EDGE_ORIENTATION; i++)
				OrientedEdgeCount[i] = (byte)NumberOfSetBits(i);

			File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "oriented_edge_count.bin"), OrientedEdgeCount);
			Console.WriteLine("Created oriented edge count file");

			int[] cornerDigits = new int[8];
			for (int i = 0; i < MAX_CORNER_ORIENTATION; i++)
			{
				int count = 0;
				for (int j = 0; j < 8; j++)
				{
					if (cornerDigits[j] == 0)
						count++;
				}


				OrientedCornersCount[i] = (byte)count;

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

			File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "oriented_corner_count.bin"), OrientedCornersCount);
			Console.WriteLine("Created oriented corner count file");

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

				PositionedEdgesCount[i] = (byte)count;
			}

			File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "positioned_corner_count.bin"), PositionedCornersCount);
			Console.WriteLine("Created positioned corner count file");

			permBuffer = new int[12];
			for (int i = 0; i < MAX_EDGE_PERMUTATION; i++)
			{
				GetIndexedPerm(permBuffer, i);

				int count = 0;
				for (int j = 0; j < 12; j++)
				{
					if (permBuffer[j] == j)
						count++;

				}
					if(i % 10_000_000 == 0)
						Console.WriteLine(i.ToString("000 000 000"));

				PositionedEdgesCount[i] = (byte)count;
			}

			File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "positioned_edge_count.bin"), PositionedEdgesCount);
			Console.WriteLine("positoned oriented edge count file");

			int NumberOfSetBits(int value)
			{
				value = value - ((value >> 1) & 0x55555555);
				value = (value & 0x33333333) + ((value >> 2) & 0x33333333);
				return (((value + (value >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
			}
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
		public CubeMove LastMove;


		public CubeIndex(uint edgePermutation, ushort cornerPermutation, ushort edgeOrientation, ushort cornerOrientation, CubeMove move = CubeMove.None)
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
			LastMove = move;

		}
		public CubeIndex(Random rnd)
		{
			EdgePermutation = (uint)rnd.Next((int)MAX_EDGE_PERMUTATION);
			CornerPermutation = (ushort)rnd.Next(MAX_CORNER_PERMUTATION);
			EdgeOrientation = (ushort)rnd.Next(MAX_EDGE_ORIENTATION);
			CornerOrientation = (ushort)rnd.Next(MAX_CORNER_ORIENTATION);
			LastMove = (CubeMove)rnd.Next(18);
		}

		public CubeIndex(CubeIndex old, CubeMove m)
		{
			EdgePermutation = old.EdgePermutation;
			CornerPermutation = old.CornerPermutation;
			EdgeOrientation = old.EdgeOrientation;
			CornerOrientation = old.CornerOrientation;
			LastMove = CubeMove.None;
			MakeMove(m);
		}

		public CubeIndex(Cube c)
		{
			EdgePermutation = FindEdgePermutationIndex(c);
			CornerPermutation = FindCornerPermutationIndex(c);
			EdgeOrientation = FindEdgeOrientationIndex(c);
			CornerOrientation = FindCornerOrientationIndex(c);

			LastMove = CubeMove.None;
		}

		public CubeIndex(MoveSequenz ms)
		{
			EdgePermutation = 0;
			CornerPermutation = 0;
			EdgeOrientation = 0;
			CornerOrientation = 0;
			LastMove = CubeMove.None;

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
			LastMove = (CubeMove)br.ReadByte();
		}
		public void Write(BinaryWriter bw)
		{
			bw.Write(EdgePermutation);
			bw.Write(CornerPermutation);
			bw.Write(EdgeOrientation);
			bw.Write(CornerOrientation);
			bw.Write((byte)LastMove);
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

						c.SetCornerColor(x, y, z, CornerColors[cornerPerm[counter], ((flip ? 2 : 1) * (0 + 2 * cornerOrient[counter])) % 3]);
						c.SetCornerColor(y, x, z, CornerColors[cornerPerm[counter], ((flip ? 2 : 1) * (1 + 2 * cornerOrient[counter])) % 3]);
						c.SetCornerColor(z, x, y, CornerColors[cornerPerm[counter], ((flip ? 2 : 1) * (2 + 2 * cornerOrient[counter])) % 3]);

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

			LastMove = m;
		}

		public void ApplyMoveSequenz(MoveSequenz ms)
		{
			for (int i = 0; i < ms.Length; i++)
			{
				MakeMove(ms.Moves[i]);
			}
		}
		public void ApplyMoveSequenz(List<CubeMove> ms)
		{
			for (int i = 0; i < ms.Count; i++)
			{
				MakeMove(ms[i]);
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
		public static void RadixSortCubeIndices(List<CubeIndex> data, List<CubeIndex> CubeBuffer)
		{
			const int BIT_COUNT = 8;
			const int RADIX = 1 << BIT_COUNT;
			int N = data.Count;

			int[] indices = new int[RADIX];

			if (CubeBuffer.Count < N)
			{
				CubeBuffer.Clear();
				CubeBuffer.AddRange(data);
				//Console.WriteLine("Reallocated");
			}

			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < RADIX; j++)
					indices[j] = 0;
				for (int j = 0; j < N; j++)
					indices[data[j][i]]++;

				for (int j = 1; j < RADIX; j++)
					indices[j] += indices[j - 1];
				for (int j = N - 1; j >= 0; j--)
					CubeBuffer[--indices[data[j][i]]] = data[j];

				var swap = CubeBuffer;
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

			for (int i = 0; i <= MAX_SEARCH_DEPTH && ret.Count == 0; i++)
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
					for (int i = 0; i < 18; i++)
					{
						currentMoves.Push((CubeMove)i);
						DFS(depth + 1, NextEdgeOrient[orientation, i]);
						currentMoves.Pop();
					}
				}
			}
		}

		public static SortedBuckets SolvedTree(int maxDepth)
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(maxDepth);
			SortedBuckets set = new SortedBuckets(2_000);

			//HashSet<CubeIndex> set = new HashSet<CubeIndex>();

			ulong counter = 0;

			int currentMaxDepth = 0;
			for (currentMaxDepth = 0; currentMaxDepth <= maxDepth; currentMaxDepth++)
			{
				DFS(0, new CubeIndex(), new MoveBlocker());
				set.RemoveDuplicates();
			}

			set.RemoveDuplicates();
			return set;

			void DFS(int depth, CubeIndex cube, MoveBlocker blocker)
			{
				set.Add(cube);
				if (counter++ % 100_000_000 == 0)
				{
					set.RemoveDuplicates();
					//Console.WriteLine("Sorted: " + set.Count.ToString("000 000 000 000"));
				}

				if (depth < currentMaxDepth)
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

		public static HashSet<CubeIndex> SolvedTreeOrientedEdgesSet(int maxDepth)
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(maxDepth);
			HashSet<CubeIndex> set = new HashSet<CubeIndex>();

			//HashSet<CubeIndex> set = new HashSet<CubeIndex>();

			int currentMaxDepth = 0;
			for (currentMaxDepth = 0; currentMaxDepth <= maxDepth; currentMaxDepth++)
			{
				DFS(0, new CubeIndex(), new MoveBlocker());
			}
			return set;

			void DFS(int depth, CubeIndex cube, MoveBlocker blocker)
			{
				set.Add(cube);

				if (depth < currentMaxDepth)
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
		public static SortedBuckets SolvedTreeOrientedEdges(int maxDepth)
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(maxDepth);
			SortedBuckets set = new SortedBuckets(2_000);

			//HashSet<CubeIndex> set = new HashSet<CubeIndex>();

			ulong counter = 0;

			int currentMaxDepth = 0;
			for (currentMaxDepth = 0; currentMaxDepth <= maxDepth; currentMaxDepth++)
			{
				DFS(0, new CubeIndex(), new MoveBlocker());
				set.RemoveDuplicates();
			}

			set.RemoveDuplicates();
			return set;

			void DFS(int depth, CubeIndex cube, MoveBlocker blocker)
			{
				set.Add(cube);
				if (counter++ % 100_000_000 == 0)
				{
					set.RemoveDuplicates();
					//Console.WriteLine("Sorted: " + set.Count.ToString("000 000 000 000"));
				}

				if (depth < currentMaxDepth)
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

		public static MoveSequenz FindSolutionWithEO(CubeIndex cube)
		{
			const int MAX_DEPTH = 10;

			List<MoveSequenz> eoMoves = FindEdgeOrientationMoves(cube.EdgeOrientation);
			Console.WriteLine("Eo Length: " + eoMoves[0].Length + " count: " + eoMoves.Count);
			List<CubeMove> solution = new List<CubeMove>();

			bool foundSolution = false;

			//foreach (MoveSequenz ms in eoMoves)
			//{
			//	CubeIndex copy = cube;

			//	copy.ApplyMoveSequenz(ms);

			//	if (FindEOSolution(copy))
			//	{
			//		solution.InsertRange(0, ms.Moves);
			//		break;
			//	}
			//}

			int currentMaxDepth = 0;

			for (currentMaxDepth = 0; currentMaxDepth <= MAX_DEPTH && !foundSolution; currentMaxDepth++)
			{
				//Console.WriteLine("MaxSearchDepth: " + currentMaxDepth);
				Parallel.ForEach(eoMoves, ms =>
				{
					CubeIndex copy = cube;

					copy.ApplyMoveSequenz(ms);

					if (FindEOSolution(copy))
					{
						solution.InsertRange(0, ms.Moves);
						//Console.WriteLine("EO: " + ms);
					}
				});
			}

			Console.WriteLine("Finished all threads");
			Console.WriteLine("Solution length: " + solution.Count);
			return new MoveSequenz(solution);


			bool FindEOSolution(CubeIndex cube)
			{
				Stack<CubeMove> currentMoves = new Stack<CubeMove>(20);

				return DFS(0, cube, new MoveBlocker());

				bool DFS(int depth, CubeIndex cube, MoveBlocker blocker)
				{
					if (foundSolution) return false;

					if (GetEdgeOrientedMoveTree.Contains(cube))
					{
						if (!foundSolution)
						{
							foundSolution = true;

							//Console.WriteLine("Found solution!");
							solution.Clear();

							//Console.WriteLine("DFS moves: " + string.Join(" ", currentMoves.Reverse()));

							solution.AddRange(currentMoves.Reverse());
							solution.AddRange(FindSolutionInEOTree(cube));

							return true;
						}

						return false;
					}

					if (depth < currentMaxDepth)
					{
						for (int i = 0; i < 18; i++)
						{
							if (blocker[i / 3]) continue;
							if (i / 3 > 3 && i % 3 != 1) continue;

							currentMoves.Push((CubeMove)i);
							if (DFS(depth + 1, new CubeIndex(cube, (CubeMove)i), new MoveBlocker(blocker, i / 3)))
								return true;
							currentMoves.Pop();
						}
					}

					return false;
				}


			}
		}

		public static List<CubeMove> FindSolutionInEOTree(CubeIndex cube)
		{
			List<CubeMove> list = new List<CubeMove>();

			while (!cube.IsSovled)
			{
				if (GetEdgeOrientedMoveTree.TryGetValue(cube, out cube))
				{
					if (!cube.IsSovled)
					{
						list.Add(MoveSequenz.ReverseMove(cube.LastMove));
						cube.MakeMove(MoveSequenz.ReverseMove(cube.LastMove));
					}
				}
				else
				{
					Console.WriteLine("Cube not in EO set");
					return null;
				}
			}

			//Console.WriteLine("Tree solution: " + string.Join(" " , list));

			return list;
		}

		public static MoveSequenz FindSolutionBruteForce(CubeIndex cube)
		{			
			List<CubeMove> solution = new List<CubeMove>();
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(20);

			int currentMaxDepth;
			for (int i = 0; i <= 20; i++)
			{
				currentMaxDepth = i;
				Console.WriteLine(currentMaxDepth);
				if(DFS(0, cube, new MoveBlocker()))
				{
					break;
				}
			}


			Console.WriteLine("Solution length: " + solution.Count);
			return new MoveSequenz(solution);


			bool DFS(int depth, CubeIndex cube, MoveBlocker blocker)
			{
				if (GetMoveTree.Contains(cube))
				{
					//Console.WriteLine("Found solution!");
					solution.Clear();

					//Console.WriteLine("DFS moves: " + string.Join(" ", currentMoves.Reverse()));

					solution.AddRange(currentMoves.Reverse());
					solution.AddRange(FindSolutionInTree(cube));

					Console.WriteLine(string.Join(" ", solution));

					Console.WriteLine("Kek");

					return true;
				}

				if (depth < currentMaxDepth)
				{
					for (int i = 0; i < 18; i++)
					{
						if (blocker[i / 3]) continue;

						currentMoves.Push((CubeMove)i);
						if (DFS(depth + 1, new CubeIndex(cube, (CubeMove)i), new MoveBlocker(blocker, i / 3)))
							return true;
						currentMoves.Pop();
					}
				}

				return false;
			}


			
		}

		public static List<CubeMove> FindSolutionInTree(CubeIndex cube)
		{
			List<CubeMove> list = new List<CubeMove>();

			while (!cube.IsSovled)
			{
				if (GetMoveTree.TryGetValue(cube, out cube))
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

			//Console.WriteLine("Tree solution: " + string.Join(" " , list));

			return list;
		}

		public static MoveSequenz FindCommutator(CubeIndex cube, int maxDepth)
		{
			CubeIndex copy = new CubeIndex();

			Stack<CubeMove> currentMoves = new Stack<CubeMove>();
			List<CubeMove> ret = new List<CubeMove>();
			MoveSequenz ms = new MoveSequenz();
			int bestSolution = int.MaxValue;
			int currentMaxLength;
			for(currentMaxLength = 2; currentMaxLength < maxDepth; currentMaxLength++)
			{

				Console.WriteLine("MaxSearchDepth: " + currentMaxLength);
				GenerateMoveSeq(0, cube, new MoveBlocker());
			}

			if(bestSolution < int.MaxValue)
			{
				return new MoveSequenz(ret);
			}

			return null;

			void GenerateMoveSeq(int depth, CubeIndex cube, MoveBlocker blocker)
			{
				if(depth == currentMaxLength)
				{
					CubeMove[] moves = currentMoves.Reverse().ToArray();
					List<CubeMove> list = new List<CubeMove>(moves.Length * 2);

					for (int setupLength = 0; setupLength < 2; setupLength++)
					{
						for (int firstLength = 1; firstLength < moves.Length; firstLength++)
						{
							int secondLength = moves.Length - setupLength - firstLength;

							if (setupLength + firstLength + secondLength != moves.Length) 
								Console.WriteLine("Alarm");

							for (int i = firstLength - 1; i >= 0; i--)
								cube.MakeMove(MoveSequenz.ReverseMove(moves[setupLength + i]));

							for (int i = secondLength - 1; i >= 0; i--)
								cube.MakeMove(MoveSequenz.ReverseMove(moves[setupLength + firstLength + i]));

							for (int i = 0; i < setupLength; i++)
								cube.MakeMove(moves[i]);

							if (cube.IsSovled)
							{
								list.Clear();
								list.AddRange(moves);

								for (int i = firstLength - 1; i >= 0; i--)
									list.Add(MoveSequenz.ReverseMove(moves[setupLength + i]));

								for (int i = secondLength - 1; i >= 0; i--)
									list.Add(MoveSequenz.ReverseMove(moves[setupLength + firstLength + i]));

								for (int i = 0; i < setupLength; i++)
									list.Add(MoveSequenz.ReverseMove(moves[i]));

								ms.Moves.Clear();
								ms.Moves.AddRange(list);

								ms.Reduce();

								if(ms.Length < bestSolution)
								{
									bestSolution = ms.Length;

									string s = "Setup: ";
									for (int i = 0; i < setupLength; i++)
										s += moves[i].ToString() + " ";

									s += "\nFirst: ";
									for (int i = 0; i < firstLength; i++)
										s += moves[i + setupLength].ToString() + " ";

									s += "\nSecond: ";
									for (int i = 0; i < secondLength; i++)
										s += moves[i + setupLength + firstLength].ToString() + " ";

									Console.WriteLine(s);

									Console.WriteLine("Full: " + ms + " [" + ms.Moves.Count + "]");
								}
							}
						}
					}
				}
				else 
				{
					for (int i = 0; i < 18; i++)
					{
						if (blocker[i / 3]) continue;

						currentMoves.Push((CubeMove)i);
						GenerateMoveSeq(depth + 1, new CubeIndex(cube, (CubeMove)i), new MoveBlocker(blocker, i / 3, true));

						currentMoves.Pop();
					}
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