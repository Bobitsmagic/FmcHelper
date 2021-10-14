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
		public void IdentityTest()
		{
			SymmetryElement identity = SymmetryGroup.Elements[0];

			Assert.IsTrue(identity.IsIdentity);

			foreach(SymmetryElement element in SymmetryGroup.Elements)
			{
				Assert.AreEqual(element, element * identity);
				Assert.AreEqual(element, identity * element);
			}			
		}

		[Test]
		public void IndexTest()
		{
			for (int i = 0; i < SymmetryGroup.Elements.Length; i++)
			{
				Assert.AreEqual(i, SymmetryGroup.Elements[i].Index);
			}
		}

	}
}
