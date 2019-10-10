using Game.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using Ray = Unity.Physics.Ray;
using RaycastHit = Unity.Physics.RaycastHit;

namespace DiscordTests
{ 
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.Entities;
    using Unity.Transforms;
    using Unity.Rendering;
    using Unity.Collections;
    using Unity.Mathematics;
 
 
    public class Testing : MonoBehaviour
    {
        public static Testing    instance;
        public        GameObject go;
        public        Mesh       mesh;
        public        Material   material;
 
        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
 
            EntityManager entityManager = World.Active.EntityManager;
 
       
 
            EntityArchetype parentEntityArchetype = entityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(LocalToWorld)
            );
            //create the parent entity
            Entity parent = entityManager.CreateEntity(parentEntityArchetype);
            entityManager.SetComponentData(parent, new Scale { Value = 1f });
 
            //create children
            EntityArchetype childEntityArchetype = entityManager.CreateArchetype(
                //typeof(Translation),
                //typeof(Rotation),
                //typeof(Scale),
                typeof(RenderMesh),
                typeof(LocalToWorld),
                typeof(LocalToParent),
                typeof(Parent)
           
            );
 
 
            NativeArray<Entity> entityArray = new NativeArray<Entity>(1, Allocator.Temp);
            entityManager.CreateEntity(childEntityArchetype, entityArray);
 
            for (int i = 0; i < entityArray.Length; i++)
            {
                Entity entity = entityArray[i];
                entityManager.SetComponentData(entity, new Parent { Value = parent });
                //entityManager.SetComponentData(entity, new Scale { Value = 1f });
                entityManager.SetSharedComponentData(entity, new RenderMesh
                {
                    mesh     = mesh,
                    material = material
                });
            }
 
