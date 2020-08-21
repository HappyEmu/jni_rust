using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Google.Protobuf;

namespace csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var specHandle = LibGrouper.LoadSpecification("hello/from/csharp");
            var total = new Stopwatch();
            var assembling = new Stopwatch();
            var serializing = new Stopwatch();
            var grouping = new Stopwatch();
            var parsing = new Stopwatch();
            
            var accum = 0;
            
            total.Start();
            for (int i = 0; i < 1_000_000; i++)
            {
                assembling.Start();
                var diagnoses = new List<PatientCase.Types.Diagnosis>(16);
                for (int j = 0; j < 10; j++)
                {
                    var diag = new PatientCase.Types.Diagnosis();
                    diag.Code = "DIAG" + j;
                    
                    diagnoses.Add(diag);
                }
                
                var procedures = new List<PatientCase.Types.Procedure>(16);
                for (int j = 0; j < 10; j++)
                {
                    var proc = new PatientCase.Types.Procedure();
                    proc.Code = "PROC" + j;
                    proc.Date = 1 << 20;
                    proc.Side = PatientCase.Types.Procedure.Types.Side.B;

                    procedures.Add(proc);
                }

                var pc = new PatientCase();
                pc.Id = "1337";
                pc.AgeYears = 42;
                pc.AdmDate = (1989 << 20 | 5 << 16 | 23 << 11);
                pc.SepDate = (2019 << 20 | 6 << 16 | 27 << 11);
                pc.BirthDate = (1989 << 20 | 6 << 16 | 29 << 11);
                pc.Diagnoses.AddRange(diagnoses);
                pc.Procedures.AddRange(procedures);
                
                assembling.Stop();

                var result = LibGrouper.Group(pc, specHandle, grouping, parsing, serializing);
                accum += result.CalculateSize();
            }
            total.Stop();
            
            Console.WriteLine("Took {0}ms, {1}, Assembling: {2}ms, Serializing: {5}ms, Grouping: {3}ms, Parsing: {4}ms",
                total.Elapsed.TotalMilliseconds,
                accum,
                assembling.Elapsed.TotalMilliseconds,
                grouping.Elapsed.TotalMilliseconds,
                parsing.Elapsed.TotalMilliseconds,
                serializing.Elapsed.TotalMilliseconds
            );
        }
    }
}