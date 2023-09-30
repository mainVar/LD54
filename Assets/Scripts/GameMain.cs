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
        private Systems updateSystems;
        public GameObject prefab;
        void Awake() {
            world = new World();
            MonoConverter.Init(world);
            var grid2D = new Grid2D(8, 11,5 , world, new Vector2(-10,-15));
            Injector.AddAsSingle(grid2D);
            updateSystems = new Systems(world)
            
                    .Add(new PrefabSpawnerSystem(prefab))
                    .Add(new Collision2DPostSystem())
                    .Add(new Collision2DGroup())
                    .Add(new SyncTransformSystem2())
                    .Add(new RemoveComponentSystem(typeof(EntityConvertedEvent)))
                ;
            updateSystems.Init();
            
            
            new DebugInfo(world);
        }

        // Update is called once per frame
        void Update()
        {
            if (world != null) {
                updateSystems.OnUpdate();
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
}

