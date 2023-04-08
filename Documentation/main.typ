#import "template.typ": *

#show: project.with(
  title: "Cube Algorithm",
  authors: (
    "Bobitsmagic",
  ),
)

= Move definitions Vocabulary
$M = { "L ", "L2 ", "L' ", "R ", "R2 ", "R' ", 
       "D ", "D2 ", "D' ", "U ", "U2 ", "U' ",
       "B ", "B2 ", "B' ", "F ", "F2 ", "F' "}$
- Edge, Corner, Center, Tile, Face
= IndexCube
We are in need of class that represents a 3 by 3 rubiks cube. The IndexCube stores the permuation and orientation state of the corners and edges seperately.

== Edge Permuation

The position of all edges is a permuation of 12 elements. Instead of using 12 integer to store the position of every edge we use one integer that stores the lexicographic index of the permuation. There are $ceil(log_2(12!)) = 29$ bits needed to store all possible indices and we use a 32 bit or 4 byte integer to store this value. We use the family of bijective functions $E_i: {0, 1, dots, 11} arrow {0, 1, dots, 11}$ where $i in {0, 1, dots, 12! - 1}$ is the 0-based lexicographic index of the permuation that $E_i$ represents. 

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
After the move $"L "$ (90° clockwise rotation of the orange side) the permuation matrix has the following values:
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

The function $E_(47174400)$ represents the permuation after applying the move $"L "$ to the solved cube. We will use $E_"X "$ with $"X " in M$ as a shorthand notation for the permuation that a move  $"X "$ performs.

 The lexicographic index of any permuation $pi: {0, ..., N - 1} arrow {0, ..., N - 1}$ of $N$ elments can be computed by the following formular:
$ sum_(i = 0)^(N - 1)((N - 1 - i)! dot.op sum_(j = i + 1)^(N - 1) cases(1 "if" pi(i) > pi(j), 0 "otherwise")) $

=== Edge permuation moves
In order to make a move a we need to create a function that takes the current permuation state and a move and returns the new state. We declare $P_E: {0, dots, 12! - 1} times M arrow {0, dots, 12! - 1}$. We can find the value of our function by converting the index of the permuation into its matrix form, perform the move on that matrix and then calculate its new lexicographic index. 

Doing this for every move would be very slow so we store the result of all our computations into a table. This table would have a size of $12! dot.op 18 dot.op 4 "byte" approx 34.5 "gb"$. This can be greatly reduced by some observations. 

Turning a side $180°$ or turning it counterclockwise can be achived by turning it clockwise $2$ or $3$ times respectively. Therefore only $frac(1, 3)$ of the space is needed by defining $P_E: (i, "X2 ") |-> P_E(P_E(i, "X2 "), "X ")$ and $P_E: (i, "X' ") |-> P_E(P_E(P_E(i, "X "), "X "), "X ")$ for all $"X " in { "L ", "R ", "D ", "U ", "B ", "F " }$.

We declare a helper function $f_"X ": {0, dots, 12! - 1} arrow {0, dots, 12! - 1}$ for every move $ "X " in { "L ", "R ", "D ", "U ", "B ", "F " }$. Now we define $f_"X "$ such that $P_E: (i, "X ") |-> (i + f_"X "(i)) mod 12!$. By definition $f_"X "$ contains the difference modulo $12!$ of the index before and after applying the function $P_E$.

This helper function can be compressed in 3 diffrent ways. 

=== Flip at half
The equation $f_"X "(i) + f_"X "(12! - i - 1) = 12!$ holds for all moves $ "X " in { "L ", "R ", "D ", "U ", "B ", "F " }$. Proof (?)

Therefore we can reduce the amount of space needed by a factor of $2$ by defining $f_"X ": i |-> 12! - f_" X"(12! - i - 1)$ for all $i >= frac(12!, 2)$

=== Repeating cycles
Every function $f_"X "$ has a minimum cycle length $L in D_(12!)$ such that  

$ forall i, j in {0, dots, 12! - 1}: i ident j mod L arrow.double f_"X "(i) = f_"X "(j) $.

This compression method reduces the space needed by a factor of $frac(12!, L)$.

=== Repeating values
Every function $f_"X "$ has a maximum track length $T_"X " in D_(12!)$ of repeating values where $D_n$ is the set of divisors of $n$. The following equation has to be checked for every divisor seperately:

$ forall i, j in {0, dots, 12! - 1}: floor(frac(i, T_"X ")) = floor(frac(j, T_"X ")) arrow.double f_"X "(i) = f_"X "(j)$

In the worst case this value is $1$ but for $3$ of the $6$ moves this compression is viable reducing the space needed by a factor of $T_"X "$. 

=== Reduction
We can now define 

