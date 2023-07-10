#[derive(Clone, Copy)]
pub struct MoveBlocker {
    blocked: u8
}

pub const POSSIBLE_MOVES: [&[u8]; 7] = [
    &[         3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17], //L
    &[                  6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17], //R
    
    &[0, 1, 2, 3, 4, 5,          9, 10, 11, 12, 13, 14, 15, 16, 17], //D
    &[0, 1, 2, 3, 4, 5,                     12, 13, 14, 15, 16, 17], //U

    &[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11,             15, 16, 17], //B
    &[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11,                       ], //F
    &[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17], //None
];

pub const POSSIBLE_MOVES_EO: [&[u8]; 7] = [
    &[         3, 4, 5, 6, 7, 8, 9, 10, 11,     13,         16    ], //L
    &[                  6, 7, 8, 9, 10, 11,     13,         16    ], //R
    
    &[0, 1, 2, 3, 4, 5,          9, 10, 11,     13,         16    ], //D
    &[0, 1, 2, 3, 4, 5,                         13,         16    ], //U

    &[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11,                 16    ], //B
    &[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11,                       ], //F
    
    &[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11,     13,         16    ], //None
];

impl MoveBlocker {
    pub fn new () -> Self {
        return MoveBlocker {blocked: 0};
    }
    pub fn new_move(src: MoveBlocker, m: u8) -> Self {
        let mut ret = src.clone();
        ret.update_blocked(m);

        return ret;
    }
    pub fn side_blocked(&self, index: u8) -> bool {
        return ((self.blocked >> index) & 1) != 0
    }
    pub fn move_blocked(&self, m: u8) -> bool {
        return self.side_blocked(m / 3);
    }
    pub fn update_blocked(& mut self, m: u8) {
        let side = m / 3;
        self.blocked |= 1 << side;

        if side % 2 == 1 {
            self.blocked |= 1 << (side - 1);
        }

        for i in 0..6 {
            if i / 2 != side / 2 {
                self.blocked &= !(1 << i);
            }
        }
    }

    pub fn insert_possible_moves(&self, list: &mut Vec<u8>) {
        list.clear();
        for m in 0..18 {
            if !self.move_blocked(m){
                list.push(m);
            }
        }
    }
}