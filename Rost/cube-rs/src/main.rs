#![allow(dead_code)]

mod cube_representations;
mod move_sequence;
mod move_blocker;
mod pieces;
mod cube_move;
mod permutation;
mod cube_collections;

use cube_collections::sorted_set::SortedSet;
use cube_representations::corner_cube_table::{self, CornerCubeTable};
use cube_representations::edge_perm::EdgePerm;
use cube_representations::edge_perm_table::EdgePermTable;
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
use cube_representations::corner_cube::CornerCube;
use move_sequence::MoveSequence;

use std::fs::File;
use std::io::prelude::*;

use crate::cube_collections::sealed_set::SealedSet;
use crate::cube_representations::corner_cube;
use crate::move_blocker::MoveBlocker;
use crate::move_sequence::print_move;

fn main() {
    //let table = CornerCubeTable::init(&PieceCubeTable::init());
    let table = EdgePermTable::init();

    for i in 0..13 {
        let start = Instant::now();
        let set = build_edge_table(i, &table);
        let duration = start.elapsed();
        println!("N: {}, Count: {}, Time: {:?}", i, set.len(), duration);
    }

}

fn build_corner_table(max_depth: i32, table: &CornerCubeTable) -> HashSet<CornerCube> {
    let mut set = HashSet::new();
    let mut list: Vec<CornerCube> = Vec::new();
    list.push(CornerCube::get_solved());
    set.insert(CornerCube::get_solved());

    let mut next_list: Vec<CornerCube> = Vec::new();

    for d in 0..max_depth {
        for cube in &list {
            for m in 0..18 {
                if m == cube_move::F || m == cube_move::FP || m == cube_move::B || m == cube_move::BP {
                    continue;
                } 

                let n = CornerCube::new_move(&cube, m, table); 

                if set.insert(n) {
                    next_list.push(n);
                }
            }
        }

        list.clear();
        list.append(&mut next_list);     
        next_list.clear();
    }


    return set;
     
}

fn build_edge_table(max_depth: i32, table: &EdgePermTable) -> Vec<u32> {
    fn remove_duplicates(list: &mut Vec<u32>, start: usize){
        list[start..] .sort();
        
        let mut current = list[start];
        let mut delete_count = 0;
        
        for i in (start + 1)..list.len() {
            let cube = list[i];
            
            if cube != current {
                current = cube;
                list[i - delete_count] = cube;
            }
            else {
                delete_count += 1;
            }
        }
        
        println!("Removed {} from {} elements ({})", delete_count, list.len(), delete_count as f32 / list.len() as f32);
        list.drain((list.len() - delete_count)..(list.len()));
    }

    let mut set: Vec<u32> = Vec::new();
    set.push(0);

    for d in 0..max_depth {
        let pre_count = set.len();

        for i in 0..pre_count {
            let cube = EdgePerm::from_index(set[i]);

            for m in 0..18 {
                if m == cube_move::F || m == cube_move::FP || m == cube_move::B || m == cube_move::BP {
                    continue;
                } 

                let n = EdgePerm::new_move(&cube, m, table).get_index(); 
                
                set.push(n);
            }

            const DUP: u32 = 1 << 24;
            if i as u32 % DUP == DUP - 1 {
                remove_duplicates(&mut set, pre_count)
            }
        }
        

        remove_duplicates(&mut set, 0);
    }

    return set;
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