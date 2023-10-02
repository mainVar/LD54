using LD54.Sound;
using UnityEngine;
using Wargon.ezs;

namespace LD54 {
    partial class EnemyDeathSystem : UpdateSystem {
        [Inject] private GameService gameService;
        [Inject] BakedParticles bakedParticles;
        public override void Update() {
            entities.Each((Entity entity, DeathEvent deathEvent, Enemy enemy, TransformComponent transform, DeathSound sound, Pooled pooled, DeathEffect deathEffect) => {
                SoundManager.Instance.PlaySound(sound.value);
                pooled.SetActive(false);
                entity.Remove<DeathEvent>();
                bakedParticles.Show(deathEffect.value, transform.position, Quaternion.identity);
                gameService.killedEnemies++;
            });
        }
    }
}