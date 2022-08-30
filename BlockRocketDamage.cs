namespace Oxide.Plugins
{
    [Info("BlockRocketDamage", "Lonely Planet", "1.0.2")]
    class BlockRocketDamage : RustPlugin
    {
        object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo hitinfo)
        {

            if (entity is BasePlayer)
            {
                var player = entity as BasePlayer;
                if (player == null) return null;
                if (!hitinfo.WeaponPrefab) return null;
                if (!hitinfo.WeaponPrefab.ShortPrefabName.Contains("rocket")) return null;
                    hitinfo.damageTypes.ScaleAll(0.15f);
                return null;
            }
            return null;
        }
    }
}

/*
namespace Oxide.Plugins
{
    [Info("BlockRocketDamage", "Lonely Planet", "1.0.2")]
    class BlockRocketDamage : RustPlugin
    {
        object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo hitinfo)
        {
            
            if (entity is BasePlayer)
            {
                var player = entity as BasePlayer;
                if (player == null) return null;
                Puts("was player");
            
                if (hitinfo.WeaponPrefab.ShortPrefabName.Contains("rocket"))
                {
                    Puts("blocked damage");
                    hitinfo.damageTypes.ScaleAll(0.15f);
                }
                return null;
            }
            return null;
        }
    }
}
*/