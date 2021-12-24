using CubeAD;
using NUnit.Framework;


namespace CubeTester
{
	public class SideTester
	{
		[Test]
		public void InitialValueTest()
		{
			StickerSide side;
			for (int i = 0; i < 6; i++)
			{
				side = new StickerSide((CubeColor)i);

				for (int j = 0; j < 8; j++)
				{
					Assert.AreEqual(side[j], (uint)i);
				}
			}
		}
		[Test]
		public void SetValueTest()
		{
			StickerSide side;
			for (int i = 0; i < 6; i++)
			{
				side = new StickerSide((CubeColor)i);

				for (int j = 0; j < 6; j++)
				{
					side[j] = (uint)j;
				}

				for (int j = 0; j < 6; j++)
				{
					Assert.AreEqual(side[j], (uint)j);
				}
			}
		}

		[Test]
		public void RotateTest()
		{
			StickerSide side;
			for (int i = 0; i < 6; i++)
			{
				side = new StickerSide((CubeColor)i);

				for (int j = 0; j < 6; j++)
				{
					side[j] = (uint)j;
				}

				side.Rotate(1);
				side.Rotate(2);
				side.Rotate(3);
				side.Rotate(2);

				for (int j = 0; j < 6; j++)
				{
					Assert.AreEqual(side[j], (uint)j);
				}
			}
		}

		[Test]
		public void StripeTest()
		{
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					StickerSide src = new StickerSide((CubeColor)3);
					StickerSide dest = new StickerSide((CubeColor)5);

					System.Diagnostics.Debug.Write(dest.ToString() + " -> ");
					dest.SetStripe(i, src.GetStripe(j));
					System.Diagnostics.Debug.WriteLine(dest.ToString());

					Assert.AreEqual(dest.GetStripe(i), src.GetStripe(j));
					for (int k = 0; k < 4; k++)
					{
						if (k != i)
							Assert.AreNotEqual(dest.GetStripe(k), src.GetStripe(j));
					}
				}
			}
		}
	}
}