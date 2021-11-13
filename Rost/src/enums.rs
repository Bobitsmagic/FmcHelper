#[repr(u8)]
#[derive(Clone, Copy)]
pub enum CubeColor {
    Orange = 0, Red,
    Yellow, White,
    Green, Blue,
    None
}

pub unsafe fn CubeColor_to_u8(color: CubeColor) -> u8 {
    std::mem::transmute::<CubeColor, u8>(color)
}

pub unsafe fn u8_to_CubeColor(value: u8) -> CubeColor {
    std::mem::transmute::<u8, CubeColor>(value)
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

pub unsafe fn CubeMove_to_u8(color: CubeMove) -> u8 {
    std::mem::transmute::<CubeMove, u8>(color)
}

pub unsafe fn u8_to_CubeMove(value: u8) -> CubeMove {
    std::mem::transmute::<u8, CubeMove>(value)
}