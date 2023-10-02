using UnityEngine;
using Wargon.ezs;
using Wargon.ezs.Unity;

namespace LD54 {
    partial class OnPlayerSpawnSystem : UpdateSystem {
        
        [Inject]
        GameService gameService;
        
        public override void Update() {
            entities.Each((Entity e, TransformRef transformRef, WeaponReference weapon, Player playerTag, EntityConvertedEvent convertedEvent) => {
                var childEntity = transformRef.value.GetChild(0).GetComponent<MonoEntity>();
                childEntity.ConvertToEntity();
                weapon.value = childEntity;
                weapon.value.Entity.SetOwner(e);
                gameService.PlayerEntity = e;
                Debug.Log("Player Spawned");
            });
        }
    }
}