using Animation2D;
using Roguelike.Physics2D;
using UnityEngine;
using Wargon.DI;
using Wargon.ezs;
using Wargon.ezs.Unity;

namespace LD54 {
    [DefaultExecutionOrder(-35)]
    public class GameMain : MonoBehaviour {
        private World world;
        private Systems update;
        private Systems fixedUpdate;
        [SerializeField] MonoEntity prefab;
        [SerializeField] private AnimationsHolder animationsHolder;
        [SerializeField] private GameService gameService;
        private Grid2D grid2D;
        private bool run;
        public void Stop() {
            run = false;
        }
        void Awake() {
            Application.targetFrameRate = 1400;
            QualitySettings.vSyncCount = 0;
            // Configs.PoolsCacheSize = 5000;
            // Configs.EntityCacheSize = 5000;
            // Configs.ComponentCacheSize = 5000;
            world = new World();
            MonoConverter.Init(world);
            grid2D = new Grid2D(16, 16,5 , world, new Vector2(-10,-12));
            Injector.AddAsSingle(grid2D);
            Injector.AddAsSingle(animationsHolder);
            Injector.AddAsSingle(gameService);
            Injector.AddAsSingle(this);
            update = new Systems(world)
                    
                    .Add(new CameraAimSystem())
                    .Add(new OnSpawnTransformSystem())
                    .Add(new OnPlayerSpawnSystem())
                    .Add(new InputSystem())
                    .Add(new EnemyAISystem())
                    .Add(new PoolLifetimeSystem())
                    .Add(new MoveByInputSystem())
                    
                    .Add(new PrefabSpawnerSystem(prefab))
                    
                    .Add(new EnemyAttackDelay())
                    .Add(new WeaponShootingSystem())
                    .Add(new ProjectileMoveSystem())
                    .Add(new RotateByMouseSystem())
                    .Add(new Animation2DRunIdleStatesSystem())
                    .Add(new Animation2DStateDirectionSystem())
                    .Add(new Animation2DSystem())
                    
                    //.Add(new UpdateCollisionGridPosition())
                    .Add(new Collision2DPostSystem())
                    .Add(new DamageSystem())
                    .Add(new EnemyDeathSystem())
                    .Add(new PlayerDeathSystem())
                    
                    
                    .Add(new Collision2DGroup())
                    .Add(new SyncTransformSystem2())
                    .Add(new RemoveComponentSystem(typeof(EntityConvertedEvent)))
                    .Add(new RemoveComponentSystem(typeof(PooledEvent)))
                    .Add(new RoomSpawnerSystem())
                ;
            update.Init();
            // fixedUpdate = new Systems(world)
            //
            //     ;
            //
            // fixedUpdate.Init();
#if UNITY_EDITOR
            new DebugInfo(world);
#endif
            run = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (run) {
                update.OnUpdate();
            }
        }

        private void OnDestroy() {
            run = false;
            if(world==null) return;
            world.Destroy();
            world = null;
            grid2D.Clear();
            Injector.Dispose();
        }
    }
}

