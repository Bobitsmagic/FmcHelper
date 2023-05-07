const TILE_COUNT : i32 = 6 * 9;

const SOLVED_DATA : [u8; TILE_COUNT as usize] = [
    0, 0, 0, 0, 0, 0, 0, 0, 0, 
    1, 1, 1, 1, 1, 1, 1, 1, 1, 
    2, 2, 2, 2, 2, 2, 2, 2, 2, 
    3, 3, 3, 3, 3, 3, 3, 3, 3, 
    4, 4, 4, 4, 4, 4, 4, 4, 4, 
    5, 5, 5, 5, 5, 5, 5, 5, 5
];
pub struct TileCube {
    data: [u8; TILE_COUNT as usize]
}