using CubeAD.IndexCubeSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CubeAD.PreCompTables;

namespace CubeAD
{
	/// <summary>
	/// Provides algorithms for searching through diffrent sets of cubes
	/// </summary>
	public static class SearchingAlgorithms
	{
		/// <returns> All shortest <see cref="MoveSequenz"/> to orient all edges </returns>
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
		
		/// <returns> A tree containing all cubes that can be solved in <paramref name="maxDepth"/> moves or less </returns>
		public static SortedBucketsSet GenerateSolvedTree(int maxDepth)
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(maxDepth);
			SortedBucketsSet set = new SortedBucketsSet(2_000);

			ulong counter = 0;

			int currentMaxDepth;
			for (currentMaxDepth = 0; currentMaxDepth <= maxDepth; currentMaxDepth++)
			{
				IndexCube start = new IndexCube();
				start.LastMove = CubeMove.None;
				DFS(0, start, new MoveBlocker());
				set.RemoveDuplicates();
			}

			set.RemoveDuplicates();
			return set;

			void DFS(int depth, IndexCube cube, MoveBlocker blocker)
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

						currentMoves.Push((CubeMove)i);
						DFS(depth + 1, new IndexCube(cube, (CubeMove)i), new MoveBlocker(blocker, (CubeMove)i));
						currentMoves.Pop();
					}
				}
			}
		}

		/// <returns> A tree containing all (edge oriented) cubes that can be solved in <paramref name="maxDepth"/> moves (no F, F', B or B') or less </returns>
		public static SortedBucketsSet GenerateSolvedTreeOrientedEdges(int maxDepth)
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(maxDepth);
			SortedBucketsSet set = new SortedBucketsSet(2_000);

			ulong counter = 0;

			int currentMaxDepth;
			for (currentMaxDepth = 0; currentMaxDepth <= maxDepth; currentMaxDepth++)
			{
				DFS(0, new IndexCube(), new MoveBlocker());
				set.RemoveDuplicates();
			}

			set.RemoveDuplicates();
			return set;

			void DFS(int depth, IndexCube cube, MoveBlocker blocker)
			{
				set.Add(cube);
				if (counter++ % 100_000_000 == 0)
				{
					set.RemoveDuplicates();
					Console.WriteLine("Sorted: " + set.Count.ToString("000 000 000 000"));
				}

				if (depth < currentMaxDepth)
				{
					for (int i = 0; i < 18; i++)
					{
						if (blocker[i / 3]) continue;
						if (i / 3 > 3 && i % 3 != 1) continue;

						currentMoves.Push((CubeMove)i);
						DFS(depth + 1, new IndexCube(cube, (CubeMove)i), new MoveBlocker(blocker, (CubeMove)i));
						currentMoves.Pop();
					}
				}
			}
		}

		/// <summary>
		/// Searches for a solution for edge orientation and then solves the cube from there using only EO preserving moves
		/// </summary>
		public static MoveSequenz FindSolutionWithEO(IndexCube cube)
		{
			const int MAX_DEPTH = 10;

			List<MoveSequenz> eoMoves = FindEdgeOrientationMoves(cube.EdgeOrientationIndex);
			Console.WriteLine("Eo Length: " + eoMoves[0].Count + " count: " + eoMoves.Count);
			List<CubeMove> solution = new();

			bool foundSolution = false;
			int currentMaxDepth = 0;

			for (currentMaxDepth = 0; currentMaxDepth <= MAX_DEPTH && !foundSolution; currentMaxDepth++)
			{
				Parallel.ForEach(eoMoves, ms =>
				{
					IndexCube copy = cube;

					copy.ApplyMoveSequenz(ms);

					if (FindEOSolution(copy))
					{
						solution.InsertRange(0, ms.Moves);
					}
				});
			}

			Console.WriteLine("Finished all threads");
			Console.WriteLine("Solution length: " + solution.Count);
			return new MoveSequenz(solution);

			bool FindEOSolution(IndexCube cube)
			{
				Stack<CubeMove> currentMoves = new Stack<CubeMove>(20);

				return DFS(0, cube, new MoveBlocker());

				bool DFS(int depth, IndexCube cube, MoveBlocker blocker)
				{
					if (foundSolution) return false;

					if (GetEdgeOrientedMoveTree.Contains(cube))
					{
						if (!foundSolution)
						{
							foundSolution = true;

							solution.Clear();
							solution.AddRange(currentMoves.Reverse());
							solution.AddRange(GetEdgeOrientedMoveTree.SearchSolutionInTree(cube));

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
							if (DFS(depth + 1, new IndexCube(cube, (CubeMove)i), new MoveBlocker(blocker, (CubeMove)i)))
								return true;
							currentMoves.Pop();
						}
					}

					return false;
				}
			}
		}
		
		/// <summary>
		/// Searches for a solution with increasing depth
		/// </summary>
		public static MoveSequenz FindSolutionBruteForce(IndexCube cube)
		{
			List<CubeMove> solution = new List<CubeMove>();
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(20);

			int currentMaxDepth;
			for (int i = 0; i <= 20; i++)
			{
				currentMaxDepth = i;
				Console.WriteLine(currentMaxDepth);
				if (DFS(0, cube, new MoveBlocker()))
				{
					break;
				}
			}


			Console.WriteLine("Solution length: " + solution.Count);
			return new MoveSequenz(solution);


			bool DFS(int depth, IndexCube cube, MoveBlocker blocker)
			{
				if (GetMoveTree.Contains(cube))
				{
					//Console.WriteLine("Found solution!");
					solution.Clear();

					//Console.WriteLine("DFS moves: " + string.Join(" ", currentMoves.Reverse()));

					solution.AddRange(currentMoves.Reverse());
					solution.AddRange(GetMoveTree.SearchSolutionInTree(cube));

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
						if (DFS(depth + 1, new IndexCube(cube, (CubeMove)i), new MoveBlocker(blocker, (CubeMove)i)))
							return true;
						currentMoves.Pop();
					}
				}

				return false;
			}



		}

		/// <summary>
		/// Searches for a solution in parallel using a certain amount of threads
		/// </summary>
		public static MoveSequenz FindSolutionBruteForceMultithreaded(IndexCube cube, int threadCount = 4)
		{
			bool foundSolution = false;
			int currentMaxDepth;
			List<CubeMove> solution = new List<CubeMove>();
			Stack<CubeMove>[] currentMoves = new Stack<CubeMove>[18];
			GetMoveTree.Contains(new IndexCube());

			Queue<CubeMove> openList = new(18);

			for (int i = 0; i < currentMoves.Length; i++)
				currentMoves[i] = new Stack<CubeMove>(20);

			for (int i = 0; i <= 20 && !foundSolution; i++)
			{
				currentMaxDepth = i;
				Console.WriteLine(currentMaxDepth);
				Thread[] threads = new Thread[threadCount];

				openList.Clear();
				for (int j = 0; j < 18; j++)
					openList.Enqueue((CubeMove)j);
				
				for(int j = 0; j < threadCount; j++)
                {
					threads[j] = new Thread(CallThread);
					threads[j].Start(new IndexCube(cube, (CubeMove)j));
                }

				for (int j = 0; j < threadCount; j++)
				{
					threads[j].Join();
				}
			}

			Console.WriteLine("Solution length: " + solution.Count);
			return new MoveSequenz(solution);

			void CallThread()
			{
                while (true)
                {
					CubeMove lastMove;
					lock (openList)
                    {
						if(openList.Count > 0)
                        {
							lastMove = openList.Dequeue();
						}
						else 
							break;
                    }

					IndexCube cast = new IndexCube(cube, lastMove);
					MoveBlocker mb = new MoveBlocker();
					mb.UpdateBlocked(lastMove);
					int index = (int)cast.LastMove;
					currentMoves[index].Clear();

					if (DFS(0, cast, mb))
					{
						Console.WriteLine("Setupmove: " + cast.LastMove);
					}

					bool DFS(int depth, IndexCube cube, MoveBlocker blocker)
					{
						if (foundSolution) return false;

						if (GetMoveTree.Contains(cube))
						{
							lock (solution)
							{
								if (!foundSolution)
								{
									foundSolution = true;
									Console.WriteLine("Found solution!");
									solution.Clear();

									solution.AddRange(currentMoves[index].Reverse());
									solution.AddRange(GetMoveTree.SearchSolutionInTree(cube));

									Console.WriteLine(string.Join(" ", solution));

									return true;
								}
							}


							return false;
						}

						if (depth < currentMaxDepth)
						{
							for (int i = 0; i < 18; i++)
							{
								if (blocker[i / 3]) continue;

								currentMoves[index].Push((CubeMove)i);
								if (DFS(depth + 1, new IndexCube(cube, (CubeMove)i), new MoveBlocker(blocker, (CubeMove)i)))
									return true;
								currentMoves[index].Pop();
							}
						}

						return false;
					}
                }
			}
		}		

		/// <summary>
		/// Searches for a solution using a commutator of the form (S A B A' B' S')  with A, B, S being lists of moves
		/// </summary>
		public static MoveSequenz FindCommutator(IndexCube cube, int maxDepth)
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>();
			List<CubeMove> ret = new List<CubeMove>();
			MoveSequenz ms = new MoveSequenz();
			int bestSolution = int.MaxValue;
			int currentMaxLength;
			for (currentMaxLength = 2; currentMaxLength < maxDepth; currentMaxLength++)
			{

				Console.WriteLine("MaxSearchDepth: " + currentMaxLength);
				GenerateMoveSeq(0, cube, new MoveBlocker());
			}

			if (bestSolution < int.MaxValue)
			{
				return new MoveSequenz(ret);
			}

			return null;

			void GenerateMoveSeq(int depth, IndexCube cube, MoveBlocker blocker)
			{
				if (depth == currentMaxLength)
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

								if (ms.Count < bestSolution)
								{
									bestSolution = ms.Count;

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
						GenerateMoveSeq(depth + 1, new IndexCube(cube, (CubeMove)i), new MoveBlocker(blocker, (CubeMove)i, true));
						currentMoves.Pop();
					}
				}
			}
		}
	}
}
