using LD54.Sound;
using Wargon.ezs;

namespace LD54 {
    partial class EnemyDeathSystem : UpdateSystem {
        [Inject] private GameService gameService;
        public override void Update() {
            entities.Each((Entity entity, DeathEvent deathEvent, Enemy enemy, TransformRef transformRef, DeathSound sound, Pooled pooled) => {
                SoundManager.Instance.PlaySound(sound.value);
                pooled.SetActive(false);
                entity.Remove<DeathEvent>();
                gameService.killedEnemies++;
            });
        }
    }
}