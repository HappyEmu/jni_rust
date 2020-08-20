using System.Runtime.InteropServices;

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
            EntryPoint = "group"
        )]
        public static extern unsafe byte* Group(byte* pcPtr, uint pcLen, long handle, out uint length);
    }
}