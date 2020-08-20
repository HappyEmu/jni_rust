public class LibGrouper {
    static {
        System.loadLibrary("grouper");
    }

    public static native long loadSpecification(String url);
    public static native byte[] group(byte[] pc, long specHandle);
}
