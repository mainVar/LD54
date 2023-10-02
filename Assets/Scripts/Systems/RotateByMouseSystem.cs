using System;
using System.Linq;
using Animation2D;
using UnityEngine;
using Wargon.ezs;

namespace LD54 {
    partial class RotateByMouseSystem : UpdateSystem {
        private readonly string[] runStates = new[] { "N", "NW", "W", "SW", "S", "SE", "E", "NE" };
        public override void Update() {
            entities.Each((Entity playerE, SpriteAnimation spriteAnimation, InputComponent input, AnimationIndex animationIndex, WeaponReference weapon, RotationByMouse mouseTag) => {
                if (!weapon.value.Entity.IsNULL()) {
                    var e = weapon.value.Entity;
                    // if(!e.HasOwner())
                    //     e.SetOwner(playerE);
                    ref var wTransform = ref weapon.value.Entity.Get<TransformRef>();
                    var difference = Wargon.Kit.MousePosition() - wTransform.value.position;
                    difference.Normalize();
                    var rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                    //var r = getAngle(rotZ);
                    wTransform.value.rotation = Quaternion.Euler(0, 0, rotZ);
                    animationIndex.value = DirectionToIndex(difference);
                    if (animationIndex.value != animationIndex.old) {
                        spriteAnimation.Play(runStates[animationIndex.value]);
                        animationIndex.old = animationIndex.value;
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
        private readonly float[] rots =
        {
            -11.25f, -22.5f, -33.75f, -45f, -56.25f, -67.5f, -78.75f, -90f, -101.25f, -112.5f, -123.75f, -135f, -146.25f,
            -157.5f, -168.75f,
            0f, 11.25f, 22.5f, 33.75f, 45f, 56.25f, 67.5f, 78.75f, 90f, 101.25f, 112.5f, 123.75f, 135f, 146.25f, 157.5f,
            168.75f, 180f
        };
        private float getAngle(float rot)
        {
            return rots.Aggregate((x, y) => Math.Abs(x - rot) < Math.Abs(y - rot) ? x : y);
        }
    }
}