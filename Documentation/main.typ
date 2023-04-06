#import "template.typ": *

// Take a look at the file `template.typ` in the file panel
// to customize this template and discover how it works.
#show: project.with(
  title: "Cube Algorithm",
  authors: (
    "Bobitsmagic",
  ),
)

// We generated the example code below so you can see how
// your document will look. Go ahead and replace it with
// your own content!
 
= IndexCube
We are in need of class that represents a 3 by 3 rubiks cube. The IndexCube stores the state of the corners and edges seperately. KEk

== Edge indices 

The position of all edges is a permuation of 12 elements. Instead of using 12 integer to store the position of every edge we use one integer that stores the lexicographic index of the permuation. There are $ceil(log_2(12!)) = 29$ bits needed to store all possible indices and we use a 32 bit integer to store this value. We use the family of bijective functions $E_i: {0, 1, dots, 11} arrow {0, 1, dots, 11}$ where $i in {0, 1, dots, 12! - 1}$ is the lexicographic index of the permuation that $E_i$ represents. 

A 12 element permuation will be represented by the following 2 row matrix:
$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
x_0, x_1, x_2, x_3, x_4, x_5, x_6, x_7, x_8, x_9, x_10, x_11) $. 

Every edge has to corrospond to an index between 1 and 12 now. We sort the edges by their $x, y$ and $z$ components on a right handed coordinate system with the green facing towards positive $z$ and white facing towards positive $y$. In the following cube net the resulting indices can be seen. 

#let n = none
#let w = white
#let o = orange
#let g = green
#let b = blue
#let r = red
#let y = yellow

#let emptySquare = square(fill: none, stroke: none, size: 1cm)
#let tile(color, text) = (
  square(fill: color, stroke: black, size:  1cm)[#set align(center); #set align(horizon); #text]
)

#grid(
  columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
  emptySquare, emptySquare, emptySquare, 
  tile(w, ""),
  tile(w, "6"),
  tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, "3"),
  tile(w, ""),
  tile(w, "11"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, ""),
  tile(w, "7"),
  tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(o, ""),
  tile(o, "3"),
  tile(o, ""),
  tile(g, ""),
  tile(g, "7"),
  tile(g, ""),
  tile(r, ""),
  tile(r, "11"),
  tile(r, ""),
  tile(b, ""),
  tile(b, "6"),
  tile(b, ""),
  
  tile(o, "1"),
  tile(o, ""),
  tile(o, "2"),
  tile(g, "2"),
  tile(g, ""),
  tile(g, "10"),
  tile(r, "10"),
  tile(r, ""),
  tile(r, "9"),
  tile(b, "9"),
  tile(b, ""),
  tile(b, "1"),
  
  tile(o, ""),
  tile(o, "0"),
  tile(o, ""),
  tile(g, ""),
  tile(g, "5"),
  tile(g, ""),
  tile(r, ""),
  tile(r, "8"),
  tile(r, ""),
  tile(b, ""),
  tile(b, "4"),
  tile(b, ""),
  
  emptySquare, emptySquare, emptySquare, 
  tile(y, ""),
  tile(y, "5"),
  tile(y, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, "0"),
  tile(y, ""),
  tile(y, "8"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, ""),
  tile(y, "4"),
  tile(y, ""),
)

The specific selection of indices will reduce the amount of space needed to store a matrix in discussed in section *KEK*.
After the move L (90° clockwise rotation of the orange side) the permuation matrix has the following values:
$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
bold(1), bold(3), bold(0), bold(2), 4, 5, 6, 7, 8, 9, 10, 11) $. 

// new position of edge at position i ?


Inverse (?)
$ mat(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11;
bold(2), bold(0), bold(3), bold(1), 4, 5, 6, 7, 8, 9, 10, 11) $. 

#grid(
  columns: (1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm, 1cm), 
  emptySquare, emptySquare, emptySquare, 
  tile(w, ""),
  tile(w, "6"),
  tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, "3"),
  tile(w, ""),
  tile(w, "11"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(w, ""),
  tile(w, "7"),
  tile(w, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare,
  
  tile(o, ""),
  tile(o, "3"),
  tile(o, ""),
  tile(g, ""),
  tile(g, "7"),
  tile(g, ""),
  tile(r, ""),
  tile(r, "11"),
  tile(r, ""),
  tile(b, ""),
  tile(b, "6"),
  tile(b, ""),
  
  tile(o, "1"),
  tile(o, ""),
  tile(o, "2"),
  tile(g, "2"),
  tile(g, ""),
  tile(g, "10"),
  tile(r, "10"),
  tile(r, ""),
  tile(r, "9"),
  tile(b, "9"),
  tile(b, ""),
  tile(b, "1"),
  
  tile(o, ""),
  tile(o, "0"),
  tile(o, ""),
  tile(g, ""),
  tile(g, "5"),
  tile(g, ""),
  tile(r, ""),
  tile(r, "8"),
  tile(r, ""),
  tile(b, ""),
  tile(b, "4"),
  tile(b, ""),
  
  emptySquare, emptySquare, emptySquare, 
  tile(y, ""),
  tile(y, "5"),
  tile(y, ""),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, "0"),
  tile(y, ""),
  tile(y, "8"),
  emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, emptySquare, 
  tile(y, ""),
  tile(y, "4"),
  tile(y, ""),
)

#link("https://jperm.net/3x3/moves")

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

Every edge has 2 possible orientations. To represent all edge orientation, 12 bits are needed which are stored into a single 16 bit integer.

The influence of the edge permuation to the edge orientation is defined as follows. 
== Corner indices