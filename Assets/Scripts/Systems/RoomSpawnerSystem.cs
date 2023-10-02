using Wargon.ezs;
using Wargon.ezs.Unity;

namespace LD54
{
    partial class RoomSpawnerSystem : UpdateSystem
    {
        public override void Update()
        {
            entities.Without<Inactive>().Each((Entity entity, ActiveRoom activeRoom, EnableRoomSpawner enableRoomSpawner) =>
            {
                foreach (var spawnPoints in activeRoom.EnemySpawnersList.EnemySpawners)
                {
                    spawnPoints.gameObject.SetActive(true);
                }
                
                entity.Remove<EnableRoomSpawner>();
                entity.Add(new Inactive());
            });
        }
    }
}