$ f_"X ": i |->  cases(
  12! - f_"X "(12! - i - 1) &"if" i >= frac(12!, 2),
  g_"X "(floor(frac(i, T_"X ")) mod L_"X ")) &"otherwise"
) $ 

where for all $i in {0, dots, 12! - 1}$

$ g_"X ": {0, dots, frac(C_"X ", 2 dot.op T_"X ")} -> {0, dots, 12! - 1} $ 

are all values we need to store. 

*TODO*: Final values of reduction

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
- Move the edge $i$ to position $i$ by without using the moves $"F ", "F' ", "B "$ or $"B' " $  (90° rotation on the Z-axis).
- Check whether the edge is oriented.

In practice this can be checked faster by choosing a tile for a certain edge and storing all faces this tile can be on by using our restricted set of moves. The orientation can be defined similary for the other 2 axis but we will use the Z-Axis as the base for our orientations.

=== Edge orientation move
We define the function $O_E: {0, dots, 2^12 - 1} times M -> {0, dots, 2^12 - 1}$. The space needed to store this function is $2^12 dot.op 18 dot.op 2 "byte" approx 147 "kb"$. Similar to the edge permuation function this can be calculated for every move seperately. The new orientation state after applying a move can be calculated by applying the permuation of that move on the orientation state. For the moves ${"F ", "F' ", "B ", "B' "}$ all edges affected by that move have their orientation flipped after the permuation was applied. 

== Corner permuation
The position of all corners is a permuation of 8 elements. We need $ceil(log_2(8!)) = 16$ bits to store the lexicographic index of a corner permuation. Similar to the edges we use the family of bijective functions $C_i: {0, 1, dots, 7} arrow {0, 1, dots, 7}$ where $i in {0, 1, dots, 7! - 1}$ is the $0$-based lexicographic index of the permuation that $C_i$ represents. Every corner and every position corrosponds to an index between $0$ and $7$ now. All corners are sorted by their X, Y and Z coordinates as seen in the following cube net.

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

=== Corner permuation move
We define the function $P_C: {0, dots, 8! - 1} times M -> {0, dots, 8! - 1}$. The space needed to store this function is $8! dot.op 18 dot.op 2 "byte" approx 1452 "kb"$. All those values are stored and are initially calculated similar to the edge permuation values.

== Corner orientation
Every corner has 3 diffrent orientations. To represent all corner orientations $ceil(log_2(3^8)) = 13$ bit are needed which are stored into a single $16$ bit integer. The orientation of a corner is defined as follows:
- Every axis gets a corresponding index from $0$ to $2$
- We choose a pair of non adjacent colors called base colors  
- Every corner contains a tile with exactly one of our base colors
- The orientation of a corner is the index corresponding to the axis our tile with the base color is facing

We choose orange and red as our base colors and the X, Y and Z-axis are numbered $0, 1$ and $2$ respectively.

=== Corner orientation move
We define the function $P_C: {0, dots, 3^8 - 1} times M -> {0, dots, 3^8 - 1}$. The space needed to store this function is $3^8 dot.op 18 dot.op 2 "byte" approx 236 "kb"$. All those values are stored and are initially calculated by *magic*. 

*TODO*: Add extra modell to explain corner orientation function

= Symmetries
There are $48$ symmetries a $3$ by $3$ cube can have. We define the set of faces or colors $F: {O, R, Y, W, B, G}$.

Every symmetry is defined as a bijective function $S_i: F -> F$ for all $i in {0, ..., 47}$ that maps faces of to other faces such that $forall x in F: f(S_i(x)) = S_i(f(x))$ with the flip function $ f: F -> F, x |-> cases(
  R &"if" x = O,
  O &"if" x = R,
  W &"if" x = Y,
  Y &"if" x = W,
  G &"if" x = B,
  B &"if" x = G,
) $ that maps opposite sides to each other. 

The amount of symmetries follows from the $24$ orientations a cube can have and a factor of $2$ by adding a reflection to every one of them. 

An example symmetry is the reflectional symmetry along the X-Z-plane which maps yellow to white and vice versa. In the following cube net the symmetry plane is shown.

#let stroke_tile(color) = (
  square(fill: color, stroke: black, size:  1cm)[#line(start: (0%, 50%), end: (100%, 50%))]
) 

