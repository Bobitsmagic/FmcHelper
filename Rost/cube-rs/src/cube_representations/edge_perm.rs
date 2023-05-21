use crate::{move_sequence::MoveSequence, permutation};

use super::edge_perm_table::{self, EdgePermTable};

#[derive(PartialEq, Eq, Hash, Clone, Copy)]
pub struct EdgePerm {
    pub positions: [u8; 12]
}

impl EdgePerm {
    pub fn new() -> Self {
        let mut array = [0 as u8; 12];

        for i in 0..12 {
            array[i] = i as u8;
        }

        return EdgePerm { positions: array }
    }

    pub fn from_index(index: u32) -> Self {
        let array = permutation::get_indexed_perm(12, index);
        let mut buffer = [0 as u8; 12]; 
        for i in 0..12 {
            buffer[i] = array[i];
        }

        return  EdgePerm { positions: buffer };
    }

    pub fn make_move(&mut self, m: u8, table: &EdgePermTable) {
        let next = table.next_position[m as usize];
        for i in 0..12 {
            self.positions[i] = next[self.positions[i] as usize];
        }
    }

    pub fn apply_move_sequence(&mut self, sequence: &MoveSequence, table: &EdgePermTable){
        for m in &sequence.moves {
            self.make_move(*m, table);
        }
    }

    pub fn new_move(src: &EdgePerm, m: u8, table: &EdgePermTable) -> Self {
        let mut ret = (*src).clone();
        ret.make_move(m, table);

        return ret;
    }

    pub fn get_index(&self) -> u32 {
        return permutation::get_index(&self.positions);
    }
}
