using System;
using System.Collections.Generic;
using System.IO;

namespace CubeAD
{
	//A set of all ZBLL algorithms
	public static class ZBLLAlgos
	{
		public static List<List<MoveSequenz>> Cases = new List<List<MoveSequenz>>();
		public static void FromFile()
		{
			StringReader sr = new StringReader(File.ReadAllText(Directory.GetCurrentDirectory() + @"\casesmap.txt"));

			int minLength = int.MaxValue;
			int maxLength = 0;

			int mode = 0;
			string line;
			List<MoveSequenz> best = new List<MoveSequenz>();
			int shortest = int.MaxValue;
			while ((line = sr.ReadLine()) != null)
			{
				switch (mode)
				{
					case 0:
						if (line.Contains('['))
						{
							mode = 1;
							shortest = int.MaxValue;
							best.Clear();
						}
						break;
					case 1:
						if (line.Contains(']'))
						{
							mode = 0;
							minLength = Math.Min(minLength, shortest);
							maxLength = Math.Max(maxLength, shortest);
							Cases.Add(best);
						}
						else
						{
							line = line.Replace("\"", "").Replace("'", "P").Replace(",", "").Trim();
							//Console.WriteLine(line);

							MoveSequenz ms = new MoveSequenz(line);

							if (ms.Length == shortest)
							{
								best.Add(ms);
							}

							if (ms.Length < shortest)
							{
								shortest = ms.Length;
								best.Clear();
								best.Add(ms);
							}
						}
						break;
				}
			}

			Console.WriteLine("Destinct cases: " + Cases.Count + " min: " + minLength + " max: " + maxLength);

		}
	}
}
