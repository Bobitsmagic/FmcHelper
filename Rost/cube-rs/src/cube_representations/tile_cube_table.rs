const TILE_COUNT : i32 = 54;
const MOVE_COUNT : i32 = 18;
pub struct TileCubeTable {
    pub move_array: [[i32; TILE_COUNT as usize]; 18],
    pub non_id_moves: [[i32; 20 as usize]; 18]
}

impl TileCubeTable{
    pub fn init() -> Self {
        let mut move_array = [[0 as i32; TILE_COUNT as usize]; MOVE_COUNT as usize];

        const AFFECTED_TILES: [[i32; 12]; 6] = [
            //Orange
            [33, 30, 27, 51, 48, 45, 24, 21, 18, 38, 41, 44],
            //Red
            [20, 23, 26, 47, 50, 53, 29, 32, 35, 42, 39, 36],
            //Yellow
            [0, 1, 2, 45, 46, 47, 9, 10, 11, 36, 37, 38],
            //White
            [44, 43, 42, 17, 16, 15, 53, 52, 51, 8, 7, 6 ],
            //Blue
            [11, 14, 17, 35, 34, 33, 6, 3, 0, 18, 19, 20 ],
            //Green
            [2, 5, 8, 27, 28, 29, 15, 12, 9, 26, 25, 24]
        ];

        const ROTATION_ARRAY: [i32; 9] = [6, 3, 0, 7, 4, 1, 8, 5, 2];
        
        for side in 0..6 {
            let mut template: [i32; TILE_COUNT as usize] = [0; TILE_COUNT as usize];
            for i in 0..TILE_COUNT {
                template[i as usize] = i;
            }

            for i in 0..9 {
                template[side * 9 + i] = ROTATION_ARRAY[i] + side as i32 * 9;
            }

            for i in 0..12 {
                template[AFFECTED_TILES[side][i] as usize] = AFFECTED_TILES[side][(i + 3) % 12];
            }

            let mut buffer : [i32; TILE_COUNT as usize] = [0; TILE_COUNT as usize];

            buffer.copy_from_slice(&template);

            move_array[side * 3 + 0 as usize].copy_from_slice(&template);
            
            for i in 0..TILE_COUNT {
                template[i as usize] = buffer[buffer[i as usize] as usize];
            }
            move_array[side * 3 + 1 as usize].copy_from_slice(&template);
            
            for i in 0..TILE_COUNT {
                template[i as usize] = buffer[buffer[buffer[i as usize] as usize] as usize];
            }
            move_array[side * 3 + 2 as usize].copy_from_slice(&template);
         }

         let mut non_id_moves = [[0 as i32; 20 as usize]; MOVE_COUNT as usize];
         for m in 0..18 {
            let mut count = 0;
            for i in 0..TILE_COUNT {
                if move_array[m][i as usize] != i {
                    non_id_moves[m][count] = i;
                    count += 1;  
                }
            }
         }

        return TileCubeTable {move_array, non_id_moves};
    }
}