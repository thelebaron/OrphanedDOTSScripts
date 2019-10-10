
using UnityEditor;
using UnityEngine;

namespace Plugins.Procedural
{
	[CustomEditor(typeof(Floor))]
	public class FloorEditor : Editor
	{
		public Vector3 center;
		public int xSize;
		public int ySize;
		public int zSize;
		
		private Base script;
		//private float dashSize = 0.2f;
		private GameObject mesh;
	
		public override void OnInspectorGUI()
		{
			script = (Floor) target;
			DrawDefaultInspector();
			center = script.CenterPosition;
			center = EditorGUILayout.Vector3Field("Center", center);
			script.CenterPosition = center;

			xSize = script.XSize;
			ySize = script.YSize;
			zSize = script.ZSize;
			xSize = EditorGUILayout.IntField("size x:", xSize);
			ySize = EditorGUILayout.IntField("size y:", ySize);
			zSize = EditorGUILayout.IntField("size z:", zSize);
			
			script.XSize = xSize; script.YSize = ySize; script.ZSize = zSize;
			
			mesh = (GameObject) EditorGUILayout.ObjectField("test prefab", mesh , typeof(GameObject), false);

			
			if(GUILayout.Button("Generate"))
			{
				script.Generate();
			}
		}

		protected virtual void OnSceneGUI()
		{
			script = (Base) target;

			Handles.color = Color.yellow;
			Vector3 size = new Vector3(script.XSize, script.YSize, script.ZSize);
			
			EditorGUI.BeginChangeCheck();
			
			//init grab saved values
			Vector3 newCenterPosition = script.CenterPosition;
			
			newCenterPosition = Handles.PositionHandle(script.CenterPosition, Quaternion.identity);
			var rounded = new Vector3(Mathf.RoundToInt(newCenterPosition.x),Mathf.RoundToInt(newCenterPosition.y),Mathf.RoundToInt(newCenterPosition.z));
			script.CenterPosition = rounded;
			
			Handles.DrawWireCube( script.CenterPosition, new Vector3(script.XSize * 2, script.YSize*2, script.ZSize* 2));
			GUIStyle style = new GUIStyle(); style.normal.textColor = Color.green;


			Handles.Label( script.CenterPosition, "Center", style );
			
			
			//draw spheres at the bottom points
			Handles.SphereHandleCap(0, Utils.GetCornerX0(script.CenterPosition, size), Quaternion.identity, 0.2f, EventType.Repaint);
			Handles.Label(Utils.GetCornerX0(script.CenterPosition, size), "x0", style );
			Handles.SphereHandleCap(0, Utils.GetCornerX1(script.CenterPosition, size), Quaternion.identity, 0.2f, EventType.Repaint);
			Handles.Label(Utils.GetCornerX1(script.CenterPosition, size), "x1", style );
			Handles.SphereHandleCap(0, Utils.GetCornerX2(script.CenterPosition, size), Quaternion.identity, 0.2f, EventType.Repaint);
			Handles.Label(Utils.GetCornerX2(script.CenterPosition, size), "x2", style );
			Handles.SphereHandleCap(0, Utils.GetCornerX3(script.CenterPosition, size), Quaternion.identity, 0.2f, EventType.Repaint);
			Handles.Label(Utils.GetCornerX3(script.CenterPosition, size), "x3", style );
			
			//draw spheres at the top points
			Handles.SphereHandleCap(0, Utils.GetCornerA0(script.CenterPosition, size), Quaternion.identity, 0.2f, EventType.Repaint);
			Handles.Label(Utils.GetCornerA0(script.CenterPosition, size), "a0", style );
			Handles.SphereHandleCap(0, Utils.GetCornerA1(script.CenterPosition, size), Quaternion.identity, 0.2f, EventType.Repaint);
			Handles.Label(Utils.GetCornerA1(script.CenterPosition, size), "a1", style );
			Handles.SphereHandleCap(0, Utils.GetCornerA2(script.CenterPosition, size), Quaternion.identity, 0.2f, EventType.Repaint);
			Handles.Label(Utils.GetCornerA2(script.CenterPosition, size), "a2", style );
			Handles.SphereHandleCap(0, Utils.GetCornerA3(script.CenterPosition, size), Quaternion.identity, 0.2f, EventType.Repaint);
			Handles.Label(Utils.GetCornerA3(script.CenterPosition, size), "a3", style );
			Repaint();
			
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(script, "Change Target Position");
			}
			//Handles.ArrowHandleCap(0, mapBoundsD, Quaternion.identity, 1, EventType.Repaint);
		}

		

	}
}