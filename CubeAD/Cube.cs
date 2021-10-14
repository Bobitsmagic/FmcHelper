using System;
using System.Collections.Generic;

namespace CubeAD
{
	public class Cube
	{
		const int SIDE_COUNT = 6;

		private static Side[] SolvedColors =
{
			new Side((CubeColor)0),
			new Side((CubeColor)1),
			new Side((CubeColor)2),
			new Side((CubeColor)3),
			new Side((CubeColor)4),
			new Side((CubeColor)5)
		};



		private static (int Side, int Stripe)[] BottomIndices =
		{
			(4, 0),
			(1, 1),
			(5, 2),
			(0, 3)
		};
		private static (int Side, int Stripe)[] TopIndices =
		{
			(5, 0),
			(0, 1),
			(4, 2),
			(1, 3)
		};
		private static (int Side, int Stripe)[] BackIndices =
		{
			(0, 0),
			(3, 0),
			(1, 0),
			(2, 0)
		};
		private static (int Side, int Stripe)[] FrontIndices =
		{
			(0, 2),
			(3, 2),
			(1, 2),
			(2, 2)
		};

		private static (int side1, int square1, int side2, int square2)[] OrangeBlocks =
		{
			(3, 3, 4, 2),
			(3, 2, 5, 3),
			(5, 2, 2, 1),
			(4, 3, 2, 0)
		};

		private static (int side1, int square1, int side2, int square2)[] RedBlocks =
		{
			(2, 3, 4, 0),
			(2, 2, 5, 1),
			(3, 1, 5, 0),
			(3, 0, 4, 1)
		};

		Side[] Sides = new Side[SIDE_COUNT];
		byte Blocked;

		public Cube()
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

			ResetBlocked();
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

