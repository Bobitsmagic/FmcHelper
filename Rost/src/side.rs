use crate::enums::{CubeColor, u8_to_CubeColor};

//consts
pub const FACE_COUNT: i32 = 8;
pub const BIT_COUNT: i32 = FACE_COUNT * 3;
pub const BIT_MASK: u32 = (1 << 24) - 1;

pub const  STRIPE_MASK: [u32; 4] = [
    511_u32,
    511_u32 << 6,
    511_u32 << 12,
    7_u32 | (63_u32 << 18)];

pub const SQUARE_MASK: [u32; 4] = [
    511_u32 << 3,
    511_u32 << 9,
    511_u32 << 15,
    63_u32 | (7_u32 << 21)];

pub fn rotate_value(value: u32, count: i32) -> u32 {
    ((value << (count * 6)) & BIT_MASK) | (value >> (BIT_COUNT - count * 6))
}

pub const SOLVED_SIDES: [Side; 6] = [
    Side {data: 0},
    Side {data: 1 | (1 << 3) | (1 << 6) | (1 << 9) | (1 << 12) | (1 << 15) | (1 << 18) | (1 << 21) },
    Side {data: 2 | (2 << 3) | (2 << 6) | (2 << 9) | (2 << 12) | (2 << 15) | (2 << 18) | (2 << 21) },
    Side {data: 3 | (3 << 3) | (3 << 6) | (3 << 9) | (3 << 12) | (3 << 15) | (3 << 18) | (3 << 21) },
    Side {data: 4 | (4 << 3) | (4 << 6) | (4 << 9) | (4 << 12) | (4 << 15) | (4 << 18) | (4 << 21) },
    Side {data: 5 | (5 << 3) | (5 << 6) | (5 << 9) | (5 << 12) | (5 << 15) | (5 << 18) | (5 << 21) },
];


#[derive(Eq, PartialEq, Hash, Clone, Copy)]
pub struct Side {
    data: u32,
}

impl Side {
    pub fn new(value: u32) -> Self {
        Side { data: value }
    }    

    pub fn get_color(&self, index: i32) -> CubeColor {
            u8_to_CubeColor(((self.data >> (index * 3)) & 7) as u8)
    }
    pub fn get_color_2d(&self, x: i32, y: i32) -> CubeColor {
        if y == 0 {
            return self.get_color(x);
        }

        if y == 2 {
            return self.get_color(6 - x)
        }
        
        return self.get_color(if x == 0 {7} else {3});
    }

    pub fn set_color(&mut self, index: i32, color: CubeColor) {        
        self.data &= !(7_u32 << index * 3);
        self.data |= (color as u32) << (index * 3);
    }

    pub fn get(&self, index: i32) -> u32 {
        (self.data >> (index * 3)) & 7
    }
    pub fn set(&mut self, index: i32, color: u32) {
        self.data &= !(7_u32 << index * 3);
        self.data |= color << (index * 3);
    }

    pub fn fits_pattern(&self, pattern: Side, bitmask: u32) -> bool {
        (self.data ^ pattern.data) & bitmask == 0_u32
    }

    pub fn is_pair(&self, index: i32) -> bool{
        self.get(index) == self.get((index + 1) % 8)
    }

    pub fn rotate(&mut self, count: i32){
        self.data = ((self.data << (count * 6)) & BIT_MASK) | (self.data >> (BIT_COUNT - count * 6));
    }

    pub fn get_stripe(&self, stripe: i32) -> u32 {
        rotate_value(self.data & STRIPE_MASK[stripe as usize], (4 - stripe) & 3)
    }

    pub fn set_stripe(&mut self, stripe: i32, value: u32){
        self.data &= !STRIPE_MASK[stripe as usize];
        self.data |= rotate_value(value, stripe);
    }

    pub fn print_raw(&self){
        for i in 0..8 {
            print!("{} ", self.get_color(i).to_string());
        }
    }
}