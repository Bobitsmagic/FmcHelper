#import "template.typ": *

#show: project.with(
  title: "Cube Algorithm",
  authors: (
    "Bobitsmagic",
  ),
)

= Move definitions Vocabulary
$M = {$ *L*, *L'*, *L2*, *R*, *R'*, *R2*, *D*, *D'*, *D2*, *U*, *U'*, *U2*, *B*, *B'*, *B2*, *F*, *F'*, *F2* $}$
- Edge, Corner, Center, Tile, Face
= IndexCube
We are in need of class that represents a 3 by 3 rubiks cube. The IndexCube stores the permuation and orientation state of the corners and edges seperately.

== Edge Permuation

The position of all edges is a permuation of 12 elements. Instead of using 12 integer to store the position of every edge we use one integer that stores the lexicographic index of the permuation. There are $ceil(log_2(12!)) = 29$ bits needed to store all possible indices and we use a 32 bit integer to store this value. We use the family of bijective functions $E_i: {0, 1, dots, 11} arrow {0, 1, dots, 11}$ where $i in {0, 1, dots, 12! - 1}$ is the 0-based lexicographic index of the permuation that $E_i$ represents. 

A 12 element permuation will be represented by the following 2 row matrix:
$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
x_0, x_1, x_2, x_3, x_4, x_5, x_6, x_7, x_8, x_9, x_10, x_11) $. 

Every edge has to correspond to an index between 0 and 11 now. We sort the edges by their X, Y and Z components on a right handed coordinate system with the green facing towards positive Z and white facing towards positive Y. In the following cube net the resulting indices can be seen. 

#let n = none
#let w = white
#let o = orange
#let g = green
#let b = blue
#let r = red
#let y = yellow
#let c = gray

#let emptySquare = square(fill: none, stroke: none, size: 1cm)
#let tile(color, text) = (
  square(fill: color, stroke: black, size:  1cm)[#set align(center); #set align(horizon); #text]
) 

//Solved edges with indices
#grid(
  columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
  emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(w, "6"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, "3"),   tile(w, ""),    tile(w, "11"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(w, "7"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(c, ""),    tile(o, "3"),   tile(c, ""),
  tile(c, ""),    tile(g, "7"),   tile(c, ""),
  tile(c, ""),    tile(r, "11"),  tile(c, ""),
  tile(c, ""),    tile(b, "6"),   tile(c, ""),
  
  tile(o, "1"),   tile(o, ""),    tile(o, "2"),
  tile(g, "2"),   tile(g, ""),    tile(g, "10"),
  tile(r, "10"),  tile(r, ""),    tile(r, "9"),
  tile(b, "9"),   tile(b, ""),    tile(b, "1"),
  
  tile(c, ""),    tile(o, "0"),   tile(c, ""),
  tile(c, ""),    tile(g, "5"),   tile(c, ""),
  tile(c, ""),    tile(r, "8"),   tile(c, ""),
  tile(c, ""),    tile(b, "4"),   tile(c, ""),
  
  emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(y, "5"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, "0"),   tile(y, ""),    tile(y, "8"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(y, "4"),   tile(c, ""),
)
 
The specific selection of indices will reduce the amount of space needed to store a matrix in discussed in section *KEK*.
After the move *L* (90° clockwise rotation of the orange side) the permuation matrix has the following values:
$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
#text(red)[1], #text(red)[3], #text(red)[0], #text(red)[2], 4, 5, 6, 7, 8, 9, 10, 11) $. 

//Edge Indices after L move
#grid(
  columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
  emptySquare, emptySquare, emptySquare, 
  tile(c, ""),  tile(w, "6"), tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(b, "1"), tile(w, ""),  tile(w, "11"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),  tile(w, "7"), tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(c, ""),  tile(o, "1"),   tile(c, ""),    
  tile(c, ""),  tile(g, "7"),   tile(c, ""),
  tile(c, ""),  tile(r, "11"),  tile(c, ""),
  tile(c, ""),  tile(b, "6"),   tile(c, ""),
  
  tile(o, "0"), tile(o, ""),    tile(o, "3"),
  tile(w, "3"), tile(g, ""),    tile(g, "10"),
  tile(r, "10"),tile(r, ""),    tile(r, "9"),
  tile(b, "9"), tile(b, ""),    tile(y, "0"),
  
  tile(c, ""),  tile(o, "2"),   tile(c, ""),
  tile(c, ""),  tile(g, "5"),   tile(c, ""),
  tile(c, ""),  tile(r, "8"),   tile(c, ""),
  tile(c, ""),  tile(b, "4"),   tile(c, ""),
  
  emptySquare, emptySquare, emptySquare, 
  tile(c, ""),  tile(y, "5"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(g, "2"), tile(y, ""),    tile(y, "8"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),  tile(y, "4"),   tile(c, ""),
)

There are 2 ways to define the relation of the state of the cube and the permutation matrix. The matrix above describes the new position of each edge. The edge 0 (orange-yellow) is now at postion 1, where the orange-blue edge belongs. The edge 1 is now positoned where edge 3 would be in a solved cube. 

The other interpretation would be the inverse of this Permuation. 

$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
#text(red)[2], #text(red)[0], #text(red)[3], #text(red)[1], 4, 5, 6, 7, 8, 9, 10, 11) $. 

Here the bottom row describes which edge is now at this position. So At position 0 there is edge 2, at position 1 there is edge 0. We will use the first interpretation as it makes composition of functions more intuitive. 

The function $E_(47174400)$ represents permuation after the Move *L*. The lexicographic index of any permuation $pi: {0, ..., N - 1} arrow {0, ..., N - 1}$ of $N$ elments can be computed by the following formular:
$ sum_(i = 0)^(N - 1)((N - 1 - i)! dot.op sum_(j = i + 1)^(N - 1) cases(1 "if" pi(i) > pi(j), 0 "otherwise")) $

== Edge orientation
Every edge has 2 possible orientations. To represent all edge orientations, 12 bits are needed which are stored into a single 16 bit integer. To make the edge orientation independent from the edge permuation state the $i$-th value in the bit vector corresponds to the orientation of the edge at position $i$. We write an edge orientation similar to a permuation with a 2 row matrix. 

$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) $.

A 0 would correspond to an oriented edge and a 1 to a flipped edge. The superflip has all edges not oriented but in the right position.  

$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1) $.

