#![allow(dead_code)]

mod cube_representations;
mod move_sequence;
mod move_blocker;
mod pieces;
mod cube_move;
mod permutation;
mod cube_collections;
mod bucket_heap;
mod sorted_heap;

use bucket_heap::BucketHeap;
use sorted_heap::SortedHeap;
use cube_collections::sorted_set::SortedSet;
use cube_representations::corner_cube_table::{self, CornerCubeTable};
use cube_representations::edge_perm::EdgePerm;
use cube_representations::edge_perm_table::EdgePermTable;
use cube_representations::index_cube::{IndexCube, self};
use cube_representations::piece_cube_table::PieceCubeTable;
use rand::prelude::*;

use std::collections::{HashSet, HashMap};
use std::hint::black_box;
use std::{thread, vec};
use std::time::{Instant, Duration};
use cube_representations::piece_cube::PieceCube;
use cube_representations::tile_cube::TileCube;
use cube_representations::tile_cube_table::TileCubeTable;
use cube_representations::corner_cube::CornerCube;
use move_sequence::MoveSequence;

use std::cmp;
use std::fs::File;
use std::io::{Read, Write};

use crate::cube_collections::sealed_set::SealedSet;
use crate::cube_representations::corner_cube;
use crate::move_blocker::MoveBlocker;
use crate::move_sequence::print_move;

fn main() {
    let table = PieceCubeTable::init();
    
    //let cc_table = CornerCubeTable::init(&PieceCubeTable::init());
    //build_corner_table(12, &cc_table);
    //println!("Created corner table");
    //
    //let ep_table = EdgePermTable::init();
    //build_edge_table(13, &ep_table);
    //println!("Created edge table");

    
    let corner_cost = load_corner_table("corners_eo.bin".to_owned());
    let edge_cost = load_edge_table("edges_eo.bin".to_owned());
    
    println!("Loaded tables");
    
    let set = SealedSet::new(&create_sorted_set(8, &table).data);

    println!("Loaded look up");

    let mut rng = StdRng::seed_from_u64(13);

    for _ in 0..10000 {
        let mut cube = PieceCube::get_solved();
        
        let ms = MoveSequence::get_random_eo(30, &mut rng);
        ms.print();
        cube.apply_move_sequence(&ms, &table);
        //cube.apply_move_sequence(&MoveSequence::from_string("R2 L2 F2 U2 R2"), &table);
        
        let mut start = Instant::now();
        //a_star_order(cube, &table, &corner_cost, &edge_cost);
        a_star_set(cube, &table, &corner_cost, &edge_cost, &set);
        //a_star(cube, &table, &corner_cost, &edge_cost);
        let mut duration = start.elapsed();

        println!("{:?}", duration);
    }
}

fn load_corner_table(path: String) -> Vec<u8> {

    //std::fs::read(path)
    let mut data = Vec::new();
    let mut f = File::open(path).expect("Unable to open file");
    f.read_to_end(&mut data).expect("Unable to read data");

    return  data;

    let count = data.len() / (corner_cube::SIZE_IN_BYTES + 1) as usize;

    println!("Found {} corner cubes {} bytes", count, data.len());

    let mut ret: Vec<u8> = vec![255; index_cube::MAX_CORNER_PERM as usize * index_cube::MAX_CORNER_ORIENT as usize];

    let mut index = 0;
    for _ in 0..count {

        ret[CornerCube::read_from_buffer(&data, index).get_index() as usize] = data[index + 4];
        
        index += 5;
    }

    return ret;
}   

fn load_edge_table(path: String) -> Vec<u8> {
    let mut data = Vec::new();
    let mut f = File::open(path).expect("Unable to open file");
    f.read_to_end(&mut data).expect("Unable to read data");  

    println!("Found {} edge cubes", data.len());

    return data;
}   

fn build_corner_table(max_depth: i32, table: &CornerCubeTable) {
    let mut set = HashMap::new();
    let mut list: Vec<CornerCube> = Vec::new();
    list.push(CornerCube::get_solved());
    set.insert(CornerCube::get_solved(), 0);

    let mut next_list: Vec<CornerCube> = Vec::new();

    for d in 0..max_depth {
        for cube in &list {
            for m in 0..18 {
                if m == cube_move::F || m == cube_move::FP || m == cube_move::B || m == cube_move::BP {
                    continue;
                } 
                
                let n = CornerCube::new_move(&cube, m, table); 
                
                if !set.contains_key(&n) {
                    set.insert(n, (d + 1) as u8);
                    next_list.push(n);
                }
            }
        }
        
        list.clear();
        list.append(&mut next_list);     
        next_list.clear();
        
        println!("Max depth: {} with {} new elements sum: {}", d, list.len(), set.len());
    }

    let mut ret: Vec<u8> = vec![255; corner_cube::MAX_INDEX as usize]; 

    for pair in set {
        ret[pair.0.get_index() as usize] = pair.1;
    }

    let mut f = File::create("corners_eo.bin").expect("Unable to create file");
    f.write_all(&ret).expect("Unable to write Data");
    f.flush();     
}

fn build_edge_table(max_depth: i32, table: &EdgePermTable) {
    let mut next: Vec<u32> = Vec::new();
    let mut current: Vec<u32> = Vec::new();
    current.push(0);

    let mut ret: Vec<u8> = vec![255; index_cube::MAX_EDGE_PERM as usize];
    ret[0] = 0;

    for d in 0..max_depth {
        for perm in &current {
            let cube = EdgePerm::from_index(*perm);

            for m in 0..18 {
                if m == cube_move::F || m == cube_move::FP || m == cube_move::B || m == cube_move::BP {
                    continue;
                } 

                let n = EdgePerm::new_move(&cube, m, table).get_index(); 
                
                if ret[n as usize] == 255 {
                    ret[n as usize] = (d + 1) as u8;
                    next.push(n);
                }
            }
        }

        current.clear();
        current.append(&mut next);
        next.clear();

        println!("Max depth: {} with {}", d, current.len());
    }

    let mut f = File::create("edges_eo.bin").expect("Unable to create file");
    f.write_all(&ret).expect("Unable to write Data");
    f.flush();
}

