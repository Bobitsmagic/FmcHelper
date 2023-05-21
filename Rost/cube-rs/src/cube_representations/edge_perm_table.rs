use super::piece_cube_table;

const AFFECTED_EDGES_CW: [[i32; 4]; 6] = piece_cube_table::AFFECTED_EDGES_CW;

pub struct EdgePermTable {
    pub next_position: [[u8; 12]; 18]
}

impl EdgePermTable {
    pub fn init() -> Self {
        let mut next_position: [[u8; 12]; 18] = [[0; 12]; 18];
        for m in 0..18 {
            let side = m / 3;
            let count = (m % 3) + 1;
        
            let edges = AFFECTED_EDGES_CW[side as usize];

            for p in 0..12 {
                let mut next_pos = p;

                //Is affected by move
                let mut index = -1;
                for i in 0..4{
                    if next_pos == edges[i]{
                        index = i as i32;
                    }
                }
                
                if index != -1 {
                    next_pos = edges[((index + count) % 4) as usize];
                }

                next_position[m as usize][p as usize] = next_pos as u8; 
            }
        } 

        return  EdgePermTable{ next_position };
    }
}