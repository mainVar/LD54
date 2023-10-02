using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Wargon.ezs;

public class BakedParticles : MonoBehaviour {
    public List<KeyValueRef<string, ParticleSystem>> list;
    private Dictionary<string, ParticleToEmit> systems = new();
    private ParticleSystem.EmitParams Params;
    private bool initializedFlag;

    public void Initialize() {
        if(initializedFlag) return;
        foreach (var keyValueRef in list) {
            
            systems.Add(keyValueRef.Key, new ParticleToEmit(keyValueRef.Value));
        }
        initializedFlag = true;
    }

    private void Awake() {
        Initialize();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ParticleToEmit Get(string name) {
        if (systems.TryGetValue(name, out var system))
            return system;
        throw new KeyNotFoundException(name);
    }

    public void Show(string name, Vector3 pos, bool flipX = false) {
        pos.z = pos.y * 0.01F;
        Params.position = pos;
        var rotation3D = Params.rotation3D;

        if (flipX == true) {
            rotation3D.x = 180;
            rotation3D.z = 180;
        }
        else {
            rotation3D.x = 0;
            rotation3D.z = 0;
        }

        Params.rotation3D = rotation3D;
        Get(name).Emit(Params, 1);
    }
}
[System.Serializable]
public class KeyValueRef<TKey, TValue> {
    public TKey Key;
    public TValue Value;
}

[EcsComponent]
public struct ParticleEmitView {
    public string name;
}

public struct ParticleToEmit {
    private ParticleSystem[] Systems;
    
    public ParticleToEmit(ParticleSystem particleSystem) {
        var childs = particleSystem.GetComponentsInChildren<ParticleSystem>();
        Systems = new ParticleSystem[childs.Length + 1];
        Systems[0] = particleSystem;
        for (var i = 1; i < Systems.Length; i++) {
            Systems[i] = childs[i-1];
        }
    }

    public void Emit(ParticleSystem.EmitParams param, int coint) {
        for (var i = 0; i < Systems.Length; i++) {
            Systems[i].Emit(param, coint);
        }
    }
}
    public struct Triggered { }
    public sealed partial class ParticleEmitTriggerSystem : UpdateSystem {
    [Inject] private BakedParticles BakedParticles;
    private ParticleSystem.EmitParams Params;
    protected override void OnCreate() {
        BakedParticles.Initialize();
    }

    public override void Update() {
        entities.Each((ParticleEmitView view, TransformComponent transform, Triggered triggered) => {
            Params.position = transform.position;
            BakedParticles.Get(view.name).Emit(Params, 1);
        });
    }
}

public static class ParticleSystemExtensions {
    public static void Destroy(this ParticleSystem.Particle particle) {
        particle.remainingLifetime = -1;
    }
}
