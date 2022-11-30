using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using static CubeAD.PreCompTables;
using static CubeAD.Permutation;

namespace CubeAD
{
	/// <summary>
	/// A class representing a 3x3x3 rubiks cube storing the color of each sticker
	/// </summary>
	public class StickerCube
	{
		const int SIDE_COUNT = 6;
		const int LEFT = 0;
		const int RIGHT = 1;
		const int BOTTOM = 2;
		const int TOP = 3;
		const int FRONT = 4;
		const int BACK = 5;

		//The side values of a solved Cube
		private static readonly StickerSide[] SolvedColors =
{
			new StickerSide((CubeColor)0),
			new StickerSide((CubeColor)1),
			new StickerSide((CubeColor)2),
			new StickerSide((CubeColor)3),
			new StickerSide((CubeColor)4),
			new StickerSide((CubeColor)5)
		};

		//Adjacent stripes of adj sides (Clockwise order)
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

		//SIDE_COUNT Sides of the cube
		public StickerSide[] Sides = new StickerSide[SIDE_COUNT];

		/// <summary>
		/// Creates a solved cube
		/// </summary>
		public StickerCube() 
		{
			Reset();
		}
		public void Reset()
		{
			for (int i = 0; i < SIDE_COUNT; i++)
			{
				Sides[i] = new StickerSide((CubeColor)i);
			}
		}

		/// <summary>
		/// Creates a copy of a cube
		/// </summary>
		public StickerCube(StickerCube old) 
		{
			for (int i = 0; i < SIDE_COUNT; i++)
			{
				Sides[i] = old.Sides[i];
			}
		}

		/// <summary>
		/// Creates copy of a <see cref="StickerCube"/> and makes a <see cref="CubeMove"/> afterwards
		/// </summary>
		public StickerCube(StickerCube old, CubeMove m)
		{
			for (int i = 0; i < SIDE_COUNT; i++)
			{
				Sides[i] = old.Sides[i];
			}

			MakeMove(m);
		}
		
		public void CopyValuesFrom(StickerCube c)
		{
			for (int i = 0; i < SIDE_COUNT; i++)
			{
				Sides[i] = c.Sides[i];
			}
		}

		public void ApplyMoveSequenz(MoveSequenz s)
		{
			for (int i = 0; i < s.Moves.Count; i++)
				MakeMove(s.Moves[i]);
		}

		public void MakeMove(CubeMove m)
		{
			int side = (int)m / 3;

			//Rotate affected side
			Sides[side].Rotate(((int)m % 3) + 1);

			uint buffer;
			int moveType = (int)m % 3;
			(int Side, int Stripe)[] stripes = AdjStripes[side];

			//Swap stickers from adjacent sides
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
				for (int i = 0; i < SIDE_COUNT; i++)
					if (Sides[i] != SolvedColors[i]) return false;

				return true;
			}
		}

		//The index of the sticker on the first side which is adjacent to the second side 
		public static int[,] AdjEdges = new int[SIDE_COUNT, SIDE_COUNT]
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

		/// <returns>The color of the sticker on <paramref name="side1"/> adjacent to <paramref name="side2"/> </returns>
		public CubeColor GetEdgeColor(int side1, int side2)
		{
			return (CubeColor)Sides[side1][AdjEdges[side1, side2]];
		}
		/// <summary>
		/// Set the color of the sticker on <paramref name="side1"/> adjacent to <paramref name="side2"/> to <paramref name="color"/> 
		/// </summary>
		public void SetEdgeColor(int side1, int side2, int color)
		{
			Sides[side1][AdjEdges[side1, side2]] = (uint)color;
		}

