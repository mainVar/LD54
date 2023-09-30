using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Wargon.ezs;
using Wargon.ezs.Unity;

public delegate void AddEntityToPoolDelegate<A>(int id) where A : unmanaged;

public delegate void RemoveEntityFormPoolDelegate<A>(int id) where A : unmanaged;
public static partial class EcsExtensions
{
    public static MonoEntity GetMonoEntity(this GameObject gameObject)
    {
        return gameObject.GetComponent<MonoEntity>();
    }

    public unsafe delegate void InternalDelegate(void* fn, void* bagPtr);
    public unsafe delegate void InternalParallelForDelegate(void* fn, void* bagPtr, int index);

    public delegate void LambdaRef<A, B>(ref A a, ref B b);

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static unsafe void EachWithJob4<A, B>(this Entities ezs, LambdaRef<A, B> lambdaRef) where A : unmanaged where B : unmanaged
    // {
    //     var entityType = ezs.GetEntityTypeFromArrayTypePair<A, B>();
    //     var handle = GCHandle.Alloc(lambdaRef);
    //     EntityQueue q = default;
    //     q.Init(entityType.Count, EntityQueue.Mode.Parallel);
    //     q.Add(ref entityType.poolA.items);
    //     q.Add(ref entityType.poolB.items);
    //     q.SetEntities(entityType.entities);
    //     
    //     var job = new EachDelegateJob<A, B>
    //     {
    //         fns = Marshal.GetFunctionPointerForDelegate(lambdaRef),
    //         func = BurstCompiler.CompileFunctionPointer<InternalParallelForDelegate>(EachDelegateJob<A,B>.Run),
    //         queue = q
    //     };
    //     job.Schedule(entityType.Count, 0).Complete();
    //     handle.Free();
    //     q.Dispose();
    // }
    
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A>(this Entities ezs, ref AExecutor jobExecute)
        where A : unmanaged where AExecutor : unmanaged, IJobExecute<A>
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePair<A>();
        var entities = entityType.entities;
        EachWithJob<A, AExecutor> job = default;
        job.Set(entities, ref entityType.poolA.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A, B>(this Entities ezs, ref AExecutor jobExecute) where A : unmanaged
        where B : unmanaged
        where AExecutor : unmanaged, IJobExecute<A, B>
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePair<A, B>();
        var entities = entityType.entities;
        EachWithJob<A, B, AExecutor> job = default;
        job.Set(entities, entityType.poolA.items, entityType.poolB.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
// #if UNITY_EDITOR
//         job.Clear();
// #endif
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJobS<AExecutor, A, NA>(this Entities.EntitiesWithout<NA> ezs, ref AExecutor jobExecute) where NA : struct
        where A : unmanaged
        where AExecutor : struct, IJobExecute<A>
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A>();
        var entities = entityType.entities;
        EachWithJob<A, AExecutor> job = default;
        job.Set(entities, ref entityType.poolA.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
#if UNITY_EDITOR
        job.Clear();
#endif
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A, B, NA>(this Entities.EntitiesWithout<NA> ezs, ref AExecutor jobExecute) where NA : struct
        where A : unmanaged
        where B : unmanaged
        where AExecutor : unmanaged, IJobExecute<A, B>
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A, B>();
        var entities = entityType.entities;
        EachWithJob<A, B, AExecutor> job = default;
        job.Set(entities, entityType.poolA.items, entityType.poolB.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
#if UNITY_EDITOR
        job.Clear();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A, B, NA, NB>(this Entities.EntitiesWithout<NA, NB> ezs,
        ref AExecutor jobExecute) where A : unmanaged
        where B : unmanaged
        where AExecutor : unmanaged, IJobExecute<A, B> where NA : struct where NB : struct
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A, B>();
        var entities = entityType.entities;
        EachWithJob<A, B, AExecutor> job = default;
        job.Set(entities, entityType.poolA.items, entityType.poolB.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
#if UNITY_EDITOR
        job.Clear();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A, B, C>(this Entities ezs, ref AExecutor jobExecute) where A : unmanaged
        where B : unmanaged
        where C : unmanaged
        where AExecutor : unmanaged, IJobExecute<A, B, C> 
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePair<A, B, C>();
        var entities = entityType.entities;
        EachWithJob<A, B, C, AExecutor> job = default;
        job.Set(entities, entityType.poolA.items, entityType.poolB.items, entityType.poolС.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A, B, C, NA, NB>(this Entities.EntitiesWithout<NA, NB> ezs,
        ref AExecutor jobExecute) where A : unmanaged where NA : struct where NB : struct
        where B : unmanaged
        where C : unmanaged
        where AExecutor : unmanaged, IJobExecute<A, B, C>
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A, B, C>();
        var entities = entityType.entities;
        EachWithJob<A, B, C, AExecutor> job = default;
        job.Set(entities, entityType.poolA.items, entityType.poolB.items, entityType.poolС.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
#if UNITY_EDITOR
        job.Clear();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EachWithJob<AExecutor, A, B, C, D>(this Entities ezs, ref AExecutor jobExecute)
        where A : unmanaged
        where B : unmanaged
        where C : unmanaged
        where D : unmanaged
        where AExecutor : unmanaged, IJobExecute<A, B, C, D>
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePair<A, B, C, D>();
        var entities = entityType.entities;
        EachWithJob<A, B, C, D, AExecutor> job = default;
        job.Set(entities, entityType.poolA.items, entityType.poolB.items, entityType.poolС.items,
            entityType.poolD.items, ref jobExecute);
        job.Schedule(entityType.Count, 0).Complete();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Each<A>(this Entities ezs, LambdaRef<A> lambda) where A : struct
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePair<A>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]]);
    }

    public static void Each<A, B, C, NA>(this Entities.EntitiesWithout<NA> ezs, LambdaRRC<A, B, C> lambda)
        where NA : struct where A : struct where B: struct where C : struct
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A, B, C>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                c[entities[index]]);
    }

    public static void Each<A, B, NA>(this Entities.EntitiesWithout<NA> ezs, LambdaRef<A, B> lambda)
        where NA : struct where A : struct where B: struct
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A, B>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]]);
    }

    public static void Each<A, B, C, NA>(this Entities.EntitiesWithout<NA> ezs, LambdaRef<A, B, C> lambda)
        where NA : struct where A : struct where B: struct where C : struct
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A, B, C>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                ref c[entities[index]]);
    }

    public static void Each<A, B, C, D, NA>(this Entities.EntitiesWithout<NA> ezs, LambdaRef<A, B, C, D> lambda)
        where NA : struct where A : struct where B: struct where C : struct where D : struct
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A, B, C, D>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        var d = entityType.poolD.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                ref c[entities[index]],
                ref d[entities[index]]);
    }

    public static void Each<A, B, C, D, E, NA>(this Entities.EntitiesWithout<NA> ezs, LambdaRef<A, B, C, D, E> lambda)
        where NA : struct where A : struct where B: struct where C : struct where D : struct where E : struct
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A, B, C, D, E>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        var d = entityType.poolD.items;
        var e = entityType.poolE.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                ref c[entities[index]],
                ref d[entities[index]],
                ref e[entities[index]]);
    }

    public static void Each<A, B, NA, NB>(this Entities.EntitiesWithout<NA, NB> ezs, LambdaRef<A, B> lambda)
        where NA : struct where NB : struct where A : struct where B: struct
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A, B>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]]);
    }

    public static void Each<A, B, C, NA, NB>(this Entities.EntitiesWithout<NA, NB> ezs, LambdaRef<A, B, C> lambda)
        where NA : struct where NB : struct where A : struct where B: struct where C : struct
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A, B, C>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                ref c[entities[index]]);
    }

    public static void Each<A, B, C, D, NA, NB>(this Entities.EntitiesWithout<NA, NB> ezs, LambdaRef<A, B, C, D> lambda)
        where NA : struct where NB : struct where A : struct where B: struct where C : struct where D : struct
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A, B, C, D>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        var d = entityType.poolD.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                ref c[entities[index]],
                ref d[entities[index]]);
    }

    public static Entities Each<A, B, C, D, E, NA, NB>(this Entities.EntitiesWithout<NA, NB> ezs,
        LambdaRef<A, B, C, D, E> lambda)  where NA : struct where NB : struct where A : struct where B: struct where C : struct where D : struct where E : struct
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A, B, C, D, E>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        var b = entityType.poolB.items;
        var c = entityType.poolС.items;
        var d = entityType.poolD.items;
        var e = entityType.poolE.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(ref a[entities[index]],
                ref b[entities[index]],
                ref c[entities[index]],
                ref d[entities[index]],
                ref e[entities[index]]);
        return ezs;
    }

    public static void EachWithJobs<A, NA>(this Entities.EntitiesWithout<NA> ezs, Lambda<A> lambda) where NA : struct where A : struct
    {
        var entityType = ezs.GetEntityTypeFromArrayTypePairWithout<A>();
        var entities = entityType.entities;
        var a = entityType.poolA.items;
        for (var index = 0; index < entityType.Count; index++)
            lambda(a[entities[index]]);
    }
}

