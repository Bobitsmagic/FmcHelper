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

TileCube tc = TileCube.GetSolved();

for(long i = 0; i < 67108864; i++)
{
	tc.MakeMove((CubeMove)((i * 1337) % 18));
}

Console.WriteLine(tc.SideView());

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
