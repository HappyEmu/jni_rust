import com.google.common.base.Stopwatch;
import com.google.protobuf.InvalidProtocolBufferException;

import java.util.concurrent.TimeUnit;
import java.util.stream.Collectors;
import java.util.stream.IntStream;

public class Main {
    public static void main(String[] args) throws InvalidProtocolBufferException {
        var specHandle = LibGrouper.loadSpecification("url/to/load/specification");

        var total = Stopwatch.createUnstarted();
        var assembling = Stopwatch.createUnstarted();
        var serializing = Stopwatch.createUnstarted();
        var grouping = Stopwatch.createUnstarted();
        var parsing = Stopwatch.createUnstarted();

        total.start();

        var accum = 0;
        for (int i = 0; i < 1_000_000; i++) {
            assembling.start();
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
                .build();
            assembling.stop();

            serializing.start();
            var serialized = pc.toByteArray();
            serializing.stop();

            // System.out.println("PC size is " + pc.length + " bytes");

            grouping.start();
            var result = LibGrouper.group(serialized, specHandle);
            grouping.stop();

            parsing.start();
            var parsed = Pc.Result.parseFrom(result);
            parsing.stop();

            accum += parsed.getSerializedSize();
            // System.out.println(parsed);
        }

        total.stop();

        System.out.println("Total: " + total.elapsed(TimeUnit.MICROSECONDS) / 1000.0);
        System.out.println("Assembling: " + assembling.elapsed(TimeUnit.MICROSECONDS) / 1000.0);
        System.out.println("Serializing: " + serializing.elapsed(TimeUnit.MICROSECONDS) / 1000.0);
        System.out.println("Grouping: " + grouping.elapsed(TimeUnit.MICROSECONDS) / 1000.0);
        System.out.println("Parsing: " + parsing.elapsed(TimeUnit.MICROSECONDS) / 1000.0);
    }
}
