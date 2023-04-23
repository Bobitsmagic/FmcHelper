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
	public class MoveSequenz
	{
		public static Random Rnd = new Random(0);

		public static readonly MoveSequenz SuperFlip = new MoveSequenz("U R2 F B R B2 R U2 L B2 R U' D' R2 F R' L B2 U2 F2");
		public static readonly MoveSequenz CheckerBoard = new MoveSequenz("L2 R2 D2 U2 F2 B2");
		public static readonly MoveSequenz FmcWr = new MoveSequenz("R' U' F D2 L2 F R2 U2 R2 B D2 L B2 D' B2 L' R' B D2 B U2 L U2 R' U' F");

		public static readonly MoveSequenz APerm = new MoveSequenz("R B' R F2 R' B R F2 R2");
		public static readonly MoveSequenz GPerm = new MoveSequenz("R U R' U' D R2 U' R U' R' U R' U R2 D'");
		public static readonly MoveSequenz UPerm = new MoveSequenz("R' U R' U' R' U' R' U R U R2");
		public static readonly MoveSequenz TPerm = new MoveSequenz("R U R' U' R' F R2 U' R' U' R U R' F'");
		public static readonly MoveSequenz HPerm = new MoveSequenz("R2 U2 R' U2 R2 U2 R2 U2 R' U2 R2");

		//Positiones the orange yellow blue corner
		public static readonly MoveSequenz CornerPermutation = new MoveSequenz("L' L' L' B2 R R R B L L L B2 R' R' R' D L' L' L' B2 R R R");

		public static readonly MoveSequenz InverseCubeScramble = new MoveSequenz("D2 U' R2 D2 B2 R2 B2 D L' D' B' D2 U L R' B' R D F U2");


		public int Count { get { return Moves.Count; } }

		public readonly List<CubeMove> Moves;

		public MoveSequenz(IEnumerable<CubeMove> moves)
		{
			Moves = moves.ToList();
		}
		public MoveSequenz(List<CubeMove> moves)
		{
			Moves = moves;
		}
		public MoveSequenz(params CubeMove[] cm)
		{
			Moves = cm.ToList();
		}
		public MoveSequenz(MoveSequenz src)
		{
			Moves = src.Moves.ToList();
		}

		/// <summary>
		/// Creates a <see cref="MoveSequenz"/> from a string of <see cref="CubeMove"/> 
		/// </summary>
		public MoveSequenz(string s)
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
		/// Generates a random irreducable <see cref="MoveSequenz"/> with <paramref name="length"/> moves.
		/// </summary>
		public MoveSequenz(int length)
		{
			Moves = new List<CubeMove>(length);

			ReRoll(length);
		}

		/// <summary>
		/// Randomizes the moves of this <see cref="MoveSequenz"/>
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

		/// <returns>A <see cref="MoveSequenz"/> that reverses all moves of this <see cref="MoveSequenz"/> </returns>
		public MoveSequenz GetReverse()
		{
			return new MoveSequenz(Moves.Select(x => ReverseMove(x)).Reverse().ToList());
		}


		//[TODO] unit test with moveblocker
		/// <summary>
		/// Reduces this <see cref="MoveSequenz"/> if possible
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

		/// <returns> A new <see cref="MoveSequenz"/> with all moves transformed by <paramref name="se"/> </returns>
		public MoveSequenz Rotate(SymmetryElement se)
		{
			return new MoveSequenz(Moves.Select(x => se.TransformMove(x)));
		}

		/// <returns>The reverse <see cref="CubeMove"/> to <paramref name="m"/></returns>
		public static CubeMove ReverseMove(CubeMove m)
		{
			int val = (int)m;
			return (CubeMove)(val - val % 3 + (2 * val + 2) % 3); 
		}

		public bool EqualOverSymmetry(MoveSequenz other)
		{
			if (Count != other.Count)
				return false;

			foreach(var se in SymmetryElement.Elements)
			{
				MoveSequenz c = other.Rotate(se);

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
