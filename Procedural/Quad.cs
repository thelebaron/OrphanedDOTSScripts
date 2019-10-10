using System;
using UnityEngine;

namespace Plugins.Procedural
{
    [Serializable]
    public class Quad
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
        public Vector3 d;
    }
    
    [Serializable]
    public class Room
    {
        public Vector3 mapCenter;
        public Vector3 position;
        public Cell[] cells;
    }
    
    [Serializable]
    public class Cell
    {
        public Vector3 mapCenter;
        
        public enum CellType
        {
            Floor, Wall, Ceiling, Middle
        }

        public Vector3 facingDir;
        public Color color;
        
        public Vector3 position;
        public const int size = 2;
        
    }
        

    
}