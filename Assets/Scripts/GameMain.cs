using Animation2D;
using Roguelike.Physics2D;
using Unity.Mathematics;
using UnityEngine;
using Wargon.DI;
using Wargon.ezs;
using Wargon.ezs.Unity;
using Random = UnityEngine.Random;

namespace LD54 {
    [DefaultExecutionOrder(-35)]
    public class GameMain : MonoBehaviour {
        private World world;
        private Systems update;
        private Systems fixedUpdate;
        [SerializeField] GameObject prefab;
        [SerializeField] private AnimationsHolder animationsHolder;
        void Awake() {
            world = new World();
            MonoConverter.Init(world);
            var grid2D = new Grid2D(8, 11,5 , world, new Vector2(-10,-15));
            Injector.AddAsSingle(grid2D);
            Injector.AddAsSingle(animationsHolder);
            update = new Systems(world)
                    
                    .Add(new OnSpawnTransformSystem())
                    .Add(new InputSystem())
                    .Add(new MoveByInputSystem())
                    .Add(new PrefabSpawnerSystem(prefab))
                    .Add(new Animation2DStateDirectionSystem())
                    .Add(new Animation2DSystem())
                    .Add(new Collision2DPostSystem())
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

    [EcsComponent]
    public struct Health {
        public int current;
        public int max;
    }

    public struct CollisionPos {
        public float2 value;
    }

    public struct CollisionID {
        public int value;
    }
    public partial class Collision2DPostSystem : UpdateSystem {
        private EntityQuery projectiles;
        private EntityQuery withHealth;
        [Inject] private Grid2D grid2D;

        protected override void OnCreate() {
            projectiles = world.GetQuery()
                .Without<Inactive>()
                .Without<CollisionPos>()
                .With<Circle2D>()
                .With<Pooled>()
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
                entity.GetOrCreate<CollisionID>().value = hit.Index;
                entity.GetOrCreate<CollisionPos>().value = hit.Pos;
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
            entities.Each((TransformComponent TransformComponent, TransformRef transformRef, PooledEvent s) => {
                TransformComponent.position = transformRef.value.position;
                TransformComponent.rotation = transformRef.value.rotation;
                TransformComponent.scale = transformRef.value.localScale;
            });
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
        private GameObject prefab;
        private bool spawn;
        public PrefabSpawnerSystem(GameObject prefab) {
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
                Object.Instantiate(prefab, pos, Quaternion.identity);
            }

            if (Input.GetKey(KeyCode.R)) {
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.z = 0;
                var cached = Cast.CircleOverlap(pos, 6F, out int count);
                for (int i = 0; i < count; i++) {
                    var hit = cached[i];
                    var e = world.GetEntity(hit.Index);
                    if (!e.IsNULL()) {
                        Object.Destroy(e.Get<TransformRef>().value.gameObject);
                        e.Destroy();
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

    public partial class InputSystem : UpdateSystem {
        public override void Update() {
            entities.Without<Inactive>().Each((InputComponent input) =>
            {
                input.horizontal = Input.GetAxis("Horizontal");
                input.vertical = Input.GetAxis("Vertical");
            });
        }
    }
    public partial class MoveByInputSystem : UpdateSystem {
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
    [EcsComponent]
    public struct MoveSpeed {
        public float value;
    }
    public partial class Animation2DStateDirectionSystem : UpdateSystem {
        private readonly string[] runStates = new[] { "N", "NW", "W", "SW", "S", "SE", "E", "NE" };
        public override void Update() {
            entities.Each((Entity e, SpriteAnimation spriteAnimation, InputComponent input, AnimationIndex animationIndex) => {
                animationIndex.value = DirectionToIndex(new Vector2(input.horizontal, input.vertical));
                if (animationIndex.value != animationIndex.old) {
                    spriteAnimation.Play(runStates[animationIndex.value]);
                    animationIndex.old = animationIndex.value;
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
    [EcsComponent]
    public struct InputComponent {
        public float horizontal;
        public float vertical;
    }
    [EcsComponent]
    public struct AnimationIndex {
        public int value;
        public int old;
    }
}

