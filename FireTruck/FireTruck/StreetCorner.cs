using System.Collections.Generic;

namespace FireTruck
{
    class StreetCorner
    {
        public Dictionary<int, StreetCorner> adjacentStreetCorners = new Dictionary<int, StreetCorner>(); // gets populated in 2nd Pass of Program.cs
        public int number = -1; // dummy value
        public bool visited = false; // starts as false by default

        public StreetCorner(int num) // Constructor
        {
            number = num;
        }
    }
}
