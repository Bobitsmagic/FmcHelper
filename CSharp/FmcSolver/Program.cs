using CubeAD;
using CubeAD.CubeIndexSets;
using CubeAD.CubeRepresentation;
using CubeAD.IndexCubeSets;
using Microsoft.CodeAnalysis;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.ExceptionServices;
using System.Security.Principal;

const int COUNT = 396_647_119;
Console.WriteLine("required size: " + ((long)COUNT * IndexCube.SIZE_IN_BYTES).ToString("000 000 000 000"));


int fac = 1;
while (fac * fac < COUNT)
	fac++;

Console.WriteLine(int.MaxValue);
Console.WriteLine(Array.MaxLength);

Stopwatch sw = Stopwatch.StartNew();
List<int> primes = GetAllPrimes(Array.MaxLength - 1);

Console.WriteLine("Sol: " + FindBiggestWholeFractionLessThanMax(COUNT, IndexCube.PADDED_SIZE_IN_BYTES));
Console.WriteLine(sw.ElapsedMilliseconds);
//IndexCube[] array = new IndexCube[396_647_119];


//Stopwatch sw = Stopwatch.StartNew();
//for (int d = 9; d < 10; d++)
//{
//    Console.WriteLine("\nWorking: " + d);

//    sw.Restart();
//    var set = PieceCube.GetUniqueSymCubes(d);

//    Console.WriteLine("Count : " + set.Count.ToString("0 000 000 000"));
//    Console.WriteLine("Time: " + sw.ElapsedMilliseconds.ToString("000 000"));

//    sw.Restart();

//    Console.WriteLine("Getting array");
//    var array = set.GetArray();
//    Console.WriteLine("Time: " + sw.ElapsedMilliseconds.ToString("000 000"));

//    set = null;
//    GC.Collect();
//    Console.WriteLine("Creating sealed hashset");
//    sw.Restart();
//    SealedHashset sh = new SealedHashset(array);
//    Console.WriteLine("Time: " + sw.ElapsedMilliseconds.ToString("000 000"));

//    sw.Restart();
//    sh.SaveToFile("sym_" + d + "_data.bin", "sym_" + d + "_index.bin");
//    Console.WriteLine("Time: " + sw.ElapsedMilliseconds.ToString("000 000"));

//}

Console.WriteLine("Done");
Console.ReadLine();
//Console.WriteLine(string.Join("kkekekekkkkkkkkkkkkkkkkkkkkkkkkkkkk\n", openlist.Select(x => x.GetSideView())));

//1
//2
//9
//75
//934
//12 077
//159 131
//2 101 575
//27 762 103

long FindBiggestWholeFractionLessThanMax(long count, int size)
{
	if (count * size <= int.MaxValue)
	{
		return count;
	}

	foreach(int p in primes)
	{
		if(count % p == 0)
		{
            Console.WriteLine(p);
			if (count / p * size <= int.MaxValue)
			{
				return count / p;
			}
        }
	}

	return -1;
}

List<int> GetAllPrimes(int max)
{
	bool[] primes = new bool[max + 1];
	Array.Fill(primes, true);

	List<int> ret = new List<int>();

	for (long i = 2; i * i < max + 1; i++)
	{
		if (primes[i - 1])
		{
			for (long j = i * i; j <= max; j += i)
			{
				primes[j - 1] = false;
			}
		}
	}


	

	Console.WriteLine($"There are {ret.Count} primes up to {max}");

	return ret;
}