#grid(
  columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
  emptySquare, emptySquare, emptySquare, 
  tile(w, ""),    tile(w, ""),   tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, ""),   tile(w, ""),    tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, ""),    tile(w, ""),   tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(o, ""),    tile(o, ""),    tile(o, ""),
  tile(g, ""),    tile(g, ""),    tile(g, ""),
  tile(r, ""),    tile(r, ""),    tile(r, ""),
  tile(b, ""),    tile(b, ""),    tile(b, ""),
  
  stroke_tile(o), stroke_tile(o),    stroke_tile(o),
  stroke_tile(g), stroke_tile(g),    stroke_tile(g),
  stroke_tile(r), stroke_tile(r),    stroke_tile(r),
  stroke_tile(b), stroke_tile(b),    stroke_tile(b),
  
  tile(o, ""),    tile(o, ""),   tile(o, ""),
  tile(g, ""),    tile(g, ""),   tile(g, ""),
  tile(r, ""),    tile(r, ""),   tile(r, ""),
  tile(b, ""),    tile(b, ""),   tile(b, ""),
  
  emptySquare, emptySquare, emptySquare, 
  tile(y, ""),    tile(y, ""),   tile(y, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, ""),   tile(y, ""),    tile(y, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, ""),    tile(y, ""),   tile(y, ""),
).

The corresponding function $S_2$ would be 
$ S_2: x |-> cases(
  W &"if" x = Y,
  Y &"if" x = W,
  x &"otherwise"
) $
.

Tranforming a cube by a symmetry can be done in 3 diffrent ways.

== Move symmetry
If the scramble sequence $Q$ to a cube is known then the sequence $Q'$ where every move is transformed by the symmetry should result in the cube transformed by the symmetry.

Every move on the a side $X in F$ gets transformed to a new face by the symmetry function $S_i$ and if the symmetry contains a reflection then the rotation direction is also reversed. 

An example would be the T-perm:
$ ("R ", "U ", "R'", "U'"), "R'", "F ", "R2", ("U'", "R'", "U'", "R "), "U ", "R'", "F'" $.
Using the X-Z-plane reflection symmetry we defined before we can tranform  all moves as follows:
$ ("R'", "D'", "R ", "D "), "R ", "F'", "R2", ("D ", "R ", "D ", "R'"), "D'", "R ", "F " $.
Note that since we map white to yellow all $"U "$ and $"U'"$ moves are transformed to $"D'"$ and $"D "$ respectively. All other moves just get their rotation direction reversed except for $"R2"$ which stays the same. The following $2$ cube nets show the T-perm and its transformation performed on a solved cube.

#let color_viewer(width, ..elements) = (
  grid(
    columns: (width, width, width, width, width, width, width, width, width, width, width, width), 
    ..elements.pos().map(color => {
      if color == none {
        square(fill: none, stroke: none, size: width)
      } else {
        square(fill: color, stroke: black, size: width)
      }
    })
  )
)


/* Template
#color_viewer(0.5cm, 
n, n, n,  w, w, w,  n, n, n,  n, n, n,
n, n, n,  w, w, w,  n, n, n,  n, n, n,
n, n, n,  w, w, w,  n, n, n,  n, n, n,
o, o, o,  g, g, g,  r, r, r,  b, b, b,
o, o, o,  g, g, g,  r, r, r,  b, b, b,
o, o, o,  g, g, g,  r, r, r,  b, b, b,
n, n, n,  y, y, y,  n, n, n,  n, n, n,
n, n, n,  y, y, y,  n, n, n,  n, n, n,
n, n, n,  y, y, y,  n, n, n,  n, n, n,
)
*/

#color_viewer(0.6cm, 
n, n, n,  w, w, w,  n, n, n,  n, n, n,
n, n, n,  w, w, w,  n, n, n,  n, n, n,
n, n, n,  w, w, w,  n, n, n,  n, n, n,
o, r, o,  g, g, r,  b, o, g,  r, b, b,
o, o, o,  g, g, g,  r, r, r,  b, b, b,
o, o, o,  g, g, g,  r, r, r,  b, b, b,
n, n, n,  y, y, y,  n, n, n,  n, n, n,
n, n, n,  y, y, y,  n, n, n,  n, n, n,
n, n, n,  y, y, y,  n, n, n,  n, n, n,
)


#color_viewer(0.6cm, 
n, n, n,  w, w, w,  n, n, n,  n, n, n,
n, n, n,  w, w, w,  n, n, n,  n, n, n,
n, n, n,  w, w, w,  n, n, n,  n, n, n,
o, o, o,  g, g, g,  r, r, r,  b, b, b,
o, o, o,  g, g, g,  r, r, r,  b, b, b,
o, r, o,  g, g, r,  b, o, g,  r, b, b,
n, n, n,  y, y, y,  n, n, n,  n, n, n,
n, n, n,  y, y, y,  n, n, n,  n, n, n,
n, n, n,  y, y, y,  n, n, n,  n, n, n,
)

== Tile symmetry
When working on a tile/sticker based representation of a cube a symmetry transformation can be performed by copying each tile to its position after applying the symmetry and then

Corner state $P_C$, Symmetryfunction $S_C$:

$ P_C' = S_C^(-1) compose P_C compose S_C $

*TODO* Edges, orientation, Test 