			uint buffer = 0;
			switch ((int)m)
			{
				#region Left
				case 0:
					buffer = Sides[2].GetStripe(1);
					Sides[2].SetStripe(1, Sides[5].GetStripe(3));
					Sides[5].SetStripe(3, Sides[3].GetStripe(3));
					Sides[3].SetStripe(3, Sides[4].GetStripe(3));
					Sides[4].SetStripe(3, buffer);
					break;
				case 1:
					buffer = Sides[2].GetStripe(1);
					Sides[2].SetStripe(1, Sides[3].GetStripe(3));
					Sides[3].SetStripe(3, buffer);

					buffer = Sides[4].GetStripe(3);
					Sides[4].SetStripe(3, Sides[5].GetStripe(3));
					Sides[5].SetStripe(3, buffer);
					break;
				case 2:
					buffer = Sides[2].GetStripe(1);
					Sides[2].SetStripe(1, Sides[4].GetStripe(3));
					Sides[4].SetStripe(3, Sides[3].GetStripe(3));
					Sides[3].SetStripe(3, Sides[5].GetStripe(3));
					Sides[5].SetStripe(3, buffer);
					break;
				#endregion

				#region Right
				case 3:
					buffer = Sides[2].GetStripe(3);
					Sides[2].SetStripe(3, Sides[4].GetStripe(1));
					Sides[4].SetStripe(1, Sides[3].GetStripe(1));
					Sides[3].SetStripe(1, Sides[5].GetStripe(1));
					Sides[5].SetStripe(1, buffer);
					break;
				case 4:
					buffer = Sides[2].GetStripe(3);
					Sides[2].SetStripe(3, Sides[3].GetStripe(1));
					Sides[3].SetStripe(1, buffer);

					buffer = Sides[4].GetStripe(1);
					Sides[4].SetStripe(1, Sides[5].GetStripe(1));
					Sides[5].SetStripe(1, buffer);
					break;
				case 5:
					buffer = Sides[2].GetStripe(3);
					Sides[2].SetStripe(3, Sides[5].GetStripe(1));
					Sides[5].SetStripe(1, Sides[3].GetStripe(1));
					Sides[3].SetStripe(1, Sides[4].GetStripe(1));
					Sides[4].SetStripe(1, buffer);
					break;
				#endregion

				#region Bottom
				case 6:
					buffer = Sides[BottomIndices[0].Side].GetStripe(BottomIndices[0].Stripe);

					Sides[BottomIndices[0].Side].SetStripe(BottomIndices[0].Stripe,
						Sides[BottomIndices[1].Side].GetStripe(BottomIndices[1].Stripe));

					Sides[BottomIndices[1].Side].SetStripe(BottomIndices[1].Stripe,
						Sides[BottomIndices[2].Side].GetStripe(BottomIndices[2].Stripe));

					Sides[BottomIndices[2].Side].SetStripe(BottomIndices[2].Stripe,
						Sides[BottomIndices[3].Side].GetStripe(BottomIndices[3].Stripe));

					Sides[BottomIndices[3].Side].SetStripe(BottomIndices[3].Stripe, buffer);

					break;
				case 7:
					buffer = Sides[BottomIndices[0].Side].GetStripe(BottomIndices[0].Stripe);
					Sides[BottomIndices[0].Side].SetStripe(BottomIndices[0].Stripe,
						Sides[BottomIndices[2].Side].GetStripe(BottomIndices[2].Stripe));
					Sides[BottomIndices[2].Side].SetStripe(BottomIndices[2].Stripe, buffer);

					buffer = Sides[BottomIndices[1].Side].GetStripe(BottomIndices[1].Stripe);
					Sides[BottomIndices[1].Side].SetStripe(BottomIndices[1].Stripe,
						Sides[BottomIndices[3].Side].GetStripe(BottomIndices[3].Stripe));
					Sides[BottomIndices[3].Side].SetStripe(BottomIndices[3].Stripe, buffer);
					break;
				case 8:
					buffer = Sides[BottomIndices[0].Side].GetStripe(BottomIndices[0].Stripe);

					Sides[BottomIndices[0].Side].SetStripe(BottomIndices[0].Stripe,
						Sides[BottomIndices[3].Side].GetStripe(BottomIndices[3].Stripe));

					Sides[BottomIndices[3].Side].SetStripe(BottomIndices[3].Stripe,
						Sides[BottomIndices[2].Side].GetStripe(BottomIndices[2].Stripe));

					Sides[BottomIndices[2].Side].SetStripe(BottomIndices[2].Stripe,
						Sides[BottomIndices[1].Side].GetStripe(BottomIndices[1].Stripe));

					Sides[BottomIndices[1].Side].SetStripe(BottomIndices[1].Stripe, buffer);
					break;
				#endregion

				#region Top
				case 9:
					buffer = Sides[TopIndices[0].Side].GetStripe(TopIndices[0].Stripe);

					Sides[TopIndices[0].Side].SetStripe(TopIndices[0].Stripe,
						Sides[TopIndices[3].Side].GetStripe(TopIndices[3].Stripe));

					Sides[TopIndices[3].Side].SetStripe(TopIndices[3].Stripe,
						Sides[TopIndices[2].Side].GetStripe(TopIndices[2].Stripe));

					Sides[TopIndices[2].Side].SetStripe(TopIndices[2].Stripe,
						Sides[TopIndices[1].Side].GetStripe(TopIndices[1].Stripe));

					Sides[TopIndices[1].Side].SetStripe(TopIndices[1].Stripe, buffer);
					break;
				case 10:
					buffer = Sides[TopIndices[0].Side].GetStripe(TopIndices[0].Stripe);
					Sides[TopIndices[0].Side].SetStripe(TopIndices[0].Stripe,
						Sides[TopIndices[2].Side].GetStripe(TopIndices[2].Stripe));
					Sides[TopIndices[2].Side].SetStripe(TopIndices[2].Stripe, buffer);

					buffer = Sides[TopIndices[1].Side].GetStripe(TopIndices[1].Stripe);
					Sides[TopIndices[1].Side].SetStripe(TopIndices[1].Stripe,
						Sides[TopIndices[3].Side].GetStripe(TopIndices[3].Stripe));
					Sides[TopIndices[3].Side].SetStripe(TopIndices[3].Stripe, buffer);
					break;
				case 11:
					buffer = Sides[TopIndices[0].Side].GetStripe(TopIndices[0].Stripe);

					Sides[TopIndices[0].Side].SetStripe(TopIndices[0].Stripe,
						Sides[TopIndices[1].Side].GetStripe(TopIndices[1].Stripe));

					Sides[TopIndices[1].Side].SetStripe(TopIndices[1].Stripe,
						Sides[TopIndices[2].Side].GetStripe(TopIndices[2].Stripe));

					Sides[TopIndices[2].Side].SetStripe(TopIndices[2].Stripe,
						Sides[TopIndices[3].Side].GetStripe(TopIndices[3].Stripe));

					Sides[TopIndices[3].Side].SetStripe(TopIndices[3].Stripe, buffer);
					break;
				#endregion

				#region Back
				case 12:
					buffer = Sides[BackIndices[0].Side].GetStripe(BackIndices[0].Stripe);

					Sides[BackIndices[0].Side].SetStripe(BackIndices[0].Stripe,
						Sides[BackIndices[1].Side].GetStripe(BackIndices[1].Stripe));

					Sides[BackIndices[1].Side].SetStripe(BackIndices[1].Stripe,
						Sides[BackIndices[2].Side].GetStripe(BackIndices[2].Stripe));

					Sides[BackIndices[2].Side].SetStripe(BackIndices[2].Stripe,
						Sides[BackIndices[3].Side].GetStripe(BackIndices[3].Stripe));

					Sides[BackIndices[3].Side].SetStripe(BackIndices[3].Stripe, buffer);
					break;
				case 13:
					buffer = Sides[BackIndices[0].Side].GetStripe(BackIndices[0].Stripe);
					Sides[BackIndices[0].Side].SetStripe(BackIndices[0].Stripe,
						Sides[BackIndices[2].Side].GetStripe(BackIndices[2].Stripe));
					Sides[BackIndices[2].Side].SetStripe(BackIndices[2].Stripe, buffer);

					buffer = Sides[BackIndices[1].Side].GetStripe(BackIndices[1].Stripe);
					Sides[BackIndices[1].Side].SetStripe(BackIndices[1].Stripe,
						Sides[BackIndices[3].Side].GetStripe(BackIndices[3].Stripe));
					Sides[BackIndices[3].Side].SetStripe(BackIndices[3].Stripe, buffer);
					break;
				case 14:
					buffer = Sides[BackIndices[0].Side].GetStripe(BackIndices[0].Stripe);

					Sides[BackIndices[0].Side].SetStripe(BackIndices[0].Stripe,
						Sides[BackIndices[3].Side].GetStripe(BackIndices[3].Stripe));

					Sides[BackIndices[3].Side].SetStripe(BackIndices[3].Stripe,
						Sides[BackIndices[2].Side].GetStripe(BackIndices[2].Stripe));

					Sides[BackIndices[2].Side].SetStripe(BackIndices[2].Stripe,
						Sides[BackIndices[1].Side].GetStripe(BackIndices[1].Stripe));

					Sides[BackIndices[1].Side].SetStripe(BackIndices[1].Stripe, buffer);
					break;
				#endregion

				#region Front
				case 15:
					buffer = Sides[FrontIndices[0].Side].GetStripe(FrontIndices[0].Stripe);

					Sides[FrontIndices[0].Side].SetStripe(FrontIndices[0].Stripe,
						Sides[FrontIndices[3].Side].GetStripe(FrontIndices[3].Stripe));

					Sides[FrontIndices[3].Side].SetStripe(FrontIndices[3].Stripe,
						Sides[FrontIndices[2].Side].GetStripe(FrontIndices[2].Stripe));

					Sides[FrontIndices[2].Side].SetStripe(FrontIndices[2].Stripe,
						Sides[FrontIndices[1].Side].GetStripe(FrontIndices[1].Stripe));

					Sides[FrontIndices[1].Side].SetStripe(FrontIndices[1].Stripe, buffer);
					break;
				case 16:
					buffer = Sides[FrontIndices[0].Side].GetStripe(FrontIndices[0].Stripe);
					Sides[FrontIndices[0].Side].SetStripe(FrontIndices[0].Stripe,
						Sides[FrontIndices[2].Side].GetStripe(FrontIndices[2].Stripe));
					Sides[FrontIndices[2].Side].SetStripe(FrontIndices[2].Stripe, buffer);

					buffer = Sides[FrontIndices[1].Side].GetStripe(FrontIndices[1].Stripe);
					Sides[FrontIndices[1].Side].SetStripe(FrontIndices[1].Stripe,
						Sides[FrontIndices[3].Side].GetStripe(FrontIndices[3].Stripe));
					Sides[FrontIndices[3].Side].SetStripe(FrontIndices[3].Stripe, buffer);
					break;
				case 17:
					buffer = Sides[FrontIndices[0].Side].GetStripe(FrontIndices[0].Stripe);

					Sides[FrontIndices[0].Side].SetStripe(FrontIndices[0].Stripe,
						Sides[FrontIndices[1].Side].GetStripe(FrontIndices[1].Stripe));

					Sides[FrontIndices[1].Side].SetStripe(FrontIndices[1].Stripe,
						Sides[FrontIndices[2].Side].GetStripe(FrontIndices[2].Stripe));

					Sides[FrontIndices[2].Side].SetStripe(FrontIndices[2].Stripe,
						Sides[FrontIndices[3].Side].GetStripe(FrontIndices[3].Stripe));

					Sides[FrontIndices[3].Side].SetStripe(FrontIndices[3].Stripe, buffer);
					break;
					#endregion
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
						s += Sides[4][x, y].ToString()[0];

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
						s += Sides[5][x, y].ToString()[0];

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
				if (Sides[0].FitsPattern(SolvedColors[0], Side.SQUARE_MASK[i]))
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

				if (Sides[1].FitsPattern(SolvedColors[1], Side.SQUARE_MASK[i]))
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

			return topCounter +bottomCounter;
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
			//0
			(0, 4, 0, 7),
			(0, 4, 1, 6),
			(0, 3, 2, 7),
			(0, 3, 3, 6),
			(0, 5, 4, 7),
			(0, 5, 5, 6),
			(0, 2, 6, 3),
			(0, 2, 7, 2),

			//1
			(1, 4, 0, 3),
			(1, 4, 1, 2),
			(1, 2, 2, 7),
			(1, 2, 3, 6),
			(1, 5, 4, 3),
			(1, 5, 5, 2),
			(1, 3, 6, 3),
			(1, 3, 7, 2),

			//2
			(2, 4, 0, 1),
			(2, 4, 1, 0),
			(2, 5, 4, 5),
			(2, 5, 5, 4),

			//3
			(3, 4, 0, 5),
			(3, 4, 1, 4),
			(3, 5, 4, 1),
			(3, 5, 5, 0)
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

		static int[] TopOrientationSides = {
			4, 1, 5, 0
		};
		static int[] BottomOrientationSides = {
			4, 0, 5, 1
		};
		static int[] BottomOrientationFaces =
		{
			1, 5, 5, 3
		};
		public int CountOrientedEgdes()
		{
			int counter = 0;

			for (int i = 1; i < 8; i += 2)
			{
				if (Sides[2][i] / 2 == 1)
					counter++;
				if (Sides[3][i] / 2 == 1)
					counter++;
			}

			if (Sides[4][3] / 2 == 1)
				counter++;
			if (Sides[4][7] / 2 == 1)
				counter++;

			if (Sides[5][3] / 2 == 1)
				counter++;
			if (Sides[5][7] / 2 == 1)
				counter++;

			for (int i = 1; i < 8; i += 2)
			{
				if (Sides[2][i] / 2 == 2)
				{
					if (Sides[BottomOrientationSides[i / 2]][BottomOrientationFaces[i / 2]] / 2 != 1)
						counter++;
				}
				if (Sides[3][i] / 2 == 2)
				{
					if (Sides[TopOrientationSides[i / 2]][(i + 4) % 8] / 2 != 1)
						counter++;
				}
			}

			if (Sides[4][3] / 2 == 2 && Sides[1][1] / 2 != 1)
				counter++;
			if (Sides[5][3] / 2 == 2 && Sides[1][5] / 2 != 1)
				counter++;

			if (Sides[4][7] / 2 == 2 && Sides[0][1] / 2 != 1)
				counter++;
			if (Sides[5][7] / 2 == 2 && Sides[0][5] / 2 != 1)
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
