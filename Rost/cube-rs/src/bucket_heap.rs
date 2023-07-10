use crate::{cube_representations::piece_cube::PieceCube};

pub struct BucketHeap {
    data: Vec<Vec<(PieceCube, u8, u8)>>,
    count: u32,
    inserts: u64
}

impl BucketHeap {
    pub fn len(&self) -> u32 {
        return self.count;
    }

    pub fn new() -> Self {
        return BucketHeap { data: Vec::new(), count: 0, inserts: 0 }
    }

    pub fn add(&mut self, depth: u8, heuristic: u8, cube: PieceCube, last_move: u8) {
        if (depth as u32 + heuristic as u32) > 255 {
            println!("{}, {}", depth, heuristic);
        }

        let index = (depth + heuristic) as usize;

        if index >= self.data.len() {
            let dif = index - self.data.len() + 1;
            
            for i in 0..dif {
                self.data.push(Vec::new());
            }
        }
        
        self.data[index].push((cube, last_move, depth));

        self.count += 1;

        self.inserts += 1;

        //if self.inserts % (1 << 24) == 0 {
        //    for i in 0..self.data.len() {
        //        print!("{:2} -> {:.2}, ", i, self.data[i].len() as f32 / self.count as f32);
        //    }
//
        //    println!();
        //} 
    }

    pub fn pop(&mut self) -> (PieceCube, u8, u8) {

        self.count -= 1;
        for i in 0..self.data.len() {

            if self.data[i].len() > 0 {
                return self.data[i].pop().unwrap();
            }
        }

        println!("This should never happen");

        return (PieceCube::get_solved(), 18, 255);
    }
}