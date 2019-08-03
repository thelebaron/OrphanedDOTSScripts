using System.Collections.Generic;
using Game.Components.Player;
using Game.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public struct Seed : IComponentData
{
    public uint Value;
}

public struct CartoonBody : IComponentData
{
    public float BreathingSquashYAmplitude;// = 0.085f;
    public float BreathingSquashYRate;// = 1;
    public float BreathingSquashXZAmplitude;// = 0.1085f;
    public float BreathingSquashXZRate;// = 1;
    public float3 PositionOffset;
}

// eye component, attaches to an entity's position - this is not a child of the character but a free floating entity.
// the socket entity is a child of the character.
public struct CartoonEye : IComponentData
{
    public CartoonEye(Entity ent)
    {
        SocketEntity = ent;
    }
    public Entity SocketEntity;
}

//dust trail component - if a character has this, it emits a dust trail
public struct DustTrail : IComponentData
{
    public Entity DustPrefabEntity;
    public float Rate;
    public int MinAmount;
    public int MaxAmount;
    public float m_CurrentTime;
}


public class CartoonProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject EyePrefab;
    public Entity EyePrefabEntity;
    public GameObject LeftEyeSocket;
    public GameObject RightEyeSocket;
    public Entity LeftEyeEntity;
    public Entity RightEyeEntity;

    public GameObject DustTrailPrefab;
    public Entity DustTrailEntity;

    public float BreathingSquashYAmplitude = 0.035f;
    public float BreathingSquashYRate = 1;
    public float BreathingSquashXZAmplitude = 0.065f;
    public float BreathingSquashXZRate = 1;
    
    public float3 PositionOffset;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new CartoonBody
        {
            BreathingSquashYAmplitude = BreathingSquashYAmplitude,
            BreathingSquashYRate = BreathingSquashYRate,
            BreathingSquashXZAmplitude = BreathingSquashXZAmplitude,
            BreathingSquashXZRate = BreathingSquashXZRate,
            PositionOffset = PositionOffset
        });
        dstManager.AddComponentData(entity, new NonUniformScale {Value = new float3(1,1,1)});
