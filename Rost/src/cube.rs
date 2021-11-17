#![allow(dead_code)]

use std::cmp;
use colored::Colorize;
use crate::{enums::{CubeColor, CubeMove, u8_to_CubeColor}, side::{SOLVED_SIDES, Side}, side};

const SIDE_COUNT: i32 = 6;
const LEFT: i32  = 0;
const RIGHT: i32  = 1;
const BOTTOM: i32  = 2;
const TOP: i32  = 3;
const FRONT: i32  = 4;
const BACK: i32  = 5;

//Adj stripes of adj sides (Clockwise order)
struct SideStripe(i32, i32);
const ADJ_STRIPES: [[SideStripe; 4]; 6] = [
    [
        SideStripe(BOTTOM,    1),
        SideStripe(BACK,      3),
        SideStripe(TOP,       3),
        SideStripe(FRONT,     3)
    ],
    [
        SideStripe(BOTTOM,    3),
        SideStripe(FRONT,     1),
        SideStripe(TOP,       1),
        SideStripe(BACK,      1)
    ],
    [
        SideStripe(LEFT,      3),
        SideStripe(FRONT,     2),
        SideStripe(RIGHT,     1),
        SideStripe(BACK,      0)
    ],
    [
        SideStripe(LEFT,      1),
        SideStripe(BACK,      2),
        SideStripe(RIGHT,     3),
        SideStripe(FRONT,     0)
    ],
    [
        SideStripe(LEFT,      2),
        SideStripe(TOP,       2),
        SideStripe(RIGHT,     2),
        SideStripe(BOTTOM,    2)
    ],
    [
        SideStripe(LEFT,      0),
        SideStripe(BOTTOM,    0),
        SideStripe(RIGHT,     0),
        SideStripe(TOP,       0)
    ],
];

struct SideSquare(i32, i32, i32, i32);

const ORANGE_BLOCKS: [SideSquare; 4] = [
    SideSquare(TOP,       3, BACK,    2),
    SideSquare(TOP,       2, FRONT,   3),
    SideSquare(BOTTOM,    1, FRONT,   2),
    SideSquare(BOTTOM,    0, BACK,    3)
];
const RED_BLOCKS: [SideSquare; 4] = [
    SideSquare(BOTTOM,    3, BACK,    0),
    SideSquare(BOTTOM,    2, FRONT,   1),
    SideSquare(TOP,       1, FRONT,   0),
    SideSquare(TOP,       0, BACK,    1)
];

const ADJ_EDGES: [[i32; 6]; 6] = [
    //Left
    [ -1, -1, 7, 3, 5, 1 ],
    //Right
    [ -1, -1, 3, 7, 5, 1 ],

    //Bottom
    [ 3, 7, -1, -1, 5, 1 ],
    //Top
    [ 7, 3, -1, -1, 5, 1 ],

    //Front
    [ 7, 3, 5, 1, -1, -1 ],
    //Back
    [ 7, 3, 1, 5, -1, -1 ],
];

const ADJ_CORNERS: [[i32; 12]; 6] = [
        //Left						removed cuz redundancy
        [ -1, -1, -1, -1, 6, 0, 4, 2, -2, -2, -2, -2 ],
        //Right
        [ -1, -1, -1, -1, 4, 2, 6, 0, -2, -2, -2, -2 ],

        //Bottom
        [ 4, 2, 6, 0, -1, -1, -1, -1, -2, -2, -2, -2 ],
        //Top
        [ 6, 0, 4, 2, -1, -1, -1, -1, -2, -2, -2, -2 ],

        //Front
        [ 6, 0, 4, 2, -2, -2, -2, -2, -1, -1, -1, -1],
        //Back
        [ 0, 6, 2, 4, -2, -2, -2, -2, -1, -1, -1, -1],
];

#[derive(Eq, PartialEq, Hash, Clone)]
pub struct Cube{
    sides: [Side; 6],
}

impl Cube {
    pub fn new() -> Self {
        Cube{sides: [
                SOLVED_SIDES[0],
                SOLVED_SIDES[1],
                SOLVED_SIDES[2],
                SOLVED_SIDES[3],
                SOLVED_SIDES[4],
                SOLVED_SIDES[5],
            ]}
    }

    pub fn reset(&mut self){
        for i in 0..6 {
            self.sides[i] = SOLVED_SIDES[i];
        }
    }

    pub fn make_move(&mut self, m: CubeMove){
        let side: i32 = (m as i32) / 3;

        let move_type: i32 = m as i32;
        self.sides[side as usize].rotate((move_type % 3) + 1);

        
        let stripes = &ADJ_STRIPES[side as usize];
        match move_type  {
            0 => {
                let buffer = self.sides[stripes[0].0 as usize].get_stripe(stripes[0].1);

                self.sides[stripes[0].0 as usize].set_stripe(stripes[0].1,
                    self.sides[stripes[3].0 as usize].get_stripe(stripes[3].1));

                self.sides[stripes[3].0 as usize].set_stripe(stripes[3].1,
                    self.sides[stripes[2].0 as usize].get_stripe(stripes[2].1));

                self.sides[stripes[2].0 as usize].set_stripe(stripes[2].1,
                    self.sides[stripes[1].0 as usize].get_stripe(stripes[1].1));

                self.sides[stripes[1].0 as usize].set_stripe(stripes[1].1, buffer);
            },
            1 => {
                let mut buffer = self.sides[stripes[0].0 as usize].get_stripe(stripes[0].1);
                self.sides[stripes[0].0 as usize].set_stripe(stripes[0].1,
                    self.sides[stripes[2].0 as usize].get_stripe(stripes[2].1));
                self.sides[stripes[2].0 as usize].set_stripe(stripes[2].1, buffer);
    
                buffer = self.sides[stripes[1].0 as usize].get_stripe(stripes[1].1);
                self.sides[stripes[1].0 as usize].set_stripe(stripes[1].1,
                    self.sides[stripes[3].0 as usize].get_stripe(stripes[3].1));
                self.sides[stripes[3].0 as usize].set_stripe(stripes[3].1, buffer);
            },
            3 => {
                let buffer = self.sides[stripes[0].0 as usize].get_stripe(stripes[0].1);

                self.sides[stripes[0].0 as usize].set_stripe(stripes[0].1,
                    self.sides[stripes[1].0 as usize].get_stripe(stripes[1].1));

                self.sides[stripes[1].0 as usize].set_stripe(stripes[1].1,
                    self.sides[stripes[2].0 as usize].get_stripe(stripes[2].1));

                self.sides[stripes[2].0 as usize].set_stripe(stripes[2].1,
                    self.sides[stripes[3].0 as usize].get_stripe(stripes[3].1));

                self.sides[stripes[3].0 as usize].set_stripe(stripes[3].1, buffer);
            },

            _ => {},
        }
    }