public interface IJobExecute<T>
{
    void ForEach(ref T t);
}

public interface IJobExecute<T, T2>
    where T : unmanaged where T2 : unmanaged
{
    void ForEach(ref T t, ref T2 t2);
}

public interface IJobExecute<T, T2, T3>
    where T : unmanaged
    where T2 : unmanaged
    where T3 : unmanaged
{
    void ForEach(ref T t, ref T2 t2, ref T3 t3);
}

public interface IJobExecute<T, T2, T3, T4>
    where T : unmanaged
    where T2 : unmanaged
    where T3 : unmanaged
    where T4 : unmanaged
{
    void ForEach(ref T t, ref T2 t2, ref T3 t3, ref T4 t4);
}

public interface IJobExecute<T, T2, T3, T4, T5>
    where T : unmanaged
    where T2 : unmanaged
    where T3 : unmanaged
    where T4 : unmanaged
    where T5 : unmanaged
{
    void ForEach(ref T t, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5);
}

[BurstCompile(CompileSynchronously = true)]
public struct EachWithJob<A, Executor> : IJobParallelFor
    where Executor : struct, IJobExecute<A>
    where A : unmanaged
{
    private Executor executionFunc;
    private NativeWrappedData<int> Entites;
    private NativeWrappedData<A> ItemsA;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int[] entites, ref A[] items, ref Executor action)
    {
        Entites = NativeMagic.WrapToNative(entites);
        ItemsA = NativeMagic.WrapToNative(items);
        executionFunc = action;
    }


    public void Execute(int index)
    {
        var entity = Entites.Array[index];
        var item = ItemsA.Array[entity];
        executionFunc.ForEach(ref item);
        ItemsA.Array[entity] = item;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
#if UNITY_EDITOR
        NativeMagic.UnwrapFromNative(Entites);
        NativeMagic.UnwrapFromNative(ItemsA);
#endif
    }
}

