using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace CubeAD
{
	/// <summary>
	/// A class holding a sequenz of <see cref="CubeMove"/> 
	/// </summary>
	public class MoveSequence
	{
		public static Random Rnd = new Random(1337);

		public static readonly MoveSequence SuperFlip = new MoveSequence("U R2 F B R B2 R U2 L B2 R U' D' R2 F R' L B2 U2 F2");
		public static readonly MoveSequence CheckerBoard = new MoveSequence("L2 R2 D2 U2 F2 B2");
		public static readonly MoveSequence FmcWr = new MoveSequence("R' U' F D2 L2 F R2 U2 R2 B D2 L B2 D' B2 L' R' B D2 B U2 L U2 R' U' F");

		public static readonly MoveSequence APerm = new MoveSequence("R B' R F2 R' B R F2 R2");
		public static readonly MoveSequence GPerm = new MoveSequence("R U R' U' D R2 U' R U' R' U R' U R2 D'");
		public static readonly MoveSequence UPerm = new MoveSequence("R' U R' U' R' U' R' U R U R2");
		public static readonly MoveSequence TPerm = new MoveSequence("R U R' U' R' F R2 U' R' U' R U R' F'");
		public static readonly MoveSequence HPerm = new MoveSequence("R2 U2 R' U2 R2 U2 R2 U2 R' U2 R2");

		//Positiones the orange yellow blue corner
		public static readonly MoveSequence CornerPermutation = new MoveSequence("L' L' L' B2 R R R B L L L B2 R' R' R' D L' L' L' B2 R R R");

		public static readonly MoveSequence InverseCubeScramble = new MoveSequence("D2 U' R2 D2 B2 R2 B2 D L' D' B' D2 U L R' B' R D F U2");


		public int Count { get { return Moves.Count; } }

		public readonly List<CubeMove> Moves;

		public MoveSequence(IEnumerable<CubeMove> moves)
		{
			Moves = moves.ToList();
		}
		public MoveSequence(List<CubeMove> moves)
		{
			Moves = moves;
		}
		public MoveSequence(params CubeMove[] cm)
		{
			Moves = cm.ToList();
		}
		public MoveSequence(MoveSequence src)
		{
			Moves = src.Moves.ToList();
		}

		/// <summary>
		/// Creates a <see cref="MoveSequence"/> from a string of <see cref="CubeMove"/> 
		/// </summary>
		public MoveSequence(string s)
		{
			Moves = s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => FromString(x)).ToList();

			CubeMove FromString(string s)
			{
				int sum = (int)(CubeMove)Enum.Parse(typeof(CubeMove), s[0].ToString(), true);

				if (s.Length == 2)
					sum += s[1] == '2' ? 1 : 2;

				return (CubeMove)sum;
			}
		}

		/// <summary>
		/// Generates a random irreducable <see cref="MoveSequence"/> with <paramref name="length"/> moves.
		/// </summary>
		public MoveSequence(int length)
		{
			Moves = new List<CubeMove>(length);

			ReRoll(length);
		}

		/// <summary>
		/// Randomizes the moves of this <see cref="MoveSequence"/>
		/// </summary>
		public void ReRoll(int length)
		{
			Moves.Clear();

			MoveBlocker mb = new MoveBlocker();

			List<int> list = new List<int>(6);
			for (int i = 0; i < length; i++)
			{
				list.Clear();

				for (int j = 0; j < 6; j++)
				{
					if (!mb[j])
						list.Add(j);
				}

				int index = Rnd.Next(list.Count);
				int side = list[index];

				Moves.Add((CubeMove)(side * 3 + Rnd.Next(3)));

				mb.UpdateBlocked(Moves.Last());
			}
		}

		/// <returns>A <see cref="MoveSequence"/> that reverses all moves of this <see cref="MoveSequence"/> </returns>
		public MoveSequence GetReverse()
		{
			return new MoveSequence(Moves.Select(x => ReverseMove(x)).Reverse().ToList());
		}


		//[TODO] unit test with moveblocker
		/// <summary>
		/// Reduces this <see cref="MoveSequence"/> if possible
		/// </summary>
		public void Reduce()
		{
			bool reduced = false;

			do
			{
				reduced = false;
				for(int i = 0; i < Moves.Count - 1; i++)
				{
					int v1 = (int)Moves[i];
					int v2 = (int)Moves[i + 1];

					if (v1 / 3 == v2 / 3)
					{
						if((v1 % 3) + (v2 % 3) == 2)
						{
							Moves.RemoveRange(i, 2);
							reduced = true;
							break;
						}
						else
						{
							Moves.RemoveAt(i);
							Moves[i] = (CubeMove)((v1 % 3) + (v2 % 3) + (v1 / 3));
							reduced = true;
						}
					}
				}
			} while (reduced);
		}

		/// <returns> A new <see cref="MoveSequence"/> with all moves transformed by <paramref name="se"/> </returns>
		public MoveSequence TransForm(SymmetryElement se)
		{
			return new MoveSequence(Moves.Select(x => se.TransformMove(x)));
		}

		/// <returns>The reverse <see cref="CubeMove"/> to <paramref name="m"/></returns>
		public static CubeMove ReverseMove(CubeMove m)
		{
			int val = (int)m;
			return (CubeMove)(val - val % 3 + (2 * val + 2) % 3); 
		}

		public bool EqualOverSymmetry(MoveSequence other)
		{
			if (Count != other.Count)
				return false;

			foreach(var se in SymmetryElement.Elements)
			{
				MoveSequence c = other.TransForm(se);

				bool res = true;
				for(int i = 0; i < Count; i++)
				{
					if (Moves[i] != c.Moves[i])
					{
						res = false;
						break;
					}
				}

				if (res) return true;
			}

			return false;
		}

		public override string ToString()
		{
			return string.Join(" ", Moves.Select(x => x.ToString().Replace('P', '\'')));
		}
	}
}
