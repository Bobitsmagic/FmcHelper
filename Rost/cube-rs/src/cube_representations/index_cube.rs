use crate::cube_move;
use std::hash::{Hash, Hasher};

pub const MAX_EDGE_PERM: u32 = 479_001_600;
pub const MAX_EDGE_ORIENT: u16 = 4096;
pub const MAX_CORNER_PERM: u16 = 40320;
pub const MAX_CORNER_ORIENT: u16 = 6561;

#[derive(PartialEq, Eq, Copy, Clone, PartialOrd, Ord)]
pub struct IndexCube {
    pub edge_perm: u32,
    pub edge_orient: u16,
    pub corner_perm: u16,
    pub corner_orient: u16,
    pub last_move: u8
}

impl IndexCube {
    pub fn new(edge_perm: u32, edge_orient: u16, corner_perm: u16, corner_orient: u16, last_move: u8) -> Self {
        return IndexCube { edge_perm, edge_orient, corner_perm, corner_orient, last_move };
    }

    pub fn get_solved() -> Self {
        return Self::new(0, 0, 0, 0, cube_move::NONE);
    }

    pub fn is_solved(&self) -> bool {
        return self.edge_perm == 0 && self.edge_orient == 0 && self.corner_perm == 0 && self.corner_orient == 0;
    }
}

impl Hash for IndexCube {
    fn hash<H: Hasher>(&self, state: &mut H) {
        self.edge_perm.hash(state);
        self.edge_orient.hash(state);
        self.corner_perm.hash(state);
        self.corner_orient.hash(state);
    }
}