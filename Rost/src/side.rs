use crate::enums::{self, CubeColor, CubeColor_to_u8, u8_to_CubeColor};

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

#[derive(Eq, PartialEq, Hash)]
pub struct Side {
    data: u32,
}

//statics


impl Side {
    pub fn new(value: u32) -> Self {
        Side { data: value }
    }    

    pub fn get_color(&self, index: i32) -> CubeColor {
        unsafe {
            u8_to_CubeColor(((self.data >> (index * 3)) & 7) as u8)
        }
    }
    pub fn set_color(&mut self, index: i32, color: CubeColor) {
        unsafe {
            self.data &= !(7_u32 << index * 3);
			self.data |= (CubeColor_to_u8(color) as u32) << (index * 3);
        }
    }

    pub fn get(&self, index: i32) -> u32 {
        (self.data >> (index * 3)) & 7
    }
    pub fn set(&mut self, index: i32, color: u32) {
        self.data &= !(7_u32 << index * 3);
        self.data |= color << (index * 3);
    }

    pub fn initWithColor(color: CubeColor) -> Self {
        let mut ret: Side = Side { data: 0 };

        for i in 0..FACE_COUNT {
            ret.set_color(i, color);
        }

        ret
    }

    pub fn fits_pattern(&self, pattern: Side, bitmask: u32) -> bool {
        (self.data ^ pattern.data) == 0_u32
    }

    pub fn is_pair(&self, index: i32) -> bool{
        self.get(index) == self.get((index + 1) % 8)
    }

    pub fn rotate(&mut self, count: i32){
        self.data = ((self.data << (count * 6)) & BIT_MASK) | (self.data >> (BIT_COUNT - count * 6));
    }

    pub fn get_strpe(&self, stripe: i32) -> u32 {
        rotate_value(self.data & STRIPE_MASK[stripe as usize], (4 - stripe) & 3)
    }

    pub fn set_stripe(&mut self, stripe: i32, value: u32){
        self.data &= !STRIPE_MASK[stripe as usize];
        self.data |= rotate_value(value, stripe);
    }
}