[BurstCompile(CompileSynchronously = true)]
public struct EachWithJob<A, B, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobExecute<A, B>
    where A : unmanaged
    where B : unmanaged
{
    private Executor executionFunc;
    private NativeWrappedData<int> Entites;
    private NativeWrappedData<A> ItemsA;
    private NativeWrappedData<B> ItemsB;

    public void Set(int[] entites, A[] itemsA, B[] itemsB, ref Executor action)
    {
        Entites = NativeMagic.WrapToNative(entites);
        ItemsA = NativeMagic.WrapToNative(itemsA);
        ItemsB = NativeMagic.WrapToNative(itemsB);
        executionFunc = action;
    }

    public void Execute(int index)
    {
        var entity = Entites.Array[index];
        var itemA = ItemsA.Array[entity];
        var itemB = ItemsB.Array[entity];
        executionFunc.ForEach(ref itemA, ref itemB);
        ItemsA.Array[entity] = itemA;
        ItemsB.Array[entity] = itemB;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
#if UNITY_EDITOR
        NativeMagic.UnwrapFromNative(Entites);
        NativeMagic.UnwrapFromNative(ItemsA);
        NativeMagic.UnwrapFromNative(ItemsB);
#endif
    }
}

[BurstCompile(CompileSynchronously = true)]
public struct EachWithJob<A, B, C, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobExecute<A, B, C>
    where A : unmanaged
    where B : unmanaged
    where C : unmanaged
{
    private Executor executionFunc;
    private NativeWrappedData<int> Entites;
    private NativeWrappedData<A> ItemsA;
    private NativeWrappedData<B> ItemsB;
    private NativeWrappedData<C> ItemsC;

    public void Set(int[] entites, A[] itemsA, B[] itemsB, C[] itemsC, ref Executor action)
    {
        Entites = NativeMagic.WrapToNative(entites);
        ItemsA = NativeMagic.WrapToNative(itemsA);
        ItemsB = NativeMagic.WrapToNative(itemsB);
        ItemsC = NativeMagic.WrapToNative(itemsC);
        executionFunc = action;
    }

    public void Execute(int index)
    {
        var entity = Entites.Array[index];
        var itemA = ItemsA.Array[entity];
        var itemB = ItemsB.Array[entity];
        var itemC = ItemsC.Array[entity];
        executionFunc.ForEach(ref itemA, ref itemB, ref itemC);
        ItemsA.Array[entity] = itemA;
        ItemsB.Array[entity] = itemB;
        ItemsC.Array[entity] = itemC;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
#if UNITY_EDITOR
        NativeMagic.UnwrapFromNative(Entites);
        NativeMagic.UnwrapFromNative(ItemsA);
        NativeMagic.UnwrapFromNative(ItemsB);
        NativeMagic.UnwrapFromNative(ItemsC);
#endif
    }
}

