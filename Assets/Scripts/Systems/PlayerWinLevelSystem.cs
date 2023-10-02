using Wargon.ezs;
using Wargon.ezs.Unity;

namespace LD54
{
    partial class PlayerWinLevelSystem : UpdateSystem
    {
        public override void Update()
        {
            entities.Without<Inactive>().Each((Entity entity, PlayerWin playerWin, WinColliderZone winColliderZone) =>
            {
                winColliderZone.UiController.PlayerWin();
                entity.Remove<PlayerWin>();
                entity.Add(new Inactive());
            });
        }
    }
}