fn a_star(cube: PieceCube, table: &PieceCubeTable, corner_cost: &Vec<u8>, edge_cost: &Vec<u8>) {
    fn get_cost(pc: &PieceCube, corner_cost: &Vec<u8>, edge_cost: &Vec<u8>) -> u8 {

        let c1 = corner_cost[pc.get_corner_index() as usize];
        let c2 = edge_cost[pc.get_edge_perm_index() as usize];

        if c1 == 255 {
            println!("corner not found: {} ", pc.get_corner_index());
        }

        if c2 == 255 {
            println!("edge not found: {}", pc.get_edge_perm_index());
        }

        let dif = (c1 as i32 - c2 as i32).abs(); 
        if dif > 9 {
            println!("Dif corner: {} edge {}", c1, c2);
        }

        return cmp::max(c1, c2);
        //return c2;
    }

    let mut heap = BucketHeap::new();
    heap.add(0, get_cost(&cube, corner_cost, edge_cost), cube, MoveBlocker::new());

    while heap.len() > 0 {
        let pair = heap.pop();
        let cube = &pair.0;
        let mb = pair.1;
        let depth = pair.2;

        for m in 0..18 {
            if mb.move_blocked(m) {
                continue;
            }

            if m == cube_move::F || m == cube_move::FP || m == cube_move::B || m == cube_move::BP {
                continue;
            }

            let next = PieceCube::new_move(cube, m, table);
            
            if next.is_solved() {
                println!("Found solution at cost: {} ", depth + 1);

                return;
            }

            let cost = get_cost(&next, corner_cost, edge_cost);

            heap.add(depth + 1,  cost,  next, mb);
        }
    }
}

fn a_star_order(cube: PieceCube, table: &PieceCubeTable, corner_cost: &Vec<u8>, edge_cost: &Vec<u8>) {
    fn get_cost(pc: &PieceCube, corner_cost: &Vec<u8>, edge_cost: &Vec<u8>) -> (u8, u8) {

        let c1 = corner_cost[pc.get_corner_index() as usize];
        let c2 = edge_cost[pc.get_edge_perm_index() as usize];

        if c1 == 255 {
            println!("corner not found: {} ", pc.get_corner_index());
        }

        if c2 == 255 {
            println!("edge not found: {}", pc.get_edge_perm_index());
        }

        let dif = (c1 as i32 - c2 as i32).abs(); 
        if dif > 9 {
            println!("Dif corner: {} edge {}", c1, c2);
        }

        return (cmp::max(c1, c2), dif as u8);
        //return c2;
    }

    let mut heap = SortedHeap::new();
    let pair = get_cost(&cube, corner_cost, edge_cost);
    heap.add(0, pair.0, pair.1 , cube, MoveBlocker::new());

    while heap.len() > 0 {
        let pair = heap.pop();
        let cube = &pair.0;
        let mb = pair.1;
        let depth = pair.2;

        for m in 0..18 {
            if mb.move_blocked(m) {
                continue;
            }

            if m == cube_move::F || m == cube_move::FP || m == cube_move::B || m == cube_move::BP {
                continue;
            }

            let next = PieceCube::new_move(cube, m, table);
            
            if next.is_solved() {
                println!("Found solution at cost: {} ", depth + 1);

                return;
            }

            let cost = get_cost(&next, corner_cost, edge_cost);

            heap.add(depth + 1,  cost.0, cost.1,  next, mb);
        }
    }
}


fn a_star_set(cube: PieceCube, table: &PieceCubeTable, corner_cost: &Vec<u8>, edge_cost: &Vec<u8>, set: &SealedSet){
    fn get_cost(pc: &PieceCube, corner_cost: &Vec<u8>, edge_cost: &Vec<u8>) -> u8 {

        let c1 = corner_cost[pc.get_corner_index() as usize];
        let c2 = edge_cost[pc.get_edge_perm_index() as usize];

        if c1 == 255 {
            println!("corner not found: {} ", pc.get_corner_index());
        }

        if c2 == 255 {
            println!("edge not found: {}", pc.get_edge_perm_index());
        }

        let dif = (c1 as i32 - c2 as i32).abs(); 
        if dif > 9 {
            println!("Dif corner: {} edge {}", c1, c2);
        }

        return cmp::max(c1, c2);
        //return c2;
    }

    let mut heap = BucketHeap::new();
    heap.add(0, get_cost(&cube, corner_cost, edge_cost), cube, MoveBlocker::new());

    while heap.len() > 0 {
        let pair = heap.pop();
        let cube = &pair.0;
        let mb = pair.1;
        let depth = pair.2;

        for m in 0..18 {
            if mb.move_blocked(m) {
                continue;
            }

            if m == cube_move::F || m == cube_move::FP || m == cube_move::B || m == cube_move::BP {
                continue;
            }

            let next = PieceCube::new_move(cube, m, table);

            let cost = get_cost(&next, corner_cost, edge_cost);
            
            if cost <= 8 {
                if set.try_get_value(next.get_index_cube()).0 {
                    println!("Found solution in table at cost: {} ", depth + 1 + cost);

                    return;
                }
            }


            heap.add(depth + 1,  cost,  next, mb);
        }
    }
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

            if m == cube_move::F || m == cube_move::FP || m == cube_move::B || m == cube_move::BP {
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

    println!("Set count: {} size in bytes: {}", set.len(), set.len() * 12);

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