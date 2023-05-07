#![allow(dead_code)]

mod cube_representations;

use crate::cube_representations::piece_cube::{PieceCube, self};
use crate::cube_representations::tile_cube::TileCube;
use std::mem;
use std::time::{Instant, Duration};


fn main() {
    
    
    benchmark();
    print!("Kek: {} ", mem::size_of::<TileCube>());
    
}


fn benchmark() {
    let mut count : i64 = 1;

    let mut start = Instant::now();
    bench_count(count);
    let mut duration = start.elapsed();

    while(duration.as_secs() < 1){
        count *= 2;

        if count < 0 {
            println!("!!!!!!!!!!!!!!!Probably cut optimized!!!!!!!!!!");
            break;
        }

        start = Instant::now();
        bench_count(count);
        duration = start.elapsed();
        
        println!("Total count: {}", count);
    }

    println!("Total duration: {:?}", duration);
    println!("Total count: {}", count);

    let nanos = duration.as_nanos() / count as u128;
    if nanos > 1000 {
        println!("Average time: {:?}", Duration::from_nanos(nanos as u64));
    }
    else {
        println!("Average time: {:.3} ns", duration.as_nanos() as f64 / (count as f64));
    }
    
}

fn bench_count(count : i64) {
    let mut res = vec![false; 2 as usize];

    for i in 0..count {
        res[(i % 2) as usize] = PieceCube::new(i as i32).is_solved();    
    }

    print!("Anti opt kek: {:?} ", res[0]);
}