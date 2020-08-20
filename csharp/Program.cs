using System;
using Google.Protobuf;

namespace csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var specHandle = LibGrouper.LoadSpecification("hello/from/csharp");
            
            var pc = new PatientCase();
            pc.Id = "1337";
            pc.AgeYears = 42;
            pc.AdmDate = (1989 << 20 | 5 << 16 | 23 << 11);
            pc.SepDate = (2019 << 20 | 6 << 16 | 27 << 11);
            pc.BirthDate = (1989 << 20 | 6 << 16 | 29 << 11);

            var pcArray = pc.ToByteArray();

            unsafe
            {
                fixed(byte* ptr = &pcArray[0])
                {
                    uint length = 0;
                    byte* result = LibGrouper.Group(ptr, (uint) pcArray.Length, specHandle, out length);

                    Console.WriteLine(length);
                }
            }
        }
    }
}