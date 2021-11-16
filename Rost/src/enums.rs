use std::string;

#[repr(u8)]
#[derive(Clone, Copy)]
pub enum CubeColor {
    Orange = 0, Red,
    Yellow, White,
    Green, Blue,
    None
}

pub fn u8_to_CubeColor(value: u8) -> CubeColor {{
}
    unsafe{
        std::mem::transmute::<u8, CubeColor>(value)
    }   
}

#[repr(u8)]
#[derive(Clone, Copy)]
pub enum CubeMove {
    L = 0, L2, LP,
    R, R2, RP,
    D, D2, DP,
    U, U2, UP,
    F, F2, FP,
    B, B2, BP,
    None
}

pub fn u8_to_CubeMove(value: u8) -> CubeMove {
    unsafe {
        std::mem::transmute::<u8, CubeMove>(value)
    }
}
impl CubeColor {
    pub fn to_string(&self) -> char {
        match self {
            CubeColor::Orange =>    'O',
            CubeColor::Red =>       'R',
            CubeColor::Yellow =>    'Y',
            CubeColor::White =>     'W',
            CubeColor::Green =>     'G',
            CubeColor::Blue =>      'B',

            _ => 'X',
        }
    }
}