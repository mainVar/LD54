using UnityEngine;
using Wargon.ezs;

namespace LD54 {
    partial class PoolLifetimeSystem : UpdateSystem {
        public override void Update() {
            var dt = Time.deltaTime;
            entities.Each((Pooled pool) => {

                if (pool.CurrentLifeTime > 0f) {
                    pool.CurrentLifeTime -= dt;
                }
                else {
                    pool.SetActive(false);
                }
            });
        }
    }
}