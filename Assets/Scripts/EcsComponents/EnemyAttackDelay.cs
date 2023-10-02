using UnityEngine;
using Wargon.ezs;

namespace LD54 {
    partial class EnemyAttackDelay : UpdateSystem {
        public override void Update() {
            var dt = Time.deltaTime;
            entities.Without<CanAttack>().Each((Entity e, EnemyAttack enemyAttack) => {
                if (enemyAttack.delayCounter > 0) {
                    enemyAttack.delayCounter -= dt;
                }
                else {
                    enemyAttack.delayCounter = enemyAttack.delay;
                    e.Add(new CanAttack());
                }
            });
        }
    }
}