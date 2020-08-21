using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Google.Protobuf;

namespace csharp
{
    public class LibGrouper
    {
        [DllImport(
            "/Users/gerberur/work/playground/rust_ffi/libgrouper/target/release/libgrouper.dylib",
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "load_specification"
        )]
        public static extern long LoadSpecification(string url);
        
        [DllImport(
            "/Users/gerberur/work/playground/rust_ffi/libgrouper/target/release/libgrouper.dylib",
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "free_specification"
        )]
        public static extern long FreeSpecification(long handle);

        [DllImport(
            "/Users/gerberur/work/playground/rust_ffi/libgrouper/target/release/libgrouper.dylib",
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "group"
        )]
        private static extern IntPtr NativeGroup(IntPtr pcPtr, uint pcLen, long handle, out uint length);
        
        [DllImport(
            "/Users/gerberur/work/playground/rust_ffi/libgrouper/target/release/libgrouper.dylib",
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "free_byte_array"
        )]
        public static extern long FreeByteArray(IntPtr ptr, int len);

        public static Result Group(PatientCase pc, long specHandle, Stopwatch grouping, Stopwatch parsing, Stopwatch serializing)
        {
            serializing.Start();
            var bytes = pc.ToByteArray();
            serializing.Stop();
            
            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    // Group
                    grouping.Start();
                    uint resultLength;
                    IntPtr resultPtr = NativeGroup((IntPtr) ptr, (uint) bytes.Length, specHandle, out resultLength);
                    grouping.Stop();
                    
                    parsing.Start();
                    // Copy from native to managed memory
                    byte[] buffer = new byte[resultLength];
                    Marshal.Copy(resultPtr, buffer, 0, (int) resultLength);

                    // Free native memory
                    LibGrouper.FreeByteArray(resultPtr, (int) resultLength);
                    // TODO [Perf] Find way to omit need for copy
                        
                    // Parse
                    var parsed = Result.Parser.ParseFrom(buffer);
                    parsing.Stop();
                    return parsed;
                }
            }
        }
    }
}