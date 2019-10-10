using UnityEngine;

namespace Plugins.Procedural
{
    public static class Utils
    {
        
                
		/// <summary>
		/// gets the corners of the world position of the bounding box from the center: gross maths!
		/// </summary>
		/// <returns></returns>
		public static Vector3 GetCornerX0(Vector3 centerPosition, Vector3 size)
		{
			var x = centerPosition;
			float PosY = x.y - (float)size.y;
			float PosX = x.x - (float)size.x;
			float PosZ = x.z - (float)size.z;
			
			x = new Vector3(PosX,PosY,PosZ);
			
			return x;
		}
		
		public static Vector3 GetCornerX1(Vector3 centerPosition, Vector3 size)
		{
			var x1 = centerPosition;
			float PosY = x1.y - (float)size.y;
			float PosX = x1.x - (float)size.x;
			float PosZ = x1.z + (float)size.z;
			
			x1 = new Vector3(PosX,PosY,PosZ);
			
			return x1;
		}
		public static Vector3 GetCornerX2(Vector3 centerPosition, Vector3 size)
		{
			var x1 = centerPosition;
			float PosY = x1.y - (float)size.y;
			float PosX = x1.x + (float)size.x;
			float PosZ = x1.z + (float)size.z;
			
			x1 = new Vector3(PosX,PosY,PosZ);
			
			return x1;
		}
		public static Vector3 GetCornerX3(Vector3 centerPosition, Vector3 size)
		{
			var x1 = centerPosition;
			float PosY = x1.y - (float)size.y;
			float PosX = x1.x + (float)size.x;
			float PosZ = x1.z - (float)size.z;
			
			x1 = new Vector3(PosX,PosY,PosZ);
			
			return x1;
		}
		
		public static Vector3 GetCornerA0(Vector3 centerPosition, Vector3 size)
		{
			var x = centerPosition;
			float PosY = x.y + (float)size.y;
			float PosX = x.x - (float)size.x;
			float PosZ = x.z - (float)size.z;
			
			x = new Vector3(PosX,PosY,PosZ);
			
			return x;
		}
		
		public static Vector3 GetCornerA1(Vector3 centerPosition, Vector3 size)
		{
			var x1 = centerPosition;
			float PosY = x1.y + (float)size.y;
			float PosX = x1.x - (float)size.x;
			float PosZ = x1.z + (float)size.z;
			
			x1 = new Vector3(PosX,PosY,PosZ);
			
			return x1;
		}
		public static Vector3 GetCornerA2(Vector3 centerPosition, Vector3 size)
		{
			var x1 = centerPosition;
			float PosY = x1.y + (float)size.y;
			float PosX = x1.x + (float)size.x;
			float PosZ = x1.z + (float)size.z;
			
			x1 = new Vector3(PosX,PosY,PosZ);
			
			return x1;
		}
		public static Vector3 GetCornerA3(Vector3 centerPosition, Vector3 size)
		{
			var x1 = centerPosition;
			float PosY = x1.y + (float)size.y;
			float PosX = x1.x + (float)size.x;
			float PosZ = x1.z - (float)size.z;
			
			x1 = new Vector3(PosX,PosY,PosZ);
			
			return x1;
		}
        
    }
}