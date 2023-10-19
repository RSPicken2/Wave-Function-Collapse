# Wave Function Collapse
 An Implementation of the wave function collapse pattern generator
 
This project is based on the work of github contributor Maxim Gumin : https://github.com/mxgmn/WaveFunctionCollapse
 
Wave Function Collapse is a method of pattern generation based on using a starting image as a "rulebook" to determine how tiles with an established size can link together in a new image. 
A random starting tile can be chosen and then based off its rules, the tiles around it can be narrowed down to only being a certain type of tile themselves.

For example, a beach tile can have a rule established that it can only be connected to a sea tile or a grass tile. And maybe only in certain directions or quantities.

This program takes an input of a png file called "input.png" in the debug file and uses that to generate a new image called "output.png" in the same file.
Currently it is configued to generate a top down pixel art image of a landscape with grass, trees, sea and some houses.

The dimentions of the input and output image, as well as the pixel dimentions of individual tiles can be altered at the top of program.cs
