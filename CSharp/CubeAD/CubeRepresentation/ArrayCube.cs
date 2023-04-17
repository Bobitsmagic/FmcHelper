using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using CubeAD.Pieces;

namespace CubeAD.CubeRepresentation
{
    public class ArrayCube : IComparable<ArrayCube>
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

        //Maps 3 colors to a corner tile index
        static int[,,] FullCornerTileIndex = new int[6, 6, 6];
		static int[,,] FullCornerIndex = new int[6, 6, 6]; 
		static int[] IndexedCornerTilesIndexLinear = new int[8 * 3];

		//Maps 2 colors to an edge tile index
		static int[,] FullEdgeIndex = new int[6, 6];
        static int[] IndexedEdgeTileIndexLinear = new int[12 * 2];
        static int[,,] EdgeIsOrientedTable = new int[12, 6, 6]; // pos, color, color
        

		static CubeColor[] ArrayBuffer = new CubeColor[TILE_COUNT];

        static int[][] SymmetryArrays = new int[48][];

        CubeColor[] Data = new CubeColor[TILE_COUNT];
        static int GetTileIndex(int side, int x, int y)
        {
            return side * 9 + x + 3 * y;
        }
        static int[][] MoveArray = new int[18][];
        static int[][] NonIdMoves = new int[18][];
        static int[][][] MoveCycles = new int[18][][];

        static readonly CubeColor[] SolvedArray = new CubeColor[TILE_COUNT];

        public static List<ArrayCube> GetAllCubes(int maxDepth)
        {
            List<ArrayCube> list = new List<ArrayCube>();

            Backtrack(GetSolved(), 0);

            return list;

            void Backtrack(ArrayCube cube, int depth)
            {
                list.Add(cube);

                if (depth == maxDepth)
                    return;

                for(int i = 0; i < 18; i++)
                {
                    Backtrack(new ArrayCube(cube, (CubeMove)i), depth + 1);
                }
            }
        }
        public static HashSet<ArrayCube> GetUniqueCubes(int maxDepth)
        {
            HashSet<ArrayCube> list = new HashSet<ArrayCube>();

            Backtrack(GetSolved(), 0);

            return list;

            void Backtrack(ArrayCube cube, int depth)
            {
                if (!list.Add(cube))
                    return;

                if (depth == maxDepth)
                    return;

                for (int i = 0; i < 18; i++)
                {
                    Backtrack(new ArrayCube(cube, (CubeMove)i), depth + 1);
                }
            }
        }

        static ArrayCube()
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

                for(int i = 0; i < TILE_COUNT; i++)
                {
                    if (MoveArray[m][i] != i)
                    {
                        list.Add(i);
                    }
                }
                NonIdMoves[m] = list.ToArray();

                List<int[]> foundCycle = new List<int[]>();
                bool[] visited = new bool[TILE_COUNT];
                for (int i = 0; i < TILE_COUNT; i++)
                {
                    if (!visited[i])
                    {

                        list.Clear();
                        int start = i;
                        int current = MoveArray[m][start];
                        list.Add(start);

                        while(start != current)
                        {
                            list.Add(current);
                            current = MoveArray[m][current];
                        }

                        foreach(int idx in list)
                            visited[idx] = true;

                        if(list.Count > 1)
                        {
                            foundCycle.Add(list.ToArray());
                        }
                    }

                    MoveCycles[m] = foundCycle.ToArray();
                }
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

			index = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (y / 2 == x / 2) continue;

