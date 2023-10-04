using Roguelike.Physics2D;
using Unity.Mathematics;
using UnityEngine;
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
        private EntityQuery activeRoom;
        private EntityQuery winCollide;
        private Pool<Circle2D> colliders;
        private Pool<Pooled> pooledObjets;
        [Inject] private Grid2D grid2D;
        [Inject] private BakedParticles particles;
        protected override void OnCreate() {
            colliders = world.GetPool<Circle2D>();
            pooledObjets = world.GetPool<Pooled>();
            projectiles = world.GetQuery()
                .Without<Inactive>()
                .Without<CollisionPos>()
                .With<Circle2D>()
                .With<Pooled>()
                .With<Damage>()
                .With<Impact>()
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


            activeRoom = world.GetQuery()
                .Without<Inactive>()
                .With<Circle2D>()
                .With<ActiveRoom>();
            
            winCollide = world.GetQuery()
                .Without<Inactive>()
                .With<Circle2D>()
                .With<WinColliderZone>();

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
                    ref var p = ref pooledObjets.Get(entityFrom.id);
                    ref var collider = ref colliders.Get(entityFrom.id);
                    ref var transform = ref entityFrom.Get<TransformComponent>();
                    p.SetActive(false);
                    collider.collided = false;
                    // BULLETS COLLISIONS
                    particles.Show(entityFrom.Get<Impact>().key, transform.position, transform.rotation);
                    
                }
                if (enemies.Has(in entityTo)) {
                    world.CreateEntity().Add(new EntityDamagedEvent() {
                        @from = entityFrom,
                        to = entityTo,
                        amount = entityFrom.Get<Damage>().value
                    });
                }

                if (player.Has(in entityTo) && entityFrom.Has<CanAttack>()) {
                    world.CreateEntity().Add(new EntityDamagedEvent {
                        @from = entityFrom,
                        to = entityTo,
                        amount = entityFrom.Get<Damage>().value
                    });
                    entityFrom.Remove<CanAttack>();
                }


                if (activeRoom.Has(in entityTo) && entityFrom.Has<Player>())
                {
                    entityTo.Add(new EnableRoomSpawner());
                }
                
                if (winCollide.Has(in entityTo) && entityFrom.Has<Player>())
                {
                    entityTo.Add(new PlayerWin());
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