using UnityEditor;
using UnityEngine;

namespace Deprecated
{
#if (UNITY_EDITOR) 
	public static class LightGizmoDrawer
	{
		[DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
		static void DrawGizmoForMyScript(Light scr, GizmoType gizmoType)
		{
			if (!Util.editorDebug)
				return;
			
			var finalsize = 0.5f;
			var cubesize = Vector3.one * finalsize;
			var color = new Color(0.98f, 1f, 0f, 0.20f);
			Gizmos.color = color;
			GUIStyle style = new GUIStyle();
			style.normal.textColor = color;
			Handles.color = Color.blue;
			if(Util.editorDebug)
				Handles.Label(scr.transform.position, "" + scr.intensity, style);
			//Gizmos.color = Color.yellow;
			Gizmos.DrawCube(scr.transform.position, cubesize);
			
			if(scr.lightmapBakeType == LightmapBakeType.Realtime)
				Gizmos.color = Color.white;
			if(scr.lightmapBakeType == LightmapBakeType.Mixed)
				Gizmos.color = Color.yellow;
			if(scr.lightmapBakeType == LightmapBakeType.Baked)
				Gizmos.color = Color.black;
			
			Gizmos.DrawWireCube(scr.transform.position, cubesize);
		}
	}
#endif
}