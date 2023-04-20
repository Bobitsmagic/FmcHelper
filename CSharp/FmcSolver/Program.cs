using CubeAD;
using CubeAD.CubeRepresentation;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;


Stopwatch sw = Stopwatch.StartNew();
for(int i = 0; i < 10; i++)
{
    Console.WriteLine("MaxDepth: " + i);
    Console.WriteLine("Count: " + TileCube.GetUniqueSymCubes(i).Count.ToString("000 000 000 000"));
    Console.WriteLine("Time: " + sw.ElapsedMilliseconds.ToString("000 000"));
}

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