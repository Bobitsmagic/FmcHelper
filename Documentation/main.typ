#import "template.typ": *

#show: project.with(
  title: "Cube Algorithm",
  authors: (
    "Bobitsmagic",
  ),
)
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

#let color_viewer(width, ..elements) = (
  grid(
    columns: (width, width, width + 0.1cm, width, width, width + 0.1cm, width, width, width + 0.1cm, width, width, width), 
    ..elements.pos().map(color => {
      if color == none {
        square(fill: none, stroke: none, size: width)
      } else {
        square(fill: color, stroke: black, size: width)
      }
    })
  )
)

= Faces
We define the set of faces or colors $cal(F): {O, R, Y, W, B, G}$.
Order: $O < R < Y < W < B < G$
= Move definitions Vocabulary
$M = { "L ", "L2 ", "L' ", "R ", "R2 ", "R' ", 
       "D ", "D2 ", "D' ", "U ", "U2 ", "U' ",
       "B ", "B2 ", "B' ", "F ", "F2 ", "F' "}$
- Edge, Corner, Center, Tile, Face

Flip function $ f: F -> F, x |-> cases(
  R &"if" x = O,
  O &"if" x = R,
  W &"if" x = Y,
  Y &"if" x = W,
  G &"if" x = B,
  B &"if" x = G,
) $

#grid(
  columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
  emptySquare, emptySquare, emptySquare, 
  tile(w, "33"),    tile(w, "34"),   tile(w, "35"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, "30"),   tile(w, "31"),    tile(w, "32"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, "27"),    tile(w, "28"),   tile(w, "29"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(o, "6"),    tile(o, "7"),    tile(o, "8"),
  tile(g, "51"),    tile(g, "52"),    tile(g, "53"),
  tile(r, "15"),    tile(r, "16"),    tile(r, "17"),
  tile(b, "42"),    tile(b, "43"),    tile(b, "44"),
  
  tile(o, "3"), tile(o, "4"),    tile(o, "5"),
  tile(g, "48"), tile(g, "49"),    tile(g, "50"),
  tile(r, "12"), tile(r, "13"),    tile(r, "14"),
  tile(b, "39"), tile(b, "40"),    tile(b, "41"),
  
  tile(o, "0"),    tile(o, "1"),   tile(o, "2"),
  tile(g, "45"),    tile(g, "46"),   tile(g, "47"),
  tile(r, "9"),    tile(r, "10"),   tile(r, "11"),
  tile(b, "36"),    tile(b, "37"),   tile(b, "38"),
  
  emptySquare, emptySquare, emptySquare, 
  tile(y, "24"),    tile(y, "25"),   tile(y, "26"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, "21"),   tile(y, "22"),    tile(y, "23"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, "18"),    tile(y, "19"),   tile(y, "20"),
).

= Sticker cube
We use 2 functions to store the state of a cube. The first function stores the state of all edges and the second function the state of all corners. First we define the set of edges $cal(E) subset F^2$. There are $12$ pairs in $F^2$ that are not valid edges. Those edges either have the form  $(X, X), X in F$ or $(X, f(X)), X in F$ where $X$ and $Y$ are opposite faces. The set of corners is defined es $cal(C) subset F^3$. All edges that contain the same color multiple times or colors of opposite sides are not valid. For every state $i in {0, dots, 2^12 dot.op 12! dot.op 3^8 dot.op 8! - 1}$ of the cube we define the function 
$ f_i: cal(E) union cal(C) -> F $
that return the colors on a specific edge or corner. Now there are a few restrictions that this function has to follow: 
*TODO* add is edge contraint, is corner constraint, permut corner constraint
- $forall (a, b), in cal(E): (a, b) |-> f  (b, a) |-> (y, x)$
- $forall (a, b, c),  (x, y, z) in cal(C): (a, b, c) |-> (x, y, z) <=> 
(a, c, b) |-> (x, z, y) <=> dots $ for all 6 permuations 
. In a solved cube the function $f_0$ would be defined as follows $f_0: (a, b) |-> a, (a, b, c) |-> a$ for all $a, b, c in F$ 


the identity function. 

= IndexCube
The IndexCube stores the permuation and orientation state of the corners and edges seperately.

== Edge Permuation

The position of all edges is a permuation of 12 elements. Instead of using 12 integer to store the position of every edge we use one integer that stores the lexicographic index of the permuation. There are $ceil(log_2(12!)) = 29$ bits needed to store all possible indices and we use a 32 bit or 4 byte integer to store this value. We define the family of bijective functions $E_i: {0, 1, dots, 11} arrow {0, 1, dots, 11}$ where $i in {0, 1, dots, 12! - 1}$ is the 0-based lexicographic index of the permuation that $E_i$ represents. 

