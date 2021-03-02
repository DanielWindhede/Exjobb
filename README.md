# Track map generator for Formula 1 race tracks

A procedural seed based tool which generates designs for valid Formula 1 race tracks based on Voronoi Diagrams and random walk. Bézier curves are used to smooth out corners. Application is made using Unity.

The generated track is obeying the rules of the [FIA](https://www.fia.com/regulation/) as to create valid Formula 1 circuits. 
Some of these rules include:
* min/max circuit length
* Finish line placement
* Straight lengths

## Example of generated track map

![pic-1](https://github.com/DanielWindhede/Exjobb/blob/main/ReadmePictures/readmePic1.jpg?raw=true)

## Installation
Not available for download as of now

## How it works (roughly)
### 1. Delaunay Triangulation
Generate a delaunay triangulation from procedurally placed points within a set area through Bowyer Watson algorithm using a super triangle. Do not generate triangles whose circumcenter lies outside of the given area.

![pic-1](https://github.com/DanielWindhede/Exjobb/blob/main/ReadmePictures/readmePic5.jpg?raw=true)

### 2. Voronoi Diagram
Generate the Voronoi Diagram from the Delaunay Triangulation through it's duality.

![pic-2](https://github.com/DanielWindhede/Exjobb/blob/main/ReadmePictures/readmePic4.jpg?raw=true)

### 3. Random & Self avoiding walk
Construct a list of points making up the circuit through a random & self avoiding walk over the Voronoi Diagram's nodes. 

![pic-3](https://github.com/DanielWindhede/Exjobb/blob/main/ReadmePictures/readmePic3.jpg?raw=true)

### 4. Bézier Curve
Smooth out certain corners procedurally based on parameters.

![pic-4](https://github.com/DanielWindhede/Exjobb/blob/main/ReadmePictures/readmePic2.jpg?raw=true)

### 5. Final image
Display the final track map with turn numbers, direction and finish line.

![pic-5](https://github.com/DanielWindhede/Exjobb/blob/main/ReadmePictures/readmePic1.jpg?raw=true)
