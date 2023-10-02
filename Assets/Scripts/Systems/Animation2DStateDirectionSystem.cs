using Animation2D;
using UnityEngine;
using Wargon.ezs;

namespace LD54 {
    partial class Animation2DStateDirectionSystem : UpdateSystem {
        private readonly string[] runStates = new[] { "N", "NW", "W", "SW", "S", "SE", "E", "NE" };
        public override void Update() {
            entities.Without<RotationByMouse>().Each((SpriteAnimation spriteAnimation, InputComponent input, AnimationIndex animationIndex) => {
                var dir = new Vector2(input.horizontal, input.vertical);
                if (dir.magnitude > 0.1f) {
                    animationIndex.value = DirectionToIndex(dir);
                    if (animationIndex.value != animationIndex.old) {
                        if (animationIndex.value < runStates.Length) {
                            spriteAnimation.Play(runStates[animationIndex.value]);
                            animationIndex.old = animationIndex.value;
                        }
                    }
                }

            });
            
        }

        private int DirectionToIndex(Vector2 dir) {
            var normDir = dir.normalized;
            float step = 360 / 8;
            float offset = step / 2;
            float angle = Vector2.SignedAngle(Vector2.up, normDir);

            angle += offset;
            if (angle < 0) {
                angle += 360;
            }

            float stepCount = angle / step;
            return Mathf.FloorToInt(stepCount);
            return 1;
        }
    }
}