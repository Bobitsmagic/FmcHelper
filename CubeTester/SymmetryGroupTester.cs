using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using CubeAD;
using System.Linq;

namespace CubeTester
{
	class SymmetryGroupTester
	{
		[Test]
		public void GeneratorTest()
		{

			List<SymmetryElement>[] subGroups = new List<SymmetryElement>[SymmetryGroup.ColorMap.Length];
			for(int i = 0; i < subGroups.Length; i++)
			{
				subGroups[i] = SymmetryGroup.ColorMap[i].GenerateMultGroup();
			}

			List<SymmetryElement> genGroup = new List<SymmetryElement>();

			int count = 0;
			//skipping identity
			for (int i = 1; i < SymmetryElement.AllSymmetryElements.Length; i++)
			{
				SymmetryElement element = SymmetryElement.AllSymmetryElements[i];

				element.GenerateMultGroup(genGroup);
				Console.WriteLine(genGroup.Count);

				int hits = 0;
				foreach(List<SymmetryElement> group in subGroups)
				{
					if (group.Count != genGroup.Count) continue;

					if (group.SequenceEqual(genGroup)) hits++;
				}

				if(hits != 1)
				{
					System.Diagnostics.Debug.WriteLine(count++ + " Kek " + genGroup.Count);
					System.Diagnostics.Debug.WriteLine(string.Join("\n", genGroup));
				}

				Assert.AreEqual(1, hits);
			}
		}

		[Test]
		public void Log2Test()
		{
			int targetVal = 0;
			for(uint i = 1; i < 1000000; i++)
			{
				if (i == (1 << (targetVal + 1)))
					targetVal++;

				Assert.AreEqual(targetVal, SymmetryGroup.Log2(i));
			}
		}
	}
}
