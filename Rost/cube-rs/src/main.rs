#![allow(dead_code)]

mod cube_representations;
mod move_sequence;
mod move_blocker;
mod pieces;

use rand::prelude::*;
use rand_chacha::ChaCha20Rng;

use std::hint::black_box;
use std::{mem, backtrace};
use std::time::{Instant, Duration};
use cube_representations::piece_cube::PieceCube;
use cube_representations::tile_cube::TileCube;
use cube_representations::tile_cube_table::Table;
use move_sequence::MoveSequence;
use pieces::corner;

use crate::move_blocker::MoveBlocker;
use crate::move_sequence::print_move;

fn main() {
    
    let table: Table = Table::init();
    //benchmark();

    let mut cube = TileCube::get_solved();
    let mut rng = SeedableRng::seed_from_u64(1337);

    let ms = MoveSequence::get_random(11, &mut rng);
    ms.print();

    cube.apply_move_sequence(&ms, &table);

    let mut start = Instant::now();
    dfs(cube, &table);
    let dur = start.elapsed();

    println!("Time: {:?}", dur);

    //let ms = MoveSequence::get_random(1000);
    //ms.print();
}

 fn dfs(cube: TileCube, table: &Table) {
    for max_depth in 0..12 {
        fn backtrack(cube: &TileCube, mb: MoveBlocker, depth: i32, max_depth: i32, table: &Table) -> bool {
            if depth == max_depth {
                return cube.is_solved();
            }

            for m in 0..18 {
                if mb.move_blocked(m) {
                    continue;
                }

                if backtrack(&TileCube::new_move(&cube, m, table), MoveBlocker::new_move(mb, m), depth + 1, max_depth, table) {
                    print_move(m);
                    return  true;
                }
            }

            return false;
        }

        if backtrack(&cube, MoveBlocker::new(),  0, max_depth, table) {
            println!();
            return;
        }
    }

    println!("No sol :(")
 }

fn benchmark() {
    let mut count : i64 = 1;
    let table: Table = Table::init(); 
    let ms: MoveSequence = MoveSequence::get_random(1000, &mut StdRng::seed_from_u64(1337));

    let mut start = Instant::now();
    bench_count(count,  &table, &ms);
    let mut duration = start.elapsed();

    while duration.as_secs() < 1 {
        count *= 2;

        if count < 0 {
            println!("!!!!!!!!!!!!!!!Probably cut optimized!!!!!!!!!!");
            break;
        }

        start = Instant::now();
        bench_count(count, &table, &ms);
        duration = start.elapsed();
        
        println!("Total count: {}", count);
    }

    println!("Total duration: {:?}", duration);
    println!("Final count: {}", count);

    let nanos = duration.as_nanos() / count as u128;
    if nanos > 1000 {
        println!("Average time: {:?}", Duration::from_nanos(nanos as u64));
    }
    else {
        println!("Average time: {:.3} ns", duration.as_nanos() as f64 / (count as f64));
    }
}

fn bench_count(count : i64, table: &Table,  sequence: &MoveSequence) {
    
    let mut cube = TileCube::get_solved();

    for _ in 0..count {
        cube.apply_move_sequence(sequence, table);
    }

    black_box(cube);
}