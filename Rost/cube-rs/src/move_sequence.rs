use std::ops::Index;
use std::vec;
use rand::rngs::StdRng;
use rand::Rng;

use crate::cube_move;
use crate::move_blocker::MoveBlocker;

const NAMES: [&str; 18] = [
    "L ", "L2", "L'", 
    "R ", "R2", "R'",
    "D ", "D2", "D'",
    "U ", "U2", "U'",
    "B ", "B2", "B'",
    "F ", "F2", "F'",];

pub struct MoveSequence {
    pub moves: Vec<u8>
}

pub fn print_move(m: u8){
    print!("{} ", NAMES[m as usize]);
}

impl MoveSequence {
    pub fn new(moves: Vec<u8>) -> Self {
        return MoveSequence{moves};
    }

    pub fn from_string(str: &str) -> Self {
        const MOVES: [&str; 18] = [
            "L", "L2", "LP",
            "R", "R2", "RP",

            "D", "D2", "DP",
            "U", "U2", "UP",

            "B", "B2", "BP",
            "F", "F2", "FP",
        ];
        
        let mut split = str.split(' ');

        let mut list: Vec<u8> = Vec::new();

        for part in split {
            for i in 0..18 {
                if MOVES[i] == part {
                    list.push(i as u8);
                    break;
                }
            }
        }

        return MoveSequence::new(list);

    }

    pub fn get_random(count: i32, rng: &mut StdRng) -> Self { 
        let mut seq = MoveSequence { moves: vec![0 as u8; count as usize]};

        seq.randomize(count, rng);

        return seq;
    }

    pub fn get_random_eo(count: i32, rng: &mut StdRng) -> Self { 
        let mut seq = MoveSequence { moves: vec![0 as u8; count as usize]};

        seq.randomize_eo(count, rng);

        return seq;
    }

    pub fn randomize(&mut self, count: i32, rng: &mut StdRng){
        self.moves.clear();
        
        let mut list = Vec::new();
        let mut mb = MoveBlocker::new();

        for _ in 0..count {
            mb.insert_possible_moves(&mut list);
            let m = list[rng.gen_range(0..list.len())];

            mb.update_blocked(m);
            self.moves.push(m);
        }
    }

    pub fn randomize_eo(&mut self, count: i32, rng: &mut StdRng){
        self.moves.clear();
        
        let mut list = Vec::new();
        let mut mb = MoveBlocker::new();

        for _ in 0..count {
            mb.insert_possible_moves(&mut list);
            for i in (0..list.len()).rev(){
                let m = list[i];
                if m == cube_move::F || m == cube_move::FP || m == cube_move::B || m == cube_move::BP {
                    list.remove(i);
                }
            }

            let m = list[rng.gen_range(0..list.len())];

            mb.update_blocked(m);
            self.moves.push(m);
        }
    }

    pub fn print(&self) {
        for m in &self.moves {
            print!("{} ", NAMES[*m as usize]);
        }
        println!();
    }
}