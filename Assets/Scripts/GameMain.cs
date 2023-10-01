using System;
using System.Linq;
using Animation2D;
using LD54.Sound;
using Roguelike.Physics2D;
using Unity.Mathematics;
using UnityEngine;
using Wargon.DI;
using Wargon.ezs;
using Wargon.ezs.Unity;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace LD54 {
    [DefaultExecutionOrder(-35)]
    public class GameMain : MonoBehaviour {
        private World world;
        private Systems update;
        private Systems fixedUpdate;
        [SerializeField] MonoEntity prefab;
        [SerializeField] private AnimationsHolder animationsHolder;
        [SerializeField] private GameService gameService;
        
        void Awake() {
            Configs.PoolsCacheSize = 5000;
            Configs.EntityCacheSize = 5000;
            Configs.ComponentCacheSize = 5000;
            world = new World();
            MonoConverter.Init(world);
            var grid2D = new Grid2D(10, 15,5 , world, new Vector2(-10,-15));
            Injector.AddAsSingle(grid2D);
            Injector.AddAsSingle(animationsHolder);
            Injector.AddAsSingle(gameService);
            
            update = new Systems(world)
                    
                    
                    .Add(new OnSpawnTransformSystem())
                    .Add(new OnPlayerSpawnSystem())
                    .Add(new InputSystem())
                    .Add(new EnemyAISystem())
                    .Add(new PoolLifetimeSystem())
                    .Add(new MoveByInputSystem())
                    
                    .Add(new PrefabSpawnerSystem(prefab))
                    
                    
                    .Add(new WeaponShootingSystem())
                    .Add(new ProjectileMoveSystem())
                    .Add(new RotateByMouseSystem())
                    .Add(new Animation2DRunIdleStatesSystem())
                    .Add(new Animation2DStateDirectionSystem())
                    .Add(new Animation2DSystem())
                    
                    .Add(new Collision2DPostSystem())
                    .Add(new DamageSystem())
                    .Add(new EnemyDeathSystem())
                    .Add(new PlayerDeathSystem())
                    
                    
                    .Add(new Collision2DGroup())
                    .Add(new SyncTransformSystem2())
                    .Add(new RemoveComponentSystem(typeof(EntityConvertedEvent)))
                    .Add(new RemoveComponentSystem(typeof(PooledEvent)))
                ;
            update.Init();
            fixedUpdate = new Systems(world)

                ;
            
            fixedUpdate.Init();

            new DebugInfo(world);
        }

        // Update is called once per frame
        void Update()
        {
            if (world != null) {
                update.OnUpdate();
            }
        }
        void FixedUpdate()
        {
            if (world != null) {
                fixedUpdate.OnUpdate();
            }
        }
        private void OnDestroy() {
            if(world==null) return;
            world.Destroy();
            world = null;
            Grid2D.Instance.Clear();
        }
    }

    public struct CollisionPos {
        public float2 value;
    }

    public struct CollisionID {
        public int value;
    }
    partial class Collision2DPostSystem : UpdateSystem, 
        IRemoveBefore<CollisionPos>, 
        IRemoveBefore<CollisionID> 
    {
        private EntityQuery projectiles;
        private EntityQuery withHealth;
        [Inject] private Grid2D grid2D;
        protected override void OnCreate() {
            projectiles = world.GetQuery()
                .Without<Inactive>()
                .Without<CollisionPos>()
                .With<Circle2D>()
                .With<Pooled>()
                .With<Damage>()
                .With<Owner>();

            withHealth = world.GetQuery()
                .Without<Inactive>()
                .With<Circle2D>()
                .With<Health>();


        }

        public override void Update() {
            var hits = grid2D.Hits;

            while (hits.Count > 0) {
                var hit = hits.Dequeue();
                if(hit.From ==-1) continue;
                Entity entity = world.GetEntity(hit.From);
                Entity entityTo = default;
                if (hit.Index != -1) {
                    entityTo = world.GetEntity(hit.Index);
                    entity.GetOrCreate<CollisionID>().value = hit.Index;
                }

                entity.GetOrCreate<CollisionPos>().value = hit.Pos;
                if (projectiles.Has(in entity)) {
                    entity.Get<Pooled>().SetActive(false);
                    // BULLETS COLLISIONS
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                }
                if (withHealth.Has(in entityTo)) {
                    world.CreateEntity().Add(new EntityDamagedEvent() {
                        @from = entity,
                        to = entityTo,
                        amount = entity.Get<Damage>().value
                    });
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

    partial class OnSpawnTransformSystem : UpdateSystem {
        public override void Update() {

            entities.Each((TransformComponent TransformComponent, TransformRef transformRef, EntityConvertedEvent s) => {
                TransformComponent.position = transformRef.value.position;
                TransformComponent.rotation = transformRef.value.rotation;
                TransformComponent.scale = transformRef.value.localScale;
            });
        }
    }
    partial class PrefabSpawnerSystem : UpdateSystem {
        private float time = 0.001f;
        private float timer;
        private MonoEntity prefab;
        private bool spawn;
        public PrefabSpawnerSystem(MonoEntity prefab) {
            this.prefab = prefab;}
        public override void Update() {
            timer -= Time.deltaTime;
            if (Input.GetKey(KeyCode.Space))
                spawn = !spawn;
            if (Input.GetKey(KeyCode.Space)) {
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.z = 0;
                pos.x += Random.Range(-0.1f, 0.1f);
                pos.y += Random.Range(-0.1f, 0.1f);
                //for (int i = 0; i < 6; i++) {
                //var pos = RandomPointOnCircleEdge(19,Camera.main.transform.position);
                ObjectPool.ReuseEntity(prefab, pos, Quaternion.identity);
            }

            if (Input.GetKey(KeyCode.R)) {
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.z = 0;
                var cached = Cast.CircleOverlap(pos, 2F, out int count);
                for (int i = 0; i < count; i++) {
                    var hit = cached[i];
                    var e = world.GetEntity(hit.Index);
                    if (!e.IsNULL()) {
                        if(e.Has<Pooled>())
                            e.Get<Pooled>().SetActive(false);
                    }

                }
                cached.Dispose();
            }

        }
        private Vector3 RandomPointOnCircleEdge(float radius, Vector2 offset)
        {
            //return Quaternion.Euler( 0, 0, Random.Range(0,360)) * Vector3.up * radius;
            var vector2 = Random.insideUnitCircle.normalized * radius;
            vector2 += offset;
            return new Vector3(vector2.x, vector2.y, 0);
        }
    }

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
    partial class MoveByInputSystem : UpdateSystem {
        private const float MINIMUM_INPUT = 0.01f;
        public override void Update() {
            var dt = Time.deltaTime;
            entities.Without<Inactive>().Each((TransformComponent transform, InputComponent input, MoveSpeed speed, Circle2D circle2D) =>
            {
                if (!circle2D.collided) {
                    float trueMoveSpeed;

                    if (Mathf.Abs(input.horizontal) > MINIMUM_INPUT && Mathf.Abs(input.vertical) > MINIMUM_INPUT)
                        trueMoveSpeed = speed.value * 0.75f;
                    else
                        trueMoveSpeed = speed.value;
            
                    if (input.horizontal > MINIMUM_INPUT || input.horizontal < -MINIMUM_INPUT)
                        transform.position += new Vector3(input.horizontal * trueMoveSpeed * dt, 0);

                    if (input.vertical > MINIMUM_INPUT || input.vertical < -MINIMUM_INPUT)
                        transform.position += new Vector3(0, input.vertical * trueMoveSpeed * dt);
                }

            });
        }
    }
    
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

    partial class WeaponShootingSystem : UpdateSystem {
        public override void Update() {
            var dt = Time.deltaTime;
            entities.Each((Entity e, Weapon weapon, ProjectilePerShot projectilePerShot, AttackDelay attackDelay, Owner owner, ShootingSound soundData) => {
                if (weapon.timer > 0) {
                    weapon.timer -= dt;
                }
                else {
                    if (Input.GetMouseButton(0)) {
                        for (int i = 0; i < projectilePerShot.value; i++) {
                            var rot = Quaternion.Euler(0, 0, weapon.firePoint.eulerAngles.z + Random.Range(-weapon.spread, weapon.spread));
                            var bullet = ObjectPool.ReuseEntity(weapon.projectile, weapon.firePoint.position, Quaternion.identity);
                            ref var dir = ref bullet.GetRef<Direction>();
                            dir.value = (rot * Vector3.right).normalized;
                            bullet.Get<TransformComponent>().rotation = rot;
                            bullet.SetOwner(owner.Value);
                            
                        }
                        SoundManager.Instance.PlaySound(soundData.SoundData);
                        weapon.timer = attackDelay.value;
                    }
                }
            });
        }
    }

    partial class ProjectileMoveSystem : UpdateSystem
    {
        private float dt;
        public override void Update()
        {
            dt = Time.deltaTime;
            entities.Without<Inactive>().Each((MoveSpeed speed, TransformComponent transform, Direction direction) =>
            {
                transform.position += direction.value * speed.value * dt;
            });
        }
    }
    
    partial class OnPlayerSpawnSystem : UpdateSystem {
        
        [Inject]
        GameService gameService;
        
        public override void Update() {
            entities.Each((Entity e, TransformRef transformRef, WeaponReference weapon, Player playerTag, EntityConvertedEvent convertedEvent) => {
                var childEntity = transformRef.value.GetChild(0).GetComponent<MonoEntity>();
                childEntity.ConvertToEntity();
                weapon.value = childEntity;
                weapon.value.Entity.SetOwner(e);
                gameService.PlayerEntity = e;
                Debug.Log("Player Spawned");
            });
        }
    }
    
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

    partial class PoolLifetimeSystem : UpdateSystem {
        public override void Update() {
            var dt = Time.deltaTime;
            entities.Each((Pooled pool) => {

                if (pool.CurrentLifeTime > 0f) {
                    pool.CurrentLifeTime -= dt;
                }
                else {
                    pool.SetActive(false);
                }
            });
        }
    }

    public struct EntityDamagedEvent {
        public Entity from;
        public Entity to;
        public int amount;
    }
    [EcsComponent]
    public struct Damage {
        public int value;
    }
    public struct DeathEvent {}
    [EcsComponent] public struct Enemy{}

    [EcsComponent]
    public struct DeathSound {
        public SoundData value;
    }
    partial class DamageSystem : UpdateSystem {
        private EntityQuery damagedQuery;
        private Pool<EntityDamagedEvent> damagedEvent;
        private Pool<Health> healthes;
        protected override void OnCreate() {
            damagedQuery = world.GetQuery().With<EntityDamagedEvent>();
            damagedEvent = world.GetPool<EntityDamagedEvent>();
            healthes = world.GetPool<Health>();
        }

        public override void Update() {
            for (var i = 0; i < damagedQuery.Count; i++) {
                var entityIndex = damagedQuery.GetEntityIndex(i);
                ref var damageEvent = ref damagedEvent.Get(entityIndex);

                ref var health = ref damageEvent.to.Get<Health>();

                health.current -= damageEvent.amount;
                if (health.current <= 0) {
                    damageEvent.to.Add(new DeathEvent());
                }
                world.GetEntity(entityIndex).Destroy();
            }
        }
    }

    partial class EnemyDeathSystem : UpdateSystem {
        
        public override void Update() {
            entities.Each((Entity entity, DeathEvent deathEvent, Enemy enemy, TransformRef transformRef, DeathSound sound, Pooled pooled) => {
                SoundManager.Instance.PlaySound(sound.value);
                pooled.SetActive(false);
                entity.Remove<DeathEvent>();
            });
        }
    }
    partial class PlayerDeathSystem : UpdateSystem {
        public override void Update() {
            entities.Each((Entity entity, Pooled pooled, DeathEvent deathEvent, Player enemy) => {
                pooled.SetActive(false);
                entity.Remove<DeathEvent>();
            });
        }
    }
}

