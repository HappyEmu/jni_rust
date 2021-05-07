public class LibGrouper {
    static {
        System.loadLibrary("grouper");
    }

    // Specification
    public static native long loadSpecificationFromPath(String path, int tariff);
    public static native long loadSpecificationFromBytes(byte[] bytes, int tariff);
    public static native void freeSpecification(long handle);

    // Grouper + SupplementGrouper
    public static native byte[] group(long specificationHandle, byte[] pc);
    public static native byte[] groupSupplements(long specificationHandle, byte[] pc);

    // Parsers
    public static native long createParser(int type, int tariff);
    public static native void freeParser(long handle);
    public static native byte[] parse(long handle, String pc);
    public static native byte[] getParserWarnings(long handle);

    // Cost weights
    public static native long loadSwissDrgCatalogFromPath(String path);
    public static native long loadSwissDrgCatalogFromBytes(byte[] bytes);


    public static native long loadTarpsyCatalogFromPath(String path);
    public static native long loadTarpsyCatalogFromBytes(byte[] bytes);

    public static native long loadStRehaCatalogFromPath(String path);
    public static native long loadStRehaCatalogFromBytes(byte[] bytes);
}
