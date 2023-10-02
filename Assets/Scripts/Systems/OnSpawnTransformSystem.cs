using Wargon.ezs;
using Wargon.ezs.Unity;

namespace LD54 {
    partial class OnSpawnTransformSystem : UpdateSystem {
        public override void Update() {

            entities.Each((TransformComponent TransformComponent, TransformRef transformRef, EntityConvertedEvent s) => {
                TransformComponent.position = transformRef.value.position;
                TransformComponent.rotation = transformRef.value.rotation;
                TransformComponent.scale = transformRef.value.localScale;
            });
        }
    }
}