//superflip edge indices
#grid(
  columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
  emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(b, "6"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(o, "3"),   tile(w, ""),    tile(r, "11"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(g, "7"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(c, ""),    tile(w, "3"),   tile(c, ""),
  tile(c, ""),    tile(w, "7"),   tile(c, ""),
  tile(c, ""),    tile(w, "11"),  tile(c, ""),
  tile(c, ""),    tile(w, "6"),   tile(c, ""),
  
  tile(b, "1"),   tile(o, ""),    tile(g, "2"),
  tile(o, "2"),   tile(g, ""),    tile(r, "10"),
  tile(g, "10"),  tile(r, ""),    tile(b, "9"),
  tile(r, "9"),   tile(b, ""),    tile(o, "1"),
  
  tile(c, ""),    tile(y, "0"),   tile(c, ""),
  tile(c, ""),    tile(y, "5"),   tile(c, ""),
  tile(c, ""),    tile(y, "8"),   tile(c, ""),
  tile(c, ""),    tile(y, "4"),   tile(c, ""),
  
  emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(g, "5"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(o, "0"),   tile(y, ""),    tile(r, "8"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(b, "4"),   tile(c, ""),
)

It is trivial to check whether an edge is oriented or not if it is at the original position. In any other position we have to define a logic that determines whether the edge is oriented or flipped. We choose one of the 3 main axis we base our orientation on. If we choose the Z-axis we can define the orientation of an edge as follows:
- Move the edge $i$ to position $i$ by without using the moves *F*, *F'*, *B* or *B'* (90° rotation on the Z-axis).
- Check whether the edge is oriented.

In practice this can be checked faster by choosing a tile for a certain edge and storing all faces this tile can be on by using our restricted set of moves. The orientation can be defined similary for the other 2 axis but we will use the Z-Axis as the base for our orientations.

== Corner Permuation
The position of all corners is a permuation of 8 elements. We need $ceil(log_2(8!)) = 16$ bits to store the lexicographic index of a corner permuation. Similar to the edges we use the family of bijective functions $C_i: {0, 1, dots, 7} arrow {0, 1, dots, 7}$ where $i in {0, 1, dots, 7! - 1}$ is the 0-based lexicographic index of the permuation that $C_i$ represents. Every corner and every position corrosponds to an index between 0 and 7 now. All corners are sorted by their X, Y and Z coordinates as seen in the following cube net.

#pagebreak() // !!! remove later !!!

#grid(
  columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
  emptySquare, emptySquare, emptySquare, 
  tile(w, "3"),    tile(c, ""),   tile(w, "7"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),   tile(w, ""),    tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, "2"),    tile(c, ""),   tile(w, "6"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(o, "3"),    tile(c, ""),   tile(o, "2"),
  tile(g, "2"),    tile(c, ""),   tile(g, "6"),
  tile(r, "6"),    tile(c, ""),  tile(r, "7"),
  tile(b, "7"),    tile(c, ""),   tile(b, "3"),
  
  tile(c, ""),    tile(o, ""),    tile(c, ""),
  tile(c, ""),    tile(g, ""),    tile(c, ""),
  tile(c, ""),    tile(r, ""),    tile(c, ""),
  tile(c, ""),    tile(b, ""),    tile(c, ""),
  
  tile(o, "1"),    tile(c, ""),   tile(o, "0"),
  tile(g, "0"),    tile(c, ""),   tile(g, "4"),
  tile(r, "4"),    tile(c, ""),   tile(r, "5"),
  tile(b, "5"),    tile(c, ""),   tile(b, "1"),
  
  emptySquare, emptySquare, emptySquare, 
  tile(y, "0"),    tile(c, ""),   tile(y, "4"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),   tile(y, ""),    tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, "1"),    tile(c, ""),   tile(y, "5"),
)

As with the edges the $i$-th entry in the permutation matrix describes the position to which the $i$-th corner went.

== Corner orientation
Every corner has 3 diffrent orientations. To represent all corner orientations $ceil(log_2(3^8)) = 13$ bit are needed which are stored into a single $16$ bit integer. The orientation of a corner is defined as follows:
- Every axis gets a corresponding index from $0$ to $2$
- We choose a pair of non adjacent colors called base colors  
- Every corner contains a tile with exactly one of our base colors
- The orientation of a corner is the index corresponding to the axis our tile with the base color is facing

We choose orange and red as our base colors and our X, Y and Z-axis are numbered $0, 1$ and $2$ respectively.



#let color_viewer(..elements) = (
  grid(
    columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
    ..elements.pos().map(color => {
      if color == none {
        rect(fill: none, stroke: none, height: 1cm, width: 1cm)
      } else {
        rect(fill: color, stroke: black, height: 1cm, width: 1cm)
      }
    })
  )
)