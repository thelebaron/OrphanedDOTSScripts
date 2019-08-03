using Game.Utils;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Jam
{
    public struct DustParticle : IComponentData
    {
        public uint Seed;
        public float FloatUpRate;
        public float ScaleDownRate;
        public float Life;
    }
    
    public class DustProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float FloatUpRate = 1f;
        public float ScaleDownRate = 0.35f;
        public float LifeSpan = 1.5f;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            /*
#if UNITY_EDITOR
            dstManager.SetName(entity, "Dust" + entity);
#endif*/
            dstManager.AddComponentData(entity, new DustParticle
            {
                FloatUpRate   = FloatUpRate,
                ScaleDownRate = ScaleDownRate,
                Life = LifeSpan
                
            });
            //dstManager.AddComponentData(entity,new Scale());
        }
    }

    public class DustSystem : JobComponentSystem
    {
        struct DustJob : IJobForEachWithEntity<DustParticle, Translation, NonUniformScale, Rotation, Seed>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public float deltaTime;
            public uint Random;
            
            public void Execute(Entity entity, int index, ref DustParticle d0, ref Translation t0, ref NonUniformScale n0, ref Rotation r0, ref Seed seed)
            {
                if (d0.Life <= 0)
                {
                    EntityCommandBuffer.DestroyEntity(index, entity);
                    return;
                }

                var rand    = new Unity.Mathematics.Random(seed.Value);
                var f1      = rand.NextFloat(1, 2);
                
                var q1 = rand.NextQuaternionRotation();
                r0.Value = q1;
                
                d0.Life -= deltaTime * f1;
              
                t0.Value.y += deltaTime * d0.FloatUpRate * f1;
                t0.Value.x += deltaTime * d0.FloatUpRate * rand.NextFloat(-1, 3);
                t0.Value.z += deltaTime * d0.FloatUpRate * rand.NextFloat(-1, 4);
                
                
                if (n0.Value.x <= 0||n0.Value.y <= 0||n0.Value.z <= 0)
                {
                    return;
                }
                    
                n0.Value -= maths.one * deltaTime * d0.ScaleDownRate * f1;
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new DustJob
            {
                EntityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                deltaTime           = UnityEngine.Time.fixedDeltaTime,
                Random = (uint)Random.Range(1,12345)
            };

            var handle = job.Schedule(this, inputDeps);
            endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(handle);
            
            return handle;
        }
        
        
        protected override void OnCreate()
        {
            endSimulationEntityCommandBufferSystem = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    }
    
}