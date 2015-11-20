using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Misc;

namespace Datastructures
{
    class Grid
    {
        public interface Element
        {
            Vector2<float> getPosition();
        }

        private class Cell
        {
            public List<Element> elements = new List<Element>();
        }

        


        public void build(int subdivisionCount, List<Element> elements)
        {
            Cell[] cells;
            int i;

            this.subdivisionCount = subdivisionCount;

            cells = new Cell[subdivisionCount * subdivisionCount];
            for( i = 0; i < subdivisionCount*subdivisionCount; i++ )
            {
                cells[i] = new Cell();
            }

            // put ach element into the oresponding cell
            foreach( Element iterationElement in elements )
            {
                Vector2<float> elementPosition;
                int indexX;
                int indexY;

                elementPosition = iterationElement.getPosition();

                getIndexOfPositionBound(elementPosition, out indexX, out indexY);

                cells[indexX + indexY].elements.Add(iterationElement);
            }


        }

        public List<Element> getElementsInRadius(Vector2<float> position, float radius)
        {
            int indexX, indexY;

            int iterationMinIndexX;
            int iterationMinIndexY;
            int iterationMaxIndexX;
            int iterationMaxIndexY;

            int iterationX, iterationY;

            Vector2<float> radiusVector;

            List<Element> result;

            radiusVector = new Vector2<float>();
            radiusVector.x = radius;
            radiusVector.x = radius;


            result = new List<Element>();

            getIndexOfPositionBound(position, out indexX, out indexY);

            getIndexOfPosition(position - radiusVector, false, out iterationMinIndexX, out iterationMinIndexY);
            getIndexOfPosition(position + radiusVector, false, out iterationMaxIndexX, out iterationMaxIndexY);

            for (iterationY = iterationMinIndexY; iterationY <= iterationMaxIndexY; iterationY++ )
            {
                for (iterationX = iterationMinIndexX; iterationX <= iterationMaxIndexX; iterationX++ )
                {
                    if(
                        iterationX < 0 || iterationX >= subdivisionCount  ||
                        iterationY < 0 || iterationY >= subdivisionCount
                    )
                    {
                        continue;
                    }

                    foreach( Element iterationElement in cells[iterationX + subdivisionCount*iterationY].elements )
                    {
                        if( (iterationElement.getPosition() - position).magnitude() < radius )
                        {
                            result.Add(iterationElement);
                        }
                    }
                }
            }

            return result;
        }

        private void getIndexOfPositionBound(Vector2<float> position, out int x, out int y)
        {
            getIndexOfPosition(position, true, out x, out y);
        }

        private void getIndexOfPosition(Vector2<float> position, bool bound, out int x, out int y)
        {
            Vector2<float> relativePosition = position - boundMin;
            relativePosition.x /= (boundMax.x - boundMin.x);
            relativePosition.y /= (boundMax.y - boundMin.y);

            if( bound )
            {
                System.Diagnostics.Debug.Assert(relativePosition.x >= 0.0f && relativePosition.x <= 1.0f);
                System.Diagnostics.Debug.Assert(relativePosition.y >= 0.0f && relativePosition.y <= 1.0f);
            }

            x = (int)(relativePosition.x * subdivisionCount);
            y = (int)(relativePosition.y * subdivisionCount);

            if( bound )
            {
                System.Diagnostics.Debug.Assert(x >= 0 && x < subdivisionCount);
                System.Diagnostics.Debug.Assert(y >= 0 && y < subdivisionCount);
            }
        }

        public Misc.Vector2<float> boundMin;
        public Misc.Vector2<float> boundMax;

        private Cell[] cells;
        private int subdivisionCount;

        private Vector2<float> cellSize;
    }
}
