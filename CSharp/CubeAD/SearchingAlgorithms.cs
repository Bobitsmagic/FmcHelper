using CubeAD.CubeIndexSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CubeAD.PreCompTables;

namespace CubeAD
{
	public static class SearchingAlgorithms
	{
		//Solving
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

		public static SortedBucketsSet GenerateSolvedTree(int maxDepth)
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(maxDepth);
			SortedBucketsSet set = new SortedBucketsSet(2_000);

			//HashSet<CubeIndex> set = new HashSet<CubeIndex>();

			ulong counter = 0;

			int currentMaxDepth;
			for (currentMaxDepth = 0; currentMaxDepth <= maxDepth; currentMaxDepth++)
			{
				CubeIndex start = new CubeIndex();
				start.LastMove = CubeMove.None;
				DFS(0, start, new MoveBlocker());
				set.RemoveDuplicates();
			}

			set.RemoveDuplicates();
			return set;

			void DFS(int depth, CubeIndex cube, MoveBlocker blocker)
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
		public static SortedBucketsSet GenerateSolvedTreeOrientedEdges(int maxDepth)
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(maxDepth);
			SortedBucketsSet set = new SortedBucketsSet(2_000);

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

			List<MoveSequenz> eoMoves = FindEdgeOrientationMoves(cube.EdgeOrientationIndex);
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
							solution.AddRange(FindSolutionInTree(cube, GetEdgeOrientedMoveTree));

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

		public static MoveSequenz FindSolutionBruteForceMultithreaded(CubeIndex cube)
		{
			bool foundSolution = false;
			int currentMaxDepth;
			List<CubeMove> solution = new List<CubeMove>();
			Stack<CubeMove>[] currentMoves = new Stack<CubeMove>[18];
			PreCompTables.GetMoveTree.Contains(new CubeIndex());

			for (int i = 0; i < currentMoves.Length; i++)
				currentMoves[i] = new Stack<CubeMove>(20);


			for (int i = 0; i <= 20 && !foundSolution; i++)
			{
				currentMaxDepth = i;
				Console.WriteLine(currentMaxDepth);
				Thread[] threads = new Thread[18];
				for (int j = 0; j < 18; j++)
				{
					threads[j] = new Thread(CallThread);
					threads[j].Start(new CubeIndex(cube, (CubeMove)j));
				}

				for (int j = 0; j < 18; j++)
				{
					threads[j].Join();
				}
			}

			Console.WriteLine("Solution length: " + solution.Count);
			return new MoveSequenz(solution);

			void CallThread(object o)
			{
				MoveBlocker mb = new MoveBlocker();
				CubeIndex cast = (CubeIndex)o;
				mb.UpdateBlocked((int)cast.LastMove / 3);
				int index = (int)cast.LastMove;
				//Console.WriteLine("Index: " + index);
				currentMoves[index].Clear();

				if(DFS(0, cast, mb))
				{
					Console.WriteLine("Setupmove: " + cast.LastMove);
				}

				bool DFS(int depth, CubeIndex cube, MoveBlocker blocker)
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

								//Console.WriteLine("DFS moves: " + string.Join(" ", currentMoves.Reverse()));

								solution.AddRange(currentMoves[index].Reverse());
								solution.AddRange(FindSolutionInTree(cube, GetMoveTree));

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
							if (DFS(depth + 1, new CubeIndex(cube, (CubeMove)i), new MoveBlocker(blocker, i / 3)))
								return true;
							currentMoves[index].Pop();
						}
					}

