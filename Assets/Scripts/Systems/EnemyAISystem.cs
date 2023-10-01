using Wargon.ezs;

namespace LD54 {
    partial class EnemyAISystem : UpdateSystem {
        public override void Update() {
            entities.Each((Entity entity, Player playerTag, TransformComponent playerTransform) => {
                entities.Each((Target target, InputComponent input, TransformComponent enemyTransform) => {
                    target.value = entity;
                    var direction = playerTransform.position - enemyTransform.position;
                    direction.Normalize();

                    input.horizontal = direction.x;
                    input.vertical = direction.y;
                });
            });
        }
    }
}