		//The index of the sticker on the first side which is adjacent to the second and third side
		//Only the parity of the last side is considered
		public static int[,] AdjCorners = new int[SIDE_COUNT, SIDE_COUNT * 2]
		{
			//Left						removed because of redundancy
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

		/// <returns>The color of the sticker on <paramref name="side1"/> adjacent to <paramref name="side2"/> and <paramref name="side3"/></returns>
		public CubeColor GetCornerColor(int side1, int side2, int side3)
		{
			int lower = Math.Min(side2, side3);
			int higher = Math.Max(side2, side3);

			return (CubeColor)Sides[side1][AdjCorners[side1, lower * 2 + (higher & 1)]];
		}

		/// <summary>
		/// Set the color of the sticker on <paramref name="side1"/> adjacent to <paramref name="side2"/> and <paramref name="side3"/> to <paramref name="color"/> 
		/// </summary>
		public void SetCornerColor(int side1, int side2, int side3, int color)
		{
			int lower = Math.Min(side2, side3);
			int higher = Math.Max(side2, side3);

			Sides[side1][AdjCorners[side1, lower * 2 + (higher & 1)]] = (uint)color;
		}

		/// <summary>
		///  Checks whether this <see cref="StickerCube"/> is self symmetric
		/// </summary>
		/// <param name="se">Symmetry to check for</param>
		public bool HasSymmetry(SymmetryElement se)
		{
			//Check edges
			for (int x = 0; x < SIDE_COUNT; x++)
			{
				int nextX = se.TransformColor(x);
				for (int y = 0; y < SIDE_COUNT; y++)
				{
					if (x / 2 == y / 2) continue;

					int nextY = se.TransformColor(y);
					
					//Check whether the sticker has the correct color after the transformation and the color change
					if (se.TransformColor(GetEdgeColor(x, y)) != GetEdgeColor(nextX, nextY))
						return false;
				}
			}

			//Check corners
			for (int x = 0; x < SIDE_COUNT; x++)
			{
				int nextX = se.TransformColor(x);
				for (int y = 0; y < SIDE_COUNT; y++)
				{
					if (x / 2 == y / 2) continue;
					int nextY = se.TransformColor(y);

					for (int z = 0; z < SIDE_COUNT; z++)
					{
						if (x / 2 == z / 2 || y / 2 == z / 2) continue;
						int nextZ = se.TransformColor(z);

						//Check whether the sticker has the correct color after the transformation and the color change
						if (se.TransformColor(GetCornerColor(x, y, z)) != GetCornerColor(nextX, nextY, nextZ))
							return false;
					}

				}
			}

			return true;
		}

		/// <returns>A bit vector containing a 1 for each <see cref="SymmetryElement"/> this <see cref="StickerCube"/> has</returns>
		public BitMap64 GetSymmetrySet()
		{
			BitMap64 ret = new BitMap64();

			for(int i = 0; i < SymmetryElement.ORDER; i++) {
				ret[i] = HasSymmetry(SymmetryElement.Elements[i]);
			}

			return ret;
		}

		/// <returns>Whether this <see cref="StickerCube"/> is equal to <paramref name="other"/> with any symmetry </returns>
		public bool IsEqualWithSymmetry(StickerCube other)
		{
			for (int i = 0; i < SymmetryElement.ORDER; i++)
			{
				if (IsEqualWithSymmetry(other, SymmetryElement.Elements[i]))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Computes whether this <see cref="StickerCube"/> is symmetric to <paramref name="other"/> over <paramref name="se"/>
		/// </summary>
		public bool IsEqualWithSymmetry(StickerCube other, SymmetryElement se)
		{
			//Check edges
			for (int x = 0; x < 6; x++)
			{
				int nextX = se.TransformColor(x);
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					int nextY = se.TransformColor(y);

					//Check whether the sticker has the correct color after the transformation and the color change
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

						//Check whether the sticker has the correct color after the transformation and the color change
						if (se.TransformColor(GetCornerColor(x, y, z)) != other.GetCornerColor(nextX, nextY, nextZ))
							return false;
					}

				}
			}

			return true;
		}

		/// <summary>
		/// Copies over <paramref name="se"/> transformed stickers to <paramref name="cube"/>
		/// </summary>
		public void InsertSymmetryTransformation(SymmetryElement se, StickerCube cube)
		{
			//Check edges
			for (int x = 0; x < 6; x++)
			{
				int nextX = se.TransformColor(x);
				for (int y = 0; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					int nextY = se.TransformColor(y);

					cube.SetEdgeColor(nextX, nextY, (int)se.TransformColor(GetEdgeColor(x, y)));
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

						cube.SetCornerColor(nextX, nextY, nextZ, (int)se.TransformColor(GetCornerColor(x, y, z)));
					}
				}
			}
		}

