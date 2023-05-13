pub struct Edge {
    colors: [u8; 2]
}

impl Edge {
    pub fn new(color1: u8, color2: u8) -> Self {
        return Edge {colors: [color1, color2]};
    }

    pub fn print_edge(&self) {
        println!("{:?}", self.colors);
    }
}

#[derive(Eq, PartialEq)]
pub struct SortedEdge {
    colors: [u8; 2]
}

const SORTED_EDGES: [SortedEdge; 12] = [
    SortedEdge {colors: [0, 2]},
    SortedEdge {colors: [0, 3]},
    SortedEdge {colors: [0, 4]},
    SortedEdge {colors: [0, 5]},
    
    SortedEdge {colors: [1, 2]},
    SortedEdge {colors: [1, 3]},
    SortedEdge {colors: [1, 4]},
    SortedEdge {colors: [1, 5]},

    SortedEdge {colors: [2, 4]},
    SortedEdge {colors: [2, 5]},
    SortedEdge {colors: [3, 4]},
    SortedEdge {colors: [3, 5]},
];

impl SortedEdge {
    pub fn new(color1: u8, color2: u8) -> Self {
        let mut sc = [color1, color2];
        sc.sort();

        return SortedEdge{colors: sc};
    }

    pub fn get_index(&self) -> i32 {
        for i in 0..12 {
            if SORTED_EDGES[i] == *self {
                return i as i32;
            }
        }
        
        panic!("This should never happen");
    }

    pub fn print_edge(&self) {
        println!("{:?}", self.colors);
    }
}

