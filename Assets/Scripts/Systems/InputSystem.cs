using UnityEngine;
using Wargon.ezs;
using Wargon.ezs.Unity;

namespace LD54 {
    partial class InputSystem : UpdateSystem {
        public override void Update() {
            entities.Without<Inactive>().Each((InputComponent input) => {

                input.horizontal = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0F;
                input.vertical =  Input.GetKey(KeyCode.S) ? -1 : Input.GetKey(KeyCode.W) ? 1 : 0F;
                input.run = new Vector2(input.horizontal, input.vertical).magnitude > 0.1f;
            });
        }
    }
}