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
            var stopwatch = new Stopwatch();
            var accum = 0;
            
            stopwatch.Start();
            for (int i = 0; i < 1_000_000; i++)
            {
                var diagnoses = new List<PatientCase.Types.Diagnosis>();
                for (int j = 0; j < 10; j++)
                {
                    var diag = new PatientCase.Types.Diagnosis();
                    diag.Code = "DIAG" + j;
                    
                    diagnoses.Add(diag);
                }
                
                var procedures = new List<PatientCase.Types.Procedure>();
                for (int j = 0; j < 10; j++)
                {
                    var proc = new PatientCase.Types.Procedure();
                    proc.Code = "PROC" + j;
                    proc.Date = 1 << 32;
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

                var pcArray = pc.ToByteArray();

                unsafe
                {
                    fixed(byte* ptr = &pcArray[0])
                    {
                        uint length = 0;
                        byte* result = LibGrouper.Group(ptr, (uint) pcArray.Length, specHandle, out length);
                        
                        byte[] buf = new byte[length];
                        buf = result;
                        Marshal.Copy((IntPtr) result, buf, 0, (int) length);

                        // TODO [MemoryLeak]: Release native memory
                        // TODO [Perf] Find way to omit need for copy
                        
                        var parsed = Result.Parser.ParseFrom(buf);
                        accum += parsed.CalculateSize();
                    }
                }
            }
            stopwatch.Stop();
            
            Console.WriteLine("Took {0}ms, {1}", stopwatch.ElapsedMilliseconds, accum);
        }
    }
}