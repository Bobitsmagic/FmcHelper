pub const FACTORIAL: [u32; 13] = [
    1, 1, 2, 6, 24, 120, 720, 5040, 40320, 362880, 3628800, 39916800, 479001600
];

pub fn get_indexed_perm(n: i32, mut index: u32) -> Vec<u8> {
    let mut buffer_list = vec![0 as u8; n as usize];
    for i in 0..n {
        buffer_list[i as usize] = i as u8;
    }

    let mut indices = vec![0 as u32; n as usize];
    for i in (0..(n as usize)).rev() {
        indices[i]= index / FACTORIAL[i];
        index -= indices[i] * FACTORIAL[i];
    }

    let mut permuation = vec![0 as u8; n as usize];
    for i in (0..(n as usize)).rev() {
        permuation[n as usize - i - 1] = buffer_list[indices[i] as usize];
        buffer_list.remove(indices[i] as usize);
    }

    return permuation;
}

pub fn get_index(perm: &[u8]) -> u32 {
    let mut ret: u32 = 0;

    for i in 0..perm.len() {
        let mut counter = 0;
        let p = perm[i];

        for j in (i + 1)..perm.len() {
            if p > perm[j] {
                counter += 1;
            }
        }

        ret += FACTORIAL[perm.len() - i - 1] * counter;
    }

    return ret;
}