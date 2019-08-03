using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class NavAgentProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new NavAgent());
        //dstManager.AddComponentData(entity, new Translation());
        //dstManager.AddComponentData(entity, new Rotation());
        //dstManager.AddComponentData(entity, new LocalToWorld());
        //dstManager.AddComponentData(entity, new Translation());
    }
}

public struct NavAgent : IComponentData
{
    public NavMeshLocation NavMeshLocation;
    public PathQueryStatus PathQueryStatus;
    public NavAgent(NavMeshLocation navMeshLocation, PathQueryStatus pathQueryStatus)
    {
        NavMeshLocation = navMeshLocation;
        PathQueryStatus = pathQueryStatus;
    }
}

public class NavigationSystem : JobComponentSystem
{
    public EntityQuery AgentsGroup;
    private NavMeshQuery NavMeshQuery;

    protected override void OnCreate()
    {
        AgentsGroup = GetEntityQuery(typeof(NavAgent), ComponentType.ReadWrite<Translation>(),
            ComponentType.ReadWrite<Rotation>() ,ComponentType.ReadWrite<LocalToWorld>());
        var navMeshWorld = NavMeshWorld.GetDefaultWorld();
        NavMeshQuery = new NavMeshQuery(navMeshWorld, Allocator.Persistent,100);
    }

    protected override void OnDestroyManager()
    {
        NavMeshQuery.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new MoveJob
        {
            query = NavMeshQuery,
            navAgent = AgentsGroup.ToComponentDataArray<NavAgent>(Allocator.TempJob),
            localToWorld = AgentsGroup.ToComponentDataArray<LocalToWorld>(Allocator.TempJob)
            //Translations = AgentsGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
            //PathStatus =new NativeArray<PathQueryStatus>(1, Allocator.TempJob)
        };
        var hand = job.Schedule( inputDeps);
        
        return hand;
    }
    
    
    
    public struct MoveJob : IJob
    {
        public NavMeshQuery query;
        //public NativeArray<Translation> translation;
        [DeallocateOnJobCompletion]public NativeArray<NavAgent> navAgent;
        [DeallocateOnJobCompletion]public NativeArray<LocalToWorld> localToWorld;

        public void Execute()
        {
            var startLoc = query.MapLocation(localToWorld[0].Position, Vector3.one*3.5f, 0);
            var endLoc = query.MapLocation(new Vector3(3, 0, 3), Vector3.one*3.5f, 0);
            
            //Debug.Log("MoveJob");
            if (!query.IsValid(startLoc) || !query.IsValid(endLoc))
            {
                //Debug.Log("!IsValid");
                return;
            }
            //var status = navagent.PathQueryStatus;/if(navagent.PathQueryStatus == )
            //Debug.Log(navAgent[0].PathQueryStatus);

            if (navAgent[0].PathQueryStatus == 0)
            {
                navAgent[0] = new NavAgent(startLoc, query.BeginFindPath(startLoc, endLoc));
                //Debug.Log(navAgent[0].PathQueryStatus);
            }
            
            navAgent[0] = new NavAgent(startLoc, query.UpdateFindPath(10, out var iterations));
            //navAgent.PathQueryStatus = status;
            if (navAgent[0].PathQueryStatus.Equals(PathQueryStatus.InProgress))
            {
                navAgent[0] = new NavAgent(startLoc, query.UpdateFindPath(10, out iterations));
                //var performed = 0;
                //status =  query.UpdateFindPath(10, out performed);
                //navAgent.PathQueryStatus = status;
                //Debug.Log("InProgress;");
            }

            if ((navAgent[0].PathQueryStatus & PathQueryStatus.Success) != 0)
            {
                //Debug.Log("Success;");
            }
        }
    }
}