[BurstCompile(CompileSynchronously = true)]
public struct EachWithJob<A, B, C, D, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobExecute<A, B, C, D>
    where A : unmanaged
    where B : unmanaged
    where C : unmanaged
    where D : unmanaged
{
    private Executor executionFunc;
    private NativeWrappedData<int> Entites;
    private NativeWrappedData<A> ItemsA;
    private NativeWrappedData<B> ItemsB;
    private NativeWrappedData<C> ItemsC;
    private NativeWrappedData<D> ItemsD;

    public void Set(int[] entites, A[] itemsA, B[] itemsB, C[] itemsC, D[] itemsD, ref Executor action)
    {
        Entites = NativeMagic.WrapToNative(entites);
        ItemsA = NativeMagic.WrapToNative(itemsA);
        ItemsB = NativeMagic.WrapToNative(itemsB);
        ItemsC = NativeMagic.WrapToNative(itemsC);
        ItemsD = NativeMagic.WrapToNative(itemsD);
        executionFunc = action;
    }

    public void Execute(int index)
    {
        var entity = Entites.Array[index];
        var itemA = ItemsA.Array[entity];
        var itemB = ItemsB.Array[entity];
        var itemC = ItemsC.Array[entity];
        var itemD = ItemsD.Array[entity];

        executionFunc.ForEach(ref itemA, ref itemB, ref itemC, ref itemD);

        ItemsA.Array[entity] = itemA;
        ItemsB.Array[entity] = itemB;
        ItemsC.Array[entity] = itemC;
        ItemsD.Array[entity] = itemD;
    }
}

