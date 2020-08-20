using System;
using System.Runtime.InteropServices;
using Google.Protobuf;

namespace cs_rust_ffi
{
    class Program
    {
        [DllImport("../../../../../native/grouper/target/release/libgrouper", CallingConvention = CallingConvention.Cdecl)]
        public static extern long load_specification(string url);
        [DllImport("../../../../../native/grouper/target/release/libgrouper", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe byte* group(byte* pcPtr, uint pcLen, long handle, out uint length);
        
        static void Main(string[] args)
        {
            long specHandle = load_specification("hello/from/csharp");
            
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
                    byte* result = group(ptr, (uint) pcArray.Length, specHandle, out length);

                    Console.WriteLine(length);
                }
            }
        }
    }
}