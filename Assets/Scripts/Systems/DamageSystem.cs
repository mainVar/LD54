using Wargon.ezs;

namespace LD54 {
    partial class DamageSystem : UpdateSystem {
        private EntityQuery damagedQuery;
        private Pool<EntityDamagedEvent> damagedEvent;
        protected override void OnCreate() {
            damagedQuery = world.GetQuery().With<EntityDamagedEvent>();
            damagedEvent = world.GetPool<EntityDamagedEvent>();
        }

        public override void Update() {
            for (var i = 0; i < damagedQuery.Count; i++) {
                var entityIndex = damagedQuery.GetEntityIndex(i);
                ref var damageEvent = ref damagedEvent.Get(entityIndex);
                if (!damageEvent.to.IsNULL()) {
                    ref var health = ref damageEvent.to.Get<Health>();

                    health.current -= damageEvent.amount;
                    if (health.current <= 0) {
                        damageEvent.to.Add(new DeathEvent());
                    }
                    world.GetEntity(entityIndex).Destroy();
                }
            }
        }
    }
}