[BurstCompile(CompileSynchronously = true)]
public struct EachWithJob<A, B, C, D, E, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobExecute<A, B, C, D, E>
    where A : unmanaged
    where B : unmanaged
    where C : unmanaged
    where D : unmanaged
    where E : unmanaged
{
    private Executor executionFunc;
    private NativeWrappedData<int> Entites;
    private NativeWrappedData<A> ItemsA;
    private NativeWrappedData<B> ItemsB;
    private NativeWrappedData<C> ItemsC;
    private NativeWrappedData<D> ItemsD;
    private NativeWrappedData<E> ItemsE;

    public void Set(int[] entites, A[] itemsA, B[] itemsB, C[] itemsC, D[] itemsD, E[] itemsE, ref Executor action)
    {
        Entites = NativeMagic.WrapToNative(entites);
        ItemsA = NativeMagic.WrapToNative(itemsA);
        ItemsB = NativeMagic.WrapToNative(itemsB);
        ItemsC = NativeMagic.WrapToNative(itemsC);
        ItemsD = NativeMagic.WrapToNative(itemsD);
        ItemsE = NativeMagic.WrapToNative(itemsE);
        executionFunc = action;
    }

    public void Execute(int index)
    {
        var entity = Entites.Array[index];
        var itemA = ItemsA.Array[entity];
        var itemB = ItemsB.Array[entity];
        var itemC = ItemsC.Array[entity];
        var itemD = ItemsD.Array[entity];
        var itemE = ItemsE.Array[entity];

        executionFunc.ForEach(ref itemA, ref itemB, ref itemC, ref itemD, ref itemE);

        ItemsA.Array[entity] = itemA;
        ItemsB.Array[entity] = itemB;
        ItemsC.Array[entity] = itemC;
        ItemsD.Array[entity] = itemD;
        ItemsE.Array[entity] = itemE;
    }
}
public unsafe interface IJobForUnsafeExecute<A, B> where A : unmanaged where B : unmanaged
{
    int MaxIndex { get; set; }
    void Execute(A* componentsPool1, B* componentsPool2, int* entities, int currentIndex);
}

[BurstCompile(CompileSynchronously = true)]
public unsafe struct ForWithJob<A, B, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobForUnsafeExecute<A, B>
    where A : unmanaged
    where B : unmanaged
{
    private Executor executionFunc;
    [NativeDisableUnsafePtrRestriction]
    private int* Entites;

    [NativeDisableUnsafePtrRestriction]
    private A* ItemsA;
    [NativeDisableUnsafePtrRestriction]
    private B* ItemsB;

    public void Set(int[] entites, A[] itemsA, B[] itemsB, ref Executor action)
    {
        fixed (void* a = itemsA, b = itemsB, e = entites)
        {
            ItemsA = (A*) a;
            ItemsB = (B*) b;
            Entites = (int*)e;
        }
        
        executionFunc = action;
    }

    public void Execute(int index)
    {
        executionFunc.Execute(ItemsA, ItemsB, Entites, index);
    }
}

[EcsComponent]
[StructLayout(LayoutKind.Sequential)]
public struct RaycastHitPublic
{
    public Vector3 m_Point;
    public Vector3 m_Normal;
    public int m_FaceID;
    public float m_Distance;
    public Vector2 m_UV;
    public int m_ColliderID;
}

[BurstCompile(CompileSynchronously = true)]
public struct EachJobWithRaycast<A, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobExecute<A, RaycastHit>
    where A : unmanaged
{
    [ReadOnly] public NativeArray<RaycastHit> RaycastHits;
    public NativeWrappedData<A> ItemsA;
    public NativeWrappedData<int> Entities;
    public Executor Action;

    public void Execute(int index)
    {
        var entity = Entities.Array[index];
        var itemA = ItemsA.Array[entity];
        var raycast = RaycastHits[index];
        Action.ForEach(ref itemA, ref raycast);
        ItemsA.Array[entity] = itemA;
    }
#if UNITY_EDITOR
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        NativeMagic.UnwrapFromNative(ItemsA);
        NativeMagic.UnwrapFromNative(Entities);
    }
#endif
}
[BurstCompile(CompileSynchronously = true)]
public struct EachJobWithRaycast<A,B, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobExecute<A , B, RaycastHit>
    where A : unmanaged
    where B : unmanaged

{
    [ReadOnly] public NativeArray<RaycastHit> RaycastHits;
    public NativeWrappedData<A> ItemsA;
    public NativeWrappedData<B> ItemsB;
    public NativeWrappedData<int> Entities;
    public Executor Action;

    public void Execute(int index)
    {
        var entity = Entities.Array[index];
        var itemA = ItemsA.Array[entity];
        var itemB = ItemsB.Array[entity];

        var raycast = RaycastHits[index];
        Action.ForEach(ref itemA, ref itemB,ref raycast);
        ItemsA.Array[entity] = itemA;
        ItemsB.Array[entity] = itemB;

    }
#if UNITY_EDITOR
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        NativeMagic.UnwrapFromNative(ItemsA);
        NativeMagic.UnwrapFromNative(ItemsB);

        NativeMagic.UnwrapFromNative(Entities);
    }
