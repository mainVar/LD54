using Wargon.ezs;
using Wargon.ezs.Unity;

namespace LD54
{
    partial class RoomSpawnerSystem : UpdateSystem {
        private EnemySpawnersList oldSpawner;
        public override void Update()
        {
            entities.Without<Inactive>().Each((Entity entity, ActiveRoom activeRoom, EnableRoomSpawner enableRoomSpawner) =>
            {
                foreach (var spawnPoints in activeRoom.EnemySpawnersList.EnemySpawners)
                {
                    spawnPoints.gameObject.SetActive(true);
                }

                if (oldSpawner != null) {
                    foreach (var spawnPoints in oldSpawner.EnemySpawners)
                    {
                        spawnPoints.gameObject.SetActive(false);
                    }
                }

                oldSpawner = activeRoom.EnemySpawnersList;
                entity.Remove<EnableRoomSpawner>();
                entity.Add(new Inactive());
            });
        }
    }
}