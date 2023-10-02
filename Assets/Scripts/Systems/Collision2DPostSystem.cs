using Roguelike.Physics2D;
using Wargon.ezs;
using Wargon.ezs.Unity;

namespace LD54 {
    partial class Collision2DPostSystem : UpdateSystem, 
        IRemoveBefore<CollisionPos>, 
        IRemoveBefore<CollisionID> 
    {
        private EntityQuery projectiles;
        private EntityQuery enemies;
        private EntityQuery player;
        private EntityQuery enemiesThatCanAttack;
        [Inject] private Grid2D grid2D;
        protected override void OnCreate() {
            projectiles = world.GetQuery()
                .Without<Inactive>()
                .Without<CollisionPos>()
                .With<Circle2D>()
                .With<Pooled>()
                .With<Damage>()
                .With<Owner>();

            enemies = world.GetQuery()
                .Without<Inactive>()
                .Without<Player>()
                .With<Circle2D>()
                .With<Health>();
                
            player = world.GetQuery()
                .Without<Inactive>()
                .With<Player>()
                .With<Circle2D>()
                .With<Health>();

        }

        public override void Update() {
            var hits = grid2D.Hits;

            while (hits.Count > 0) {
                var hit = hits.Dequeue();
                if(hit.From ==-1) continue;
                Entity entityFrom = world.GetEntity(hit.From);
                Entity entityTo = default;
                if (hit.Index != -1) {
                    entityTo = world.GetEntity(hit.Index);
                    entityFrom.GetOrCreate<CollisionID>().value = hit.Index;
                }

                entityFrom.GetOrCreate<CollisionPos>().value = hit.Pos;
                if (projectiles.Has(in entityFrom)) {
                    entityFrom.Get<Pooled>().SetActive(false);
                    // BULLETS COLLISIONS
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                }
                if (enemies.Has(in entityTo)) {
                    world.CreateEntity().Add(new EntityDamagedEvent() {
                        @from = entityFrom,
                        to = entityTo,
                        amount = entityFrom.Get<Damage>().value
                    });
                }

                if (player.Has(in entityTo) && entityFrom.Has<CanAttack>()) {
                    world.CreateEntity().Add(new EntityDamagedEvent() {
                        @from = entityFrom,
                        to = entityTo,
                        amount = entityFrom.Get<Damage>().value
                    });
                    entityFrom.Remove<CanAttack>();
                }
                // if (projectiles.Has(entity.id)) {
                //     if (!entity.Has<Ricochet>() && !entity.Has<FlyThrough>()) {
                //         entity.Get<Pooled>().SetActive(false);
                //     }
                //     ObjectPool.ReuseEntity(implacs.Get(entity.id).Value, new Vector3(hit.Pos.x, hit.Pos.y), Quaternion.identity).SetOwner(entity.GetOwner());
                // }
            }
        }
    }
}