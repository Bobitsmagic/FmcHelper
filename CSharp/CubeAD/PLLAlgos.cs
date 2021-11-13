using System;
using System.Collections.Generic;

namespace CubeAD
{
	//A set of all pll algorithms
	public static class PLLAlgos
	{
		public static List<MoveSequenz>[] Cases = new List<MoveSequenz>[12];

		public static void Init()
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>();

			//Avoid GC Pressure
			List<CubeMove>[] moveBuffer = new List<CubeMove>[20];
			for (int i = 0; i < moveBuffer.Length; i++)
				moveBuffer[i] = new List<CubeMove>(18);

			bool[] used = new bool[6];
			int sideCounter = 0;
			int depth = 0;

			int MaxDepth = 11;
			int MaxSideCounter;
			long counter = 0;

			//H
			MoveSequenz ms = new MoveSequenz("R2 U2 R' U2 R2 U2 R2 U2 R' U2 R2").GetReverse();
			Cube c = new Cube();
			c.ApplyMoveSequenz(ms);

			Console.WriteLine(ms);
			c.PrintSideView();

			int firstCount;
			for (int i = 2; i < 6; i++)
			{
				MaxSideCounter = i;
				firstCount = 0;
				Console.WriteLine("MaxSides: " + MaxSideCounter);
				Solve(c);
			}

			void Solve(Cube cube)
			{
				counter++;
				if (counter % 100000000 == 0)
				{
					Console.WriteLine((counter / 1000000).ToString("000 000 000"));
				}

				if (cube.IsAUF() && sideCounter == MaxSideCounter)
				{
					Console.WriteLine("Solution found: " + string.Join(" ", currentMoves));
				}

				if (depth < MaxDepth)
				{
					List<CubeMove> list = moveBuffer[depth];
					list.Clear();
					cube.AddPossibleMoves(list);

					foreach (CubeMove move in list)
					{
						if (depth == 0) Console.WriteLine("FC: " + firstCount++);
						if (!used[(int)move / 3])
						{
							if (sideCounter < MaxSideCounter)
							{
								used[(int)move / 3] = true;
								sideCounter++;
								depth++;

								currentMoves.Push(move);
								Solve(new Cube(cube, move));
								currentMoves.Pop();

								depth--;
								sideCounter--;
								used[(int)move / 3] = false;
							}
						}
						else
						{
							depth++;
							currentMoves.Push(move);
							Solve(new Cube(cube, move));
							currentMoves.Pop();
							depth--;
						}
					}
				}
			}
		}
	}
}
