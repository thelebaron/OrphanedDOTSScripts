using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

[ExecuteInEditMode]
public class PathTester : MonoBehaviour
{
    public Transform Source;
    public Transform Target;
    public PathFindingSystem System;

    public enum PathFindingSystem
    {
        Default = 0,
        Experimental,
    }

    void Update()
    {
        if (Source == null || Target == null)
            return;

        if (System == PathFindingSystem.Default)
        {
            NavMeshPath pathResult = new NavMeshPath();
            if (NavMesh.CalculatePath(Source.position, Target.position, NavMesh.AllAreas, pathResult))
            {
                DebugDrawPath(pathResult.corners);
            }
        }
        else if (System == PathFindingSystem.Experimental)
        {
            var agentId = GetAgentTypeId("Humanoid");
            if (TryFindPath(Source.position, Target.position, agentId, 0.1f, NavMesh.AllAreas, out Vector3[] path))
            {
                DebugDrawPath(path);
            }
        }
    }

    bool TryFindPath(Vector3 start, Vector3 end, int agentId, float agentRadius, int areas, out Vector3[] path)
    {
        const int maxPathLength = 100;

        using (var result = new NativeArray<PolygonId>(100, Allocator.Persistent))
        using (var query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, 100))
        {
            var from = query.MapLocation(start, Vector3.one * 10, 0);
            var to = query.MapLocation(end, Vector3.one * 10, 0);
            var status = query.BeginFindPath(from, to, areas);
            int maxIterations = 1024;

            while (true)
            {
                switch (status)
                {
                    case PathQueryStatus.InProgress:
                        status = query.UpdateFindPath(maxIterations, out int currentIterations);
                        break;

                    case PathQueryStatus.Success:

                        var finalStatus = query.EndFindPath(out int pathLength);
                        var pathResult = query.GetPathResult(result);
                        var straightPath = new NativeArray<NavMeshLocation>(pathLength, Allocator.Temp);
                        var straightPathFlags = new NativeArray<StraightPathFlags>(pathLength, Allocator.Temp);
                        var vertexSide = new NativeArray<float>(pathLength, Allocator.Temp);

                        try
                        {
                            int straightPathCount = 0;
                            var pathStatus = PathUtils.FindStraightPath(query, start, end, result, pathLength, ref straightPath, ref straightPathFlags, ref vertexSide, ref straightPathCount, maxPathLength);
                            if (pathStatus == PathQueryStatus.Success)
                            {
                                path = new Vector3[straightPathCount];
                                for (int i = 0; i < straightPathCount; i++)
                                {
                                    path[i] = straightPath[i].position;
                                }
                                return true;
                            }

                            path = default;
                            Debug.Log($"Nav query failed with the status: {status}");
                            return false;
                        }
                        finally
                        {
                            straightPath.Dispose();
                            straightPathFlags.Dispose();
                            vertexSide.Dispose();
                        }

                    case PathQueryStatus.Failure:
                        path = default;
                        return false;

                    default:
                        Debug.Log($"Nav query failed with the status: {status}");
                        path = default;
                        return false;
                }
            }
        }
    }

    private int GetAgentTypeId(string agentName)
    {        
        var count = NavMesh.GetSettingsCount();
        for (var i = 0; i < count; i++)
        {
            var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
            if (NavMesh.GetSettingsNameFromID(id) == agentName)
                return i;
        }
        throw new ArgumentException($"NavMeshAgent with name '{agentName}' not found");
    }

    private void DebugDrawPath(Vector3[] corners)
    {
        if (corners.Length > 0)
        {
            Debug.DrawLine(Source.transform.position, corners[0], Color.blue);

            for (int i = 1; i < corners.Length; i++)
            {
                Debug.DrawLine(corners[i - 1], corners[i], Color.blue);
            }            
            Debug.DrawLine(Target.transform.position, corners[corners.Length-1], Color.blue);
        }
    }

    //
    // Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
    //
    // This software is provided 'as-is', without any express or implied
    // warranty.  In no event will the authors be held liable for any damages
    // arising from the use of this software.
    // Permission is granted to anyone to use this software for any purpose,
    // including commercial applications, and to alter it and redistribute it
    // freely, subject to the following restrictions:
    // 1. The origin of this software must not be misrepresented; you must not
    //    claim that you wrote the original software. If you use this software
    //    in a product, an acknowledgment in the product documentation would be
    //    appreciated but is not required.
    // 2. Altered source versions must be plainly marked as such, and must not be
    //    misrepresented as being the original software.
    // 3. This notice may not be removed or altered from any source distribution.
    //

    // The original source code has been modified by Unity Technologies, Zulfa Juniadi and Jeffrey Vella.

    // This code is from https://github.com/zulfajuniadi/unity-ecs-navmesh/blob/master/Assets/NavJob/Systems/NavMeshQuerySystem.cs

    [Flags]
    public enum StraightPathFlags
    {
        Start = 0x01, // The vertex is the start position.
        End = 0x02, // The vertex is the end position.
        OffMeshConnection = 0x04 // The vertex is start of an off-mesh link.
    }

    public class PathUtils
    {
        public static float Perp2D(Vector3 u, Vector3 v)
        {
            return u.z * v.x - u.x * v.z;
        }

        public static void Swap(ref Vector3 a, ref Vector3 b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        // Retrace portals between corners and register if type of polygon changes
        public static int RetracePortals(NavMeshQuery query, int startIndex, int endIndex, NativeSlice<PolygonId> path, int n, Vector3 termPos, ref NativeArray<NavMeshLocation> straightPath, ref NativeArray<StraightPathFlags> straightPathFlags, int maxStraightPath)
        {


            for (var k = startIndex; k < endIndex - 1; ++k)
            {
                var type1 = query.GetPolygonType(path[k]);
                var type2 = query.GetPolygonType(path[k + 1]);
                if (type1 != type2)
                {
                    Vector3 l, r;
                    var status = query.GetPortalPoints(path[k], path[k + 1], out l, out r);
                    float3 cpa1, cpa2;
                    GeometryUtils.SegmentSegmentCPA(out cpa1, out cpa2, l, r, straightPath[n - 1].position, termPos);
                    straightPath[n] = query.CreateLocation(cpa1, path[k + 1]);

                    straightPathFlags[n] = (type2 == NavMeshPolyTypes.OffMeshConnection) ? StraightPathFlags.OffMeshConnection : 0;
                    if (++n == maxStraightPath)
                    {
                        return maxStraightPath;
                    }
                }
            }

            if (n > straightPath.Length-1) // Fix for IndexOutOfRangeException with one point on straightPath[n]
            {
                return n;
            }

            straightPath[n] = query.CreateLocation(termPos, path[endIndex]);
            straightPathFlags[n] = query.GetPolygonType(path[endIndex]) == NavMeshPolyTypes.OffMeshConnection ? StraightPathFlags.OffMeshConnection : 0;
            return ++n;
        }

        public static PathQueryStatus FindStraightPath(NavMeshQuery query, Vector3 startPos, Vector3 endPos, NativeSlice<PolygonId> path, int pathSize, ref NativeArray<NavMeshLocation> straightPath, ref NativeArray<StraightPathFlags> straightPathFlags, ref NativeArray<float> vertexSide, ref int straightPathCount, int maxStraightPath)
        {
            if (!query.IsValid(path[0]))
            {
                straightPath[0] = new NavMeshLocation(); // empty terminator
                return PathQueryStatus.Failure; // | PathQueryStatus.InvalidParam;
            }

            straightPath[0] = query.CreateLocation(startPos, path[0]);

            straightPathFlags[0] = StraightPathFlags.Start;

            var apexIndex = 0;
            var n = 1;

            if (pathSize > 1)
            {
                var startPolyWorldToLocal = query.PolygonWorldToLocalMatrix(path[0]);

                var apex = startPolyWorldToLocal.MultiplyPoint(startPos);
                var left = new Vector3(0, 0, 0); // Vector3.zero accesses a static readonly which does not work in burst yet
                var right = new Vector3(0, 0, 0);
                var leftIndex = -1;
                var rightIndex = -1;

                for (var i = 1; i <= pathSize; ++i)
                {
                    var polyWorldToLocal = query.PolygonWorldToLocalMatrix(path[apexIndex]);

                    Vector3 vl, vr;
                    if (i == pathSize)
                    {
                        vl = vr = polyWorldToLocal.MultiplyPoint(endPos);
                    }
                    else
                    {
                        var success = query.GetPortalPoints(path[i - 1], path[i], out vl, out vr);
                        if (!success)
                        {
                            return PathQueryStatus.Failure; // | PathQueryStatus.InvalidParam;
                        }

                        vl = polyWorldToLocal.MultiplyPoint(vl);
                        vr = polyWorldToLocal.MultiplyPoint(vr);
                    }

                    vl = vl - apex;
                    vr = vr - apex;

                    // Ensure left/right ordering
                    if (Perp2D(vl, vr) < 0)
                        Swap(ref vl, ref vr);

                    // Terminate funnel by turning
                    if (Perp2D(left, vr) < 0)
                    {
                        var polyLocalToWorld = query.PolygonLocalToWorldMatrix(path[apexIndex]);
                        var termPos = polyLocalToWorld.MultiplyPoint(apex + left);

                        n = RetracePortals(query, apexIndex, leftIndex, path, n, termPos, ref straightPath, ref straightPathFlags, maxStraightPath);
                        if (vertexSide.Length > 0)
                        {
                            vertexSide[n - 1] = -1;
                        }

                        //Debug.Log("LEFT");

                        if (n == maxStraightPath)
                        {
                            straightPathCount = n;
                            return PathQueryStatus.Success; // | PathQueryStatus.BufferTooSmall;
                        }

                        apex = polyWorldToLocal.MultiplyPoint(termPos);
                        left.Set(0, 0, 0);
                        right.Set(0, 0, 0);
                        i = apexIndex = leftIndex;
                        continue;
                    }
                    if (Perp2D(right, vl) > 0)
                    {
                        var polyLocalToWorld = query.PolygonLocalToWorldMatrix(path[apexIndex]);
                        var termPos = polyLocalToWorld.MultiplyPoint(apex + right);

                        n = RetracePortals(query, apexIndex, rightIndex, path, n, termPos, ref straightPath, ref straightPathFlags, maxStraightPath);
                        if (vertexSide.Length > 0)
                        {
                            vertexSide[n - 1] = 1;
                        }

                        //Debug.Log("RIGHT");

                        if (n == maxStraightPath)
                        {
                            straightPathCount = n;
                            return PathQueryStatus.Success; // | PathQueryStatus.BufferTooSmall;
                        }

                        apex = polyWorldToLocal.MultiplyPoint(termPos);
                        left.Set(0, 0, 0);
                        right.Set(0, 0, 0);
                        i = apexIndex = rightIndex;
                        continue;
                    }

                    // Narrow funnel
                    if (Perp2D(left, vl) >= 0)
                    {
                        left = vl;
                        leftIndex = i;
                    }
                    if (Perp2D(right, vr) <= 0)
                    {
                        right = vr;
                        rightIndex = i;
                    }
                }
            }

            // Remove the the next to last if duplicate point - e.g. start and end positions are the same
            // (in which case we have get a single point)
            if (n > 0 && (straightPath[n - 1].position == endPos))
                n--;


            n = RetracePortals(query, apexIndex, pathSize - 1, path, n, endPos, ref straightPath, ref straightPathFlags, maxStraightPath);
            

            if (vertexSide.Length > 0)
            {
                vertexSide[n - 1] = 0;
            }

            if (n == maxStraightPath)
            {
                straightPathCount = n;
                return PathQueryStatus.Success; // | PathQueryStatus.BufferTooSmall;
            }

            // Fix flag for final path point
            straightPathFlags[n - 1] = StraightPathFlags.End;

            straightPathCount = n;
            return PathQueryStatus.Success;
        }
    }

    public class GeometryUtils
    {

        // Calculate the closest point of approach for line-segment vs line-segment.
        public static bool SegmentSegmentCPA(out float3 c0, out float3 c1, float3 p0, float3 p1, float3 q0, float3 q1)
        {
            var u = p1 - p0;
            var v = q1 - q0;
            var w0 = p0 - q0;

            float a = math.dot(u, u);
            float b = math.dot(u, v);
            float c = math.dot(v, v);
            float d = math.dot(u, w0);
            float e = math.dot(v, w0);

            float den = (a * c - b * b);
            float sc, tc;

            if (den == 0)
            {
                sc = 0;
                tc = d / b;

                // todo: handle b = 0 (=> a and/or c is 0)
            }
            else
            {
                sc = (b * e - c * d) / (a * c - b * b);
                tc = (a * e - b * d) / (a * c - b * b);
            }

            c0 = math.lerp(p0, p1, sc);
            c1 = math.lerp(q0, q1, tc);

            return den != 0;
        }
    }

}
