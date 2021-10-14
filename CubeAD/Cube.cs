using System;
using System.Collections.Generic;

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
				(BOTTOM,	1),
				(BACK,		3),
				(TOP,		3),
				(FRONT,		3)
			},

			//Right
			new (int Side, int Stripe)[]
			{
				(BOTTOM,	3),
				(FRONT,		1),
				(TOP,		1),
				(BACK,		1)
			},

			//Bottom
			new (int Side, int Stripe)[]
			{
				(LEFT,		3),
				(FRONT,		2),
				(RIGHT,		1),
				(BACK,		0)
			},

			//Top
			new (int Side, int Stripe)[]
			{
				(LEFT,		1),
				(BACK,		2),
				(RIGHT,		3),
				(FRONT,		0)
			},

			//Front
			new (int Side, int Stripe)[]
			{
				(LEFT,		2),
				(TOP,		2),
				(RIGHT,		2),
				(BOTTOM,	2)
			},

			//Back
			new (int Side, int Stripe)[]
			{
				(LEFT,		0),
				(BOTTOM,	0),
				(RIGHT,		0),
				(TOP,		0)
			}
		};
		
		//Squares that form a block (Clockwise after squares of main side)
		private static (int side1, int square1, int side2, int square2)[] OrangeBlocks =
		{
			(TOP,		3, BACK,	2),
			(TOP,		2, FRONT,	3),
			(BOTTOM,	1, FRONT,	2),
			(BOTTOM,	0, BACK,	3)
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

		public void Reset()
		{
			for (int i = 0; i < SIDE_COUNT; i++)
			{
				Sides[i] = new Side((CubeColor)i);
			}

			Blocked = 0;
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
		public void ApplyScramble(MoveSequenz s)
		{
			for (int i = 0; i < s.Moves.Count; i++)
				MakeMove(s.Moves[i]);

			ResetBlocked(); //does not necessarily need to be reset
		}

		public void ResetBlocked()
		{
			Blocked = 0;
		}

		//[TODO] Clean up switch block
		public void MakeMove(CubeMove m)
		{
			int side = (int)m / 3;

			Sides[side].Rotate(((int)m % 3) + 1);

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
		
		public bool HasSymmetry(SymmetryElement se)
		{
			//Check edges
			for (int x = 0; x < 6; x++)
			{
				int nextX = se.Transform(x);
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					int nextY = se.Transform(y);

					if (se.TransformColor(GetEdgeColor(x, y)) != GetEdgeColor(nextX, nextY))
						return false;
				}
			}

			//Check corners
			for (int x = 0; x < 6; x++)
			{
				int nextX = se.Transform(x);
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;
					int nextY = se.Transform(y);

					for (int z = 0; z < 6; z++)
					{
						if (x / 2 == z / 2 || y / 2 == z / 2) continue;
						int nextZ = se.Transform(z);

						if (se.TransformColor(GetCornerColor(x, y, z)) != GetCornerColor(nextX, nextY, nextZ))
							return false;
					}

				}
			}

			return true;
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
			(LEFT, BACK,	0, 7),
			(LEFT, BACK,	1, 6),
			(LEFT, TOP,		2, 7),
			(LEFT, TOP,		3, 6),
			(LEFT, FRONT,	4, 7),
			(LEFT, FRONT,	5, 6),
			(LEFT, BOTTOM,	6, 3),
			(LEFT, BOTTOM,	7, 2),

			//1
			(RIGHT, BACK,	0, 3),
			(RIGHT, BACK,	1, 2),
			(RIGHT, BOTTOM, 2, 7),
			(RIGHT, BOTTOM, 3, 6),
			(RIGHT, FRONT,	4, 3),
			(RIGHT, FRONT,	5, 2),
			(RIGHT, TOP,	6, 3),
			(RIGHT, TOP,	7, 2),

			//2
			(BOTTOM, BACK,	0, 1),
			(BOTTOM, BACK,	1, 0),
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

		//[TODO] Fix green/blue pieces 
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
			for(int x = 0; x < 6; x++)
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
