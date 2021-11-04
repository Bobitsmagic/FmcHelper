using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CubeAD
{
	public class Cube
	{
		const int SIDE_COUNT = 6;
		const int LEFT = 0;
		const int RIGHT = 1;
		const int BOTTOM = 2;
		const int TOP = 3;
		const int FRONT = 4;
		const int BACK = 5;

		private static readonly Side[] SolvedColors =
{
			new Side((CubeColor)0),
			new Side((CubeColor)1),
			new Side((CubeColor)2),
			new Side((CubeColor)3),
			new Side((CubeColor)4),
			new Side((CubeColor)5)
		};

		//Adj stripes of adj sides (Clockwise order)
		private static readonly (int Side, int Stripe)[][] AdjStripes = new (int Side, int Stripe)[][]
		{
			//Left
			new (int Side, int Stripe)[]
			{
				(BOTTOM,    1),
				(BACK,      3),
				(TOP,       3),
				(FRONT,     3)
			},

			//Right
			new (int Side, int Stripe)[]
			{
				(BOTTOM,    3),
				(FRONT,     1),
				(TOP,       1),
				(BACK,      1)
			},

			//Bottom
			new (int Side, int Stripe)[]
			{
				(LEFT,      3),
				(FRONT,     2),
				(RIGHT,     1),
				(BACK,      0)
			},

			//Top
			new (int Side, int Stripe)[]
			{
				(LEFT,      1),
				(BACK,      2),
				(RIGHT,     3),
				(FRONT,     0)
			},

			//Front
			new (int Side, int Stripe)[]
			{
				(LEFT,      2),
				(TOP,       2),
				(RIGHT,     2),
				(BOTTOM,    2)
			},

			//Back
			new (int Side, int Stripe)[]
			{
				(LEFT,      0),
				(BOTTOM,    0),
				(RIGHT,     0),
				(TOP,       0)
			}
		};

		//Squares that form a block (Clockwise after squares of main side)
		private static (int side1, int square1, int side2, int square2)[] OrangeBlocks =
		{
			(TOP,       3, BACK,    2),
			(TOP,       2, FRONT,   3),
			(BOTTOM,    1, FRONT,   2),
			(BOTTOM,    0, BACK,    3)
		};

		private static (int side1, int square1, int side2, int square2)[] RedBlocks =
		{
			(BOTTOM,    3, BACK,    0),
			(BOTTOM,    2, FRONT,   1),
			(TOP,       1, FRONT,   0),
			(TOP,       0, BACK,    1)
		};

		Side[] Sides = new Side[SIDE_COUNT];
		byte Blocked;

		public Cube()
		{
			Reset();
		}
		public Cube(Cube old)
		{
			Blocked = old.Blocked;

			for (int i = 0; i < 6; i++)
			{
				Sides[i] = old.Sides[i];
			}
		}

		public Cube(Cube old, CubeMove m)
		{
			Blocked = old.Blocked;

			for (int i = 0; i < 6; i++)
			{
				Sides[i] = old.Sides[i];
			}

			MakeMove(m);
		}
		public void Reset()
		{
			for (int i = 0; i < SIDE_COUNT; i++)
			{
				Sides[i] = new Side((CubeColor)i);
			}

			Blocked = 0;
		}
		public void CopyValuesFrom(Cube c)
		{
			Blocked = c.Blocked;

			for (int i = 0; i < 6; i++)
			{
				Sides[i] = c.Sides[i];
			}
		}

		public void AddPossibleMoves(List<CubeMove> list)
		{
			list.Clear();
			for (int i = 0; i < 6; i++)
			{
				if (((Blocked >> i) & 1) == 0)
				{
					for (int j = 0; j < 3; j++)
					{
						list.Add((CubeMove)(i * 3 + j));
					}
				}
			}
		}

		//ALARM
		public void ApplyMoveSequenz(MoveSequenz s)
		{
			for (int i = 0; i < s.Moves.Count; i++)
				MakeMove(s.Moves[i]);

			ResetBlocked(); //does not necessarily need to be reset
		}

		public void ResetBlocked()
		{
			Blocked = 0;
		}

		public void MakeMove(CubeMove m)
		{
			int side = (int)m / 3;

			Sides[side].Rotate(((int)m % 3) + 1);

			UpdateBlocked(side);

			uint buffer;
			int moveType = (int)m % 3;
			(int Side, int Stripe)[] stripes = AdjStripes[side];

			switch (moveType)
			{
				case 0:
					buffer = Sides[stripes[0].Side].GetStripe(stripes[0].Stripe);

					Sides[stripes[0].Side].SetStripe(stripes[0].Stripe,
						Sides[stripes[3].Side].GetStripe(stripes[3].Stripe));

					Sides[stripes[3].Side].SetStripe(stripes[3].Stripe,
						Sides[stripes[2].Side].GetStripe(stripes[2].Stripe));

					Sides[stripes[2].Side].SetStripe(stripes[2].Stripe,
						Sides[stripes[1].Side].GetStripe(stripes[1].Stripe));

					Sides[stripes[1].Side].SetStripe(stripes[1].Stripe, buffer);
					break;

				case 1:
					buffer = Sides[stripes[0].Side].GetStripe(stripes[0].Stripe);
					Sides[stripes[0].Side].SetStripe(stripes[0].Stripe,
						Sides[stripes[2].Side].GetStripe(stripes[2].Stripe));
					Sides[stripes[2].Side].SetStripe(stripes[2].Stripe, buffer);

					buffer = Sides[stripes[1].Side].GetStripe(stripes[1].Stripe);
					Sides[stripes[1].Side].SetStripe(stripes[1].Stripe,
						Sides[stripes[3].Side].GetStripe(stripes[3].Stripe));
					Sides[stripes[3].Side].SetStripe(stripes[3].Stripe, buffer);
					break;

				case 2:
					buffer = Sides[stripes[0].Side].GetStripe(stripes[0].Stripe);

					Sides[stripes[0].Side].SetStripe(stripes[0].Stripe,
						Sides[stripes[1].Side].GetStripe(stripes[1].Stripe));

					Sides[stripes[1].Side].SetStripe(stripes[1].Stripe,
						Sides[stripes[2].Side].GetStripe(stripes[2].Stripe));

					Sides[stripes[2].Side].SetStripe(stripes[2].Stripe,
						Sides[stripes[3].Side].GetStripe(stripes[3].Stripe));

					Sides[stripes[3].Side].SetStripe(stripes[3].Stripe, buffer);
					break;
			}
		}

		public void UpdateBlocked(int side)
		{
			Blocked |= (byte)(1 << side);

			if (side % 2 == 0)
				Blocked |= (byte)(1 << (side + 1));

			for (int i = 0; i < 6; i++)
			{
				if (i / 2 != side / 2)
				{
					Blocked &= (byte)(~(1 << i));
				}
			}
		}

		public bool IsSolved
		{
			get
			{
				for (int i = 0; i < 6; i++)
					if (Sides[i] != SolvedColors[i]) return false;

				return true;
			}
		}

		public static int[,] AdjEdges = new int[,]
		{
			//Left
			{ -1, -1, 7, 3, 5, 1 },
			//Right
			{ -1, -1, 3, 7, 5, 1 },

			//Bottom
			{ 3, 7, -1, -1, 5, 1 },
			//Top
			{ 7, 3, -1, -1, 5, 1 },

			//Front
			{ 7, 3, 5, 1, -1, -1 },
			//Back
			{ 7, 3, 1, 5, -1, -1 },
		};
		public CubeColor GetEdgeColor(int side1, int side2)
		{
			return (CubeColor)Sides[side1][AdjEdges[side1, side2]];
		}
		public CubeColor GetEdgeColor(CubeColor side1, CubeColor side2)
		{
			return (CubeColor)Sides[(int)side1][AdjEdges[(int)side1, (int)side2]];
		}

		public void SetEdgeColor(int side1, int side2, int color)
		{
			Sides[side1][AdjEdges[side1, side2]] = (uint)color;
		}

		public static int[,] AdjCorners =
		{
			//Left						removed cuz redundancy
			{ -1, -1, -1, -1, 6, 0, 4, 2, -2, -2, -2, -2 },
			//Right
			{ -1, -1, -1, -1, 4, 2, 6, 0, -2, -2, -2, -2 },

			//Bottom
			{ 4, 2, 6, 0, -1, -1, -1, -1, -2, -2, -2, -2 },
			//Top
			{ 6, 0, 4, 2, -1, -1, -1, -1, -2, -2, -2, -2 },

			//Front
			{ 6, 0, 4, 2, -2, -2, -2, -2, -1, -1, -1, -1},
			//Back
			{ 0, 6, 2, 4, -2, -2, -2, -2, -1, -1, -1, -1},
		};
		public CubeColor GetCornerColor(int side1, int side2, int side3)
		{
			int lower = Math.Min(side2, side3);
			int higher = Math.Max(side2, side3);

			return (CubeColor)Sides[side1][AdjCorners[side1, lower * 2 + (higher & 1)]];
		}

		public void SetCornerColor(int side1, int side2, int side3, int color)
		{
			int lower = Math.Min(side2, side3);
			int higher = Math.Max(side2, side3);

			Sides[side1][AdjCorners[side1, lower * 2 + (higher & 1)]] = (uint)color;
		}

		public bool HasSymmetry(SymmetryElement se)
		{
			//Check edges
			for (int x = 0; x < 6; x++)
			{
				int nextX = se.TransformColor(x);
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					int nextY = se.TransformColor(y);

					if (se.TransformColor(GetEdgeColor(x, y)) != GetEdgeColor(nextX, nextY))
						return false;
				}
			}

			//Check corners
			for (int x = 0; x < 6; x++)
			{
				int nextX = se.TransformColor(x);
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;
					int nextY = se.TransformColor(y);

					for (int z = 0; z < 6; z++)
					{
						if (x / 2 == z / 2 || y / 2 == z / 2) continue;
						int nextZ = se.TransformColor(z);

						if (se.TransformColor(GetCornerColor(x, y, z)) != GetCornerColor(nextX, nextY, nextZ))
							return false;
					}

				}
			}

			return true;
		}

		public BitArray GetSymmetrySet()
		{
			BitArray ret = new BitArray();

			ret[0] = true;
			BitArray inverse = new BitArray();

			var array = SymmetryElement.Elements;

			//skipping identity
			for (int i = 1; i < array.Length; i++)
			{
				if (!ret[i] && !inverse[i])
				{
					if (HasSymmetry(array[i]))
					{
						ret[i] = true;

						//multiplying all implied symmetries to avoid HasSymmetry calls
						//skipping identity
						for (int j = 1; j < array.Length; j++)
						{
							if (!ret[j]) continue;

							for (int k = 1; k < SymmetryElement.MultGroupIndices[i].Length; k++)
							{
								ret[SymmetryElement.GroupIndexTable[SymmetryElement.MultGroupIndices[i][k], j]] = true;
							}
						}
					}
					else
					{
						inverse[i] = true;

						//multiplying all implied symmetries to avoid HasSymmetry calls
						//skipping identity
						for (int j = 1; j < array.Length; j++)
						{
							//Alarm
							if (!ret[j]) continue;

							for (int k = 1; k < SymmetryElement.MultGroupIndices[i].Length; k++)
							{
								inverse[SymmetryElement.GroupIndexTable[SymmetryElement.MultGroupIndices[i][k], j]] = true;
							}
						}
					}
				}
			}

			return ret;
		}

		public string GetSimpleSideView()
		{
			string s = "";
			for (int i = 0; i < 6; i++)
			{
				s += Sides[i].WriteAsSide(((CubeColor)i).ToString()[0]) + "\n####################\n";
			}
			return s;
		}

		public bool IsEqualWithSymmetry(Cube other)
		{
			BitArray sym1 = GetSymmetrySet();
			BitArray sym2 = other.GetSymmetrySet();

			if (sym1 != sym2) return false;

			for (int i = 0; i < SymmetryElement.ORDER; i++)
			{
				if (IsEqualWithSymmetry(other, SymmetryElement.Elements[i]))
					return true;
			}

			return false;
		}
		public bool IsEqualWithSymmetry(Cube other, SymmetryElement se)
		{
			//Check edges
			for (int x = 0; x < 6; x++)
			{
				int nextX = se.TransformColor(x);
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					int nextY = se.TransformColor(y);

					if (se.TransformColor(GetEdgeColor(x, y)) != other.GetEdgeColor(nextX, nextY))
						return false;
				}
			}

			//Check corners
			for (int x = 0; x < 6; x++)
			{
				int nextX = se.TransformColor(x);
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;
					int nextY = se.TransformColor(y);

					for (int z = 0; z < 6; z++)
					{
						if (x / 2 == z / 2 || y / 2 == z / 2) continue;
						int nextZ = se.TransformColor(z);

						if (se.TransformColor(GetCornerColor(x, y, z)) != other.GetCornerColor(nextX, nextY, nextZ))
							return false;
					}

				}
			}

			return true;
		}

		public string GetSideView()
		{
			string s = new string(' ', 3 * 2 - 1) + new string('-', 3 * 2 + 1) + "\n";
			for (int y = 0; y < 3; y++)
			{
				s += new string(' ', 3 * 2 - 1) + "|";
				for (int x = 0; x < 3; x++)
				{
					if (x == 1 && y == 1)
						s += CubeColor.Blue.ToString()[0];
					else
						s += Sides[BACK][x, y].ToString()[0];

					s += x == 2 ? "|" : " ";
				}

				s += "\n";
			}

			s += new string('-', (3 * 2) * 4) + "\n";
			int[] Indices = { 0, 3, 1, 2 };
			for (int y = 0; y < 3; y++)
			{
				for (int i = 0; i < Indices.Length; i++)
				{
					for (int x = 0; x < 3; x++)
					{
						if (x == 1 && y == 1)
							s += ((CubeColor)Indices[i]).ToString()[0];
						else
							s += Sides[Indices[i]][x, y].ToString()[0];

						s += x == 2 ? "|" : " ";
					}


				}

				s += "\n";
			}
			s += new string('-', (3 * 2) * 4) + "\n";

			for (int y = 0; y < 3; y++)
			{
				s += new string(' ', 3 * 2 - 1) + "|";
				for (int x = 0; x < 3; x++)
				{
					if (x == 1 && y == 1)
						s += CubeColor.Green.ToString()[0];
					else
						s += Sides[FRONT][x, y].ToString()[0];

					s += x == 2 ? "|" : " ";
				}

				s += "\n";
			}
			s += new string(' ', 3 * 2 - 1) + new string('-', 3 * 2 + 1);

			return s + "\n";
		}

		public void PrintSideView()
		{
			string s = GetSideView();
			for (int i = 0; i < s.Length; i++)
			{
				switch (s[i])
				{
					case 'W': Console.ForegroundColor = ConsoleColor.White; break;
					case 'R': Console.ForegroundColor = ConsoleColor.Red; break;
					case 'O': Console.ForegroundColor = ConsoleColor.DarkMagenta; break;
					case 'B': Console.ForegroundColor = ConsoleColor.Blue; break;
					case 'G': Console.ForegroundColor = ConsoleColor.Green; break;
					case 'Y': Console.ForegroundColor = ConsoleColor.Yellow; break;
					default: Console.ForegroundColor = ConsoleColor.Gray; break;
				}

				Console.Write(s[i]);
			}
		}

		public int GetValue()
		{
			return CountPlacedEdges() + CountF2LPairs() * 100 + CountBlocks() * 100 * (100 - 3);
		}
		public int CountBlocks()
		{
			int topCounter = 0;
			int bottomCounter = 0;
			for (int i = 0; i < 4; i++)
			{
				if (Sides[LEFT].FitsPattern(SolvedColors[LEFT], Side.SQUARE_MASK[i]))
				{
					(int side1, int square1, int side2, int square2) = OrangeBlocks[i];
					if (Sides[side1].FitsPattern(SolvedColors[side1], Side.SQUARE_MASK[square1]) &&
						Sides[side2].FitsPattern(SolvedColors[side2], Side.SQUARE_MASK[square2]))
					{
						if (side1 == 2 || side2 == 2)
							bottomCounter++;
						else
							topCounter++;
					}
				}

				if (Sides[RIGHT].FitsPattern(SolvedColors[RIGHT], Side.SQUARE_MASK[i]))
				{
					(int side1, int square1, int side2, int square2) = RedBlocks[i];
					if (Sides[side1].FitsPattern(SolvedColors[side1], Side.SQUARE_MASK[square1]) &&
						Sides[side2].FitsPattern(SolvedColors[side2], Side.SQUARE_MASK[square2]))
					{
						if (side1 == 2 || side2 == 2)
							bottomCounter++;
						else
							topCounter++;
					}
				}
			}

			return topCounter + bottomCounter;
		}

		public int CountSquares()
		{
			int counter = 0;
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					if (Sides[i].FitsPattern(SolvedColors[i], Side.SQUARE_MASK[j]))
					{
						counter++;
					}
				}
			}

			return counter;
		}

		public bool IsPLL()
		{
			return !IsSolved && CountBlocks() == 4 && CountSquares() >= 16;
		}

		public bool IsAUF()
		{
			return !IsSolved && CountBlocks() == 4 && CountSquares() >= 16 && CountF2LPairs() >= 20;
		}

		public static (int Side1, int Side2, int pair1, int Pair2)[] F2LPairs =
		{
			//LEFT
			(LEFT, BACK,    0, 7),
			(LEFT, BACK,    1, 6),
			(LEFT, TOP,     2, 7),
			(LEFT, TOP,     3, 6),
			(LEFT, FRONT,   4, 7),
			(LEFT, FRONT,   5, 6),
			(LEFT, BOTTOM,  6, 3),
			(LEFT, BOTTOM,  7, 2),

			//1
			(RIGHT, BACK,   0, 3),
			(RIGHT, BACK,   1, 2),
			(RIGHT, BOTTOM, 2, 7),
			(RIGHT, BOTTOM, 3, 6),
			(RIGHT, FRONT,  4, 3),
			(RIGHT, FRONT,  5, 2),
			(RIGHT, TOP,    6, 3),
			(RIGHT, TOP,    7, 2),

			//2
			(BOTTOM, BACK,  0, 1),
			(BOTTOM, BACK,  1, 0),
			(BOTTOM, FRONT, 4, 5),
			(BOTTOM, FRONT, 5, 4),

			//3
			(TOP, BACK, 0, 5),
			(TOP, BACK, 1, 4),
			(TOP, FRONT, 4, 1),
			(TOP, FRONT, 5, 0)
		};
		public int CountF2LPairs()
		{
			int counter = 0;

			foreach ((int Side1, int Side2, int Pair1, int Pair2) in F2LPairs)
			{
				if (Sides[Side1].IsPair(Pair1) && Sides[Side2].IsPair(Pair2))
				{
					counter++;
				}
			}

			return counter;
		}
		public int CountPlacedEdges()
		{
			int counter = 0;
			for (int i = 0; i < 6; i++)
			{
				for (int j = 1; j < 8; j += 2)
				{
					if (Sides[i][j] == i)
						counter++;
				}
			}

			return counter;
		}

		public bool EdgeIsOriented(int side1, int side2)
		{
			if (side2 < side1)
				(side1, side2) = (side2, side1);

			int c1 = (int)GetEdgeColor(side1, side2);
			int c2 = (int)GetEdgeColor(side2, side1);


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

		public int CornerOrientaion(int side1, int side2, int side3)
		{
			if (!(side1 < side2 && side2 < side3))
				throw new ArgumentException("Sides need to be in ascending order");

			if ((int)GetCornerColor(side1, side2, side3) / 2 == 0)
				return 0;

			if ((int)GetCornerColor(side2, side1, side3) / 2 == 0)
				return 1;

			return 2;
		}

		public int CountOrientedEgdes()
		{
			int counter = 0;

			//Check white/yellow edges containing on bottom/top
			for (int i = 1; i < 8; i += 2)
			{
				if (Sides[BOTTOM][i] / 2 == 1)
					counter++;
				if (Sides[TOP][i] / 2 == 1)
					counter++;
			}


			//Check white/yellow containing edges on front/back
			if (Sides[FRONT][3] / 2 == 1)
				counter++;
			if (Sides[FRONT][7] / 2 == 1)
				counter++;

			if (Sides[BACK][3] / 2 == 1)
				counter++;
			if (Sides[BACK][7] / 2 == 1)
				counter++;

			//Check green/blue containing edges on top/bottom

			//all none bot/top sides
			for (int x = 0; x < 6; x++)
			{
				if (x / 2 == 1) continue;
				//main color (blue/green) on bot/top and secondary color (orange/red) on another side 
				if ((int)GetEdgeColor(BOTTOM, x) / 2 == 2 &&
					(int)GetEdgeColor(x, BOTTOM) / 2 == 0) counter++;

				if ((int)GetEdgeColor(TOP, x) / 2 == 2 &&
					(int)GetEdgeColor(x, TOP) / 2 == 0) counter++;
			}


			if (Sides[BACK][3] / 2 == 2 && Sides[RIGHT][1] / 2 != 1)
				counter++;
			if (Sides[FRONT][3] / 2 == 2 && Sides[RIGHT][5] / 2 != 1)
				counter++;

			if (Sides[BACK][7] / 2 == 2 && Sides[LEFT][1] / 2 != 1)
				counter++;
			if (Sides[FRONT][7] / 2 == 2 && Sides[LEFT][5] / 2 != 1)
				counter++;

			return counter;
		}

		public static HashSet<CubeIndex> GetRandomCubesDistinct(int moveCount, int count, Random rnd)
		{
			HashSet<CubeIndex> ret = new HashSet<CubeIndex>(count);

			List<CubeMove> list = new List<CubeMove>();
			while (ret.Count < count)
			{
				Cube c = new Cube();
				for (int j = 0; j < moveCount; j++)
				{
					c.AddPossibleMoves(list);

					c.MakeMove(list[rnd.Next(list.Count)]);
				}

				ret.Add(new CubeIndex(c));
			}
			Console.WriteLine("Found random cubes");

			return ret;
		}
		public static List<CubeIndex> GetRandomCubes(int moveCount, int count, Random rnd)
		{
			List<CubeIndex> ret = new List<CubeIndex>(count);

			List<CubeMove> list = new List<CubeMove>();
			while (ret.Count < count)
			{
				Cube c = new Cube();
				for (int j = 0; j < moveCount; j++)
				{
					c.AddPossibleMoves(list);

					c.MakeMove(list[rnd.Next(list.Count)]);
				}

				ret.Add(new CubeIndex(c));
			}
			Console.WriteLine("Found random cubes");

			return ret;
		}
		public static HashSet<Cube> GetSolvedCubesDFS(int maxDepth)
		{
			HashSet<Cube> ret = new HashSet<Cube>();

			//Avoid GC Pressure
			List<CubeMove>[] moveBuffer = new List<CubeMove>[20];
			for (int i = 0; i < moveBuffer.Length; i++)
				moveBuffer[i] = new List<CubeMove>(18);

			Cube[] cubeBuffer = new Cube[20];
			for (int i = 0; i < cubeBuffer.Length; i++)
				cubeBuffer[i] = new Cube();

			Stack<CubeMove> currentMoves = new Stack<CubeMove>(20);

			long bytes = GC.GetTotalMemory(true);
			long cubeCount = 0;

			Stopwatch sw = new Stopwatch();
			sw.Start();
			Solve(0);
			//Console.WriteLine("CC: " + cubeCount);
			//Console.WriteLine("Memory DIf: " + (GC.GetTotalMemory(true) - bytes).ToString("000 000 000 000"));

			return ret;

			void Solve(int depth)
			{
				Cube cube = cubeBuffer[depth];
				ret.Add(new Cube(cube));

				if (depth < maxDepth)
				{
					List<CubeMove> list = moveBuffer[depth];
					list.Clear();
					cube.AddPossibleMoves(list);
					Cube next = cubeBuffer[depth + 1];

					foreach (CubeMove move in list)
					{
						next.CopyValuesFrom(cube);
						next.MakeMove(move);

						//depth++;
						currentMoves.Push(move);

						Solve(depth + 1);
						if (depth == 0)
						{
							//Console.WriteLine(move + " done");
							//Console.WriteLine("CubeCount: " + ret.Count.ToString("0 000 000 000"));
							//Console.WriteLine("Memeory: " + GC.GetTotalMemory(true).ToString("000 000 000 000"));
							//Console.WriteLine("Time: " + sw.ElapsedMilliseconds.ToString("000 000 000"));
						}

						currentMoves.Pop();
						//depth--;
					}

					cubeCount += list.Count;
				}
			}
		}
		public static HashSet<CubeIndex> GetSolvedCubeIndicesDFS(int maxDepth)
		{
			HashSet<CubeIndex> ret = new HashSet<CubeIndex>();

			//Avoid GC Pressure
			List<CubeMove>[] moveBuffer = new List<CubeMove>[20];
			for (int i = 0; i < moveBuffer.Length; i++)
				moveBuffer[i] = new List<CubeMove>(18);

			Cube[] cubeBuffer = new Cube[20];
			for (int i = 0; i < cubeBuffer.Length; i++)
				cubeBuffer[i] = new Cube();


			long bytes = GC.GetTotalMemory(true);
			long cubeCount = 0;

			Stopwatch sw = new Stopwatch();
			sw.Start();
			Solve(0);
			//Console.WriteLine("CC: " + cubeCount);
			//Console.WriteLine("Memory DIf: " + (GC.GetTotalMemory(true) - bytes).ToString("000 000 000 000"));

			return ret;

			void Solve(int depth)
			{
				Cube cube = cubeBuffer[depth];
				ret.Add(new CubeIndex(cube));

				if (depth < maxDepth)
				{
					List<CubeMove> list = moveBuffer[depth];
					list.Clear();
					cube.AddPossibleMoves(list);
					Cube next = cubeBuffer[depth + 1];

					foreach (CubeMove move in list)
					{
						next.CopyValuesFrom(cube);
						next.MakeMove(move);
						
						//depth++;
						//currentMoves.Push(move);

						Solve(depth + 1);
						if (depth == 0)
						{
							//Console.WriteLine(move + " done");
							//Console.WriteLine("CubeCount: " + ret.Count.ToString("0 000 000 000"));
							//Console.WriteLine("Memeory: " + GC.GetTotalMemory(true).ToString("000 000 000 000"));
							//Console.WriteLine("Time: " + sw.ElapsedMilliseconds.ToString("000 000 000"));
						}

						//currentMoves.Pop();
						//depth--;
					}

					cubeCount += list.Count;
				}
			}
		}
		public static HashSet<CubeIndex> GetSolvedCubeIndicesBFS(int maxDepth)
		{
			const int CAPACITY = 1_000_000;
			HashSet<CubeIndex> ret = new HashSet<CubeIndex>(CAPACITY);

			//Avoid GC Pressure
			Cube cube = new Cube();

			List<Cube> current = new List<Cube>(CAPACITY);
			List<Cube> next = new List<Cube>(CAPACITY);

			current.Add(new Cube());
			ret.Add(new CubeIndex(current.ElementAt(0)));

			long bytes = GC.GetTotalMemory(true);

			long cubeCount = 0;
			for (int i = 0; i < maxDepth; i++)
			{
				foreach (Cube c in current)
				{
					for(int j = 0; j < 18; j++)
					{
						cube.CopyValuesFrom(c);
						cube.MakeMove((CubeMove)j);
						if(ret.Add(new CubeIndex(cube)))
							next.Add(new Cube(cube));
						
					}
				}

				if(next.Count >= next.Capacity)
					Console.WriteLine("Alarm");
				cubeCount += current.Count * 18;

				List<Cube> buffer = next;
				next = current;
				current = buffer;

				next.Clear();
			}
			//Console.WriteLine("Memory DIf: " + (GC.GetTotalMemory(true) - bytes).ToString("000 000 000 000"));
			//Console.WriteLine("MakeMoveCount: " + cubeCount);

			return ret;

			int Power(int x, int y)
			{
				int ret = 1;
				for (int i = 0; i < y; i++)
				{
					ret *= x;
				}
				return ret;
			}
		}

		public static HashSet<Cube> GetSolvedCubesBFSNaive(int maxDepth)
		{
			const int CAPACITY = 1_000_000;
			HashSet<Cube> ret = new HashSet<Cube>(CAPACITY);

			//Avoid GC Pressure
			Cube cube = new Cube();
			List<Cube> next = new List<Cube>();

			ret.Add(new Cube(cube));
			for (int i = 0; i < maxDepth; i++)
			{
				foreach (Cube c in ret)
				{
					for (int j = 0; j < 18; j++)
					{
						cube.CopyValuesFrom(c);
						cube.MakeMove((CubeMove)j);

						if (!ret.Contains(cube))
						{
							next.Add(new Cube(cube));
						}
					}
				}

				for (int j = 0; j < next.Count; j++)
					ret.Add(next[j]);

				next.Clear();
			}
			//Console.WriteLine("Memory DIf: " + (GC.GetTotalMemory(true) - bytes).ToString("000 000 000 000"));
			//Console.WriteLine("MakeMoveCount: " + cubeCount);

			return ret;

			int Power(int x, int y)
			{
				int ret = 1;
				for (int i = 0; i < y; i++)
				{
					ret *= x;
				}
				return ret;
			}
		}

		public static bool operator ==(Cube a, Cube b)
		{
			for (int i = 0; i < 6; i++)
				if (a.Sides[i] != b.Sides[i]) return false;

			return true;
		}
		public static bool operator !=(Cube a, Cube b)
		{
			return !(a == b);
		}
		public override bool Equals(object obj)
		{
			Cube c = (Cube)obj;

			for (int i = 0; i < 6; i++)
			{
				if (Sides[i] != c.Sides[i]) return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return Sides[0].GetHashCode() ^ (Sides[1].GetHashCode() << 8) ^
				Sides[2].GetHashCode() ^ (Sides[3].GetHashCode() << 8) ^
				Sides[4].GetHashCode() ^ (Sides[5].GetHashCode() << 8);
		}
	}
}
