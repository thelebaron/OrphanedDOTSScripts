using UnityEngine;

namespace Plugins.Procedural
{
    public class Floor : Base
    {
        public Vector3[] floorArray;

        public override void Generate()
        {
            base.Generate();
            floorArray = CalculateFloorArray();
        }

        public Vector3[] CalculateFloorArray()
        {

            int cells = XSize * ZSize;

            var array = new Vector3[cells];

            for (int i = 0, z = 1; z <= ZSize; z++)
            {
                for (int x = 1; x <= XSize; x++, i++)
                {
                    var positionalx = x * 2;
                    var positionalz = z * 2;
                    
                    var pos = new Vector3(positionalx + (CenterPosition.x - (XSize + 1)), CenterPosition.y - (float)YSize + 1, positionalz   + (CenterPosition.z - (ZSize + 1)));
                    array[i] = pos;
                }
            }
            
            return array;
        }

        private void OnDrawGizmos()
        {
            floorArray = CalculateFloorArray();
            cornerx0 = Utils.GetCornerX0(CenterPosition, new Vector3(XSize, YSize, ZSize));
            cornerx1 = Utils.GetCornerX1(CenterPosition, new Vector3(XSize, YSize, ZSize));
            cornerx2 = Utils.GetCornerX2(CenterPosition, new Vector3(XSize, YSize, ZSize));
            cornerx3 = Utils.GetCornerX3(CenterPosition, new Vector3(XSize, YSize, ZSize));

            //draw wire cubes
            //foreach(var v in floorArray)
                //Gizmos.DrawWireCube(v, Vector3.one * 2);
           
            //draw walls
            foreach (var v in floorArray)
            {
                if (IsWall(v))
                {
                    Gizmos.color = Color.gray;
                    Gizmos.DrawCube(v, Vector3.one * 1.95f );
                }

                if (!IsWall(v))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(v, Vector3.one * 1.95f );
                }
            }
           
        }
    }
}