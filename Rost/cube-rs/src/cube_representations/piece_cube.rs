use crate::{move_sequence::MoveSequence, permutation, cube_move::NONE};

use super::{piece_cube_table::PieceCubeTable, index_cube::IndexCube};

pub const MAX_STATE : u8 = 24;

#[derive(Eq, PartialEq, Clone, Copy, PartialOrd, Ord, Hash)]
pub struct CornerState {
    pub state: u8
}

impl CornerState {
    pub fn new(value: u8) -> Self {
        return CornerState {state: value }
    }

    pub fn new_pos_orient(pos: i32, orient: i32) -> Self {
        return CornerState {state: (pos * 3 + orient) as u8}
    }

    pub fn position(self) -> i32 {
        return (self.state as i32) / 3;
    }

    pub fn orientation(self) -> i32 {
        return (self.state as i32) % 3;
    }
}

#[derive(Eq, PartialEq, Clone, Copy, PartialOrd, Ord, Hash)]
pub struct EdgeState {
    pub state: u8
}

impl EdgeState {
    pub fn new(value: u8) -> Self {
        return EdgeState {state: value }
    }

    pub fn new_pos_orient(pos: i32, orient: i32) -> Self {
        return EdgeState {state: (pos * 2 + orient) as u8}
    }

    pub fn position(self) -> i32 {
        return (self.state as i32) / 2;
    }

    pub fn orientation(self) -> i32 {
        return (self.state as i32) % 2;
    }
}

const SOLVED_CORNERS: [CornerState; 8] = [ 
    CornerState{state: 0},
    CornerState{state: 3},
    CornerState{state: 6},
    CornerState{state: 9},
    CornerState{state: 12},
    CornerState{state: 15},
    CornerState{state: 18},
    CornerState{state: 21}
];

const SOLVED_EDGES: [EdgeState; 12] = [ 
    EdgeState{state: 0},
    EdgeState{state: 2},
    EdgeState{state: 4},
    EdgeState{state: 6},
    EdgeState{state: 8},
    EdgeState{state: 10},
    EdgeState{state: 12},
    EdgeState{state: 14},
    EdgeState{state: 16},
    EdgeState{state: 18},
    EdgeState{state: 20},
    EdgeState{state: 22}
];

#[derive(PartialEq, Eq, Clone, Ord, PartialOrd, Hash)]
pub struct PieceCube {
    corners: [CornerState; 8],
    edges: [EdgeState; 12]
}

impl PieceCube {
    pub fn get_solved() -> Self {
        return PieceCube { corners: SOLVED_CORNERS, edges: SOLVED_EDGES };
    }
    pub fn from_index_cube(src: IndexCube) -> Self {
        let mut ret = Self::get_solved();

        let edge_perm = permutation::get_indexed_perm(12,src.edge_perm);

        for i in 0..12 {
            ret.edges[i] = EdgeState::new_pos_orient(edge_perm[i] as i32, 
                ((src.edge_orient >> (11 - edge_perm[i])) & 1) as i32);
        }

        let corner_perm = permutation::get_indexed_perm(8, src.corner_perm as u32);
        let mut orientation = [0 as u16; 8];
        let mut corner_orient_index = src.corner_orient;
        for i in (0..8).rev() {
            orientation[i] = corner_orient_index % 3;
            corner_orient_index /= 3;
        }
        for i in 0..8 {
            ret.corners[i] = CornerState::new_pos_orient(corner_perm[i] as i32, 
                orientation[corner_perm[i] as usize] as i32);
        }

        return ret;
    }
    
    pub fn is_solved(&self) -> bool {
        return self.corners == SOLVED_CORNERS && 
        self.edges == SOLVED_EDGES;
    }
    
    pub fn make_move(&mut self, m: u8, table: &PieceCubeTable) {
        let next_corner = table.corner_move_array[m as usize];
        
        for i in 0..8 {
            self.corners[i] = next_corner[self.corners[i].state as usize];
        }
        
        let next_edge = table.edge_move_array[m as usize];
        for i in 0..12 {
            self.edges[i] = next_edge[self.edges[i].state as usize];
        }
    }

    pub fn apply_move_sequence(&mut self, sequence: &MoveSequence, table: &PieceCubeTable){
        for m in &sequence.moves {
            self.make_move(*m, table);
        }
    }
    
    pub fn new_move(src: &PieceCube, m: u8, table: &PieceCubeTable) -> Self {
        let mut ret = (*src).clone();
        ret.make_move(m, table);

        return ret;
    }

    pub fn get_index_cube(&self) -> IndexCube {
        return IndexCube::new(
            self.get_edge_perm_index(), 
            self.get_edge_orient_index(), 
            self.get_corner_perm_index(), 
            self.get_corner_orient_index(), 
            NONE);
    }

    pub fn get_edge_perm_index(&self) -> u32 {
        return permutation::get_index(&self.edges.map(|x| x.position() as u8));
    }
    pub fn get_corner_perm_index(&self) -> u16 {
        return permutation::get_index(&self.corners.map(|x| x.position() as u8)) as u16;
    }
    pub fn get_edge_orient_index(&self) -> u16 {
        let mut vals = [0 as i32; 12];
        for i in 0..12 {
            vals[self.edges[i].position() as usize] = self.edges[i].orientation(); 
        }

        let mut ret = 0;

        for i in 0..12 {
            ret = (ret << 1) | vals[i];
        }

        return ret as u16;
    }
    pub fn get_corner_orient_index(&self) -> u16 {
        let mut vals = [0 as i32; 8];
        for i in 0..8 {
            vals[self.corners[i].position() as usize] = self.corners[i].orientation(); 
        }

        let mut ret = 0;

        for i in 0..8 {
            ret = ret * 3 + vals[i];
        }

        return ret as u16;
    }

}