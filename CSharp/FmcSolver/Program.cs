using CubeAD;
using CubeAD.CubeRepresentation;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;


TileCube tc = TileCube.GetSolved();
IndexCube id = new IndexCube();

MoveSequenz ms = MoveSequenz.InverseCubeScramble;

tc.ApplySequence(ms.Moves);

Console.WriteLine(tc.SideView());

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