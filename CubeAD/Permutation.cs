using System;

namespace CubeAD
{
	//A class that compresses a sequenz of moves into a single data permutation
	//[TODO] unit tests
	class Permutation
	{
		static byte[][] SideSwaps =
		{
			new byte[] { 2, 3, 4, 5, 6, 7, 0, 1 },
			new byte[] { 4, 5, 6, 7, 0, 1, 2, 3 },
			new byte[] { 6, 7, 0, 1, 2, 3, 4, 5 }
		};

		static (byte Side, byte Stripe)[] StripeIndices =
		{
			(4, 3),
			(3, 3),
			(5, 3),
			(2, 1),

			(4, 1),
			(2, 3),
			(5, 1),
			(3, 1),

			(4, 0),
			(0, 3),
			(5, 2),
			(1, 1),

			(4, 2),
			(1, 3),
			(5, 0),
			(0, 1),

			(2, 0),
			(1, 0),
			(3, 0),
			(0, 0),

			(3, 2),
			(1, 2),
			(2, 2),
			(0, 2)
		};

		public byte[] Perm = new byte[48];
		public MoveSequenz Sequenz;

		public Permutation()
		{
			Sequenz = new MoveSequenz(0);
		}

		public void DoMove(CubeMove m)
		{
			byte[] sideBuffer = new byte[8];
			byte valueBuffer;
			int side = (int)m / 3;
			int count = (int)m % 3;

			for (int i = 0; i < 3; i++)
			{
				switch (count)
				{
					case 0:
						valueBuffer =
							Perm[GetIndex(StripeIndices[4 * side + 0], i)];
						Perm[GetIndex(StripeIndices[4 * side + 0], i)] =
							Perm[GetIndex(StripeIndices[4 * side + 3], i)];
						Perm[GetIndex(StripeIndices[4 * side + 3], i)] =
							Perm[GetIndex(StripeIndices[4 * side + 2], i)];
						Perm[GetIndex(StripeIndices[4 * side + 2], i)] =
							Perm[GetIndex(StripeIndices[4 * side + 1], i)];
						Perm[GetIndex(StripeIndices[4 * side + 1], i)] =
							valueBuffer;
						break;

					case 1:
						valueBuffer = Perm[GetIndex(StripeIndices[4 * side + 0], i)];
						Perm[GetIndex(StripeIndices[4 * side + 0], i)] =
							Perm[GetIndex(StripeIndices[4 * side + 2], i)];
						Perm[GetIndex(StripeIndices[4 * side + 2], i)] = valueBuffer;

						valueBuffer = Perm[GetIndex(StripeIndices[4 * side + 1], i)];
						Perm[GetIndex(StripeIndices[4 * side + 1], i)] =
							Perm[GetIndex(StripeIndices[4 * side + 3], i)];
						Perm[GetIndex(StripeIndices[4 * side + 3], i)] = valueBuffer;
						break;

					case 2:
						valueBuffer =
							Perm[GetIndex(StripeIndices[4 * side + 0], i)];
						Perm[GetIndex(StripeIndices[4 * side + 0], i)] =
							Perm[GetIndex(StripeIndices[4 * side + 1], i)];
						Perm[GetIndex(StripeIndices[4 * side + 1], i)] =
							Perm[GetIndex(StripeIndices[4 * side + 2], i)];
						Perm[GetIndex(StripeIndices[4 * side + 2], i)] =
							Perm[GetIndex(StripeIndices[4 * side + 3], i)];
						Perm[GetIndex(StripeIndices[4 * side + 3], i)] =
							valueBuffer;
						break;
				}
			}

			int offset = side * 8;
			byte[] permBuffer = SideSwaps[count];
			Buffer.BlockCopy(Perm, offset, sideBuffer, 0, 8);

			for (int i = 0; i < 8; i++)
			{
				Perm[offset + i] = sideBuffer[permBuffer[i]];
			}

			int GetIndex((int Side, int Stripe) tuple, int index)
			{
				return tuple.Side * 8 + ((tuple.Stripe * 2 + index) % 8);
			}
		}
	}
}
