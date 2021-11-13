#[derive(Eq, PartialEq, Hash)]
pub struct BitArray{
    data: u64,
}

impl BitArray {
    pub fn new(value: u64) -> Self {
        BitArray { data: value }
    }

    pub fn empty(&self) -> bool {
        self.data == 0
    }

    pub fn bit_count(&self) -> i32 {
        let mut buffer: u64 = self.data;
        buffer = buffer - ((buffer >> 1) & 0x5555555555555555);
        buffer = (buffer & 0x3333333333333333) + ((buffer >> 2) & 0x3333333333333333);
        ((((buffer + (buffer >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56) as i32
    }

    pub fn get_bit(&self, index: i32) -> bool{
        ((self.data >> index) & 1) != 0
    }

    pub fn set_bit(&mut self, index: i32, value: bool) {
        if value{
            self.data |= 1_u64 << index;
        }
        else {
            self.data &= !(1_u64 << index);
        }
    }
}