using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using CubeAD;

namespace CubeTester
{
	class SymmetryElementTester
	{
		[Test]
		public void MultTest()
		{
			SymmetryElement rot90 = SymmetryGroup.ColorMap[0];
			SymmetryElement rot180 = SymmetryGroup.ColorMap[1];

			rot90 *= rot90;

			Assert.AreEqual(rot180, rot90);
		}

	}
}
