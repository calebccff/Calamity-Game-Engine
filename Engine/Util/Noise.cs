using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.FileSystem;
using Microsoft.Xna.Framework;

namespace TestShader
{
    /// <summary>
    /// An abstract class that represents a 2 dimensional noise generator
    /// </summary>
    abstract class Noise2D
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="Noise"/>.
        /// </summary>
        public Noise2D()
        {

        }

        /// <summary>
        /// Generates the noise 
        /// </summary>
        public abstract void Generate();

        /// <summary>
        /// Gets the noise at the specified integer point
        /// </summary>
        /// <param name="i">x coordinate</param>
        /// <param name="j">y coordinate</param>
        /// <returns>The noise at the integer point (i,j) </returns>
        public virtual float this[int i, int j]
        {
            get { return getVal(i, j); }
        }

        /// <summary>
        /// Gets the noise at the specified integer point
        /// </summary>
        /// <param name="point"> The integer point</param>
        /// <returns>The noise at <paramref name="point"/> </returns>
        public virtual float this[Point point]
        {
            get { return getVal(point.X, point.Y); }
        }

        /// <summary>
        /// Gets the noise at the point
        /// </summary>
        /// <param name="i"> x coordinate</param>
        /// <param name="j"> y coordinate</param>
        /// <returns> The noise at the point (i,j)</returns>
        public virtual float this[float i, float j]
        {
            get { return getVal(i, j); }
        }

        /// <summary>
        /// Gets the noise at the specified vector
        /// </summary>
        /// <param name="point"> The vector</param>
        /// <returns>The noise at <paramref name="vector"/> </returns>
        public virtual float this[Vector2 vector]
        {
            get { return getVal(vector.X, vector.Y); }
        }