					return false;
				}
			}

			
		}


		public static void PrintCornerOrientdist()
		{
			int checkedCorner = 0;

			Dictionary<int, int> dic = new Dictionary<int, int>();
			
			for(int i = 0; i < 18; i++)
			{
				CubeIndex first = new CubeIndex();
				first.MakeMove((CubeMove)i);
				for(int j = 0; j < 18; j++)
				{
					CubeIndex second = new CubeIndex();
					second.MakeMove((CubeMove)j);

					CubeIndex third =	new CubeIndex();
					third.MakeMove((CubeMove)i);
					third.MakeMove((CubeMove)j);


					int[] firstPerm = Permutation.GetInverse(first.GetCornerPerm());
					int[] secondPerm = Permutation.GetInverse(second.GetCornerPerm());
					int[] thirdPerm = Permutation.GetInverse(third.GetCornerPerm());

					int v1 = first.GetSingleCornerOrientation(firstPerm[checkedCorner]);
					int v2 = second.GetSingleCornerOrientation(thirdPerm[checkedCorner]);
					int v3 = third.GetSingleCornerOrientation(thirdPerm[checkedCorner]);

					int mesh = v1 + v2 * 3;


					Console.WriteLine((CubeMove)i + " " + (CubeMove)j + ": " + v1 + " "  + v2 + " -> " + v3);

					if (dic.ContainsKey(mesh))
					{
						if(dic[mesh] != v3)
						{
							Console.WriteLine("Mistkae");
						}
					}
					else
					{
						dic.Add(mesh, v3);	
					}
					
				}
			}
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
				if (DFS(0, cube, new MoveBlocker()))
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
					solution.AddRange(FindSolutionInTree(cube, GetMoveTree));

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
		public static List<CubeMove> FindSolutionInTree(CubeIndex cube, SealedHashset hs)
		{
			List<CubeMove> list = new List<CubeMove>();

			while (!cube.IsSovled)
			{
				if (hs.TryGetValue(cube, out cube))
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

		public static void CheckTree(SealedHashset sh)
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(20);
			List<CubeMove> list = new List<CubeMove>();
			HashSet<CubeIndex> visited = new HashSet<CubeIndex>();
			int currentMaxDepth = 0;

			for(int i = 0; i <= MAX_MOVE_DEPTH; i++)
			{
				Console.WriteLine("SearchDepth: " + i);
				DFS(0, new CubeIndex(), new MoveBlocker());
				currentMaxDepth++;
			}

			void DFS(int depth, CubeIndex cube, MoveBlocker blocker)
			{
				if (depth == currentMaxDepth)
				{
					if (visited.Add(cube))
					{
						if (!sh.Contains(cube))
						{
							Console.WriteLine("This should not happen");
							Console.WriteLine("Actual: " + string.Join(" ", currentMoves.Reverse()));
						}
						else if(depth != FindSolutionInTree(cube, sh).Count)
						{
							Console.WriteLine("Oh no " + FindSolutionInTree(cube, sh).Count);
							Console.WriteLine("Actual: " + string.Join(" ", currentMoves.Reverse()));
							Console.WriteLine("Found:  " + string.Join(" ", FindSolutionInTree(cube, sh)));
						}
					}
				}

				if (depth < currentMaxDepth)
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

		public static void CheckEOTree(SealedHashset sh)
		{
			Stack<CubeMove> currentMoves = new Stack<CubeMove>(20);
			List<CubeMove> list = new List<CubeMove>();
			HashSet<CubeIndex> visited = new HashSet<CubeIndex>();
			int currentMaxDepth = 0;

			for (int i = 0; i <= MAX_EO_MOVE_DEPTH; i++)
			{
				Console.WriteLine("SearchDepth: " + i);
				DFS(0, new CubeIndex(), new MoveBlocker());
				currentMaxDepth++;
			}

			void DFS(int depth, CubeIndex cube, MoveBlocker blocker)
			{
				if (depth == currentMaxDepth)
				{
					if (visited.Add(cube))
					{
						if (!sh.Contains(cube))
						{
							Console.WriteLine("This should not happen");
							Console.WriteLine("Actual: " + string.Join(" ", currentMoves.Reverse()));
						}
						else if (depth != FindSolutionInTree(cube, sh).Count)
						{
							Console.WriteLine("Oh no " + FindSolutionInTree(cube, sh).Count);
							Console.WriteLine("Actual: " + string.Join(" ", currentMoves.Reverse()));
							Console.WriteLine("Found:  " + string.Join(" ", FindSolutionInTree(cube, sh)));
						}
					}
				}

				if (depth < currentMaxDepth)
				{
					for (int i = 0; i < 18; i++)
					{
						if (blocker[i / 3]) continue;
						if (i / 3 > 3 && i % 3 != 1) continue; //Skipping F, F', B, B'
						currentMoves.Push((CubeMove)i);

						DFS(depth + 1, new CubeIndex(cube, (CubeMove)i), new MoveBlocker(blocker, i / 3));

						currentMoves.Pop();
					}
				}
			}
		}
		public static MoveSequenz FindCommutator(CubeIndex cube, int maxDepth)
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

			void GenerateMoveSeq(int depth, CubeIndex cube, MoveBlocker blocker)
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

								if (ms.Length < bestSolution)
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
	}
}
