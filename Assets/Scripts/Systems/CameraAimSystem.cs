using Wargon.ezs;

namespace LD54 {
    partial class CameraAimSystem : UpdateSystem {
        public override void Update() {
            entities.Each((Player Player, TransformComponent transformComponent) => {
                entities.Each((CameraTarget target, TransformRef transformRef) => {

                    var mousePos = Wargon.Kit.MousePosition();
                    transformRef.value.position = (mousePos + transformComponent.position) / 2;

                });
            });

        }
    }
}