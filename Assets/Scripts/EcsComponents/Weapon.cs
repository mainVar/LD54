using UnityEngine;
using Wargon.ezs.Unity;

namespace LD54 {
    [EcsComponent]
    public struct Weapon {
        public MonoEntity projectile;
        public Transform firePoint;
        public float spread;
        public float timer;
    }
}