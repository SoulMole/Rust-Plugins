using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Cold Damage", "Lonely Planet", "1.1.4")]
    public class ColdDamage : RustPlugin
    {
        void Init()
        {
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                player.metabolism.temperature.value = 32;
                player.metabolism.temperature.min = 32;
                player.metabolism.temperature.max = 32;
            }
        }
        void OnPlayerConnected(BasePlayer player)
        {
            player.metabolism.temperature.value = 32;
            player.metabolism.temperature.min = 32;
            player.metabolism.temperature.max = 32;
        }
    }
}