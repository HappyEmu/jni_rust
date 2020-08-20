using System;
using System.Runtime.InteropServices;

namespace cs_rust_ffi
{
    class Program
    {
        [DllImport("../../../../../native/grouper/target/release/libgrouper", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong load_specification(string url);
        [DllImport("../../../../../native/grouper/target/release/libgrouper", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe byte* group(string pc, ulong handle, out uint length);
        
        static unsafe void Main(string[] args)
        {
            ulong specHandle = load_specification("hello/from/csharp");

            uint length = 0;
            byte* result = group("thisisapc", specHandle, out length);

            Console.WriteLine(length);
        }
    }
}