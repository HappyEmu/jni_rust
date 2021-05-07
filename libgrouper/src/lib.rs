#![allow(dead_code)]
#![allow(unused_variables)]

mod protos;

use lazy_static::lazy_static;

use std::collections::HashMap;
use std::sync::Mutex;
use std::time::Duration;
use std::os::raw::c_char;
use std::ffi::CStr;

use jni::objects::{JClass, JString};
use jni::sys::{jbyteArray, jlong, jint};
use jni::JNIEnv;

use protobuf::{Message, SingularPtrField};

type SpecHandle = i64;

lazy_static! {
    static ref SPECIFICATIONS: Mutex<HashMap<SpecHandle, Spec>> = Mutex::new(HashMap::new());
}

#[derive(Default)]
struct Spec {
    url: String,
    some_data: HashMap<String, Vec<String>>,
}

impl Spec {
    fn new(url: &str) -> Self {
        Self {
            url: url.into(),
            some_data: HashMap::new(),
        }
    }
}

#[no_mangle]
pub extern "C" fn load_specification(url: *const c_char) -> SpecHandle {
    let c_url = unsafe { CStr::from_ptr(url) }.to_str().unwrap();
    println!("Loading spec from {}", c_url);

    let mut specs = SPECIFICATIONS.lock().unwrap();
    let handle = (specs.len() + 1) as SpecHandle;
    specs.insert(handle, Spec::new("test"));

    handle
}

#[no_mangle]
pub extern "C" fn free_specification(handle: SpecHandle) {
    let mut specs = SPECIFICATIONS.lock().unwrap();
    let _ = specs.remove(&handle);
}

#[no_mangle]
pub extern "C" fn group(pc_ptr: *const u8, pc_len: usize, spec_handle: SpecHandle, res_length: *mut usize) -> *mut u8 {
    let pc = unsafe { std::slice::from_raw_parts(pc_ptr, pc_len) };
    let pc = protos::pc::PatientCase::parse_from_bytes(pc).unwrap();
    // println!("PC = {:?}", pc);

    let now = std::time::Instant::now();

    // Spend some time to simulate grouping
    loop {
        if now.elapsed().as_micros() >= 20 {
            break;
        }
        std::thread::yield_now();
    }

    let result = protos::pc::GroupResult::create("960Z", "05", 3, 4);

    let mut response = protos::pc::GroupResponse::create(result, pc)
        .write_to_bytes()
        .unwrap()
        .into_boxed_slice();

    let (data, len) = (response.as_mut_ptr(), response.len());
    std::mem::forget(response);

    //println!("{}", spec_handle);
    //println!("{:?}", res_length);
    unsafe { *res_length = len; };
    data
}

#[no_mangle]
pub extern "C" fn free_byte_array(ptr: *mut u8, len: usize) {
    let owned: Box<[u8]> = unsafe { Box::from_raw(std::slice::from_raw_parts_mut(ptr, len)) };
    std::mem::drop(owned);
}

#[allow(non_snake_case)]
#[no_mangle]
pub extern "system" fn Java_LibGrouper_loadSpecificationFromPath(
    env: JNIEnv,
    _class: JClass,
    url: JString,
    _tariff: jint
) -> jlong {
    let url = env.get_string(url);
    let url = String::from(url.unwrap());

    println!("Loading spec from path: {}", url);

    let mut specs = SPECIFICATIONS.lock().unwrap();
    let handle = (specs.len() + 1) as i64;
    specs.insert(handle, Spec::new("test-from-path"));

    handle
}

#[allow(non_snake_case)]
#[no_mangle]
pub extern "system" fn Java_LibGrouper_loadSpecificationFromBytes(
    env: JNIEnv,
    _class: JClass,
    bytes: jbyteArray,
    _tariff: jint
) -> jlong {
    let bytes = env.convert_byte_array(bytes).unwrap();

    println!("Loading spec from bytes: {} bytes", bytes.len());

    let mut specs = SPECIFICATIONS.lock().unwrap();
    let handle = (specs.len() + 1) as i64;
    specs.insert(handle, Spec::new("test-from-bytes"));

    handle
}

#[allow(non_snake_case)]
#[no_mangle]
pub extern "system" fn Java_LibGrouper_group(
    env: JNIEnv,
    class: JClass,
    handle: jlong,
    pc: jbyteArray,
) -> jbyteArray {
    let pc = env.convert_byte_array(pc).unwrap();

    // println!("Got PC = {:02X?}, handle = {}", pc, handle);

    let now = std::time::Instant::now();
    let parsed = protos::pc::PatientCase::parse_from_bytes(&pc).unwrap();
    // println!("Parsed {:?} in {:?}", parsed, now.elapsed());

    let now = std::time::Instant::now();

    // Spend some time to simulate grouping
    loop {
        if now.elapsed().as_micros() >= 20 {
            break;
        }
    }

    let result = protos::pc::GroupResult::create("960Z", "05", 3, 4);

    let response = protos::pc::GroupResponse::create(result, parsed)
        .write_to_bytes()
        .unwrap();

    // Create Java owned array from slice
    let out = env.byte_array_from_slice(&response).unwrap();
    out
}

#[allow(non_snake_case)]
#[no_mangle]
pub extern "system" fn Java_LibGrouper_groupSupplements(
    env: JNIEnv,
    class: JClass,
    handle: jlong,
    pc: jbyteArray,
) -> jbyteArray {
    let pc = env.convert_byte_array(pc).unwrap();

    let now = std::time::Instant::now();
    let parsed = protos::pc::PatientCase::parse_from_bytes(&pc).unwrap();

    let now = std::time::Instant::now();

    // Spend some time to simulate supplement grouping
    loop {
        if now.elapsed().as_micros() >= 3 {
            break;
        }
    }

    let result = protos::pc::GroupResult::create("960Z", "05", 3, 4);

    let response = protos::pc::GroupResponse::create(result, parsed)
        .write_to_bytes()
        .unwrap();

    // Create Java owned array from slice
    let out = env.byte_array_from_slice(&response).unwrap();
    out
}

impl protos::pc::GroupResult {
    fn create(drg: &str, mdc: &str, pccl: u32, gst: u32) -> Self {
        Self {
            drg: drg.to_owned(),
            mdc: mdc.to_owned(),
            pccl,
            gst,
            ..Default::default()
        }
    }
}

impl protos::pc::GroupResponse {
    fn create(
        result: protos::pc::GroupResult,
        pc: protos::pc::PatientCase
    ) -> Self {
        Self {
            result: SingularPtrField::some(result),
            pc: SingularPtrField::some(pc),
            ..Default::default()
        }
    }
}

#[cfg(test)]
mod tests {
    #[test]
    fn it_works() {
        assert_eq!(2 + 2, 4);
    }
}
