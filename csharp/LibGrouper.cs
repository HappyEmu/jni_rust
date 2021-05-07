using System;
using System.Buffers;
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
        private static extern IntPtr NativeGroup(UIntPtr pcPtr, UIntPtr pcLen, long handle, out UIntPtr length);
        
        [DllImport(
            "/Users/gerberur/work/playground/rust_ffi/libgrouper/target/release/libgrouper.dylib",
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "free_byte_array"
        )]
        public static extern long FreeByteArray(IntPtr ptr, UIntPtr len);

        public static GroupResponse Group(PatientCase pc, long specHandle, Stopwatch grouping, Stopwatch parsing, Stopwatch serializing)
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
                    IntPtr resultPtr = NativeGroup((UIntPtr) ptr, (UIntPtr) bytes.Length, specHandle, out var resultLength);
                    grouping.Stop();
                    
                    parsing.Start();
                    int checkedLen = (int) resultLength.ToUInt32();
                    // Copy from native to managed memory
                    byte[] buffer = new byte[checkedLen];
                    Marshal.Copy(resultPtr, buffer, 0, checkedLen);

                    // Free native memory
                    LibGrouper.FreeByteArray(resultPtr, resultLength);
                    // TODO [Perf] Find way to omit need for copy
                        
                    // Parse
                    var parsed = GroupResponse.Parser.ParseFrom(buffer);
                    parsing.Stop();
                    return parsed;
                }
            }
        }
    }
}