    pub fn is_solved(&self) -> bool {
        for i in 0..6 {
            if self.sides[i] != SOLVED_SIDES[i] {
                return  false;
            }
        }

        true
    }

    pub fn get_edge_color(&self, side1: i32, side2: i32) -> CubeColor {
        self.sides[side1 as usize].get_color(ADJ_EDGES[side1 as usize][side2 as usize])
    }
    pub fn set_edge_color(&mut self, side1: i32, side2: i32, color: CubeColor){
        self.sides[side1 as usize].set_color(ADJ_EDGES[side1 as usize][side2 as usize], color);
    }

    pub fn get_corner_color(&self, side1: i32, side2: i32, side3: i32) -> CubeColor {
        let lower: i32 = cmp::min(side2, side3);
        let higher: i32 = cmp::max(side2, side3);

        self.sides[side1 as usize].get_color(ADJ_CORNERS[side1 as usize][(lower * 2 + (higher & 1)) as usize])
    }

    pub fn set_corner_color(&mut self, side1: i32, side2: i32, side3: i32, color: CubeColor) {
        let lower: i32 = cmp::min(side2, side3);
        let higher: i32 = cmp::max(side2, side3);

        self.sides[side1 as usize].set_color(ADJ_CORNERS[side1 as usize][(lower * 2 + (higher & 1)) as usize], color);
    }

    pub fn edge_is_oriented(&self, side1: i32, side2: i32) -> bool {
        debug_assert!(side1 < side2);

        let c1 = self.get_edge_color(side1, side2) as u8;
        let c2 = self.get_edge_color(side2, side1) as u8;

        //if contains white or yellow
        if c1 / 2 == 1 || c2 / 2 == 1 {
            if side1 / 2 == 0 {
                c2 / 2 == 1
            }
            else {
                c1 / 2 == 1
            }
        }
        else {
            if side1 / 2 == 0 {
                c2 / 2 == 2
            }
            else {
                c1 / 2 == 2
            }
        }
    }

    pub fn corner_orientation(&self, side1: i32, side2: i32, side3: i32) -> i32 {
        debug_assert!(side1 < side2 && side2 < side3);

        if self.get_corner_color(side1, side2, side3) as i32 / 2 == 0 {
            return 0;
        }

        if self.get_corner_color(side2, side1, side3) as i32 / 2 == 0 {
            return 1;
        }

        2
    }

    pub fn get_side_view(&self) -> String{
        let mut s = " ".repeat(5) + &"-".repeat(7) + "\n";

        for y in 0..3 {
            s .push_str(&" ".repeat(5));
            s .push('|');

            for x in 0..3 {
                if x == 1 && y == 1 {
                    s.push(CubeColor::Blue.to_string());
                }
                else {
                    s.push(self.sides[BACK as usize].get_color_2d(x, y).to_string());
                }

                s.push(if x == 2 {'|'} else {' '});
            }
            s.push('\n');
        }


        s.push_str(&"-".repeat(24));
        s.push('\n');
        let inidces: [u8; 4] = [0, 3, 1, 2];

        for y in 0..3 {
            for index in inidces{
                for x in 0..3 {
                    if x == 1 && y == 1 {
                        s.push(u8_to_CubeColor(index).to_string())
                    }
                    else {
                        s.push(self.sides[index as usize].get_color_2d(x, y).to_string());
                    }

                    s.push_str(if x == 2 {"|"} else {" "});
                }
            }

            s.push('\n');
        }

        s.push_str(&"-".repeat(24));
        s.push('\n');
        for y in 0..3 {
            s .push_str(&" ".repeat(5));
            s .push_str("|");

            for x in 0..3 {
                if x == 1 && y == 1 {
                    s.push(CubeColor::Green.to_string());
                }
                else {
                    s.push(self.sides[FRONT as usize].get_color_2d(x, y).to_string());
                }

                s.push(if x == 2 {'|'} else {' '});
            }
            s.push('\n');
        }

        s
    }

    pub fn print_side_view(&self){
        let s = self.get_side_view();

        for c in s.chars() {
            match c {
                'O' => print!("{}", "O".truecolor(255, 127, 0)), 
                'R' => print!("{}", "R".red()), 
                'Y' => print!("{}", "Y".yellow()), 
                'W' => print!("{}", "W".white()), 
                'G' => print!("{}", "G".green()), 
                'B' => print!("{}", "B".blue()), 

                _ => print!("{}", c.to_string().bright_black()) 
            }
        }
    }

    pub fn print_raw(&self){
        for i in 0..6 {
            self.sides[i].print_raw();
            println!();
        }
    }
}