A 12 element permuation will be represented by the following 2 row matrix:
$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
x_0, x_1, x_2, x_3, x_4, x_5, x_6, x_7, x_8, x_9, x_10, x_11) $. 

Every edge has to correspond to an index between 0 and 11 now. 

*TODO* Define order
//Solved edges with indices
#grid(
  columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
  emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(w, "10"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, "1"),   tile(w, ""),    tile(w, "5"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(w, "11"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(c, ""),    tile(o, "1"),   tile(c, ""),
  tile(c, ""),    tile(g, "11"),   tile(c, ""),
  tile(c, ""),    tile(r, "5"),  tile(c, ""),
  tile(c, ""),    tile(b, "10"),   tile(c, ""),
  
  tile(o, "2"),   tile(o, ""),    tile(o, "3"),
  tile(g, "3"),   tile(g, ""),    tile(g, "7"),
  tile(r, "7"),   tile(r, ""),    tile(r, "6"),
  tile(b, "6"),   tile(b, ""),    tile(b, "2"),
  
  tile(c, ""),    tile(o, "0"),   tile(c, ""),
  tile(c, ""),    tile(g, "9"),   tile(c, ""),
  tile(c, ""),    tile(r, "4"),   tile(c, ""),
  tile(c, ""),    tile(b, "8"),   tile(c, ""),
  
  emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(y, "9"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, "0"),   tile(y, ""),    tile(y, "4"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(y, "8"),   tile(c, ""),
)
 
After the move $"L "$ (90° clockwise rotation of the orange side) the permuation matrix has the following values:
$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
#text(red)[2], #text(red)[3], #text(red)[1], #text(red)[0], 4, 5, 6, 7, 8, 9, 10, 11) $. 


There are 2 ways to define the relation of the state of the cube and the permutation matrix. The matrix above describes the new position of each edge. The edge 0 (orange-yellow) is now at postion 1, where the orange-blue edge belongs. The edge 1 is now positoned where edge 3 would be in a solved cube. 

The other interpretation would be the inverse of this Permuation. 

$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
#text(red)[3], #text(red)[2], #text(red)[0], #text(red)[1], 4, 5, 6, 7, 8, 9, 10, 11) $. 

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
The equation $f_"X "(i) + f_"X "(12! - i - 1) = 12!$ holds for all moves. Proof (?) Permuations (?)

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
  g_"X "(floor(frac(i mod L_"X ", T_"X "))) &"otherwise"
) $ 

where for all $i in {0, dots, 12! - 1}$

$ g_"X ": {0, dots, frac(C_"X ", 2 dot.op T_"X ")} -> {0, dots, 12! - 1} $ 

are all values we need to store. 

#table(
  columns: (auto, auto, auto, auto), 
  align: right,
  [*Moves*], [Cycle length], [Track length], [Reduced length],
  [$"L ", "L'"$], [$479 001 600$], [$40 320$], [$5 940$],
  [$"L2"$], [$43 545 600$], [$40 320$], [$540$],

  [$"R ", "R2", "R'"$], [$40 320$], [$24$], [$840$],

  [$"D ", "D2", "D'"$], [$479 001 600$], [$2$], [$119 750 400$],
  [$"U ", "U2", "U'"$], [$39 916 800$], [$1$], [$19 958 400$],
  [$"B ", "B2", "B'"$], [$3 628 800$], [$1$], [$1 814 400$],
  [$"F ", "F2", "F'"$], [$362 880$], [$1$], [$181 440$]
)

The sum of all reduced lengths of the moves ${ "L ", "R ", "D ", "U ", "B ", "F " }$ is $141 711 420$. So all values of $g_x$can be stored by using $141711420 dot.op 4 "byte" approx 567 "megabyte"$.

