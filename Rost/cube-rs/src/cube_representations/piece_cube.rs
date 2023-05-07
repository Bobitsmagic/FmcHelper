const MAX_STATE : u8 = 24;

#[derive(Eq, PartialEq)]
struct CornerState {
    state: u8
}

impl CornerState {
    pub fn new(value: u8) -> Self {
        return CornerState {state: value }
    }

    pub fn new_pos_orient(pos: i32, orient: i32) -> Self {
        return CornerState {state: (pos * 3 + orient) as u8}
    }

    pub fn position(self) -> i32 {
        return (self.state as i32) / 3;
    }

    pub fn orientation(self) -> i32 {
        return (self.state as i32) % 3;
    }
}

#[derive(Eq, PartialEq)]
struct EdgeState {
    state: u8
}

impl EdgeState {
    pub fn new(value: u8) -> Self {
        return EdgeState {state: value }
    }

    pub fn new_pos_orient(pos: i32, orient: i32) -> Self {
        return EdgeState {state: (pos * 2 + orient) as u8}
    }

    pub fn position(self) -> i32 {
        return (self.state as i32) / 2;
    }

    pub fn orientation(self) -> i32 {
        return (self.state as i32) % 2;
    }
}

const SOLVED_CORNERS: [CornerState; 8] = [ 
    CornerState{state: 0},
    CornerState{state: 3},
    CornerState{state: 6},
    CornerState{state: 9},
    CornerState{state: 12},
    CornerState{state: 15},
    CornerState{state: 18},
    CornerState{state: 21}
];

const SOLVED_EDGES: [EdgeState; 12] = [ 
    EdgeState{state: 0},
    EdgeState{state: 2},
    EdgeState{state: 4},
    EdgeState{state: 6},
    EdgeState{state: 8},
    EdgeState{state: 10},
    EdgeState{state: 12},
    EdgeState{state: 14},
    EdgeState{state: 16},
    EdgeState{state: 18},
    EdgeState{state: 20},
    EdgeState{state: 22}
];

pub struct PieceCube {
    corners: [CornerState; 8],
    edges: [EdgeState; 12]
}

impl PieceCube {
    pub fn get_solved() -> Self {
        return PieceCube { corners: SOLVED_CORNERS, edges: SOLVED_EDGES };
    }
    pub fn new(val: i32) -> Self {
        let mut ret = PieceCube { corners: SOLVED_CORNERS, edges: SOLVED_EDGES };
        ret.corners[0].state = val as u8;

        return ret;
    }

    pub fn is_solved(&self) -> bool {
        return self.corners == SOLVED_CORNERS && 
            self.edges == SOLVED_EDGES;
    }
}