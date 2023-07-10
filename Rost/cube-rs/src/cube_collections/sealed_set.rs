

use crate::{cube_representations::{index_cube::{self, IndexCube}, piece_cube::PieceCube, piece_cube_table::PieceCubeTable}, move_sequence::{MoveSequence, self}, cube_move::{NONE, self}};

const BUCKET_COUNT: usize = index_cube::MAX_EDGE_PERM as usize;

#[derive(PartialEq, Eq)]
struct CornerEdgeOrientState {
    edge_orient: u16,
    corner_perm: u16,
    corner_orient: u16
}

impl CornerEdgeOrientState {
    pub fn new(cube: IndexCube) -> Self {
        return CornerEdgeOrientState { 
            edge_orient: cube.edge_orient, 
            corner_perm: cube.corner_perm, 
            corner_orient: cube.corner_orient };
    }
}

pub struct SealedSet {
    pub max_depth: u8,
    data: Vec<CornerEdgeOrientState>,
    moves: Vec<u8>,
    start_index: Vec<u32>
}

impl SealedSet {
    pub fn new(cubes: &Vec<IndexCube>, max_depth: u8) -> Self {
        let mut data: Vec<CornerEdgeOrientState> = Vec::with_capacity(cubes.len());
        let mut moves: Vec<u8> = Vec::with_capacity(cubes.len());
        let mut start_index = vec![0 as u32; BUCKET_COUNT + 1];

        for i in 1..cubes.len() {
            if cubes[i].edge_perm < cubes[i - 1].edge_perm {
                println!("Alaaaaaaarm: list needs to be partialy sorted by edge_perm");
            }
        }

        for i in 0..cubes.len() {
            start_index[cubes[i].edge_perm as usize] += 1;
        }

        let mean = cubes.len() as f64 / BUCKET_COUNT as f64;
        println!("Avg bucket size: {}", mean);

        for i in 1..BUCKET_COUNT {
            start_index[i] += start_index[i - 1];
        }

        //shift by 1 index
        for i in (1..BUCKET_COUNT).rev() {
            start_index[i] = start_index[i - 1];
        }

        start_index[0] = 0;

        for i in 0..cubes.len() {
            data.push(CornerEdgeOrientState::new(cubes[i]));
            moves.push(cubes[i].last_move);
        }

        for m in &moves {
            if *m >= 18 {
                println!("This should not happen {}", m);
            }
        }

        return SealedSet { data, moves, start_index, max_depth }
    }

    pub fn try_get_value(&self, mut cube: IndexCube) -> (bool, IndexCube) {
        let start = self.start_index[cube.edge_perm as usize];
        let end = self.start_index[(cube.edge_perm + 1) as usize];
        
        let search = CornerEdgeOrientState::new(cube);

        for i in start..end {
            if self.data[i as usize] == search {
                
                cube.last_move = self.moves[i as usize];

                return (true, cube);
            }
        }

        return (false, IndexCube::get_solved());
    }


    pub fn find_move_sequence(&self, pc: &PieceCube, table: &PieceCubeTable) -> MoveSequence {
        let mut moves: Vec<u8> = Vec::new();
        let mut next = pc.clone();

        while !next.is_solved() {
            let pair = self.try_get_value(next.get_index_cube(cube_move::NONE));

            if !pair.0 {
                print!("Alarm");
            }

            let lm = move_sequence::reverse_move(pair.1.last_move);
            moves.push(lm);
            next.make_move(lm,  table);
        }

        return MoveSequence::new(moves);
    }
}