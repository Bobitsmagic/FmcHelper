using CubeAD;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeTester
{
	class SymmetryElementTester
	{
		[Test]
		public void Identity()
		{
			SymmetryElement identity = SymmetryElement.Elements[0];

			Assert.IsTrue(identity.IsIdentity);

			foreach (SymmetryElement element in SymmetryElement.Elements)
			{
				Assert.AreEqual(element, element * identity);
				Assert.AreEqual(element, identity * element);
			}
		}

		[Test]
		public void Index()
		{
			for (int i = 0; i < SymmetryElement.Elements.Length; i++)
			{
				Assert.AreEqual(i, SymmetryElement.Elements[i].Index);
			}
		}

		[Test]
		public void HasReflection()
		{
			//first 16 generators are rotations
			for (int i = 0; i < 16; i++)
			{
				Assert.IsFalse(SymmetryElement.Generators[i].HasReflection);
			}
			for (int i = 16; i < SymmetryElement.Generators.Length; i++)
			{
				Assert.IsTrue(SymmetryElement.Generators[i].HasReflection);
			}
		}

		//Tests whether the Generator list is complete and minimal
		[Test]
		public void GeneratorTest()
		{
			List<SymmetryElement>[] subGroups = new List<SymmetryElement>[SymmetryElement.Generators.Length];
			for (int i = 0; i < subGroups.Length; i++)
			{
				subGroups[i] = SymmetryElement.Generators[i].GenerateMultGroup();
			}

			List<SymmetryElement> genGroup = new List<SymmetryElement>();

			//skipping identity
			for (int i = 1; i < SymmetryElement.Elements.Length; i++)
			{
				SymmetryElement element = SymmetryElement.Elements[i];

				element.GenerateMultGroup(genGroup);

				int hits = 0;
				foreach (List<SymmetryElement> group in subGroups)
				{
					if (group.Count != genGroup.Count) continue;

					if (group.SequenceEqual(genGroup)) hits++;
				}

				Assert.AreEqual(1, hits);
			}
		}
	}
}
