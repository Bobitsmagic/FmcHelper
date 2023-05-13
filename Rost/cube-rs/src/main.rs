#![allow(dead_code)]

mod cube_representations;
mod move_sequence;
mod move_blocker;
mod pieces;
mod cube_move;
mod permuation;
mod cube_collections;

use cube_collections::sorted_set::SortedSet;
use cube_representations::index_cube::IndexCube;
use cube_representations::piece_cube_table::PieceCubeTable;
use rand::prelude::*;

use std::collections::HashSet;
use std::hint::black_box;
use std::thread;
use std::time::{Instant, Duration};
use cube_representations::piece_cube::PieceCube;
use cube_representations::tile_cube::TileCube;
use cube_representations::tile_cube_table::TileCubeTable;
use move_sequence::MoveSequence;


use crate::cube_collections::sealed_set::SealedSet;
use crate::move_blocker::MoveBlocker;
use crate::move_sequence::print_move;

fn main() {
    let piece_table: PieceCubeTable = PieceCubeTable::init();
    
    let mut start = Instant::now();
    let set = create_sorted_set(7, &piece_table);
    let mut duration = start.elapsed();
    println!("Count: {}", set.len());
    println!("Time: {:?}", duration);

    start = Instant::now();
    let ss = SealedSet::new(&set.data);
    duration = start.elapsed();
    println!("Time: {:?}", duration);
    
    let ms = MoveSequence::get_random(14, &mut StdRng::seed_from_u64(1337));
    ms.print();
    let mut cube = PieceCube::get_solved();
    cube.apply_move_sequence(&ms, &piece_table);
    
    start = Instant::now();
    
    look_up(cube, &piece_table, &ss);

    duration = start.elapsed();
    println!("Time: {:?}", duration);   
}

fn big_search() {
    let piece_table: PieceCubeTable = PieceCubeTable::init();
    
    let mut start = Instant::now();
    let set = create_sorted_set(7, &piece_table);
    let mut duration = start.elapsed();
    println!("Count: {}", set.len());
    println!("Time: {:?}", duration);

    start = Instant::now();
    let ss = SealedSet::new(&set.data);
    duration = start.elapsed();
    println!("Time: {:?}", duration);
    
    let ms = MoveSequence::get_random(15, &mut StdRng::seed_from_u64(1337));
    ms.print();
    let mut cube = PieceCube::get_solved();
    cube.apply_move_sequence(&ms, &piece_table);
    
    start = Instant::now();
    let mut scubes = vec![cube.clone(); 18];
    for i in 0..18 {
        scubes[i].make_move(i as u8, &piece_table)
    }

    thread::scope(|s| {
        for i in 0..9 {
            let c1 = scubes[i * 2 + 0].clone();
            let c2 = scubes[i * 2 + 1].clone();
            s.spawn(|| {
                look_up(c1, &piece_table, &ss);
                look_up(c2, &piece_table, &ss);

                print!("Finished thread ");
            });
        }        
    });
    
    duration = start.elapsed();
    println!("Time: {:?}", duration);   
}

fn dfs_tile(cube: TileCube, table: &TileCubeTable) {
    for max_depth in 0..12 {
        fn backtrack(cube: &TileCube, mb: MoveBlocker, depth: i32, max_depth: i32, table: &TileCubeTable) -> bool {
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

fn dfs_piece(cube: PieceCube, table: &PieceCubeTable) {
    for max_depth in 0..12 {
        fn backtrack(cube: &PieceCube, mb: MoveBlocker, depth: i32, max_depth: i32, table: &PieceCubeTable) -> bool {
            if depth == max_depth {
                return cube.is_solved();
            }

            for m in 0..18 {
                if mb.move_blocked(m) {
                    continue;
                }

                if backtrack(&PieceCube::new_move(&cube, m, table), MoveBlocker::new_move(mb, m), depth + 1, max_depth, table) {
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

fn create_hashset(max_depth: i32, table: &PieceCubeTable) -> HashSet<PieceCube> {
    let mut set = HashSet::new();

    fn backtrack(cube: &PieceCube, mb: MoveBlocker, depth: i32, set: &mut HashSet<PieceCube>, max_depth: i32, table: &PieceCubeTable) {
        set.insert(cube.clone());
        
        if depth == max_depth {
            return;
        }

        for m in 0..18 {
            if mb.move_blocked(m) {
                continue;
            }

            backtrack(&PieceCube::new_move(&cube, m, table), 
                MoveBlocker::new_move(mb, m),
                depth + 1, 
                set, 
                max_depth, 
                table);
        }
    }

    backtrack(&PieceCube::get_solved(), MoveBlocker::new(),  0, &mut set, max_depth, table);

    return set;
     
}

fn create_hashset_index_cube(max_depth: i32, table: &PieceCubeTable) -> HashSet<IndexCube> {
    let mut set = HashSet::new();

    fn backtrack(cube: &PieceCube, mb: MoveBlocker, depth: i32, set: &mut HashSet<IndexCube>, max_depth: i32, table: &PieceCubeTable) {
        set.insert(cube.get_index_cube());
        
        if depth == max_depth {
            return;
        }

        for m in 0..18 {
            if mb.move_blocked(m) {
                continue;
            }

            backtrack(&PieceCube::new_move(&cube, m, table), 
                MoveBlocker::new_move(mb, m),
                depth + 1, 
                set, 
                max_depth, 
                table);
        }
    }

    backtrack(&PieceCube::get_solved(), MoveBlocker::new(),  0, &mut set, max_depth, table);

    return set;
     
}

fn create_sorted_set(max_depth: i32, table: &PieceCubeTable) -> SortedSet {
    let mut set = SortedSet::new(0);

    fn backtrack(cube: &PieceCube, mb: MoveBlocker, depth: i32, set: &mut SortedSet, max_depth: i32, table: &PieceCubeTable) {
        set.insert(cube.get_index_cube());
        
        if depth == max_depth {
            return;
        }

        for m in 0..18 {
            if mb.move_blocked(m) {
                continue;
            }

            backtrack(&PieceCube::new_move(&cube, m, table), 
                MoveBlocker::new_move(mb, m),
                depth + 1, 
                set, 
                max_depth, 
                table);
        }
    }

    backtrack(&PieceCube::get_solved(), MoveBlocker::new(),  0, &mut set, max_depth, table);

    set.remove_duplicates();

    return set;
     
}

fn look_up(cube: PieceCube, table: &PieceCubeTable, set: &SealedSet){
    for max_depth in 0..9 {
        fn backtrack(cube: &PieceCube, mb: MoveBlocker, depth: i32, max_depth: i32, table: &PieceCubeTable, set: &SealedSet) -> bool {
            if depth == max_depth {
                return set.try_get_value(cube.get_index_cube()).0;
            }

            for m in 0..18 {
                if mb.move_blocked(m) {
                    continue;
                }

                if backtrack(&PieceCube::new_move(&cube, m, table), MoveBlocker::new_move(mb, m), depth + 1, max_depth, table, set) {
                    print_move(m);
                    return  true;
                }
            }

            return false;
        }

        if backtrack(&cube, MoveBlocker::new(),  0, max_depth, table, set) {
            println!();
            return;
        }
    }

    println!("No sol :(")
}

fn benchmark() {
    let mut count : i64 = 1;
    let table: PieceCubeTable = PieceCubeTable::init(); 
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

fn bench_count(count : i64, table: &PieceCubeTable,  sequence: &MoveSequence) {
    
    let mut cube = PieceCube::get_solved();

    for _ in 0..count {
        cube.apply_move_sequence(sequence, table);
    }

    black_box(cube);
}