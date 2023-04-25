using CubeAD.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Threading;

namespace CubeAD.CubeRepresentation
{
	public class TileCube : IComparable<TileCube>
	{
		public const int TILE_COUNT = 6 * 9;
		static int[,] DualSideToIndex = new int[6, 6]
		{
            //Orange
            {-1, -1, 1, 7, 3, 5},
            //Red
            {-1, -1, 10, 16, 14, 12},
            //Yellow
            {21, 23, -1, -1, 19, 25},
            //White
            {30, 32, -1, -1, 34, 28},
            //Blue
            {41, 39, 37, 43, - 1, -1},
            //Green
            {48, 50, 46, 52, -1, -1}
		};
		static int[,] CornerIndices = new int[6, 4]
		{
			{0, 2, 6, 8 },
			{11, 9, 17, 15},

			{18, 24, 20, 26},
			{33, 27, 35, 29},

			{38, 44, 36, 42},
			{45, 51, 47, 53}
		};
		static int[] CenterIndices = new int[6]
		{
			4, 13, 22, 31, 40, 49
		};

		//Maps 3 colors to a corner tile index
		static int[,,] FullCornerTileIndex = new int[6, 6, 6];
		static int[,,] FullCornerIndex = new int[6, 6, 6];
		static int[] IndexedCornerTileIndexLinear = new int[8 * 3];
		public static Corner[,,] CornerOrientColor = new Corner[8, 8, 3]; //(corner, pos, orient) -> (c1, c2, c3)

		//Maps 2 colors to an edge tile index
		static int[,] FullEdgeIndex = new int[6, 6];				// (color, color) -> tile index 
		static int[] IndexedEdgeTileIndexLinear = new int[12 * 2];	// (pos, index) -> tile index
		static int[,,] EdgeIsFlippedTable = new int[12, 6, 6];		// (pos, color, color) -> is oriented
		public static Edge[,,] EdgeOrientColor = new Edge[12, 12, 2];	// (edge, pos, orient) -> (c1, c2)

		static CubeColor[] ArrayBuffer = new CubeColor[TILE_COUNT];

		static int[][] SymmetryArrays = new int[48][];

		CubeColor[] Data = new CubeColor[TILE_COUNT];
		static int GetTileIndex(int side, int x, int y)
		{
			return side * 9 + x + 3 * y;
		}
		static int[][] MoveArray = new int[18][];
		static int[][] NonIdMoves = new int[18][];

		static readonly CubeColor[] SolvedArray = new CubeColor[TILE_COUNT];

