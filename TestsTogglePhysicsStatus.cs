using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

public struct PhysicsKinematicToggler : IComponentData
{
    public bool isKinematic;
    public float mass;
    public float inverseMass;
    public float3 inverseInertia;
}
public class TestsTogglePhysicsStatus : MonoBehaviour, IConvertGameObjectToEntity
{
    public float mass;

    public void Convert(Entity e, EntityManager em, GameObjectConversionSystem cs)
    {

        bool isKinematic = GetComponent<PhysicsBody>().MotionType != BodyMotionType.Dynamic;
        // Build mass component
        em.AddComponentData(e, new PhysicsKinematicToggler
        {
            isKinematic = isKinematic,
            mass = mass
        });
    }
}

public class ToggleDotsPhysics : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    public struct TogglerJob : IJobForEachWithEntity<PhysicsKinematicToggler, PhysicsCollider, PhysicsMass>
    {
        public bool userInput;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        
        public void Execute(Entity entity, int index, ref PhysicsKinematicToggler c0, ref PhysicsCollider c1, ref PhysicsMass c2)
        {
            if(!userInput)
                return;
            userInput = false;
            
            var massProperties = c1.MassProperties;
            c0.inverseMass = math.rcp(c0.mass);
            c0.inverseInertia = math.rcp(massProperties.MassDistribution.InertiaTensor * c0.mass);
            
            if (c0.isKinematic)
            {
                c0.isKinematic = false;
                //change to dynamic
                c2.InverseMass = c0.mass;
                c2.InverseInertia = c0.inverseInertia;
                
                entityCommandBuffer.AddComponent(index, entity, new PhysicsDamping
                {
                    Angular = 0.05f,
                    Linear = 0.01f
                });
                entityCommandBuffer.RemoveComponent<PhysicsGravityFactor>(index, entity);
            }
            else
            {
                c0.isKinematic = true;
                //change to kinematic
                c2.InverseMass = 0;
                c2.InverseInertia = float3.zero;
                
                entityCommandBuffer.RemoveComponent<PhysicsDamping>(index, entity);
                entityCommandBuffer.AddComponent(index, entity, new PhysicsGravityFactor());
            }
            

        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        endSimulationEntityCommandBufferSystem =
            World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new TogglerJob
        {
            userInput = Input.GetKeyDown(KeyCode.K),
            entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        var handle= job.Schedule(this, inputDeps);
        handle.Complete();
        
        return handle;
    }
}