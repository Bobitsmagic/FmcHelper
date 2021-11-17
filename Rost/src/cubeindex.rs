#![allow(dead_code)]

use crate::{cube::Cube, enums::{CubeColor, CubeMove}, side::STRIPE_MASK};

const MAX_EO_MOVE_DEPTH: i32 = 8;
const MAX_MOVE_DEPTH: i32 = 7;

const SIZE_IN_BYTES: i32 = 11;
const PADDED_SIZE_IN_BYTES: i32 = 12;

pub const MAX_EDGE_PERMUTATION: u32 = 479_001_600;  //12!
pub const MAX_CORNER_PERMUTATION: u16 = 40320;      //8!
pub const MAX_EDGE_ORIENTATION: u16 = 4096;         //2^12
pub const MAX_CORNER_ORIENTATION: u16 = 6561;       //3^8

const EDGE_TRACK_SIZES: [i32; 6] = [40320, 24, 2, 1, 1, 1];
const CUBE_EDGE_COLORS: [[u8; 2]; 12] = [
    [0, 2],
    [0, 3],
    [0, 4],
    [0, 5],

    [1, 2],
    [1, 3],
    [1, 4],
    [1, 5],

    [2, 4 ],
    [2, 5 ],
    [3, 4 ],
    [3, 5 ]
];
const CUBE_CORNER_COLORS: [[u8; 3]; 8] = [
    [ 0, 2, 4 ],
    [ 0, 2, 5 ],
    [ 0, 3, 4 ],
    [ 0, 3, 5 ],

    [ 1, 2, 4 ],
    [ 1, 2, 5 ],
    [ 1, 3, 4 ],
    [ 1, 3, 5 ]
];

static TABLE: LookUpTable = LookUpTable::new();
struct LookUpTable {
    pub edge_indices: [[i32; 6]; 6],
    pub corner_indices: [[[i32; 6]; 6]; 6],
    pub next_edge_perm: [Vec<u32>; 6],
    pub next_corner_perm: [[u16; 18]; MAX_CORNER_PERMUTATION as usize],
    pub next_edge_orientation: [[u16; 18]; MAX_EDGE_ORIENTATION as usize],
    pub next_corner_orientation: [[u16; 18]; MAX_CORNER_PERMUTATION as usize],

    pub oriented_edge_count: [u8; MAX_EDGE_ORIENTATION as usize],
    pub oriented_corner_count: [u8; MAX_CORNER_ORIENTATION as usize],
    pub positioned_edge_count: [u8; MAX_EDGE_PERMUTATION as usize],
    pub positioned_corner_count: [u8; MAX_CORNER_PERMUTATION as usize],
}

impl LookUpTable {
    pub fn new() -> Self {
        let mut edge_indices: [[i32; 6]; 6];
        
        let mut counter = 0;
        for x in 0..6 {
           for y in (x + 1)..6 {
                if x / 2 == y / 2 {
                    continue;
                }

                edge_indices[x][y] = counter;
                edge_indices[y][x] = counter;

                counter += 1;
           }
        }

        counter = 0;
        let mut corner_indices: [[[i32; 6]; 6]; 6];
        for x in 0..6 {
            for y in 2..4 {
                for z in 4..6 {
                    corner_indices[x][y][z] = counter;
                    corner_indices[x][z][y] = counter;

                    corner_indices[y][x][z] = counter;
                    corner_indices[y][z][x] = counter;

                    corner_indices[z][x][y] = counter;
                    corner_indices[z][y][x] = counter;
                    
                    counter += 1;
                }
            }
        }

        let c = Cube::new();
        let buffer = Cube::new();
        let CubeIndex = CubeIndex::new();
        let next_corner_perm: [[i32; 18]; MAX_CORNER_PERMUTATION as usize];
        let mut permBuffer: [i32; 8];

        LookUpTable{value: 42}
    }
}

pub fn factorial(n: i32) -> i32{
    let mut ret = 1;
    for i in 2..(n + 1) {
        ret *= i;
    }
    ret
}

pub fn get_index_perm(n: i32, idx: i32){
    let mut list: Vec<i32> = vec![0i32; n as usize];
    for i in 0..n {
        list[i as usize] = i;
    }

    let mut index = idx;
    let mut indices: Vec<i32> = vec![0i32; n as usize];
    for i in (0..n).rev() {
        indices[i as usize] = index / factorial(i);
        index -= indices[i as usize] * factorial(i);
    }

    let ret: Vec<i32> = vec![0i32; n as usize];

    for i in (0..n).rev() {
        ret[(n - i - 1) as usize] = list[indices[i as usize] as usize];
        list.remove(indices[i as usize] as usize);
    }
}

pub fn inject_index_perm(mut perm: Vec<i32>, idx: i32){
    let n = perm.len() as i32;
    let mut list: Vec<i32> = vec![0i32; n as usize];
    for i in 0..n {
        list[i as usize] = i;
    }

    let mut index = idx;
    let mut indices: Vec<i32> = vec![0i32; n as usize];
    for i in (0..n).rev() {
        indices[i as usize] = index / factorial(i);
        index -= indices[i as usize] * factorial(i);
    }

    for i in (0..n).rev() {
        perm[(n - i - 1) as usize] = list[indices[i as usize] as usize];
        list.remove(indices[i as usize] as usize);
    }
}

pub fn get_index(perm: Vec<i32>) -> i32{
    let ret: i32 = 0;
    for i in 0..perm.len() {
        let counter = 0;
        for j in (i + 1)..perm.len() {
            if perm[i] > perm[j] {
                counter += 1;
            }
        }

        ret += factorial((perm.len() - 1 - i) as i32) * counter;
    }

    ret
}

struct  CubeIndex {
    pub edge_permutation: u32,
    pub corner_permutation: u16,
    pub edge_orientation: u16,
    pub corner_orientation: u16,
    pub last_move: CubeMove
}

impl CubeIndex {
    pub fn new() -> Self {
        CubeIndex{edge_permutation: 0, corner_permutation: 0, edge_orientation: 0, corner_orientation: 0, last_move: CubeMove::None}
    }
    pub fn init(edge_permutation: u32, corner_permutation: u16, edge_orientation: u16, corner_orientation: u16, last_move: CubeMove) -> Self {
        CubeIndex{edge_permutation, corner_permutation, edge_orientation, corner_orientation, last_move}
    }
    pub fn make_move(&mut self, m: CubeMove, look_up_table: &LookUpTable) {
        self.corner_permutation = look_up_table.next_corner_perm[self.corner_permutation as usize][m as usize];

        let count = ((m as i32) % 3) + 1;
        let side = (m as i32) / 3;
        let track_length = EDGE_TRACK_SIZES[side as usize];
        let array = look_up_table.next_edge_perm[side as usize];

        for i in 0..count {
            let index = ((self.edge_permutation as i32) / track_length) % (array.len() as i32);
            if ((self.edge_permutation as i32) / track_length / (array.len() as i32)) % 2 == 0 {
                self.edge_permutation = (self.edge_permutation + array[index as usize]) % MAX_EDGE_PERMUTATION;
            }
            else {
                index = array.len() as i32 - index - 1;
                self.edge_permutation = (self.edge_permutation + MAX_EDGE_PERMUTATION - array[index as usize]) % MAX_EDGE_PERMUTATION;
            }
        }

        self.last_move = m;
    }
}