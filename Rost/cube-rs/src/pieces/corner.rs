pub struct Corner {
    colors: [u8; 3]
}

impl Corner {
    pub fn new(color1: u8, color2: u8, color3: u8) -> Self {
        return Corner {colors: [color1, color2, color3]};
    }

    pub fn print_corner(&self) {
        println!("{:?}", self.colors);
    }
}

#[derive(Eq, PartialEq)]
pub struct SortedCorner {
    colors: [u8; 3]
}

const SORTED_CORNERS: [SortedCorner; 8] = [
    SortedCorner {colors: [0, 2, 4]},
    SortedCorner {colors: [0, 2, 5]},
    SortedCorner {colors: [0, 3, 4]},
    SortedCorner {colors: [0, 3, 5]},
    SortedCorner {colors: [1, 2, 4]},
    SortedCorner {colors: [1, 2, 5]},
    SortedCorner {colors: [1, 3, 4]},
    SortedCorner {colors: [1, 3, 5]},
];

impl SortedCorner {
    pub fn new(color1: u8, color2: u8, color3: u8) -> Self {
        let mut sc = [color1, color2, color3];
        sc.sort();

        return SortedCorner{colors: sc};
    }

    pub fn get_index(&self) -> i32 {
        for i in 0..8 {
            if SORTED_CORNERS[i] == *self {
                return i as i32;
            }
        }

        panic!("This should never happen");
    }

    pub fn print_corner(&self) {
        println!("{:?}", self.colors);
    }
}