		public static List<TileCube> GetAllCubes(int maxDepth)
		{
			List<TileCube> list = new List<TileCube>();

			Backtrack(GetSolved(), 0);

			return list;

			void Backtrack(TileCube cube, int depth)
			{
				list.Add(cube);

				if (depth == maxDepth)
					return;

				for (int i = 0; i < 18; i++)
				{
					Backtrack(new TileCube(cube, (CubeMove)i), depth + 1);
				}
			}
		}
		public static HashSet<TileCube> GetUniqueCubes(int maxDepth)
		{
			HashSet<TileCube> set = new HashSet<TileCube>();
			ulong counter = 0;

			int currentMaxDepth;
			for (currentMaxDepth = 0; currentMaxDepth <= maxDepth; currentMaxDepth++)
			{
				TileCube start = GetSolved();

				DFS(0, start, new MoveBlocker());
			}

			return set;

			void DFS(int depth, TileCube cube, MoveBlocker blocker)
			{
				set.Add(cube);
				if (++counter % 10_000_000 == 0)
				{
					Console.WriteLine("Sorted: " + set.Count.ToString("000 000 000 000"));
				}

				if (depth < currentMaxDepth)
				{
					for (int i = 0; i < 18; i++)
					{
						if (blocker[i / 3]) continue;

						DFS(depth + 1, new TileCube(cube, (CubeMove)i), new MoveBlocker(blocker, (CubeMove)i));
					}
				}
			}
		}
		public static HashSet<TileCube> GetUniqueSymCubes(int maxDepth)
		{
			HashSet<TileCube> set = new HashSet<TileCube>() { GetSolved() };
			List<TileCube> currentList = new List<TileCube>();
			List<TileCube> next = new List<TileCube>();

			TileCube lowSym = new TileCube();

			currentList.Add(GetSolved());
			for (int d = 0; d < maxDepth; d++)
			{
				next.Clear();
				foreach (var cube in currentList)
				{
					lowSym.CopyValuesFrom(cube);
					for (int i = 0; i < 18; i++)
					{
						lowSym.MakeMove((CubeMove)i);

						TileCube trans = lowSym.FindLowestSymmetry();

						if (set.Add(trans))
						{
							next.Add(trans);
						}

						lowSym.MakeMove(MoveSequenz.ReverseMove((CubeMove)i));
					}
				}

				(currentList, next) = (next, currentList);
			}

			return set;
		}
		static TileCube()
		{                               //6, 12
			int[,] affectedTiles = new int[,]
			{
                //Orange
                {33, 30, 27, 51, 48, 45, 24, 21, 18, 38, 41, 44},
                //Red
                {20, 23, 26, 47, 50, 53, 29, 32, 35, 42, 39, 36},
                //Yellow
                {0, 1, 2, 45, 46, 47, 9, 10, 11, 36, 37, 38},
                //White
                {44, 43, 42, 17, 16, 15, 53, 52, 51, 8, 7, 6 },
                //Blue
                {11, 14, 17, 35, 34, 33, 6, 3, 0, 18, 19, 20 },
                //Green
                {2, 5, 8, 27, 28, 29, 15, 12, 9, 26, 25, 24},
			};
			int[][] RotationArray = new int[3][]
			{
				new int[9]{6, 3, 0, 7, 4, 1, 8, 5, 2},
				new int[9]{2, 5, 8, 1, 4, 7, 0, 3, 6},
				new int[9]{8, 7, 6, 5, 4, 3, 2, 1, 0},
			};

			//Moves
			for (int side = 0; side < 6; side++)
			{
				int[] template = Enumerable.Range(0, TILE_COUNT).ToArray();

				for (int i = 0; i < 9; i++)
				{
					template[side * 9 + i] = RotationArray[0][i] + side * 9;
				}

				for (int i = 0; i < 12; i++)
				{
					template[affectedTiles[side, i]] = affectedTiles[side, (i + 3) % 12];
				}

				int[] buffer = template.ToArray();

				MoveArray[side * 3 + 0] = template.ToArray();

				for (int i = 0; i < template.Length; i++)
					template[i] = buffer[buffer[i]];
				MoveArray[side * 3 + 1] = template.ToArray();

				for (int i = 0; i < template.Length; i++)
					template[i] = buffer[buffer[buffer[i]]];
				MoveArray[side * 3 + 2] = template.ToArray();
			}

			//Move shortcuts
			for (int m = 0; m < 18; m++)
			{
				List<int> list = new List<int>();

				for (int i = 0; i < TILE_COUNT; i++)
				{
					if (MoveArray[m][i] != i)
					{
						list.Add(i);
					}
				}
				NonIdMoves[m] = list.ToArray();
			}

			//Symmetry
			for (int i = 0; i < 48; i++)
			{
				SymmetryElement se = SymmetryElement.Elements[i];
				int[] array = Enumerable.Range(0, TILE_COUNT).ToArray();

				for (int x = 0; x < 6; x++)
				{
					array[x * 9 + 4] = se.TransformColor(x) * 9 + 4;

					for (int y = 0; y < 6; y++)
					{
						if (x / 2 == y / 2)
							continue;
						array[GetEdgeColorIndex(x, y)] = GetEdgeColorIndex(se.TransformColor(x), se.TransformColor(y));


						for (int z = 0; z < 6; z++)
						{
							if (x / 2 == z / 2 || y / 2 == z / 2)
								continue;

							array[GetCornerColorIndex(x, y, z)] = GetCornerColorIndex(se.TransformColor(x), se.TransformColor(y), se.TransformColor(z));
						}
					}
				}



				SymmetryArrays[se.Index] = array;

			}

			//Solved array
			int index = 0;
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					SolvedArray[index++] = (CubeColor)i;
				}
			}

			//Edge/Corner shortcuts            
			for (int x = 0; x < 6; x++)
			{
				for (int y = 0; y < 6; y++)
				{
					FullEdgeIndex[x, y] = new SortedEdge((CubeColor)x, (CubeColor)y).GetIndex;
					for (int z = 0; z < 6; z++)
					{
						FullCornerTileIndex[x, y, z] = -1;

						if (x / 2 == y / 2 || z / 2 == x / 2 || z / 2 == y / 2) continue;

						FullCornerTileIndex[x, y, z] = GetCornerColorIndex(x, y, z);


						FullCornerIndex[x, y, z] = new SortedCorner((CubeColor)x, (CubeColor)y, (CubeColor)z).GetIndex;
					}
				}
			}