		/// <returns> A cube net like representation</returns>
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

		/// <summary>
		/// Prints a colored side view of this <see cref="StickerCube"/> to the console
		/// </summary>
		public void PrintSideView()
		{
			string s = GetSideView();
			for (int i = 0; i < s.Length; i++)
			{
				Color color;
				switch (s[i])
				{
					case 'W': color = Color.White; break;
					case 'R': color = Color.Red; break;
					case 'O': color = Color.Orange; break;
					case 'B': color = Color.Blue; break;
					case 'G': color = Color.Green; break;
					case 'Y': color = Color.Yellow; break;
					default: color = Color.Gray; break;
				}

				Colorful.Console.Write(s[i], color);
			}

			Console.ForegroundColor = ConsoleColor.White;
			Colorful.Console.Write("\n", Color.White);
		}

		/// <returns>The orientaion of the edge at <paramref name="side1"/>/<paramref name="side2"/></returns>
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

		/// <returns>The orientaion of the corner at <paramref name="side1"/>/<paramref name="side2"/>/<paramref name="side3"/></returns>
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

		//CubeIndex generation

		/// <param name="permBuffer"> A buffer to avoid GC pressure </param>
		/// <returns> The edge permutation index of this <see cref="StickerCube"/> </returns>
		public uint FindEdgePermutationIndex(int[] permBuffer)
		{
			if (permBuffer.Length != 12)
				throw new ArgumentException("Array must be of length 12");

			int counter = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					permBuffer[EdgeIndices[(int)GetEdgeColor(x, y), (int)GetEdgeColor(y, x)]] = counter++;
				}
			}
			return (uint)GetIndex(permBuffer);
		}
		/// <returns> The edge permutation index of this <see cref="StickerCube"/> </returns>
		public uint FindEdgePermutationIndex()
		{
			return FindEdgePermutationIndex(new int[12]);
		}
		/// <param name="permBuffer"> A buffer to avoid GC pressure </param>
		/// <returns> The corner permutation index of this <see cref="StickerCube"/> </returns>
		public ushort FindCornerPermutationIndex(int[] permBuffer)
		{
			if (permBuffer.Length != 8)
				throw new ArgumentException("Array must be of length 8");

			int counter = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						permBuffer[CornerIndices[(int)GetCornerColor(x, y, z), (int)GetCornerColor(y, x, z), (int)GetCornerColor(z, x, y)]] = counter++;
					}
				}
			}

			return (ushort)GetIndex(permBuffer);
		}
		/// <returns> The edge permutation index of this <see cref="StickerCube"/> </returns>
		public ushort FindCornerPermutationIndex()
		{
			return FindCornerPermutationIndex(new int[8]);
		}
		/// <returns> The edge orientation index of this <see cref="StickerCube"/> </returns>
		public ushort FindEdgeOrientationIndex()
		{
			int ret = 0;
			for (int x = 0; x < 6; x++)
			{
				for (int y = x + 1; y < 6; y++)
				{
					if (x / 2 == y / 2) continue;

					ret = (ret << 1) | (EdgeIsOriented(x, y) ? 0 : 1);
				}
			}

			return (ushort)ret;
		}
		/// <returns> The corner orientation index of this <see cref="StickerCube"/> </returns>
		public ushort FindCornerOrientationIndex()
		{
			int ret = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 2; y < 4; y++)
				{
					for (int z = 4; z < 6; z++)
					{
						//Console.WriteLine(c.CornerOrientaion(x, y, z));
						ret = (ret * 3) + CornerOrientaion(x, y, z);
					}
				}
			}

			return (ushort)ret;
		}

		public static bool operator ==(StickerCube a, StickerCube b)
		{
			for (int i = 0; i < 6; i++)
				if (a.Sides[i] != b.Sides[i]) return false;

			return true;
		}
		public static bool operator !=(StickerCube a, StickerCube b)
		{
			return !(a == b);
		}
		public override bool Equals(object obj)
		{
			StickerCube c = (StickerCube)obj;

			for (int i = 0; i < 6; i++)
			{
				if (Sides[i] != c.Sides[i]) return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Sides[0], Sides[1], Sides[2], Sides[3], Sides[4], Sides[5]);
		}
	}
}