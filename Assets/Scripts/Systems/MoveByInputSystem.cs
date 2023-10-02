using UnityEngine;
using Wargon.ezs;
using Wargon.ezs.Unity;

namespace LD54 {
    partial class MoveByInputSystem : UpdateSystem {
        private const float MINIMUM_INPUT = 0.01f;
        public override void Update() {
            var dt = Time.deltaTime;
            entities.Without<Inactive>().Each((TransformComponent transform, InputComponent input, MoveSpeed speed) =>
            {
                float trueMoveSpeed;

                if (Mathf.Abs(input.horizontal) > MINIMUM_INPUT && Mathf.Abs(input.vertical) > MINIMUM_INPUT)
                    trueMoveSpeed = speed.value * 0.75f;
                else
                    trueMoveSpeed = speed.value;
        
                if (input.horizontal > MINIMUM_INPUT || input.horizontal < -MINIMUM_INPUT)
                    transform.position += new Vector3(input.horizontal * trueMoveSpeed * dt, 0);

                if (input.vertical > MINIMUM_INPUT || input.vertical < -MINIMUM_INPUT)
                    transform.position += new Vector3(0, input.vertical * trueMoveSpeed * dt);

            });
        }
    }
}