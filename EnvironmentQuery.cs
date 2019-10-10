using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Old
{
	public class EnvironmentQuery : MonoBehaviour
	{
		private void Start()
		{
			Query();
		}
		
		public GameObject ObjectToTestWith;

		public bool ShowSceneBounds;
		public bool ShowAllPoints;
		public bool ShowEnvironmentPoints;
		public bool ShowPotentialTetherPoints;
		public bool ShowTetherPoints;

		private Vector3 m_worldSize;
		private Vector3 m_worldCenter;

        //	private Vector3 WorldExtents;
		private Vector3 m_worldX0;
		private Vector3 m_worldX1;
		private Vector3 m_worldX2;
		private Vector3 m_worldX3;
		private Vector3 m_worldEndX0;
		private Vector3 m_worldEndX1;
		private Vector3 m_worldEndX2;
		private Vector3 m_worldEndX3;

		public Vector3Int Grid;
		public float GridXSize;
		public float GridYSize;
		public float GridZSize;

		// gets all points/vector3 positions set by the grid size
		[HideInInspector] public List<Vector3> AllEnvironmentPoints;

		public int AllEnvironmentPointsCount;

		// gets only the ones that are valid positions after being raycasted and spherecasted
		public List<Vector3> EvaluatedEnvironmentPoints;

		// tether points are evaluated points that are not too close to each other
		public List<Vector3> PotentialTetherPoints;
		public List<TetherPoint> Tethers;
		public List<Vector3> TetherPoints;
		public List<Vector3> ValidTetherPoints;

		public float TetherRadius = 1;
		public float nearestNodeDistance = 0.5f;

		private void Query()
		{
			GetSceneBounds();
			GetAllNodes();
			EvaluateNodes();
			GetTetherPoints();
			EvaluateRealtimeStatus();
		}

		public void GetSceneBounds()
		{
			//WorldBounds = RecastGraph.size

			var b = new Bounds(Vector3.zero, Vector3.zero);
			foreach (Renderer r in FindObjectsOfType(typeof(Renderer)))
			{
				b.Encapsulate(r.bounds);
			}

			// get scene bounds
			//	WorldExtents = b.extents;
			m_worldCenter = b.center;
			m_worldSize   = b.size;


			// calculate x corner
			m_worldX0 = m_worldCenter;
			var x = m_worldSize.x / 2;
			var y = m_worldSize.y / 2;
			var z = m_worldSize.z / 2;

			// continue calculating corners 
			m_worldX0.x -= x;
			m_worldX0.y -= y;
			m_worldX0.z -= z;
			m_worldX1   =  m_worldX0;
			m_worldX1.x += (x * 2);
			m_worldX2   =  m_worldX1;
			m_worldX2.y += (y * 2);
			m_worldX3   =  m_worldX2;
			m_worldX3.x -= (x * 2);

			// continue calculating corners 
			m_worldEndX0   =  m_worldX0;
			m_worldEndX0.z += (z * 2);
			m_worldEndX1   =  m_worldX1;
			m_worldEndX1.z += (z * 2);
			m_worldEndX2   =  m_worldX2;
			m_worldEndX2.z += (z * 2);
			m_worldEndX3   =  m_worldX3;
			m_worldEndX3.z += (z * 2);

			// calculate grid size between points using total size divided by the number of points in the grid
			GridXSize = m_worldSize.x / Grid.x;
			GridYSize = m_worldSize.y / Grid.y;
			GridZSize = m_worldSize.z / Grid.z;

		}

		public void GetAllNodes()
		{
			// make a list of all the nodes to check
			AllEnvironmentPoints = null;
			AllEnvironmentPoints = new List<Vector3>();

			var pos = new Vector3(m_worldX0.x - GridXSize, m_worldX0.y - GridYSize, m_worldX0.z - GridZSize);

			for (int i = 0, y = 0; y <= Grid.y; y++)
			{
				pos.y += GridYSize;
				var newX = pos.x;

				for (var x = 0; x <= Grid.x; x++)
				{

					newX += GridXSize;
					var newZ = pos.z;

					for (int z = 0; z <= Grid.z; z++, i++)
					{
						newZ += GridZSize;
						AllEnvironmentPoints.Add(new Vector3(newX, pos.y, newZ));

					}
				}
			}

			AllEnvironmentPointsCount = AllEnvironmentPoints.Count;

		}

		public void EvaluateNodes()
		{
			EvaluatedEnvironmentPoints = null;
			EvaluatedEnvironmentPoints = new List<Vector3>();



			foreach (var v in AllEnvironmentPoints)
			{
				if (ValidPoint(v))
					EvaluatedEnvironmentPoints.Add(v);

			}
		}

		public void GetTetherPoints()
		{
			// part 1
			PotentialTetherPoints = null;
			PotentialTetherPoints = new List<Vector3>();

			foreach (var v in EvaluatedEnvironmentPoints)
			{
				if (PotentialTether(v, TetherRadius))
					PotentialTetherPoints.Add(v);
			}

			// part 2
			TetherPoints = null;
			TetherPoints = new List<Vector3>();
			Tethers      = null;
			Tethers      = new List<TetherPoint>();

			for (var i = 0; i < PotentialTetherPoints.Count; i++)
			{
				/*
					//http://forum.arongranberg.com/t/how-to-get-closest-point-not-node/178/3
					// Find the closest node to this GameObject's position
					NNInfo info = AstarPath.active.GetNearest (PotentialTetherPoints[i]);
					Vector3 closest = info.position;
					TetherPoints.Add(closest);
					
					var tp = new TetherPoint();
					tp.Point = closest;
					Tethers.Add(tp);
							
					foreach (var tt in Tethers)
						tt.CheckPlayerVisibility();
					*/
			}
		}

		private void EvaluateRealtimeStatus()
		{
			ValidTetherPoints = null;
			ValidTetherPoints = new List<Vector3>();

			foreach (var tp in Tethers)
			{
				tp.CheckPlayerVisibility();
			}
		}

		private void OnDrawGizmos()
		{
			if (ShowSceneBounds)
			{
				Gizmos.color = Color.white;
				// draw debug box around world
				Gizmos.DrawWireCube(m_worldCenter, m_worldSize);
				// draw debug spheres at all 8 vertices of worldbounds
				Gizmos.color = Color.magenta;
				Gizmos.DrawWireSphere(m_worldX0,    1f);
				Gizmos.DrawWireSphere(m_worldX1,    1f);
				Gizmos.DrawWireSphere(m_worldX2,    1f);
				Gizmos.DrawWireSphere(m_worldX3,    1f);
				Gizmos.DrawWireSphere(m_worldEndX0, 1f);
				Gizmos.DrawWireSphere(m_worldEndX1, 1f);
				Gizmos.DrawWireSphere(m_worldEndX2, 1f);
				Gizmos.DrawWireSphere(m_worldEndX3, 1f);
			}

			if (ShowAllPoints)
			{
				Gizmos.color = Color.black;
				for (var i = 0; i < AllEnvironmentPoints.Count; i++)
					Gizmos.DrawWireSphere(AllEnvironmentPoints[i], 0.05f);
				Debug.Log("This is super slow, uncomment to use.");
			}

			if (ShowEnvironmentPoints)
			{
				Gizmos.color = Color.white;
				foreach (var t in EvaluatedEnvironmentPoints)
					Gizmos.DrawSphere(t, 0.15f);
			}

			if (ShowPotentialTetherPoints)
			{
				Gizmos.color = Color.blue;
				foreach (var t in PotentialTetherPoints)
				{
					Gizmos.DrawSphere(t, 0.35f);
					for (int o = 0; o < 6; o++)
					{
						Gizmos.DrawWireSphere(FindCirclePoint(t, 60f, TetherRadius, o), 0.15f);
					}
				}
			}

			if (ShowTetherPoints)
			{
				Gizmos.color = Color.green;
				foreach (var t in TetherPoints)
				{
					Gizmos.DrawSphere(t, 0.2f);

					for (int o = 0; o < 6; o++)
					{
						Gizmos.DrawWireSphere(FindCirclePoint(t, 60f, TetherRadius, o), 0.05f);
					}
				}

				/*
				Gizmos.color = Color.blue;
				for (var i = 0; i < PotentialTetherPoints.Count; i++)
				{
					Gizmos.DrawSphere(PotentialTetherPoints[i], 0.35f);
					for (int o = 0; o < 6; o++)
					{
						Gizmos.DrawWireSphere(FindCirclePoint(PotentialTetherPoints[i],60f, TetherRadius, o), 0.15f);
					}
				}
				*/
			}


			foreach (var tp in Tethers)
			{
				//tp.CheckPlayerVisibility();
				if (tp.IsVisible)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawSphere(tp.Point, 0.2f);
				}

				if (!tp.IsVisible)
				{
					Gizmos.color = Color.red;
					Gizmos.DrawSphere(tp.Point, 0.2f);
				}
			}

		}


		// from http://blackpawn.com/texts/pointinpoly/
		// returns if a vector p is inside a triangle's bounds
		private bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
		{
			return SameSide(p, a, b, c) && SameSide(p, b, a, c) && SameSide(p, c, a, b);
		}

		// returns if the vector is on the same side of another vector 
		private bool SameSide(Vector3 p1, Vector3 p2, Vector3 a, Vector3 b)
		{
			var cp1 = Vector3.Cross(b - a, p1 - a);
			var cp2 = Vector3.Cross(b - a, p2 - a);
			return Vector3.Dot(cp1, cp2) >= 0;
		}

		private static Vector3 FindCirclePoint(Vector3 center, float angle, float radius, int i)
		{
			return center + Quaternion.AngleAxis(angle * i, Vector3.up) * (Vector3.right * radius);
		}

		private bool ValidPoint(Vector3 p /*, float d*/)
		{
			var        dist = GridYSize;
			RaycastHit hit;
			Physics.Raycast(p, Vector3.down, out hit, dist, GameLayers.MASK_SIGHT);

			return hit.collider != null && hit.collider.gameObject.CompareTag(Tags.Terrain);
		}

		private bool PotentialTether(Vector3 point, float tetherRadius)
		{
			var dist = GridYSize;
			int i;
			for (i = 0; i < 6; i++)
			{
				RaycastHit hit;
				Physics.Raycast(FindCirclePoint(point, 60f, tetherRadius, i), Vector3.down, out hit, dist,
					GameLayers.MASK_SIGHT);
				if (hit.collider == null || !hit.collider.gameObject.CompareTag(Tags.Terrain))
					return false;
			}

			return true;
		}

		private static bool TracePointToObject(Vector3 point, GameObject obj)
		{
			var        pos = point                  + Vector3.up;
			var        dir = obj.transform.position - pos;
			RaycastHit hit;
			Physics.Raycast(pos, dir, out hit, Mathf.Infinity, GameLayers.MASK_SIGHT);
			if (hit.collider.gameObject.tag == "Player")
			{
				//Debug.Log(hit.collider);
				return true;
			}

			return false; //hit.collider.gameObject.tag == "Player";
		}

		private void ScanActive()
		{
			//AstarPath.active.Scan();
		}
	}

	[Serializable]
	public class TetherPoint
	{
		public Vector3 Point;
		public bool IsVisible;

		public void CheckPlayerVisibility()
		{
			var        obj = GameObject.FindGameObjectWithTag("Player");
			var        pos = Point                  + Vector3.up;
			var        dir = obj.transform.position - pos;
			RaycastHit hit;
			Physics.Raycast(pos, dir, out hit, Mathf.Infinity, GameLayers.MASK_SIGHT);

			if (hit.collider.gameObject == obj)
			{
				IsVisible = true;
			}
			else
			{
				IsVisible = false;

			}


		}

	
	}
}