#endif
}
[BurstCompile(CompileSynchronously = true)]
public struct EachJobWithRaycast<A,B,C, Executor> : IJobParallelFor
    where Executor : unmanaged, IJobExecute<A , B, C, RaycastHit>
    where A : unmanaged
    where B : unmanaged
    where C : unmanaged
{
    [ReadOnly] public NativeArray<RaycastHit> RaycastHits;
    public NativeWrappedData<A> ItemsA;
    public NativeWrappedData<B> ItemsB;
    public NativeWrappedData<C> ItemsC;
    public NativeWrappedData<int> Entities;
    public Executor Action;

    public void Execute(int index)
    {
        var entity = Entities.Array[index];
        var itemA = ItemsA.Array[entity];
        var itemB = ItemsB.Array[entity];
        var itemC = ItemsC.Array[entity];
        var raycast = RaycastHits[index];
        Action.ForEach(ref itemA, ref itemB,ref itemC,ref raycast);
        ItemsA.Array[entity] = itemA;
        ItemsB.Array[entity] = itemB;
        ItemsC.Array[entity] = itemC;
    }
#if UNITY_EDITOR
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        NativeMagic.UnwrapFromNative(ItemsA);
        NativeMagic.UnwrapFromNative(ItemsB);
        NativeMagic.UnwrapFromNative(ItemsC);
        NativeMagic.UnwrapFromNative(Entities);
    }
#endif
}

public interface IJobSystemTag{}


internal static class NativeMagic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* GetArrayPtr<T>(T[] data) where T : unmanaged
    {
        fixed (T* ptr = data)
        {
            return ptr;
        }
    }
    /// <summary>
    ///     Transform C# Array to NativeArray.
    ///     BIG THANKS FOR LEOPOTAM "https://github.com/Leopotam/ecslite-threads-unity"
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe NativeWrappedData<T> WrapToNative<T>(T[] managedData) where T : unmanaged
    {
        fixed (void* ptr = managedData)
        {
#if UNITY_EDITOR
            var nativeData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, managedData.Length, Allocator.TempJob);
            var sh = AtomicSafetyHandle.Create();
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeData, sh);
            return new NativeWrappedData<T> {Array = nativeData, SafetyHandle = sh};
#else
            return new NativeWrappedData<T> { Array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T> (ptr, managedData.Length, Allocator.None) };
#endif
        }
    }
#if UNITY_EDITOR
    public static void UnwrapFromNative<T>(NativeWrappedData<T> sh) where T : unmanaged
    {
        AtomicSafetyHandle.CheckDeallocateAndThrow(sh.SafetyHandle);
        AtomicSafetyHandle.Release(sh.SafetyHandle);
    }
#endif
}


public struct NativeWrappedData<TT> where TT : unmanaged
{
    [NativeDisableParallelForRestriction] public NativeArray<TT> Array;
#if UNITY_EDITOR
    public AtomicSafetyHandle SafetyHandle;
#endif
}



public static class UnsafeHelp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe A* ArrayToPtr<A>(A[] array) where A : unmanaged
    {
        fixed (A* ptr = array)
        {
            return ptr;
        }
    }

    public static unsafe void* ToVoidPtr<A>(this A[] array) where A : unmanaged
    {
        fixed (A* ptr = array)
        {
            return ptr;
        }
    }
    public static unsafe A* ToPointer<A>(this A[] array) where A : unmanaged
    {
        fixed (A* ptr = array)
        {
            return ptr;
        }
    }

    public static unsafe A* GetPtr<A>(this IntPtr ptr) where A : unmanaged
    {
        return (A*)ptr;
    }

    public unsafe static NativeArray<A> ToNative<A>(this A[] array) where A : unmanaged
    {
        fixed (void* ptr = array)
        {

            var nativeData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<A>(ptr, array.Length, Allocator.TempJob);
            return nativeData;
        }
    }
}

public interface IUnsafePool
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe void* Get();
}
public unsafe struct PoolBuffer
{
    [NativeDisableUnsafePtrRestriction] public void* buffer;
}