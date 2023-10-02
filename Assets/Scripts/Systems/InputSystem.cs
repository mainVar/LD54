using UnityEngine;
using Wargon.ezs;
using Wargon.ezs.Unity;

namespace LD54 {
    partial class InputSystem : UpdateSystem {
        public override void Update() {
            entities.Without<Inactive>().Each((InputComponent input) => {
                var h = Input.GetAxis("Horizontal");
                var v = Input.GetAxis("Vertical");
                input.horizontal = h > 0 ? 1 : h < 0 ? -1 : 0;
                input.vertical =  v > 0 ? 1 : v < 0 ? -1 : 0;
                input.run = new Vector2(input.horizontal, input.vertical).magnitude > 0.1f;
            });
        }
    }
}