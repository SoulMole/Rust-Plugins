namespace Oxide.Plugins
{
    [Info("Violation Kick", "Lonely Planet", "1.1.4")]
    internal class ViolationKick : RustPlugin
    {
        private string perm = "ViolationKick.prevent";
        private void Loaded()
        {
            permission.RegisterPermission(perm, this);
        }
        private object OnPlayerViolation(BasePlayer player, AntiHackType type)
        {
            if (!permission.UserHasPermission(player.UserIDString, perm)) return null;
            if (type != AntiHackType.InsideTerrain || type != AntiHackType.FlyHack) return null;
            return false;
        }
    }
}