/*
//eye stuff
        LeftEyeEntity = conversionSystem.GetPrimaryEntity(LeftEyeSocket);
        RightEyeEntity = conversionSystem.GetPrimaryEntity(RightEyeSocket);
        EyePrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(EyePrefab, World.Active);
        
        var leftEye = dstManager.Instantiate(EyePrefabEntity);
        dstManager.AddComponentData(leftEye, new CartoonEye(LeftEyeEntity));
        var rightEye = dstManager.Instantiate(EyePrefabEntity);
        dstManager.AddComponentData(rightEye, new CartoonEye(RightEyeEntity));
#if UNITY_EDITOR
        dstManager.SetName(LeftEyeEntity, "eyeSocket_L" + LeftEyeEntity);
        dstManager.SetName(RightEyeEntity, "eyeSocket_R" + RightEyeEntity);
        dstManager.SetName(leftEye, "eye_L" + leftEye);
        dstManager.SetName(rightEye, "eye_R" + rightEye);
#endif
        */
        // dust stuff
        DustTrailEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(DustTrailPrefab, World.Active);
        dstManager.AddComponentData(entity, new DustTrail
        {
            DustPrefabEntity = DustTrailEntity,
            Rate             = 0.25f,
            MinAmount        = 1,
            MaxAmount        = 5,
            m_CurrentTime    = 0
        });
        //dstManager.AddComponentData(entity, new Scale());
        //dstManager.AddComponentData(entity, new ScalePivot {Value = new float3()});
        //dstManager.AddComponentData(entity, new ScalePivotTranslation());
        //dstManager.AddComponentData(entity, new CompositeScale());
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(EyePrefab);
        referencedPrefabs.Add(DustTrailPrefab);
    }
}

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class CartoonSystem : JobComponentSystem
{
    struct EmitDust : IJobForEachWithEntity<DustTrail, CartoonBody, LocalToWorld>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public PlayerInput PlayerInput;
        public float deltaTime;
        public uint random;
        public void Execute(Entity entity, int index, ref DustTrail trail, ref CartoonBody c1, ref LocalToWorld l0)
        {
            trail.m_CurrentTime += deltaTime;
            
            var coinflip = new Unity.Mathematics.Random(12345);
            var outcome = coinflip.NextInt(0, 1);
            
            if (trail.m_CurrentTime > trail.Rate || outcome.Equals(1))
            {
                if (!PlayerInput.Move.Equals(float2.zero))
                {
                    var rand = new Unity.Mathematics.Random(12345);
                    float f1 = (float)rand.NextInt(-2, 2);
                    var intRand = rand.NextInt(trail.MinAmount, trail.MaxAmount + 5);
                    
                    for (int i = 0; i < intRand; i++)
                    {
                        var dust = CommandBuffer.Instantiate(index, trail.DustPrefabEntity);
                        var PositionOffset = new float3(0,-0.5f,0);
                        
                        CommandBuffer.SetComponent(index, dust, new Translation{ Value = l0.Position + PositionOffset});
                        var localToWorld = new LocalToWorld
                        {
                            Value = float4x4.TRS(new float3(l0.Position + PositionOffset),
                                quaternion.LookRotationSafe(l0.Forward, math.up()),
                                new float3(0.30f, 0.30f, 0.30f))
                        };
                        
                
                        CommandBuffer.SetComponent(index, dust, new LocalToWorld
                        {
                            Value = localToWorld.Value
                        });
                        
                        var seed = rand.NextUInt(1, 100) + (uint)i;
                        CommandBuffer.AddComponent(index, dust, new Seed{ Value = seed });
                    }

                    trail.m_CurrentTime = 0;

                }
                
            }
        }
    }
    struct AlignEyes : IJobForEachWithEntity<CartoonEye, Translation, Rotation>
    {
        [ReadOnly]public ComponentDataFromEntity<LocalToWorld> LocalToWorldDataFromEntity;
        
        public void Execute(Entity entity, int index, ref CartoonEye eye, ref Translation c1, ref Rotation c2)
        {
            if (LocalToWorldDataFromEntity.Exists(eye.SocketEntity))
            {
                c1.Value = LocalToWorldDataFromEntity[eye.SocketEntity].Position;
            }
        }
    }
    
    struct ScaleBodyJob : IJobForEachWithEntity<CartoonBody, NonUniformScale, Translation>
    {
        public float deltaTime;
        public float time;
        
        public void Execute(Entity entity, int index, ref CartoonBody cartoonBody, ref NonUniformScale nonUniformScale, ref Translation translation)
        {
            ScaleY(cartoonBody, ref nonUniformScale, ref translation);
            ScaleXZ(cartoonBody, ref nonUniformScale);
        }

        private void ScaleY(CartoonBody cartoonBody, ref NonUniformScale nonUniformScale, ref Translation translation)
        {
            // scale y
            var amplitude = cartoonBody.BreathingSquashYAmplitude;
            var frequency = cartoonBody.BreathingSquashYRate;
            var scale = nonUniformScale.Value;
            scale += amplitude * (math.sin(2 * math.PI * frequency * time) -
                                  math.sin(2 * Mathf.PI * frequency * (time - deltaTime))) * maths.up;
            nonUniformScale.Value.y = scale.y;
            translation.Value.y = nonUniformScale.Value.y + cartoonBody.PositionOffset.y;
            
            
        }
        
        private void ScaleXZ(CartoonBody cartoonBody, ref NonUniformScale nonUniformScale)
        {
            // scale y
            var amplitude = cartoonBody.BreathingSquashXZAmplitude;
            var frequency = cartoonBody.BreathingSquashXZRate;
            var scale = nonUniformScale.Value;
            scale -= amplitude * (math.sin(2 * math.PI * frequency * time) - math.sin(-2 * Mathf.PI * frequency * (time - deltaTime))) * maths.up;
            nonUniformScale.Value.x = scale.y;
            nonUniformScale.Value.z = scale.y;
        }
    }
    
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (!HasSingleton<PlayerInput>())
            return inputDeps;
        
        var emitDustJob = new EmitDust
        {
            deltaTime = UnityEngine.Time.fixedDeltaTime,
            CommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(), 
            PlayerInput = GetSingleton<PlayerInput>(),
            random = (uint)Random.Range(0,12345)
        };
        var emitDustHandle = emitDustJob.Schedule(this, inputDeps);
        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(emitDustHandle);
        
        var eyeJob = new AlignEyes
        {
            LocalToWorldDataFromEntity = GetComponentDataFromEntity<LocalToWorld>(),
        };
        var eyeHandle = eyeJob.Schedule(this, emitDustHandle);
        
        
        var job = new ScaleBodyJob
        {
            deltaTime = UnityEngine.Time.fixedDeltaTime,
            time = Time.time
        };
        return job.Schedule(this, eyeHandle);
    }

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
}

