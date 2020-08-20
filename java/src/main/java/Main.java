import com.google.protobuf.InvalidProtocolBufferException;

import java.util.stream.Collectors;
import java.util.stream.IntStream;

public class Main {
    public static void main(String[] args) throws InvalidProtocolBufferException {
        var specHandle = LibGrouper.loadSpecification("url/to/load/specification");

        var start = System.currentTimeMillis();

        for (int i = 0; i < 1_000_000; i++) {
            var diagnoses = IntStream.range(0, 10).mapToObj(n ->
                Pc.PatientCase.Diagnosis.newBuilder()
                    .setCode("DIAG" + n)
                    .build()
            ).collect(Collectors.toList());

            var procedures = IntStream.range(0, 10).mapToObj(n ->
                Pc.PatientCase.Procedure.newBuilder()
                    .setCode("PROC" + n)
                    .setDate(1 << 32)
                    .setSide(Pc.PatientCase.Procedure.Side.B)
                    .build()
            ).collect(Collectors.toList());

            var pc = Pc.PatientCase.newBuilder()
                .setId("1337")
                .setAgeYears(42)
                .setAdmDate(1989 << 20 | 5 << 16 | 23 << 11)
                .setSepDate(2019 << 20 | 6 << 16 | 27 << 11)
                .setBirthDate(1989 << 20 | 6 << 16 | 29 << 11)
                .addAllDiagnoses(diagnoses)
                .addAllProcedures(procedures)
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
