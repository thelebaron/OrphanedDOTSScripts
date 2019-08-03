using System.Collections;
using System.Collections.Generic;
//using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Old
{
	public class CoverQueryMainThread : MonoBehaviour
	{
		public List<Vector3> CoverPOSITIONS;

		//private  int rays = 125;
		//private  float curveAmount = 360;
		//private  Vector3 origin;
		//private  float radius = 25;

		// the spoke like positions which are raycast from
		private Vector3[] radialNodes;
		//private  Vector3[] hits;

		private void Update()
		{
			CoverPOSITIONS = CoverPositions(transform.position);


		}

		private void OnDrawGizmos()
		{
			if (CoverPOSITIONS == null) return;
			foreach (var v in CoverPOSITIONS)
			{
				Gizmos.DrawCube(v, Vector3.one / 2);
			}
		}

		public List<Vector3> CoverPositions(Vector3 origin)
		{
			int   rays        = 125;
			float curveAmount = 360;
			float radius      = 25;
			// the spoke like positions which are raycast from

			var   hits      = new List<Vector3>();
			float calcAngle = 0;
			//Vector3 dir;
			radialNodes = new Vector3[rays];

			for (int i = 0; i < rays; i++)
			{
				float posY = math.cos(calcAngle * Mathf.Deg2Rad) * radius;
				float posZ = math.sin(calcAngle * Mathf.Deg2Rad) * radius;

				radialNodes[i] =  origin.normalized + (Vector3.forward * posY) + (Vector3.right * posZ);
				calcAngle      += curveAmount / rays;
				RaycastHit hit;

				if (Physics.Raycast(radialNodes[i] + origin, -radialNodes[i].normalized * radius, out hit, radius))
					hits.Add(hit.point);
			}

			return hits;
		}
	}
}