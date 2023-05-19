use super::{index_cube::{self, IndexCube}, piece_cube_table::{self, PieceCubeTable}, piece_cube::{self, PieceCube}};

const AFFECTED_CORNERS_CW: [[i32; 4]; 6] = piece_cube_table::AFFECTED_CORNERS_CW;
const MAX_CORNER_PERM: u16 = index_cube::MAX_CORNER_PERM;
const MAX_CORNER_ORIENT: u16 = index_cube::MAX_CORNER_ORIENT;

pub struct CornerCubeTable {
    pub next_perm: Vec<Vec<u16>>, 
    pub next_orient: Vec<Vec<u16>>
}

impl CornerCubeTable {
    pub fn init(table: &PieceCubeTable) -> Self {
        let mut next_perm: Vec<Vec<u16>> = vec![vec![0 as u16; MAX_CORNER_PERM as usize]; 18];
        let mut next_orient: Vec<Vec<u16>> = vec![vec![0 as u16; index_cube::MAX_CORNER_PERM as usize]; 18];
        
        for m in 0..18 {            
            let mut perm_list = vec![0 as u16; MAX_CORNER_PERM as usize];

            for perm_index in 0..MAX_CORNER_PERM {
                let idc = 
                    IndexCube::new(0, 0, perm_index, 0, 0);

                let mut pc = PieceCube::from_index_cube(idc);

                pc.make_move(m, table);

                perm_list[perm_index as usize] = pc.get_corner_perm_index();
            }

            next_perm[m as usize] = perm_list;
            
            let mut orient_list = vec![0 as u16; MAX_CORNER_ORIENT as usize];

            for orient_index in 0..MAX_CORNER_ORIENT {
                let idc = 
                    IndexCube::new(0, 0, 0, orient_index, 0);

                let mut pc = PieceCube::from_index_cube(idc);

                pc.make_move(m, table);

                orient_list[orient_index as usize] = pc.get_corner_orient_index();
            }
            
            next_orient[m as usize] = orient_list;
        }
        
        return CornerCubeTable{next_perm, next_orient};
    }
}
