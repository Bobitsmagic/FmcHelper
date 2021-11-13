using CubeAD;
using NUnit.Framework;


namespace CubeTester
{
	public class SideTester
	{
		[Test]
		public void InitialValueTest()
		{
			Side side;
			for (int i = 0; i < 6; i++)
			{
				side = new Side((CubeColor)i);

				for (int j = 0; j < 8; j++)
				{
					Assert.AreEqual(side[j], (uint)i);
				}
			}
		}
		[Test]
		public void SetValueTest()
		{
			Side side;
			for (int i = 0; i < 6; i++)
			{
				side = new Side((CubeColor)i);

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
			Side side;
			for (int i = 0; i < 6; i++)
			{
				side = new Side((CubeColor)i);

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
					Side src = new Side((CubeColor)3);
					Side dest = new Side((CubeColor)5);

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