using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Wargon.ezs;
using Wargon.ezs.Unity;

public partial class SyncTransformSystem : UpdateSystem, IJobSystemTag
{
    private SyncTransforms syncTransforms;

    protected override void OnCreate()
    {
        syncTransforms = new SyncTransforms(world);
        entitiesInternal.Without<Inactive, NoBurst>().AddNewEntityType(syncTransforms);
    }

    public override void Update()
    {
        syncTransforms.Synchronize();
    }

    private class SyncTransforms : EntityType, IDisposable
    {
        private readonly Pool<TransformRef> classComponents;
        private readonly Pool<TransformComponent> structComponents;
        private bool disposed;
        private TransformSynchronizeJob job;
        private TransformAccessArray transformAccessArray;

        public SyncTransforms(World world) : base(world)
        {
            IncludeCount = 2;
            IncludeTypes = new[]
            {
                ComponentType<TransformComponent>.ID,
                ComponentType<TransformRef>.ID
            };
            ExcludeCount = 2;
            ExcludeTypes = new[]
            {
                ComponentType<Inactive>.ID,
                ComponentType<NoBurst>.ID
            };
            structComponents = world.GetPool<TransformComponent>();
            classComponents = world.GetPool<TransformRef>();
            transformAccessArray = new TransformAccessArray(world.EntityCacheSize);
            Count = 0;
            structComponents.OnAdd += OnAddInclude;
            structComponents.OnRemove += OnRemoveInclude;
            classComponents.OnAdd += OnAddInclude;
            classComponents.OnRemove += OnRemoveInclude;
            var PoolEx = world.GetPool<Inactive>();
            PoolEx.OnAdd += OnAddExclude;
            PoolEx.OnRemove += OnRemoveExclude;
            var PoolEx2 = world.GetPool<NoBurst>();
            PoolEx2.OnAdd += OnAddExclude;
            PoolEx2.OnRemove += OnRemoveExclude;
            world.OnCreateEntityType(this);
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            transformAccessArray.Dispose();
            disposed = true;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnAddInclude(int id)
        {
            if (HasEntity(id)) return;
            ref var data = ref world.GetEntityData(id);
            // for (var i = 0; i < ExcludeCount; i++)
            //     if (data.componentTypes.Contains(ExcludeTypes[i]))
            //         return;
            //
            // for (var i = 0; i < IncludeCount; i++)
            //     if (!data.componentTypes.Contains(IncludeTypes[i]))
            //         return;

            if (entities.Length == Count) Array.Resize(ref entities, world.totalEntitiesCount+2);
            entities[Count] = data.id;
            entitiesMap.Add(data.id, Count);
            transformAccessArray.Add(classComponents.items[id].value);
            Count++;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnRemoveExclude(int id)
        {
            if (HasEntity(id)) return;
            ref var data = ref world.GetEntityData(id);
            // for (var i = 0; i < ExcludeCount; i++)
            //     if (data.componentTypes.Contains(ExcludeTypes[i]))
            //         return;
            //
            // for (var i = 0; i < IncludeCount; i++)
            //     if (!data.componentTypes.Contains(IncludeTypes[i]))
            //         return;

            if (entities.Length == Count) Array.Resize(ref entities, world.totalEntitiesCount+2);
            entities[Count] = data.id;
            entitiesMap.Add(data.id, Count);
            transformAccessArray.Add(classComponents.items[id].value);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnRemoveInclude(int id)
        {
            if (!HasEntity(id)) return;
            var lastEntityId = entities[Count - 1];
            var indexOfEntityId = entitiesMap[id];
            entitiesMap.Remove(id);
            transformAccessArray.RemoveAtSwapBack(indexOfEntityId);
            Count--;
            if (Count > indexOfEntityId)
            {
                entities[indexOfEntityId] = lastEntityId;
                entitiesMap.Add(lastEntityId, indexOfEntityId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnAddExclude(int id)
        {
            if (!HasEntity(id)) return;
            var lastEntityId = entities[Count - 1];
            var indexOfEntityId = entitiesMap[id];
            entitiesMap.Remove(id);
            transformAccessArray.RemoveAtSwapBack(indexOfEntityId);
            Count--;
            if (Count > indexOfEntityId)
            {
                entities[indexOfEntityId] = lastEntityId;
                entitiesMap.Add(lastEntityId, indexOfEntityId);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateOnAddWithout(int id) {
            if (!HasEntity(id)) return;
            var lastEntityId = entities[Count - 1];
            var indexOfEntityId = entitiesMap[id];
            entitiesMap.Remove(id);
            transformAccessArray.RemoveAtSwapBack(indexOfEntityId);
            Count--;
            if (Count > indexOfEntityId)
            {
                entities[indexOfEntityId] = lastEntityId;
                entitiesMap.Add(lastEntityId, indexOfEntityId);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateOnRemoveWithout(in EntityData data) {
            var id = data.id;
            if (HasEntity(id)) return;

            // for (var i = 0; i < IncludeCount; i++)
            //     if (!data.componentTypes.Contains(IncludeTypes[i]))
            //         return;

            if (entities.Length == Count) Array.Resize(ref entities, world.totalEntitiesCount+2);
            entities[Count] = data.id;
            entitiesMap.Add(data.id, Count);
            transformAccessArray.Add(classComponents.items[id].value);
            Count++;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Synchronize()
        {
            if (disposed) return;
            if (Count < 1) return;
            var data = NativeMagic.WrapToNative(structComponents.items);
            TransformSynchronizeJob job = default;
            job.transformComponents = data;
            job.entities = NativeMagic.WrapToNative(entities);
            job.Schedule(transformAccessArray).Complete();
#if UNITY_EDITOR
            job.Clear();
#endif
        }

        internal override void Clear()
        {
#if UNITY_EDITOR
            Debug.Log("TRANSFORMS DISPOSED");
            transformAccessArray.Dispose();
            disposed = true;
#endif
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct TransformSynchronizeJob : IJobParallelForTransform
        {
            [NativeDisableParallelForRestriction] public NativeWrappedData<TransformComponent> transformComponents;
            [NativeDisableParallelForRestriction] public NativeWrappedData<int> entities;

            public void Execute(int index, TransformAccess transform)
            {
                var entity = entities.Array[index];

                var transformComponent = transformComponents.Array[entity];
                // transformComponent.right = transformComponent.Rotation * Vector3.right;
                // transformComponent.forward = transformComponent.Rotation * Vector3.forward;
                transform.position = transformComponent.position;
                transform.rotation = transformComponent.rotation;
                transform.localScale = transformComponent.scale;
                transformComponent.position = transform.position;
                transformComponents.Array[entity] = transformComponent;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
#if UNITY_EDITOR
                NativeMagic.UnwrapFromNative(transformComponents);
                NativeMagic.UnwrapFromNative(entities);
#endif
            }
        }
    }
}

[EcsComponent]
public struct SyncBackTransformTag
{
}
public struct StaticTag{}
public struct TransformComponent {
    public float3 position;
    public float3 scale;
    public quaternion rotation;
}

public struct TransformRef {
    public UnityEngine.Transform value;
}
