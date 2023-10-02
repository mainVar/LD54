using UnityEngine;
using Wargon.ezs;

namespace LD54 {
    partial class PlayerDeathSystem : UpdateSystem {
        public override void Update() {
            entities.Each((Entity entity, DeathEvent deathEvent, Player enemy, TransformRef transformRef) => {
                Wargon.ezs.Unity.Log.Show(Color.red, "Game Over");
                entity.Remove<DeathEvent>();
                Object.Destroy(transformRef.value.gameObject);
            });
        }
    }
}