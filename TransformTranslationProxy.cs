using System.Collections;
using System.Collections.Generic;
using Game.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public struct TransformTranslation : IComponentData
{
    
}
public class TransformTranslationProxy : MonoBehaviour,IConvertGameObjectToEntity
{
    public string newName;
    public bool isChild;
    
    private void Awake()
    {
        var goEntity = GetComponent<GameObjectEntity>();

        if (goEntity != null)
        {
            //var manager = World.Active.EntityManager;
            //var entity = goEntity.Entity;
#if UNITY_EDITOR
            //manager.SetName(entity, "***** CopyPosTOTrans_" + newName);
            Debug.Log("Converted " + newName);
#endif
            
        }
        
        
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new TransformTranslation());
        #if UNITY_EDITOR
        //dstManager.SetName(entity, "***** CopyPosTOTrans_" + newName);
        #endif
    }
}

public class TransformToTranslationSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag ( typeof (TransformTranslation ) ) ]
    struct CopyJob : IJobForEachWithEntity<Translation, LocalToWorld>
    {
        public void Execute(Entity entity, int index, ref Translation c0, ref LocalToWorld c1)
        {
            c0.Value = c1.Position;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new CopyJob();
        var handle = job.Schedule(this, inputDeps);
        
        return handle;
    }
}