        /// <summary>
        /// The value of the noise at the specified point
        /// Overloaded by the subclass
        /// </summary>
        /// <param name="i"> x coordinate</param>
        /// <param name="j"> y coordinate</param>
        /// <returns>The noise at the point (i,j)</returns>
        protected abstract float getVal(float i, float j);
    }

    /// <summary>
    /// A class that generates a 2D Perlin noise
    /// </summary>
    class Perlin : Noise2D
    {
        /// <summary>
        /// The size of a unit in the grid used to generate the noise
        /// </summary>
        float gridSize;
        /// <summary>
        /// The number of rows in the grid used to generate the noise
        /// </summary>
        int gridRows;
        /// <summary>
        /// The number of columns in the grid used to generate the noise
        /// </summary>
        int gridColumns;

        /// <summary>
        /// The grid of unit vectors used to generate the noise
        /// </summary>
        List<List<Vector2>> grid = new List<List<Vector2>>();

        /// <summary>
        /// Initializes a new instance of a <see cref="Perlin"/>
        /// </summary>
        /// <param name="gridSize"> The size of a unit in the grid</param>
        /// <param name="gridRows"> The number of rows in the grid</param>
        /// <param name="gridColumns"> The number of columns in the grid</param>
        public Perlin(float gridSize, int gridRows, int gridColumns) : base()
        {
            this.gridSize = gridSize;
            this.gridRows = gridRows;
            this.gridColumns = gridColumns;

            grid = new List<List<Vector2>>();
            for (int i = 0; i < (int)gridColumns; i++)
            {
                grid.Add(new List<Vector2>());
                for (int j = 0; j < (int)gridRows; j++)
                {
                    grid[i].Add(new Vector2(0, 0));
                }
            }
        }

        /// <summary>
        /// Regenerates the noise by randomizing the grid of unit vectors used to generate the noise
        /// </summary>
        public override void Generate()
        {
            for (int i = 0; i < (int)gridColumns; i++)
            {
                for (int j = 0; j < (int)gridRows; j++)
                {
                    float random = Calc.Range(Calc.Random, 0f, (float)int.MaxValue);
                    grid[i][j] = new Vector2((float)Math.Cos(random), (float)Math.Sin(random));
                }
            }
        }

        /// <summary>
        /// The value of the noise at the specified point
        /// </summary>
        /// <param name="i"> x coordinate</param>
        /// <param name="j"> y coordinate</param>
        /// <returns> The noise at the point (i,j)</returns>
        protected override float getVal(float i, float j)
        {
            // Get the vector
            Vector2 vector = new Vector2(i, j);

            //Find the cell that contains the specified point
            Point upperLeft = findVectorCell(i, j);
            Point upperRight = new Point(upperLeft.X + 1, upperLeft.Y);
            Point lowerLeft = new Point(upperLeft.X, upperLeft.Y + 1);
            Point lowerRight = new Point(upperLeft.X + 1, upperLeft.Y + 1);

            //Get the gradient vectors for each corner
            Vector2 upperLeftGradientVector = grid[upperLeft.X][upperLeft.Y];
            Vector2 upperRightGradientVector = grid[upperRight.X][upperRight.Y];
            Vector2 lowerLeftGradientVector = grid[lowerLeft.X][lowerLeft.Y];
            Vector2 lowerRightGradientVector = grid[lowerRight.X][lowerRight.Y];

            //Get the vectors for each corner
            Vector2 upperLeftVector = new Vector2((float)upperLeft.X * gridSize, (float)upperLeft.Y * gridSize);
            Vector2 upperRightVector = new Vector2((float)upperRight.X * gridSize, (float)upperRight.Y * gridSize);
            Vector2 lowerLeftVector = new Vector2((float)lowerLeft.X * gridSize, (float)lowerLeft.Y * gridSize);
            Vector2 lowerRightVector = new Vector2((float)lowerRight.X * gridSize, (float)lowerRight.Y * gridSize);

            //Get the distances from the specified point to each corner
            Vector2 upperLeftDelta =  (vector - upperLeftVector)/gridSize;
            Vector2 upperRightDelta = (vector - upperRightVector) / gridSize;
            Vector2 lowerLeftDelta = (vector - lowerLeftVector) / gridSize;
            Vector2 lowerRightDelta = (vector - lowerRightVector) / gridSize;

            //Find the dot products of the gradient vectors and the delta vectors
            float upperLeftDot = Vector2.Dot(upperLeftGradientVector, upperLeftDelta);
            float upperRightDot = Vector2.Dot(upperRightGradientVector, upperRightDelta);
            float lowerLeftDot = Vector2.Dot(lowerLeftGradientVector, lowerLeftDelta);
            float lowerRightDot = Vector2.Dot(lowerRightGradientVector, lowerRightDelta);

            //Find the coefficients for linear interpolation from the corners to the specified point
            float lowerRightCoefficient =  upperLeftDelta.X * upperLeftDelta.Y;
            float lowerLeftCoefficient = -upperRightDelta.X * upperRightDelta.Y;
            float upperRightCoefficient= -lowerLeftDelta.X * lowerLeftDelta.Y;
            float upperLeftCoefficient= lowerRightDelta.X * lowerRightDelta.Y;

            //Find the value of the noise at the specified point
            float value = upperLeftDot * upperLeftCoefficient + upperRightDot * upperRightCoefficient + lowerLeftDot * lowerLeftCoefficient + lowerRightDot * lowerRightCoefficient;
            return value;
        }

        /// <summary>
        /// Finds the cell that contains the specified point
        /// </summary>
        /// <param name="i"> x coordinate</param>
        /// <param name="j"> y coordinate</param>
        /// <returns>The upper left point in the cell that contains the specified point</returns>
        /// <exception cref="ArgumentException"> If the point is outside of the grid </exception>
        private Point findVectorCell(float i, float j) { 
            if(i<0 || j<0 || i>=(float)(gridColumns-1)*gridSize || j>=(gridRows-1)*gridSize) 
                throw new ArgumentException("Cell coordinates are outside of bounds "+nameof(i)+":" +i + "," + nameof(j)+":"+j+ "," + " grid size: "+gridSize+", grid rows: "+gridRows+", grid columns: "+gridColumns);         
            
            
            int x = (int)Math.Floor(i / gridSize);
            int y = (int)Math.Floor(j / gridSize);


            return new Point(x, y);
        }
    }
    
}
