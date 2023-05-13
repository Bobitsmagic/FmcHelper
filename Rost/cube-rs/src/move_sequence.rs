use std::vec;
use rand::rngs::StdRng;
use rand::Rng;

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

    pub fn get_random(count: i32, rng: &mut StdRng) -> Self { 
        let mut seq = MoveSequence { moves: vec![0 as u8; count as usize]};

        seq.randomize(count, rng);

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

    pub fn print(&self) {
        for m in &self.moves {
            print!("{} ", NAMES[*m as usize]);
        }
        println!();
    }
}