            entityArray.Dispose();
 
        }
    }
    
    /*
    public class HumanSystem : ComponentSystem
    {
        
        private EntityQuery humanQuery;
        private EntityQuery deadhumanQuery;
        private ComponentDataFromEntity<PhysicsCollider> physicsColliderData;
        private ComponentDataFromEntity<PhysicsVelocity> physicsVelocityData;
        private ComponentDataFromEntity<PhysicsMass> physicsMassData;
        private ComponentDataFromEntity<EnemyGoal> enemyGoalData;

        protected override void OnCreate()
        {
            humanQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadWrite<PhysicsCollider>(),
                    ComponentType.ReadWrite<PhysicsMass>(), 
                    ComponentType.ReadOnly<PhysicsVelocity>()
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<Dead>()
                }
            });
        }
        
        protected override void OnUpdate()
        {
            Entities.With(deadhumanQuery).ForEach((Entity entity, AiHandler handler, ref Human human, ref Health health) =>
            {
                if (handler.ai.state != State.Dead)
                {
                    Die(handler);
                }

                if (handler.ai.state == State.Dead && !handler.cleanupComponents)
                {
                    handler.cleanupComponents = true;
                    //handler.ai.PlayDeathAnimation();
                    PostUpdateCommands.RemoveComponent<PhysicsCollider>(entity);
                    PostUpdateCommands.RemoveComponent<PhysicsVelocity>(entity);
                    PostUpdateCommands.RemoveComponent<PhysicsMass>(entity);
                    PostUpdateCommands.RemoveComponent<Grounded>(entity);
                    PostUpdateCommands.RemoveComponent<Sight>(entity);
                    PostUpdateCommands.RemoveComponent<EnemyGoal>(entity);
                    PostUpdateCommands.RemoveComponent<Targetable>(entity);
                    PostUpdateCommands.RemoveComponent<CopyTransformFromGameObject>(entity);
                    PostUpdateCommands.RemoveComponent<TeamIndex>(entity);
                    PostUpdateCommands.RemoveComponent<Health>(entity);
                    PostUpdateCommands.RemoveComponent<Aggro>(entity);
                    PostUpdateCommands.RemoveComponent<BoundsCenter>(entity);
                }
            });
            
        }

    }*/
    
    /*
    public class RandomJobSystem : JobComponentSystem
    {
        private Random randomSeed = new Random(12345);

        struct job : IJob
        {
            [ReadOnly] public Random MathRandom;
            public void Execute()
            {
                var x = MathRandom.NextFloat(0, 1);
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var r = new Random((uint)randomSeed.NextInt());
            
            var job = new job
            {
                MathRandom = r
            };
            return job.Schedule(inputDeps);
        }
    }
    
    public class MovementSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.deltaTime;

            Entities.ForEach((ref MovementInput movementInput,Rigidbody rigidbody,  ref MovementSpeed movementSpeed) => 
            {
                float3 movementDirection = new float3(movementInput.x, movementInput.y, movementInput.z);
                rigidbody.AddForce(movementDirection * movementSpeed.speed * deltaTime);
            });
        }
    }

    public class MovementSpeed : MonoBehaviour
    {
        public float3 speed;
    }

    public struct MovementInput : IComponentData
    {
        public float x;
        public float y;
        public float z;
    }
    
     
    public class VoxelGenerationSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem EndSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            EndSimulationEntityCommandBufferSystem = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var DestroyInitialVoxelDataComponentJob = new DestroyInitialVoxelDataComponentJob
            {
                Cmd = EndSimulationEntityCommandBufferSystem.CreateCommandBuffer()
                //Cmd = EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
                
            }.Schedule(this);
     
     
            return DestroyInitialVoxelDataComponentJob;
        }
     
        [BurstCompile]
        struct DestroyInitialVoxelDataComponentJob : IJobForEachWithEntity<ShouldGenerateInitialVoxelData>
        {
            public EntityCommandBuffer Cmd;
            //public EntityCommandBuffer.Concurrent Cmd;
     
            public void Execute(Entity entity, int index, ref ShouldGenerateInitialVoxelData c0)
            {
                Cmd.RemoveComponent<ShouldGenerateInitialVoxelData>(entity);
                //Cmd.RemoveComponent<ShouldGenerateInitialVoxelData>(index, entity);
            }
        }
    }


    
    using Game.Components.Player;
    using Unity.Entities;
    using Unity.Transforms;

    public class BaseCharacterSystem : ComponentSystem
    {
        protected EntityQuery CameraQuery;
        protected Entity      Player;
        private ComponentDataFromEntity<Translation> TranslationData;
        private ComponentDataFromEntity<Rotation> RotationData;

        protected override void OnCreate()
        {
            EntityQueryDesc query = new EntityQueryDesc
            {
                All = new ComponentType[]{ typeof(PlayerCameraTag), typeof(Transform), typeof(Camera) }
            };
        
            CameraQuery = GetEntityQuery(query);         
        }
    
        protected override void OnUpdate()
        {

            if (HasSingleton<PlayerTag>())
                Player = GetSingletonEntity<PlayerTag>();
            else 
                return;
        
            Entities.With(CameraQuery).ForEach((Entity entity, Transform camTransform, Camera camera) =>
            {
                camera.transform.position = TranslationData[Player].Value;
                camera.transform.rotation = RotationData[Player].Value;
                
            });
        }
    }

    public class MyPlayerCamera : MonoBehaviour
    {
        
    }
    public struct PlayerCameraTag : IComponentData{}*/
    
    /*
    public class BulletSystem : JobComponentSystem
    {
        private BuildPhysicsWorld  m_BuildPhysicsWorldSystem;
        private StepPhysicsWorld   m_StepPhysicsWorld;
        private ExportPhysicsWorld m_ExportPhysicsWorld;

        protected override void OnCreate()
        {
            
            m_BuildPhysicsWorldSystem = World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
            m_StepPhysicsWorld        = World.Active.GetOrCreateSystem<StepPhysicsWorld>();
            m_ExportPhysicsWorld      = World.Active.GetOrCreateSystem<ExportPhysicsWorld>();
        }

        struct BulletJob : IJobForEachWithEntity<Translation, Rotation>
        {
            [ReadOnly] public CollisionWorld world;
            [ReadOnly] public int            numDynamicBodies;
            public float          deltaTime;

            public void Execute(Entity entity, int index, ref Translation translation, ref Rotation rotation)
            {
                RaycastInput raycastInput = new RaycastInput
                {
                    Ray    = new Ray
                    {
                        Origin = translation.Value, Direction = maths.up
                        
                    },
                    Filter = CollisionFilter.Default
                };
                RaycastHit hit = new RaycastHit();
                world.CastRay(raycastInput, out hit);
                
                if (hit.RigidBodyIndex != - 1 && hit.RigidBodyIndex < numDynamicBodies)
                {
                    Debug.Log(true);
                }
                
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            
            var physicsHandleA = JobHandle.CombineDependencies(inputDeps, m_BuildPhysicsWorldSystem.FinalJobHandle);
            var physicsHandleB = JobHandle.CombineDependencies(physicsHandleA, m_StepPhysicsWorld.FinalJobHandle, m_ExportPhysicsWorld.FinalJobHandle);
            
            BulletJob job = new BulletJob
            {
                //commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                world            = m_BuildPhysicsWorldSystem.PhysicsWorld.CollisionWorld,
                numDynamicBodies = m_BuildPhysicsWorldSystem.PhysicsWorld.CollisionWorld.NumBodies,
                deltaTime        = Time.deltaTime
            };
            var handle = job.Schedule(this, physicsHandleB);

            return handle;
        }
    }*/
}