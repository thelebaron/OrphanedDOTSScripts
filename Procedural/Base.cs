using UnityEngine;

namespace Plugins.Procedural
{
    public class Base : Seed
    {

        public int XSize;// => m_XSize;
        public int YSize;
        public int ZSize;
	    
        public Vector3 CenterPosition;
	    public Quad floorBounds;

	    public Vector3 cornerx0;
	    public Vector3 cornerx1;
	    public Vector3 cornerx2;
	    public Vector3 cornerx3;

	    public override void Generate()
	    {
		    GetSeed();
		    XSize = pseudoRandom.Next(1, 50);
		    YSize = pseudoRandom.Next(1, 5);
		    ZSize = pseudoRandom.Next(1, 50);
		    
	    }
        
        

	    public bool IsWall(Vector3 pos)
	    {
		    bool returnvalue = false;
		    
		    Vector3 size = new Vector3(XSize, YSize, ZSize);
		    
		    // Check wall 0
		    var z = Utils.GetCornerX0(CenterPosition,size).z + 1;// the 1 is the offset for doubling
		    var z2 = Utils.GetCornerX1(CenterPosition,size).z - 1;

		    var x = Utils.GetCornerX0(CenterPosition,size).x + 1;
		    var x2 = Utils.GetCornerX3(CenterPosition,size).x - 1;

		    //Debug.Log("x = "+ x + " | x2 = " + x2);
		    if ((int) pos.z == z || (int) pos.z == z2) // z wall 0
		    {
			    returnvalue = true;
		    }
		    
		    if ((int) pos.x == x|| (int) pos.x == x2)
		    {
			    returnvalue = true;
		    }
		    
		    return returnvalue;
	    }
        
    }
}