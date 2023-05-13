use crate::cube_representations::index_cube::IndexCube;

pub struct SortedSet {
    is_dirty: bool,
    pub data: Vec<IndexCube>
}

impl SortedSet {
    pub fn new(cap: usize) -> Self {
        return SortedSet { data: Vec::with_capacity(cap), is_dirty: false }
    }

    pub fn len(&self) -> usize {
        return self.data.len();
    }

    pub fn insert(&mut self, cube: IndexCube) {
        self.data.push(cube);
        self.is_dirty = true;
    }

    pub fn remove_duplicates(&mut self) {

        if self.data.len() > 1 && self.is_dirty {
            self.data.sort();
            
            let mut current = self.data[0];
            let mut delete_count = 0;
            
            for i in 1..self.data.len() {
                let cube = self.data[i];
                
                if cube != current {
                    current = cube;
                    self.data[i - delete_count] = cube;
                }
                else {
                    delete_count += 1;
                }
            }
            
            println!("Removed {} from {} elements ({})", delete_count, self.data.len(), delete_count as f32 / self.data.len() as f32);
            self.data.drain((self.data.len() - delete_count)..(self.data.len()));
        }

        self.is_dirty = false;
    }

    pub fn print_edges(&self){
        for i in 0..self.data.len() {
            println!("{}", self.data[i].edge_perm);
        }
    }
}