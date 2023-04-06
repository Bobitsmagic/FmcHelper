#import "template.typ": *

#show: project.with(
  title: "Cube Algorithm",
  authors: (
    "Bobitsmagic",
  ),
)

= Move definitions
$M = {$ *L*, *L'*, *L2*, *R*, *R'*, *R2*, *D*, *D'*, *D2*, *U*, *U'*, *U2*, *B*, *B'*, *B2*, *F*, *F'*, *F2* $}$

= IndexCube
We are in need of class that represents a 3 by 3 rubiks cube. The IndexCube stores the permuation and orientation state of the corners and edges seperately.

== Edge Permuation

The position of all edges is a permuation of 12 elements. Instead of using 12 integer to store the position of every edge we use one integer that stores the lexicographic index of the permuation. There are $ceil(log_2(12!)) = 29$ bits needed to store all possible indices and we use a 32 bit integer to store this value. We use the family of bijective functions $E_i: {0, 1, dots, 11} arrow {0, 1, dots, 11}$ where $i in {0, 1, dots, 12! - 1}$ is the lexicographic index of the permuation that $E_i$ represents. 

A 12 element permuation will be represented by the following 2 row matrix:
$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
x_0, x_1, x_2, x_3, x_4, x_5, x_6, x_7, x_8, x_9, x_10, x_11) $. 

Every edge has to corrospond to an index between 0 and 11 now. We sort the edges by their $x, y$ and $z$ components on a right handed coordinate system with the green facing towards positive $z$ and white facing towards positive $y$. In the following cube net the resulting indices can be seen. 

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
  tile(w, ""),    tile(w, "6"),   tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, "3"),   tile(w, ""),    tile(w, "11"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, ""),    tile(w, "7"),   tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(o, ""),    tile(o, "3"),   tile(o, ""),
  tile(g, ""),    tile(g, "7"),   tile(g, ""),
  tile(r, ""),    tile(r, "11"),  tile(r, ""),
  tile(b, ""),    tile(b, "6"),   tile(b, ""),
  
  tile(o, "1"),   tile(o, ""),    tile(o, "2"),
  tile(g, "2"),   tile(g, ""),    tile(g, "10"),
  tile(r, "10"),  tile(r, ""),    tile(r, "9"),
  tile(b, "9"),   tile(b, ""),    tile(b, "1"),
  
  tile(o, ""),    tile(o, "0"),   tile(o, ""),
  tile(g, ""),    tile(g, "5"),   tile(g, ""),
  tile(r, ""),    tile(r, "8"),   tile(r, ""),
  tile(b, ""),    tile(b, "4"),   tile(b, ""),
  
  emptySquare, emptySquare, emptySquare, 
  tile(y, ""),    tile(y, "5"),   tile(y, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, "0"),   tile(y, ""),    tile(y, "8"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, ""),    tile(y, "4"),   tile(y, ""),
)

The specific selection of indices will reduce the amount of space needed to store a matrix in discussed in section *KEK*.
After the move *L* (90° clockwise rotation of the orange side) the permuation matrix has the following values:
$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
#text(red)[1], #text(red)[3], #text(red)[0], #text(red)[2], 4, 5, 6, 7, 8, 9, 10, 11) $. 

