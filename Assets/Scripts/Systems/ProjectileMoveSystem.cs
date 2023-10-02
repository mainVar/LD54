using UnityEngine;
using Wargon.ezs;
using Wargon.ezs.Unity;

namespace LD54 {
    partial class ProjectileMoveSystem : UpdateSystem
    {
        private float dt;
        public override void Update()
        {
            dt = Time.deltaTime;
            entities.Without<Inactive>().Each((MoveSpeed speed, TransformComponent transform, Direction direction) =>
            {
                transform.position += direction.value * speed.value * dt;
            });
        }
    }
}