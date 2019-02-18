This is my sudoku solver and generator written in C#.  Any puzzle with a size greater than 9x9 could take awhile to solve.

Creating a readable Sudoku:
The format for creating your own Sudoku is as follows:
Line 1 - Contains the symbol set you would like to use i.e. 123456 etc, abcdefg etc.  Must contain the same number of characters as there are values in the puzzle.
Line 2 - Contains the size of the Sub-Grids i.e. 3x3 would mean a 9x9 puzzle, 2x5 would mean a 10x10 puzzle.
Line 3+ - Contains the puzzle itself using space seperated integers, 0 means an empy cell, 1 means the first character in the symbol set, 2 means the second and so on.

A full example puzzle would be as follows.

123456789
3x3
2 0 0 0 7 0 0 3 8
0 0 0 0 0 6 0 7 0
3 0 0 0 4 0 6 0 0
0 0 8 0 2 0 7 0 0
1 0 0 0 0 0 0 0 6
0 0 7 0 3 0 4 0 0
0 0 4 0 8 0 0 0 9
0 6 0 4 0 0 0 0 0
9 1 0 0 6 0 0 0 2
