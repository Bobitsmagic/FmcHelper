mod bitarray;
mod side;
mod enums;
mod cube;
use colored::*;

use crate::{cube::Cube, enums::CubeMove};

fn main() {
    println!("Hello, world!");

    println!("{:?}", side::SQUARE_MASK);
    println!("{} {} !", "it".green(), "works".blue().bold());

    let mut kek = Cube::new();
    
    kek.print_side_view();
    kek.print_raw();
    kek.make_move(CubeMove::R);

    kek.print_side_view();

    kek.print_raw();
}