					for (int z = y + 1; z < 6; z++)
					{
						if (z / 2 == x / 2 || z / 2 == y / 2) continue;


                        IndexedCornerTilesIndexLinear[index * 3 + 0] = FullCornerTileIndex[x, y, z];
						IndexedCornerTilesIndexLinear[index * 3 + 1] = FullCornerTileIndex[y, x, z];
						IndexedCornerTilesIndexLinear[index * 3 + 2] = FullCornerTileIndex[z, x, y];

						index++;
					}
				}
			}

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

			//orientation table
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

                            EdgeIsOrientedTable[index, c1, c2] = Convert.ToInt32(IsOriented());
							EdgeIsOrientedTable[index, c2, c1] = Convert.ToInt32(!IsOriented());

							bool IsOriented()
							{
								//if contains white or yellow
								if (c1 / 2 == 1 || c2 / 2 == 1)
								{
									if (x / 2 == 0)
										return c2 / 2 == 1;
									else
										return c1 / 2 == 1;
								}
								else
								{
									if (y / 2 == 0)
										return c2 / 2 == 2;
									else
										return c1 / 2 == 2;
								}
							}
						}
					}

                    index++;
				}
			}
		}

        private ArrayCube()
        {
            
        }

        public static ArrayCube GetSolved()
        {
            ArrayCube ret = new();
            ret.Reset();
            return ret;
        }

        public void Reset()
        {
            Array.Copy(SolvedArray, Data, TILE_COUNT);
        }

        public ArrayCube(ArrayCube src)
        {
            Array.Copy(src.Data, Data, TILE_COUNT);
        }

        public ArrayCube(ArrayCube src, CubeMove cm)
        {
            int[] move = MoveArray[(int)cm];

            for (int i = 0; i < TILE_COUNT; i++)
            {
                Data[move[i]] = src.Data[i];
            }
        }

        public void MakeMove(CubeMove cm)
        {
            Array.Copy(Data, ArrayBuffer, TILE_COUNT);

            int[] move = MoveArray[(int)cm];

            //Applies movement only to tiles that actually move f(x) != x
            foreach(int i in NonIdMoves[(int)cm])
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

		public int[] GetEdgePerm()
		{
			int[] ret = new int[12];
            int index = 0;
            for(int i = 0; i < 12; i++)
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
					(int)Data[IndexedCornerTilesIndexLinear[index++]],
					(int)Data[IndexedCornerTilesIndexLinear[index++]],
					(int)Data[IndexedCornerTilesIndexLinear[index++]]]] = i;
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
					(int)Data[IndexedCornerTilesIndexLinear[index++]],
					(int)Data[IndexedCornerTilesIndexLinear[index++]],
					(int)Data[IndexedCornerTilesIndexLinear[index++]]]] = i;
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
		public bool EdgeIsOriented(int side1, int side2)
        {
            if (side2 < side1)
                (side1, side2) = (side2, side1);

            int c1 = (int)Data[GetEdgeColorIndex(side1, side2)];
            int c2 = (int)Data[GetEdgeColorIndex(side2, side1)];

            //if contains white or yellow
            if (c1 / 2 == 1 || c2 / 2 == 1)
            {
                if (side1 / 2 == 0)
                    return c2 / 2 == 1;
                else
                    return c1 / 2 == 1;
            }
            else
            {
                if (side1 / 2 == 0)
                    return c2 / 2 == 2;
                else
                    return c1 / 2 == 2;
            }
        }
		public ushort FindEdgeOrientationIndex()
		{
			int ret = 0;
            int index = 0;
            for (int i = 0; i < 12; i++)
            {
				ret = (ret << 1) | EdgeIsOrientedTable[i,
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

		public ushort FindCornerOrientationIndex()
		{
			int ret = 0;
			int index = 0;
			for (int i = 0; i < 8; i++)
			{
				int val = 2;
				if ((int)Data[IndexedCornerTilesIndexLinear[index++]] < 2)
					val = 0;

				if ((int)Data[IndexedCornerTilesIndexLinear[index]] < 2)
					val = 1;

				ret = ret * 3 + val;

				index += 2;
			}

			return (ushort)ret;
		}

		public IndexCube GetIndexCube()
        {
            return new IndexCube((uint)GetInverseEdgePermIndex(), 
                (ushort)GetCornerPermIndex(), FindEdgeOrientationIndex(), FindCornerOrientationIndex());
        }
		#endregion

		#region Symmetries

		//|    Method |     Mean |     Error |    StdDev |
		//|---------- |---------:|----------:|----------:|
		//| LowestSym | 5.865 ms | 0.0589 ms | 0.0551 ms |
		public void TransformSymmetry(SymmetryElement se, ArrayCube ret)
        {
            int[] perm = SymmetryArrays[se.Index];

            for (int i = 0; i < TILE_COUNT; i++)
            {
                ret.Data[perm[i]] = se.TransformColor(Data[i]);
            }
        }
        public ArrayCube FindLowestSymmetry()
        {
            ArrayCube best = new ArrayCube(this);
            ArrayCube buffer = new ArrayCube();

            foreach (var se in SymmetryElement.Elements)
            {
                TransformSymmetry(se, buffer);

                if (buffer.CompareTo(best) < 0)
                {
                    best = new ArrayCube(buffer);
                }
            }

            return best;
        }

        public ArrayCube FindLowestSymmetryInverse()
        {
            ArrayCube best = FindLowestSymmetry();

            ArrayCube inverse = GetInverse();
            ArrayCube buffer = new ArrayCube();
            foreach (var se in SymmetryElement.Elements)
            {
                inverse.TransformSymmetry(se, buffer);

                if (buffer.CompareTo(best) < 0)
                {
                    best = buffer;
                }
            }

            return best;
        }

        public ArrayCube GetInverse()
        {
            ArrayCube ret = new ArrayCube(this);

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

		#endregion

		public string SideView()
        {
            int[] sides = new int[] { 0, 5, 1, 4 };
            string[] tiles = new string[]
            {
                "o", "r", "y", "w", "b", "g"
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

        public static bool operator ==(ArrayCube a, ArrayCube b)
        {
            return a.Data.AsSpan().SequenceEqual(b.Data);
        }
        public static bool operator !=(ArrayCube a, ArrayCube b)
        {
            return !(a == b);
        }
        public int CompareTo(ArrayCube other)
        {
            if (this == other) return 0;

            for (int i = 0; i < TILE_COUNT; i++)
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
            return HashCode.Combine(Data[0], Data[46], Data[9], Data[37], Data[33], Data[23]);
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
            return this == (ArrayCube)obj;
        }
    }
}
