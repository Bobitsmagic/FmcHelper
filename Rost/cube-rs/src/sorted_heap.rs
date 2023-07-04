use crate::{cube_representations::piece_cube::PieceCube, move_blocker::MoveBlocker};

pub struct SortedHeap {
    data: Vec<Vec<Vec<(PieceCube, MoveBlocker, u8)>>>,
    count: u32,
    inserts: u64
}

impl SortedHeap {
    pub fn len(&self) -> u32 {
        return self.count;
    }

    pub fn new() -> Self {
        return SortedHeap { data: Vec::new(), count: 0, inserts: 0 }
    }

    pub fn add(&mut self, depth: u8, heuristic: u8, disagreement: u8, cube: PieceCube, mb: MoveBlocker) {
        let index = (depth + heuristic) as usize;
        let dif_index = disagreement as usize;

        if depth + heuristic > 17 {
            return;
        }

        if index >= self.data.len() {
            let dif = index - self.data.len() + 1;
            
            for i in 0..dif {
                self.data.push(Vec::new());
            }
        }
        

        if dif_index >= self.data[index].len() {
            let dif = dif_index - self.data[index].len() + 1;
            
            for i in 0..dif {
                self.data[index].push(Vec::new());
            }
        }

        self.data[index][dif_index].push((cube, mb, depth));

        self.count += 1;
    }

    pub fn pop(&mut self) -> (PieceCube, MoveBlocker, u8) {

        self.count -= 1;
        for i in 0..self.data.len() {

            if self.data[i].len() > 0 {
                for j in (0..self.data[i].len()).rev() {
                    if self.data[i][j].len() > 0 {
                        return self.data[i][j].pop().unwrap();
                    }
                }
            }
        }

        println!("This should never happen");

        return (PieceCube::get_solved(), MoveBlocker::new(), 255);
    }
}
