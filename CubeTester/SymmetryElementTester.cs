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
		public void Identity()
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
		public void Index()
		{
			for (int i = 0; i < SymmetryGroup.Elements.Length; i++)
			{
				Assert.AreEqual(i, SymmetryGroup.Elements[i].Index);
			}
		}

		[Test]
		public void HasReflection()
		{
			//first 16 generators are rotations
			for (int i = 0; i < 16; i++)
			{
				Assert.IsFalse(SymmetryGroup.Generators[i].HasReflection);
			}
			for (int i = 16; i < SymmetryGroup.Generators.Length; i++)
			{
				Assert.IsTrue(SymmetryGroup.Generators[i].HasReflection);
			}
		}
	}
}
