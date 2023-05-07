use std::vec;

#[derive(Clone, Copy)]
pub struct MoveBlocker {
    blocked: u8
}

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