using System.Collections.Generic;
using Game.Old;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimpleMover)), CanEditMultipleObjects]
public class SimpleMoverEditor : Editor
{
    private float lastSendTime;
    protected virtual void OnSceneGUI()
    {
        float dashSize = 4.0f;

        SimpleMover simpleMover = (SimpleMover) target;

        EditorGUI.BeginChangeCheck();
        
        //check null or static
        if (simpleMover.MoverTransform == null)
        {
            Debug.LogWarning("Missing simplemover transform on " + simpleMover.name);
            return;
        }

        var rendererList = new List<Renderer>();
        foreach (Renderer objectRenderer in simpleMover.MoverTransform.GetComponentsInChildren<Renderer>()) {
            rendererList.Add(objectRenderer);
            if (objectRenderer.isPartOfStaticBatch)
            {
                Debug.LogError("static object found on simplemover, please set to nonstatic");
                break;
            }
        }

        //if (Application.isPlaying)
        var temp =1f - Mathf.Clamp01(Time.time - lastSendTime);
        Handles.color = Color.Lerp(Color.white, Color.green, temp);
        Handles.color = Color.cyan;
        var dir = simpleMover.end - simpleMover.start ;
        var steps = Mathf.FloorToInt((simpleMover.start - simpleMover.end).magnitude);
        for (var i = 0; i < steps; i++)
        {
            Handles.ArrowHandleCap(0, simpleMover.start + (dir * i), Quaternion.LookRotation(dir), 1, EventType.Repaint);
        }

        lastSendTime = Time.time;
        
        
/*
        
        if (simpleMover.triggerGo == null &&
            simpleMover.interactionType == SimpleMover.InteractionType.Activate)
        {
            
            simpleMover.triggerGo = new GameObject("trigger");
            simpleMover.triggerGo.transform.position = simpleMover.transform.position;
            simpleMover.triggerGo.transform.rotation = simpleMover.transform.rotation;
            simpleMover.triggerGo.transform.SetParent(simpleMover.transform);
            simpleMover.triggerGo.AddComponent<BoxCollider>();
            simpleMover.triggerGo.transform.GetComponent<BoxCollider>().isTrigger = true;

            simpleMover.triggerGo.AddComponent<TriggerChild>();
            simpleMover.triggerGo.transform.GetComponent<TriggerChild>().simpleMover = simpleMover;

            
        }*/
        
        if (!Application.isPlaying)
        {
            var startPos = simpleMover.transform.position;


            var endPos = Handles.PositionHandle(simpleMover.end, Quaternion.identity);
            simpleMover.end = endPos;
            simpleMover.start = simpleMover.transform.position;
        }

        Handles.Label(simpleMover.start, "Start");
        Handles.Label(simpleMover.end, "End");
        Handles.DrawDottedLine(simpleMover.start, simpleMover.end, dashSize);
        
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(simpleMover, "Change SimpleTranslator settings");

        }
    }
}
