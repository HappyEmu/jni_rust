#![allow(dead_code)]
#![allow(unused_variables)]

mod protos;

use lazy_static::lazy_static;

use std::collections::HashMap;
use std::slice::from_raw_parts;
use std::sync::Mutex;
use std::time::Duration;
use std::os::raw::c_char;
use std::ffi::CStr;

use jni::objects::{JClass, JString};
use jni::sys::{jbyteArray, jlong};
use jni::JNIEnv;

use protobuf::Message;
use protos::pc::PatientCase;

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
pub extern "C" fn group(pc_ptr: *const u8, pc_len: u32, spec_handle: SpecHandle, res_length: &mut i32) -> *const u8 {
    let pc = unsafe { from_raw_parts(pc_ptr, pc_len as usize) };
    let pc = protobuf::parse_from_bytes::<protos::pc::PatientCase>(pc);
    // println!("PC = {:?}", pc);

    let now = std::time::Instant::now();

    // Spend some time to simulate grouping
    loop {
        if now.elapsed().as_micros() >= 20 {
            break;
        }
    }

    let result = protos::pc::Result {
        drg: "960Z".to_string(),
        mdc: "05".to_string(),
        pccl: 3,
        gst: 4,
        unknown_fields: Default::default(),
        cached_size: Default::default()
    };
    let mut result = result.write_to_bytes()
        .unwrap()
        .into_boxed_slice();

    let data = result.as_ptr();
    let len = result.len();
    std::mem::forget(result);

    //println!("{}", spec_handle);
    //println!("{:?}", res_length);
    *res_length = len as i32;
    data
}

#[no_mangle]
pub extern "C" fn free_byte_array(ptr: *const u8, len: i32) {
    let owned = unsafe { from_raw_parts(ptr, len as usize) }.to_owned();
    std::mem::drop(owned);
}

#[allow(non_snake_case)]
#[no_mangle]
pub extern "system" fn Java_LibGrouper_loadSpecification(
    env: JNIEnv,
    class: JClass,
    url: JString,
) -> jlong {
    let url = env.get_string(url);
    let url = String::from(url.unwrap());

    println!("Loading spec from {}", url);

    let mut specs = SPECIFICATIONS.lock().unwrap();
    let handle = (specs.len() + 1) as i64;
    specs.insert(handle, Spec::new("test"));

    handle
}

#[allow(non_snake_case)]
#[no_mangle]
pub extern "system" fn Java_LibGrouper_group(
    env: JNIEnv,
    class: JClass,
    pc: jbyteArray,
    handle: jlong,
) -> jbyteArray {
    let pc = {
        let (ptr, _copy) = env.get_byte_array_elements(pc).unwrap();
        let ptr = ptr as *mut u8;
        let size = env.get_array_length(pc).unwrap() as usize;

        unsafe { from_raw_parts(ptr, size) }
    };

    // println!("Got PC = {:02X?}, handle = {}", pc, handle);

    let now = std::time::Instant::now();
    let parsed = protobuf::parse_from_bytes::<protos::pc::PatientCase>(pc);
    // println!("Parsed {:?} in {:?}", parsed, now.elapsed());

    let now = std::time::Instant::now();

    // Spend some time to simulate grouping
    loop {
        if now.elapsed().as_micros() >= 20 {
            break;
        }
    }
    // std::thread::sleep(Duration::from_micros(20));

    let result = protos::pc::Result {
        drg: "960Z".to_string(),
        mdc: "05".to_string(),
        pccl: 3,
        gst: 4,
        unknown_fields: Default::default(),
        cached_size: Default::default()
    };
    let result = result.write_to_bytes().unwrap();

    // Create Java owned array from slice
    let out = env.byte_array_from_slice(&result).unwrap();
    out
}

#[cfg(test)]
mod tests {
    #[test]
    fn it_works() {
        assert_eq!(2 + 2, 4);
    }
}
