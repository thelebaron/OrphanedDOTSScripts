using System;
using System.Collections;
using System.Collections.Generic;
using Game.Modules.Monsters;
using Game.Systems;
using Game.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public struct RaycastTester : IComponentData
{
    
}
public class RayTest : MonoBehaviour, IConvertGameObjectToEntity
{
    
    public void Convert(Entity e, EntityManager em, GameObjectConversionSystem conversionSystem)
    {
        em.AddComponentData(e, new CopyTransformFromGameObject());
        em.AddComponentData(e, new RaycastTester());
    }

    private void OnDrawGizmosSelected()
    {
        Debug.DrawRay(transform.position, Vector3.down * 10);
    }
}

public class RaycastTests : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_EndSimulationSystem;
    private BuildPhysicsWorld                      m_BuildPhysicsWorldSystem;
    private StepPhysicsWorld                       m_StepPhysicsWorld;
    private ExportPhysicsWorld                     m_ExportPhysicsWorld;
        
    protected override void OnCreate()
    {
        m_EndSimulationSystem = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_BuildPhysicsWorldSystem = World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorld        = World.Active.GetOrCreateSystem<StepPhysicsWorld>();
        m_ExportPhysicsWorld      = World.Active.GetOrCreateSystem<ExportPhysicsWorld>();
    }

    
    [RequireComponentTag(typeof(RaycastTester))]
    public struct CastTheRays: IJobForEach<Translation, LocalToWorld>
    {            
        [ReadOnly] public CollisionWorld                 CollisionWorld;
        public void Execute(ref Translation translation, ref LocalToWorld c2)
        {
            // Check the collisionworld for a hit
            var hit = CollisionWorld.CastRay(new RaycastInput
                {
                    Start = translation.Value, End = translation.Value + maths.down * 10, Filter = CollisionFilter.Default
                },
                out var rayHit);
            
            if (!hit)
                return;
                
            // Early return if the index is invalid
            if(rayHit.RigidBodyIndex <= 0)
                return;
                
            var hitentity  = CollisionWorld.Bodies[rayHit.RigidBodyIndex].Entity;
            var hitbody    = CollisionWorld.Bodies[rayHit.RigidBodyIndex]; //RigidBody?
            var instigator = Entity.Null;
            
            Debug.Log(hitentity);
        }
    }

    protected override JobHandle OnUpdate(JobHandle deps)
    {
        
        var physicsHandle      = JobHandle.CombineDependencies(deps, m_BuildPhysicsWorldSystem.FinalJobHandle);
        var finalPhysicsHandle = JobHandle.CombineDependencies(physicsHandle, m_StepPhysicsWorld.FinalJobHandle, m_ExportPhysicsWorld.FinalJobHandle);
        
        var job = new CastTheRays
            {
                CollisionWorld = m_BuildPhysicsWorldSystem.PhysicsWorld.CollisionWorld
            };
        var handle = job.Schedule(this, finalPhysicsHandle);
        handle.Complete();

        return handle;
    }
}
