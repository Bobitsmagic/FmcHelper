use crate::move_sequence::MoveSequence;

use super::{index_cube, corner_cube_table::CornerCubeTable};

pub const SIZE_IN_BYTES: u32 = 4;
pub const MAX_INDEX: u32 = index_cube::MAX_CORNER_PERM as u32 * index_cube::MAX_CORNER_ORIENT as u32;

#[derive(PartialEq, Eq, Clone, Copy, Hash)]
pub struct CornerCube {
    pub perm_index: u16, 
    pub orient_index: u16, 
}

impl CornerCube {
    pub fn new(perm_index: u16, orient_index: u16) -> Self {
        return CornerCube{perm_index, orient_index};
    }

    pub fn get_solved() -> Self {
        return Self::new(0, 0);
    }

    pub fn read_from_buffer(buffer: &Vec<u8>, index: usize) -> Self {
        let array: [u16; 2];
        let mut ser = [0 as u8; 4];
        ser.copy_from_slice(&buffer[index..(index + SIZE_IN_BYTES as usize)]);
        
        unsafe {
            array = std::mem::transmute(ser);    
        }
        
        return Self::new(array[0], array[1]);
    }

    pub fn write_to_buffer(&self, buffer: &mut Vec<u8>) {
        let array: [u16; 2] = [self.perm_index, self.orient_index];

        let ser: [u8; 4];
        unsafe {
             ser = std::mem::transmute(array);    
        }

        for val in ser {
            buffer.push(val);
        }
    }

    pub fn from_index(index: u32) -> Self {
        return Self::new((index % index_cube::MAX_CORNER_PERM as u32) as u16, 
        (index / index_cube::MAX_CORNER_PERM as u32) as u16);
    }

    pub fn get_index(&self) -> u32 {
        return self.perm_index as u32 + (self.orient_index as u32) * index_cube::MAX_CORNER_PERM as u32;
    }

    pub fn make_move(&mut self, m: u8, table: &CornerCubeTable){
        self.perm_index = table.next_perm[m as usize][self.perm_index as usize];
        self.orient_index = table.next_orient[m as usize][self.orient_index as usize];
    }

    pub fn apply_move_sequence(&mut self, sequence: &MoveSequence, table: &CornerCubeTable){
        for m in &sequence.moves {
            self.make_move(*m, table);
        }
    }

    pub fn new_move(src: &CornerCube, m: u8, table: &CornerCubeTable) -> Self {
        let mut ret = (*src).clone();
        ret.make_move(m, table);

        return ret;
    }
}