*TODO*: Perfomance comparison for full table and only base moves table

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
  tile(c, ""),    tile(b, "10"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(o, "1"),   tile(w, ""),    tile(r, "5"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(g, "11"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(c, ""),    tile(w, "1"),   tile(c, ""),
  tile(c, ""),    tile(w, "11"),   tile(c, ""),
  tile(c, ""),    tile(w, "5"),  tile(c, ""),
  tile(c, ""),    tile(w, "6"),   tile(c, ""),
  
  tile(b, "2"),   tile(o, ""),    tile(g, "3"),
  tile(o, "3"),   tile(g, ""),    tile(r, "7"),
  tile(g, "7"),   tile(r, ""),    tile(b, "6"),
  tile(r, "9"),   tile(b, ""),    tile(o, "1"),
  
  tile(c, ""),    tile(y, "0"),   tile(c, ""),
  tile(c, ""),    tile(y, "9"),   tile(c, ""),
  tile(c, ""),    tile(y, "4"),   tile(c, ""),
  tile(c, ""),    tile(y, "4"),   tile(c, ""),
  
  emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(g, "9"),   tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(o, "0"),   tile(y, ""),    tile(r, "4"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),    tile(b, "8"),   tile(c, ""),
)

It is trivial to check whether an edge is oriented or not if it is at the original position. In any other position we have to define a logic that determines whether the edge is oriented or flipped. We choose one of the 3 main axis we base our orientation on. If we choose the Z-axis we can define the orientation of an edge as follows:
- Move the edge $i$ to position $i$ by without using the moves $"F ", "F' ", "B "$ or $"B' " $  (90° rotation on the Z-axis).
- Check whether the edge is oriented.

In practice this can be checked faster by choosing a tile for a certain edge and storing all faces this tile can be on by using our restricted set of moves. The orientation can be defined similary for the other 2 axis but we will use the Z-Axis as the base for our orientations.

=== Edge orientation move
We define the function $O_E: {0, dots, 2^12 - 1} times M -> {0, dots, 2^12 - 1}$. The space needed to store this function is $2^12 dot.op 18 dot.op 2 "byte" approx 147 "kb"$. Similar to the edge permuation function this can be calculated for every move seperately. The new orientation state after applying a move can be calculated by applying the permuation of that move on the orientation state. For the moves ${"F ", "F' ", "B ", "B' "}$ all edges affected by that move have their orientation flipped after the permuation was applied. 

== Corner permuation
The position of all corners is a permuation of 8 elements. We need $ceil(log_2(8!)) = 16$ bits to store the lexicographic index of a corner permuation. Similar to the edges we use the family of bijective functions $C_i: {0, 1, dots, 7} arrow {0, 1, dots, 7}$ where $i in {0, 1, dots, 7! - 1}$ is the $0$-based lexicographic index of the permuation that $C_i$ represents. Every corner and every position corrosponds to an index between $0$ and $7$ now. All corners are lexicographicaly sorted by their unique tuple $(x, y, z) in {O, R} times {Y, W} times {B, G}$. 

#grid(
  columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
  emptySquare, emptySquare, emptySquare, 
  tile(w, "2"),    tile(c, ""),   tile(w, "6"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),   tile(w, ""),    tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, "3"),    tile(c, ""),   tile(w, "7"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(o, "2"),    tile(c, ""),   tile(o, "3"),
  tile(g, "3"),    tile(c, ""),   tile(g, "7"),
  tile(r, "7"),    tile(c, ""),  tile(r, "6"),
  tile(b, "6"),    tile(c, ""),   tile(b, "2"),
  
  tile(c, ""),    tile(o, ""),    tile(c, ""),
  tile(c, ""),    tile(g, ""),    tile(c, ""),
  tile(c, ""),    tile(r, ""),    tile(c, ""),
  tile(c, ""),    tile(b, ""),    tile(c, ""),
  
  tile(o, "0"),    tile(c, ""),   tile(o, "1"),
  tile(g, "1"),    tile(c, ""),   tile(g, "5"),
  tile(r, "5"),    tile(c, ""),   tile(r, "4"),
  tile(b, "4"),    tile(c, ""),   tile(b, "0"),
  
  emptySquare, emptySquare, emptySquare, 
  tile(y, "1"),    tile(c, ""),   tile(y, "5"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(c, ""),   tile(y, ""),    tile(c, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, "0"),    tile(c, ""),   tile(y, "4"),
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

= Geometric Symmetries
There are $48$ symmetries a $3$ by $3$ cube can have. 

Every symmetry is defined as a bijective function $S_i: F -> F$ for all $i in {0, ..., 47}$ that maps faces of to other faces such that $forall x in F: f(S_i(x)) = S_i(f(x))$ with the flip function $f$ that maps opposite sides to each other. 

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

An example would be the OLL-45 algorithm:
$ "F ", "R ", "U ", "R'", "U'", "F'" $.
Using the X-Z-plane reflection symmetry we defined before we can tranform  all moves as follows:
$ "F'", "R'", "D'", "R ", "D ", "F " $.
Note that since we map white to yellow all $"U "$ and $"U'"$ moves are transformed to $"D'"$ and $"D "$ respectively. All other moves just get their rotation direction reversed except for $"R2"$ which stays the same. The following $2$ cube nets show the OLL-45 algorithm and its transformed version performed on a solved cube.




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
n, n, n,  w, w, o,  n, n, n,  n, n, n,
n, n, n,  w, w, g,  n, n, n,  n, n, n,
n, n, n,  w, b, o,  n, n, n,  n, n, n,
b, o, g,  r, w, g,  w, w, w,  b, r, r,
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
b, o, g,  r, y, g,  y, y, y,  b, r, r,
n, n, n,  y, b, o,  n, n, n,  n, n, n,
n, n, n,  y, y, g,  n, n, n,  n, n, n,
n, n, n,  y, y, o,  n, n, n,  n, n, n,
)

== Sticker symmetry
When working on a sticker based representation of a cube, a symmetry transformation can be performed by copying each tile to its position after applying the symmetry and then applying the symmetry function $S_i$ on its color.

In the previous example we can see that in the top cube the blue-white edge at position $(W, G)$ gets transformed to the position $(S_2(W), S_2(G)) = (Y, G)$ and after its colors $(B, W)$ are transformed as well $(S_2(B), S_2(W)) = (B, Y)$. This has to be done for tile. Formaly this can be written by using the function $f$ defining the state of a sticker cube.
The new state $f_s$ of the cube after applying the symmetry $S_i$ on the state $f_j$ is defined as follows:
$ f_s: (S_i(a), S_i(b)) |-> (S_i(f_j(a, b)), S_i(f_j(b, a))) $
$ f_s: (S_i(x), S_i(y), S_i(z)) |-> (S_i(f_j(x, y, z)), S_i(f_j(y, x, z)), S_i(f_j(z, x, y))) $
for all $(a, b) in cal(E)$ and $(x, y, z) in cal(C)$. 

\* Magic proof that shows that both definitions of a symmetry transformation are equivalent \*

== Index symmetry
When working with a IndexCube representation the symmetry transformation is diffrent for permutation and orientation state.

*Idea*: Convert edge, corner orientation into permuation space instead of position space

*TODO*: Edges, orientation, Test 

Corner state $P_C$, Symmetryfunction $S_C$:
$ P_C' = S_C^(-1) compose P_C compose S_C $

= Inverse Symmetry
2 cubes are symmetric over inverse symmetry when the inverse of the solution to one cube solves the other. The inverse $X^(-1)$ of a sequence $X$ of moves is the sequence in reversed order and every rotation direction getting reversed as well. In the following example we consider move sequences. $X = ["L2", "D "]$ and $Y = ["D'"  "L2"]$. Now the cubes that these 2 scrambles generate are not symmetric under any previously defined symmetry but they are inversely symmetric since $X^(-1) = Y$ or equivalent $X = Y^(-1)$. 

= Symmetry hashing
Since we store a set of cubes where the shortest solution is known, a hashing algorithm is needed to efficiently search in that set. Since we store only one representative of all symmetries a specific state can have searching a random cube in that set can get tricky. It is possible to transform the cube by every symmetry and then check whether it is in our set, resulting in $48$ contain checks in our set. The following chapters talk about faster methods to achive this. 


== Lowest symmetry
A solution is to define an total order over all cubes and then only store the smallest representative of all symmetric states a cube can have. When searching for a specific cube in our set we can transform its state by all symmetries and check which one is smallest in the sense of the defined order. This lowest symmetry state is then looked up in our set.

== Symmetry resistent properties
Some properties are the same for all symmetries of a specific cube. The amount of correctly placed edges and corners is such a property. Since the can only be $8 * 12$ diffrent values for the amount of solved edges and corners this is not sufficient as a hashing function. 

#color_viewer(0.6cm,
n, n, n, y, o, y, n, n, n, n, n, n,
n, n, n, b, w, r, n, n, n, n, n, n,
n, n, n, y, o, y, n, n, n, n, n, n,
g, w, b, r, y, o, b, b, g, o, w, r,
y, o, y, b, g, r, y, r, w, r, b, g,
g, w, b, r, b, o, b, g, g, o, g, r,
n, n, n, w, o, w, n, n, n, n, n, n,
n, n, n, g, y, r, n, n, n, n, n, n,
n, n, n, w, o, w, n, n, n, n, n, n,
)

#color_viewer(0.6cm,
n, n, n, y, o, y, n, n, n, n, n, n,
n, n, n, b, w, r, n, n, n, n, n, n,
n, n, n, y, o, y, n, n, n, n, n, n,
g, w, b, r, y, o, b, b, g, o, w, r,
y, o, y, b, g, r, y, r, w, r, b, g,
g, w, b, r, b, o, b, g, g, o, g, r,
n, n, n, w, o, w, n, n, n, n, n, n,
n, n, n, g, y, r, n, n, n, n, n, n,
n, n, n, w, o, w, n, n, n, n, n, n,
)