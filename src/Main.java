import com.google.protobuf.InvalidProtocolBufferException;

public class Main {
    public static void main(String[] args) throws InvalidProtocolBufferException {
        var specHandle = LibGrouper.loadSpecification("url/to/load/specification");

        var start = System.currentTimeMillis();

        for (int i = 0; i < 1_000_000; i++) {
            var pc = Pc.PatientCase.newBuilder()
                .setId("1337")
                .setAgeYears(42)
                .setAdmDate(1989 << 20 | 5 << 16 | 23 << 11)
                .setSepDate(2019 << 20 | 6 << 16 | 27 << 11)
                .setBirthDate(1989 << 20 | 6 << 16 | 29 << 11)
                .build()
                .toByteArray();

            // System.out.println("PC size is " + pc.length + " bytes");

            var result = LibGrouper.group(pc, specHandle);
            var parsed = Pc.Result.parseFrom(result);

            // System.out.println(parsed);
        }

        var elapsed = System.currentTimeMillis() - start;
        System.out.println(elapsed);
    }
}
