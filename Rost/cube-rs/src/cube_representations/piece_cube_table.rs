use super::piece_cube::CornerState;
use super::piece_cube::EdgeState;

const AFFECTED_EDGES_CW: [[i32; 4]; 6] = [
    [0, 2, 1, 3],   //Orange
    [4, 7, 5, 6],   //Red
    [8, 0, 9, 4],   //Yellow
    [11, 1, 10, 5], //White
    [8, 6, 10, 2],  //Blue
    [9, 3, 11, 7]   //Green
];

pub const AFFECTED_CORNERS_CW: [[i32; 4]; 6] = [
    [0, 2, 3, 1],   //Orange
    [5, 7, 6, 4],   //Red
    [0, 1, 5, 4],   //Yellow
    [3, 2, 6, 7],   //White
    [4, 6, 2, 0],   //Blue
    [1, 3, 7, 5]    //Green
];

pub struct PieceCubeTable {
    pub corner_move_array: [[CornerState; 24]; 18],
    pub edge_move_array: [[EdgeState; 24]; 18]
}

impl PieceCubeTable {
    pub fn init() -> Self {
        let mut corner_move_array = [[CornerState::new(0) ; 24]; 18];
        let mut edge_move_array = [[EdgeState::new(0); 24]; 18];
        
        for m in 0..18 {
            let side = m / 3;
            let count = (m % 3) + 1;
        
            let corners = AFFECTED_CORNERS_CW[side as usize];
            //Corners
            for state in 0..24{
                let c = CornerState::new(state as u8);
                
                let mut next_orient = c.orientation();
                let mut next_pos = c.position();
                
                //Is affected by move
                let mut index = -1;
                for i in 0..4{
                    if next_pos == corners[i]{
                        index = i as i32;
                    }
                }
                
                if index != -1 {
                    if count != 2 {
                        next_orient = (2 * next_orient - (m / 6) + 3) % 3;
                    }
                    
                    next_pos = corners[((index + count) % 4) as usize];
                }
                
                corner_move_array[m as usize][state as usize] = 
                CornerState::new_pos_orient(next_pos, next_orient);
            }
            
            let edges = AFFECTED_EDGES_CW[side as usize];
            //Edges
            for state in 0..24{
                let e = EdgeState::new(state as u8);
                
                let mut next_orient = e.orientation();
                let mut next_pos = e.position();
                
                //Is affected by move
                let mut index = -1;
                for i in 0..4{
                    if next_pos == edges[i]{
                        index = i as i32;
                    }
                }
                
                if index != -1 {
                    //F, F', B, B'
                    if count != 2 && side >= 4 {
                        next_orient = 1 - next_orient;
                    }

                    next_pos = edges[((index + count) % 4) as usize];
                }

                edge_move_array[m as usize][state as usize] = 
                    EdgeState::new_pos_orient(next_pos, next_orient);
            }
        }

        return PieceCubeTable { corner_move_array, edge_move_array }
    }


}