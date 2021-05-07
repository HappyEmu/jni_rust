fn main() {
    protoc_rust::Codegen::new()
        .out_dir("src/protos")
        .include("../protos")
        .input("../protos/pc.proto")
        .run()
        .expect("protoc")
}