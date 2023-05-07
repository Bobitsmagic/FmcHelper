use crate::move_sequence::MoveSequence;

use super::tile_cube_table::Table;

const TILE_COUNT : i32 = 6 * 9;

const SOLVED_DATA : [u8; TILE_COUNT as usize] = [
    0, 0, 0, 0, 0, 0, 0, 0, 0, 
    1, 1, 1, 1, 1, 1, 1, 1, 1, 
    2, 2, 2, 2, 2, 2, 2, 2, 2, 
    3, 3, 3, 3, 3, 3, 3, 3, 3, 
    4, 4, 4, 4, 4, 4, 4, 4, 4, 
    5, 5, 5, 5, 5, 5, 5, 5, 5
];

#[derive(Eq, PartialEq, Clone)]
pub struct TileCube {
    data: [u8; TILE_COUNT as usize]
}

fn get_tile_index(side: i32, x: i32, y: i32) -> i32 {
    return side * 9 + x + y * 3;
}

impl TileCube {
    fn new() -> Self {
        return TileCube { data: [0 as u8; TILE_COUNT as usize]}
    }
    

    pub fn get_solved() -> Self {
        return TileCube { data: SOLVED_DATA }
    }

    pub fn make_move(&mut self, cube_move: u8, table: &Table) {
        //let buffer = self.data;
        //let mut buffer = [0 as u8; TILE_COUNT as usize];
        //buffer.copy_from_slice(&self.data);
        let buffer = self.data.clone();

        let move_array = table.move_array[cube_move as usize];

        for i in table.non_id_moves[cube_move as usize] {
            self.data[move_array[i as usize] as usize] = buffer[i as usize];
        }

        //for i in 0..TILE_COUNT {
        //    self.data[move_array[i as usize] as usize] = buffer[i as usize];
        //}
    }

    pub fn apply_move_sequence(&mut self, sequence: &MoveSequence, table: &Table){
        for m in &sequence.moves {
            self.make_move(*m, table);
        }
    }

    pub fn new_move(src: &TileCube, m: u8, table: &Table) -> Self {
        let mut ret = (*src).clone();
        ret.make_move(m, table);

        return ret;
    }

    pub fn is_solved(&self) -> bool{
        return self.data == SOLVED_DATA;
    }

    pub fn print_side_view(&self) {
        const SIDES: [i32; 4] = [0, 5, 1, 4];
        const TILES: [char; 7] = ['o', 'r', 'y', 'w', 'b', 'g', 'n'];

        println!("#color_viewer(0.6cm, ");

        //white
        for y in (0..3).rev() {
            print!("\t");
            for i in 0..3 {
                print!("n, ");
            }

            for x in 0..3 {
                print!("{}, ", TILES[self.data[get_tile_index(3, x, y) as usize] as usize]);
            }

            for i in 0..6 {
                print!("n, ");
            }

            println!();
        }

        //orange, green, red, blue
        for y in (0..3).rev() {
            print!("\t");
            for i in 0..4 {
                for x in 0..3 {
                    print!("{}, ", TILES[self.data[get_tile_index(SIDES[i], x, y) as usize] as usize]);
                }
            }
            println!();
        }

        //yellow
        for y in (0..3).rev() {
            print!("\t");
            for i in 0..3 {
                print!("n, ");
            }

            for x in 0..3 {
                print!("{}, ", TILES[self.data[get_tile_index(2, x, y) as usize] as usize]);
            }

            for i in 0..6 {
                print!("n, ");
            }

            println!();
        }

        println!(")");
    }
}