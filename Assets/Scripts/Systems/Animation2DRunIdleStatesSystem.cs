using Animation2D;
using Wargon.ezs;

namespace LD54 {
    partial class Animation2DRunIdleStatesSystem : UpdateSystem {
        public override void Update() {
            entities.Each((SpriteAnimation spriteAnimation, InputComponent input, AnimationStates states) => {
                if (input.run) {
                    if (states.run.GetInstanceID() != spriteAnimation.AnimationList.GetInstanceID()) {
                        spriteAnimation.AnimationList = states.run;
                        spriteAnimation.PlayForce(spriteAnimation.currentState);
                    }
                }
                else {
                    if (states.idle.GetInstanceID() != spriteAnimation.AnimationList.GetInstanceID()) {
                        spriteAnimation.AnimationList = states.idle;
                        spriteAnimation.PlayForce(spriteAnimation.currentState);
                    }
                }

            });
        }
    }
}