			//LinearCornerTileIndex
			index = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (y / 2 == x / 2) continue;

					for (int z = y + 1; z < 6; z++)
					{
						if (z / 2 == x / 2 || z / 2 == y / 2) continue;


						IndexedCornerTileIndexLinear[index * 3 + 0] = FullCornerTileIndex[x, y, z];
						IndexedCornerTileIndexLinear[index * 3 + 1] = FullCornerTileIndex[y, x, z];
						IndexedCornerTileIndexLinear[index * 3 + 2] = FullCornerTileIndex[z, x, y];

						index++;
					}
				}
			}

			//LinearEdgeTileIndex
			index = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (y / 2 == x / 2) continue;

					IndexedEdgeTileIndexLinear[index * 2 + 0] = DualSideToIndex[x, y];
					IndexedEdgeTileIndexLinear[index * 2 + 1] = DualSideToIndex[y, x];

					index++;
				}
			}

			//Edge Orientation table
			index = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (y / 2 == x / 2) continue;

					for (int c1 = 0; c1 < 6; c1++)
					{
						for (int c2 = c1 + 1; c2 < 6; c2++)
						{
							if (c2 / 2 == c1 / 2) continue;

                            //Console.WriteLine(index.ToString("00") + " (" + (CubeColor)x + " | " + (CubeColor)y + ") -> (" + (CubeColor)c1 + " | " + (CubeColor)c2 + ") -> " + IsFlipped());
                            EdgeIsFlippedTable[index, c1, c2] = Convert.ToInt32(IsFlipped());
							EdgeIsFlippedTable[index, c2, c1] = Convert.ToInt32(!IsFlipped());

							bool IsFlipped()
							{
								//if contains white or yellow
								if (c1 / 2 == 1 || c2 / 2 == 1)
								{
									if (x / 2 == 0)
										return c2 / 2 != 1;
									else
										return c1 / 2 != 1;
								}
								else
								{
									if (x / 2 == 0)
										return c2 / 2 != 2;
									else
										return c1 / 2 != 2;
								}
							}
						}
					}

					index++;
				}
			}

			//EdgeOrientTileIndices
			for(int p = 0; p < 12; p++)
			{
				for(int e = 0; e < 12; e++)
				{
					SortedEdge edge = SortedEdge.Edges[e];
					TileCube tc = GetSolved();

					tc.Data[IndexedEdgeTileIndexLinear[p * 2 + 0]] = edge.A;
					tc.Data[IndexedEdgeTileIndexLinear[p * 2 + 1]] = edge.B;

					bool isFlipped = tc.EdgeIsFlipped(p);

                    EdgeOrientColor[e, p, isFlipped ? 1 : 0] = new Edge(edge.A, edge.B);
					EdgeOrientColor[e, p, isFlipped ? 0 : 1] = new Edge(edge.B, edge.A);
				}
			}			

			//orange corners
			TileCube cube = GetSolved();
			DoMoveSeq(cube, 0);

			cube.Reset();
			cube.MakeMove(CubeMove.L);
			DoMoveSeq(cube, 1);

			cube.Reset();
			cube.MakeMove(CubeMove.LP);
			DoMoveSeq(cube, 2);

			cube.Reset();
			cube.MakeMove(CubeMove.L2);
			DoMoveSeq(cube, 3);

			//red corners
			cube.Reset();
			cube.MakeMove(CubeMove.B2);
			cube.MakeMove(CubeMove.LP);
			DoMoveSeq(cube, 4);

			cube.Reset();
			cube.MakeMove(CubeMove.D2);
			DoMoveSeq(cube, 5);

			cube.Reset();
			cube.MakeMove(CubeMove.B2);
			DoMoveSeq(cube, 6);

			cube.Reset();
			cube.MakeMove(CubeMove.R);
			cube.MakeMove(CubeMove.B2);
			DoMoveSeq(cube, 7);

            void DoMoveSeq(TileCube tc, int cornerIndex)
			{
				WritePos(tc, cornerIndex);

				foreach(CubeMove cm in MoveSequenz.CornerPermutation.Moves)
				{
					tc.MakeMove(cm);

					WritePos(tc, cornerIndex);	
				}
			}
			void WritePos(TileCube tc, int cornerIndex)
			{
				int pos = tc.GetCornerPerm()[cornerIndex];
				int orient = tc.CornerOrientaion(pos);

				Corner c = tc.GetCorner(pos);
				CornerOrientColor[cornerIndex, pos, orient] = c;
			}
		}

		public static int EdgeOrientAfterTransformation(int edgeIndex, int prevPos, int prevOrient, SymmetryElement se)
		{
			TileCube tc = GetSolved();

			Edge edge = EdgeOrientColor[edgeIndex, prevPos, prevOrient];
		
			tc.Data[IndexedEdgeTileIndexLinear[prevPos * 2 + 0]] = edge.A;
			tc.Data[IndexedEdgeTileIndexLinear[prevPos * 2 + 1]] = edge.B;

			TileCube buffer = new TileCube();
			tc.TransformSymmetry(se, buffer);

			return buffer.EdgeIsFlipped(SortedEdge.Edges[prevPos].Transform(se).GetIndex) ? 1 : 0;
		}
		public static int CornerOrientAfterTransformation(int cornerIndex, int prevPos, int prevOrient, SymmetryElement se)
		{
			TileCube tc = GetSolved();

			Corner corner = CornerOrientColor[cornerIndex, prevPos, prevOrient];

			tc.Data[IndexedCornerTileIndexLinear[prevPos * 3 + 0]] = corner.A;
			tc.Data[IndexedCornerTileIndexLinear[prevPos * 3 + 1]] = corner.B;
			tc.Data[IndexedCornerTileIndexLinear[prevPos * 3 + 2]] = corner.C;

            //Console.WriteLine(tc.SideView());

            TileCube buffer = new TileCube();
			tc.TransformSymmetry(se, buffer);
			//Console.WriteLine(buffer.SideView());
			SortedCorner sorted = SortedCorner.Corners[prevPos].Transform(se);

			return buffer.CornerOrientaion(sorted.GetIndex);
		}

		private TileCube()
		{

		}

		private void CopyValuesFrom(TileCube other)
		{
			Array.Copy(other.Data, Data, Data.Length);
		}
		public static TileCube GetSolved()
		{
			TileCube ret = new();
			ret.Reset();
			return ret;
		}

		public void Reset()
		{
			Array.Copy(SolvedArray, Data, TILE_COUNT);
		}
		public TileCube(TileCube src)
		{
			Array.Copy(src.Data, Data, TILE_COUNT);
		}
		public TileCube(TileCube src, CubeMove cm)
		{
			int[] move = MoveArray[(int)cm];

			for (int i = 0; i < TILE_COUNT; i++)
			{
				Data[move[i]] = src.Data[i];
			}
		}

		public void WritePerms()
		{
            Console.WriteLine("Edges Perm:    " + string.Join(" ", GetEdgePerm()));
			Console.WriteLine("Edges Orient:  " + string.Join(" ", GetEdgeOrientation()));
			Console.WriteLine("Corner Perm:   " + string.Join(" ", GetCornerPerm()));
			Console.WriteLine("Corner Orient: " + string.Join(" ", GetCornerOrientation()));
		}
		public TileCube(IndexCube src) : this(src.GetEdgePermutation(), src.GetEdgeOrientation(), src.GetCornerPermuation(), src.GetCornerOrientation())
		{
			
		}

		public TileCube(int[] edgeIndices, int[] edgeOrientations, int[] cornerIndices, int[] cornerOrientations)
		{


			for (int i = 0; i < CenterIndices.Length; i++)
				Data[CenterIndices[i]] = (CubeColor)i;

			int index = 0;
			for (int pos = 0; pos < 12; pos++)
			{
				int edge = edgeIndices[pos];
				var pair = EdgeOrientColor[edge, pos, edgeOrientations[pos]];

                //Console.WriteLine("Placing: " + pair);
                Data[IndexedEdgeTileIndexLinear[index++]] = pair.A;
				Data[IndexedEdgeTileIndexLinear[index++]] = pair.B;
			}

			index = 0;
			for (int pos = 0; pos < 8; pos++)
			{
				int corner = cornerIndices[pos];
				var pair = CornerOrientColor[corner, pos, cornerOrientations[pos]];

				Data[IndexedCornerTileIndexLinear[index++]] = pair.A;
				Data[IndexedCornerTileIndexLinear[index++]] = pair.B;
				Data[IndexedCornerTileIndexLinear[index++]] = pair.C;
			}
		}

		public void MakeMove(CubeMove cm)
		{
			Array.Copy(Data, ArrayBuffer, TILE_COUNT);

			int[] move = MoveArray[(int)cm];

			//Applies movement only to tiles that actually move f(x) != x
			foreach (int i in NonIdMoves[(int)cm])
			{
				Data[move[i]] = ArrayBuffer[i];
			}
		}

		public void ApplySequence(List<CubeMove> list)
		{
			foreach (CubeMove cm in list)
			{
				MakeMove(cm);
			}
		}

		#region Get Edge/Corner, Permutation
		public static int GetEdgeColorIndex(int side1, int side2)
		{
			return DualSideToIndex[side1, side2];
		}
		public static int GetEdgeColorIndex(Edge edge)
		{
			return DualSideToIndex[(int)edge.A, (int)edge.B];
		}
		public static int GetCornerColorIndex(int side1, int side2, int side3)
		{
			if (side2 > side3)
				(side2, side3) = (side3, side2);
			int index = side2 % 2 * 2 + side3 % 2;
			return CornerIndices[side1, index];
		}
		public static int GetCornerColorIndex(Corner corner)
		{
			return GetCornerColorIndex((int)corner.A, (int)corner.B, (int)corner.C);
		}

		public Edge GetEdge(int side1, int side2)
		{
			return new Edge(Data[GetEdgeColorIndex(side1, side2)], Data[GetEdgeColorIndex(side2, side1)]);
		}
		public Corner GetCorner(int side1, int side2, int side3)
		{
			return new Corner(Data[GetCornerColorIndex(side1, side2, side3)],
				Data[GetCornerColorIndex(side2, side1, side3)],
				Data[GetCornerColorIndex(side3, side1, side2)]);
		}
		public Corner GetCorner(int pos)
		{
			return new Corner(Data[IndexedCornerTileIndexLinear[3 * pos + 0]],
				Data[IndexedCornerTileIndexLinear[3 * pos + 1]],
				Data[IndexedCornerTileIndexLinear[3 * pos + 2]]);
		}


		public int[] GetEdgePerm()
		{
			int[] ret = new int[12];
			int index = 0;
			for (int i = 0; i < 12; i++)
			{
				ret[FullEdgeIndex[
					(int)Data[IndexedEdgeTileIndexLinear[index++]],
					(int)Data[IndexedEdgeTileIndexLinear[index++]]]] = i;
			}

			return ret;
		}
		public int[] GetCornerPerm()
		{
			int[] ret = new int[8];

			int index = 0;
			for (int i = 0; i < 8; i++)
			{
				ret[FullCornerIndex[
					(int)Data[IndexedCornerTileIndexLinear[index++]],
					(int)Data[IndexedCornerTileIndexLinear[index++]],
					(int)Data[IndexedCornerTileIndexLinear[index++]]]] = i;
			}

			return ret;
		}
		public int GetCornerPermIndex()
		{
			Span<int> perm = stackalloc int[8];

			int index = 0;
			for (int i = 0; i < 8; i++)
			{
				perm[FullCornerIndex[
					(int)Data[IndexedCornerTileIndexLinear[index++]],
					(int)Data[IndexedCornerTileIndexLinear[index++]],
					(int)Data[IndexedCornerTileIndexLinear[index++]]]] = i;
			}

			return Permutation.GetIndex(perm);
		}
		public int GetInverseCornerPermIndex()
		{
			Span<int> perm = stackalloc int[8];

			int index = 0;
			for (int i = 0; i < 8; i++)
			{
				perm[i] = FullCornerIndex[
					(int)Data[IndexedCornerTileIndexLinear[index++]],
					(int)Data[IndexedCornerTileIndexLinear[index++]],
					(int)Data[IndexedCornerTileIndexLinear[index++]]];
			}

			return Permutation.GetIndex(perm);
		}

		public int GetEdgePermIndex()
		{
			Span<int> perm = stackalloc int[12];
			int index = 0;
			for (int i = 0; i < 12; i++)
			{
				perm[FullEdgeIndex[
					(int)Data[IndexedEdgeTileIndexLinear[index++]],
					(int)Data[IndexedEdgeTileIndexLinear[index++]]]] = i;
			}

			return Permutation.GetIndex(perm);
		}
		public int GetInverseEdgePermIndex()
		{
			Span<int> perm = stackalloc int[12];
			int index = 0;
			for (int i = 0; i < 12; i++)
			{
				perm[i] = FullEdgeIndex[
					(int)Data[IndexedEdgeTileIndexLinear[index++]],
					(int)Data[IndexedEdgeTileIndexLinear[index++]]];
			}

			return Permutation.GetIndex(perm);
		}

		//Oriented green in front white on top
		public bool EdgeIsFlipped(int side1, int side2)
		{
			int pos = new SortedEdge((CubeColor)side1, (CubeColor)side2).GetIndex;

			return EdgeIsFlippedTable[pos,
					(int)Data[IndexedEdgeTileIndexLinear[pos * 2 + 0]],
					(int)Data[IndexedEdgeTileIndexLinear[pos * 2 + 1]]] != 0;
		}

		public bool EdgeIsFlipped(int pos)
		{
			return EdgeIsFlippedTable[pos,
					(int)Data[IndexedEdgeTileIndexLinear[pos * 2 + 0]],
					(int)Data[IndexedEdgeTileIndexLinear[pos * 2 + 1]]] != 0;
		}

		public ushort GetEdgeOrientationIndex()
		{
			int ret = 0;
			int index = 0;
			for (int i = 0; i < 12; i++)
			{
				ret = (ret << 1) | EdgeIsFlippedTable[i,
					(int)Data[IndexedEdgeTileIndexLinear[index++]],
					(int)Data[IndexedEdgeTileIndexLinear[index++]]];
			}

			return (ushort)ret;
		}

		public int CornerOrientaion(int side1, int side2, int side3)
		{
			if (!(side1 < side2 && side2 < side3))
				throw new ArgumentException("Sides need to be in ascending order");

			if ((int)Data[GetCornerColorIndex(side1, side2, side3)] / 2 == 0)
				return 0;

			if ((int)Data[GetCornerColorIndex(side2, side1, side3)] / 2 == 0)
				return 1;

			return 2;
		}
		public int CornerOrientaion(int index)
		{
			if ((int)Data[IndexedCornerTileIndexLinear[index * 3]] < 2)
				return 0;

			if ((int)Data[IndexedCornerTileIndexLinear[index * 3 + 1]] < 2)
				return 1;

			return 2;
		}

		public ushort GetCornerOrientationIndex()
		{
			int ret = 0;
			int index = 0;
			for (int i = 0; i < 8; i++)
			{
				int val = 2;
				if ((int)Data[IndexedCornerTileIndexLinear[index++]] < 2)
					val = 0;

				if ((int)Data[IndexedCornerTileIndexLinear[index]] < 2)
					val = 1;

				ret = ret * 3 + val;

				index += 2;
			}

			return (ushort)ret;
		}

		public int[] GetCornerOrientation()
		{
			return Enumerable.Range(0, 8).Select(x => CornerOrientaion(x)).ToArray();
		}
		public int[] GetEdgeOrientation()
		{
			return Enumerable.Range(0, 12).Select(x => EdgeIsFlipped(x) ? 1 : 0).ToArray();
		}

		public IndexCube GetIndexCube()
		{
			return new IndexCube((uint)GetEdgePermIndex(),
				(ushort)GetCornerPermIndex(), GetEdgeOrientationIndex(), GetCornerOrientationIndex());
		}
		#endregion

		#region Symmetries

		//|    Method |     Mean |     Error |    StdDev |
		//|---------- |---------:|----------:|----------:|
		//| LowestSym | 5.865 ms | 0.0589 ms | 0.0551 ms |
		public void TransformSymmetry(SymmetryElement se, TileCube ret)
		{
			int[] perm = SymmetryArrays[se.Index];

			for (int i = 0; i < TILE_COUNT; i++)
			{
				ret.Data[perm[i]] = se.TransformColor(Data[i]);
			}
		}
		public TileCube FindLowestSymmetry()
		{
			TileCube best = new TileCube(this);
			TileCube buffer = new TileCube();

			foreach (var se in SymmetryElement.Elements)
			{
				TransformSymmetry(se, buffer);

				if (buffer.CompareTo(best) < 0)
				{
					best = new TileCube(buffer);
				}
			}

			return best;
		}

		public TileCube FindLowestSymmetryInverse()
		{
			TileCube best = FindLowestSymmetry();

			TileCube inverse;
			TileCube buffer = new TileCube();
			foreach (var se in SymmetryElement.Elements)
			{
				TransformSymmetry(se, buffer);

				inverse = buffer.GetInverse();

				if (inverse.CompareTo(best) < 0)
				{
					best = inverse;
				}
			}

			return best;
		}

		public TileCube GetInverse()
		{
			TileCube ret = new TileCube(this);

			for (int x = 0; x < 6; x++)
			{
				for (int y = 0; y < 6; y++)
				{
					if (y / 2 == x / 2) continue;

					ret.Data[GetEdgeColorIndex(GetEdge(x, y))] = (CubeColor)x;

					for (int z = 0; z < 6; z++)
					{
						if (z / 2 == x / 2 || z / 2 == y / 2)
							continue;


						ret.Data[GetCornerColorIndex(GetCorner(x, y, z))] = (CubeColor)x;
					}
				}
			}

			return ret;
		}

		public static int[] SymmetryPerm(SymmetryElement se)
		{
			int[] ret = new int[12];

			for (int i = 0; i < 12; i++)
			{
				SortedEdge edge = SortedEdge.Edges[i];
				ret[i] = new SortedEdge(se.TransformColor(edge.A), se.TransformColor(edge.B)).GetIndex;	
			}

			return ret;
		}
		#endregion
		public string SideView()
		{
			int[] sides = new int[] { 0, 5, 1, 4 };
			string[] tiles = new string[]
			{
				"o", "r", "y", "w", "b", "g", "n"
			};

			string s = "#color_viewer(0.6cm,\n";

			for (int y = 3 - 1; y >= 0; y--)
			{
				for (int i = 0; i < 3; i++)
					s += "n, ";

				for (int x = 0; x < 3; x++)
				{
					s += tiles[(int)Data[GetTileIndex(3, x, y)]] + ", ";
				}

				for (int i = 0; i < 6; i++)
					s += "n, ";

				s += "\n";
			}
			for (int y = 3 - 1; y >= 0; y--)
			{
				for (int i = 0; i < sides.Length; i++)
				{
					for (int x = 0; x < 3; x++)
					{
						s += tiles[(int)Data[GetTileIndex(sides[i], x, y)]] + ", ";
					}
				}

				s += "\n";
			}

			for (int y = 3 - 1; y >= 0; y--)
			{
				for (int i = 0; i < 3; i++)
					s += "n, ";

				for (int x = 0; x < 3; x++)
				{
					s += tiles[(int)Data[GetTileIndex(2, x, y)]] + ", ";
				}

				for (int i = 0; i < 6; i++)
					s += "n, ";

				s += "\n";
			}


			return s + ")";

		}
		public static bool operator ==(TileCube a, TileCube b)
		{
			return a.Data.AsSpan().SequenceEqual(b.Data);
		}
		public static bool operator !=(TileCube a, TileCube b)
		{
			return !(a == b);
		}
		public int CompareTo(TileCube other)
		{
			unsafe
			{
				fixed (void* ptr = Data)
				{
					fixed(void* oth = other.Data)
					{
						
						for (int i = 0; i < TILE_COUNT / 4; i++)
						{
							int dif = ((int*)ptr)[i] - ((int*)oth)[i];
							
							if(dif != 0)
								return dif;
						}

					}
				}
			}

			for (int i = TILE_COUNT - (TILE_COUNT % 4); i < TILE_COUNT; i++)
			{
				if (Data[i] != other.Data[i])
				{
					return Data[i].CompareTo(other.Data[i]);
				}
			}

			return 0;
		}

		public override int GetHashCode()
		{
			int hash = 0;
			unsafe
			{
				fixed (void* ptr = Data)
				{
					for (int i = 0; i < TILE_COUNT / 4; i++)
					{
						hash = HashCode.Combine(hash, ((int*)ptr)[i]);
					}
				}
			}

			return hash;
		}
		public override string ToString()
		{
			int index = 0;
			string s = "";
			for (int side = 0; side < 6; side++)
			{
				for (int i = 0; i < 9; i++)
				{
					s += (int)Data[index++] + " ";
				}
				s += "\n";
			}

			return s;
		}

		public override bool Equals(object obj)
		{
			return this == (TileCube)obj;
		}
	}
}