//Edge Indices after L move
#grid(
  columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
  emptySquare, emptySquare, emptySquare, 
  tile(b, ""),  tile(w, "6"), tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(b, "1"), tile(w, ""),  tile(w, "11"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(b, ""),  tile(w, "7"), tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(o, ""),  tile(o, "1"),   tile(o, ""),    
  tile(w, ""),  tile(g, "7"),   tile(g, ""),
  tile(r, ""),  tile(r, "11"),  tile(r, ""),
  tile(b, ""),  tile(b, "6"),   tile(y, ""),
  
  tile(o, "0"), tile(o, ""),    tile(o, "3"),
  tile(w, "3"), tile(g, ""),    tile(g, "10"),
  tile(r, "10"),tile(r, ""),    tile(r, "9"),
  tile(b, "9"), tile(b, ""),    tile(y, "0"),
  
  tile(o, ""),  tile(o, "2"),   tile(o, ""),
  tile(w, ""),  tile(g, "5"),   tile(g, ""),
  tile(r, ""),  tile(r, "8"),   tile(r, ""),
  tile(b, ""),  tile(b, "4"),   tile(y, ""),
  
  emptySquare, emptySquare, emptySquare, 
  tile(g, ""),  tile(y, "5"),   tile(y, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(g, "2"), tile(y, ""),    tile(y, "8"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(g, ""),  tile(y, "4"),   tile(y, ""),
)

There are 2 ways to define the relation of the state of the cube and the permutation matrix. The matrix above describes the new position of each edge. The edge 0 (orange-yellow) is now at postion 1, where the orange-blue edge belongs. The edge 1 is now positoned where edge 3 would be in a solved cube. 

The other interpretation would be the inverse of this Permuation. 

$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
#text(red)[2], #text(red)[0], #text(red)[3], #text(red)[1], 4, 5, 6, 7, 8, 9, 10, 11) $. 

Here the bottom row describes which edge is now at this position. So At position 0 there is edge 2, at position 1 there is edge 0. We will use the first interpretation as it makes composition of functions more intuitive. 

The function $E_(47174400)$ represents permuation after the Move *L*. The lexicographic index of any permuation $pi: {0, ..., N - 1} arrow {0, ..., N - 1}$ of $N$ elments can be computed by the following formular:
$ sum_(i = 0)^(N - 1)((N - 1 - i)! dot.op sum_(j = i + 1)^(N - 1) cases(1 "if" pi(i) > pi(j), 0 "otherwise")) $

== Edge orientation
Every edge has 2 possible orientations. To represent all edge orientations, 12 bits are needed which are stored into a single 16 bit integer. To make the edge orientation independent from the edge permuation state the $i$-th value in the bit vector corrosponds to the orientation of the edge at position $i$. We write an edge orientation similar to a permuation with a 2 row matrix. 

$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) $.

A 0 would corrospond to an oriented edge and a 1 to a flipped edge. The superflip has all edges not oriented but in the right position.  

$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1) $.

//superflip edge indices
#grid(
  columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
  emptySquare, emptySquare, emptySquare, 
  tile(w, ""),    tile(b, "6"),   tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(o, "3"),   tile(w, ""),    tile(r, "11"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, ""),    tile(g, "7"),   tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(o, ""),    tile(w, "3"),   tile(o, ""),
  tile(g, ""),    tile(w, "7"),   tile(g, ""),
  tile(r, ""),    tile(w, "11"),  tile(r, ""),
  tile(b, ""),    tile(w, "6"),   tile(b, ""),
  
  tile(b, "1"),   tile(o, ""),    tile(g, "2"),
  tile(o, "2"),   tile(g, ""),    tile(r, "10"),
  tile(g, "10"),  tile(r, ""),    tile(b, "9"),
  tile(r, "9"),   tile(b, ""),    tile(o, "1"),
  
  tile(o, ""),    tile(y, "0"),   tile(o, ""),
  tile(g, ""),    tile(y, "5"),   tile(g, ""),
  tile(r, ""),    tile(y, "8"),   tile(r, ""),
  tile(b, ""),    tile(y, "4"),   tile(b, ""),
  
  emptySquare, emptySquare, emptySquare, 
  tile(y, ""),    tile(g, "5"),   tile(y, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(o, "0"),   tile(y, ""),    tile(r, "8"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, ""),    tile(b, "4"),   tile(y, ""),
)

It is trivial to check whether an edge is oriented or not if it is at the original position. In any other position we have to define a logic that determines whether the edge is oriented or flipped. We choose one of the 3 main axis we base our orientation on. If we choose the Z-axis we can define the orientation of an edge as follows:
- Move the edge $i$ to position $i$ by without using the moves *F*, *F'*, *B* or *B'* (90° rotation on the Z-axis).
- Check whether the edge is oriented.

In practice this can be checked faster by choosing a tile for a certain edge and storing all faces this tile can be on by using our restricted set of moves. The orientation can be defined similary for the other 2 axis but we will use the Z-Axis as the base for our orientations.



== Corner Permuation
The position of all corners is a permuation of 8 elements. We need $ceil(log_2(8!)) = 16$ bits to 


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