using UnityEngine;

public static class GameplayResourceBootstrap
{
    public static void RegisterPools(PoolingManager pool)
    {
        Register(pool, "Prefabs/UI/WorldUI/Part/DamageFont_TMP", true);
        Register(pool, "Prefabs/UI/WorldUI/Part/NPC_Text", true);
        Register(pool, "Prefabs/HitBox/HitBoxPlayer_Prefab", false);
        Register(pool, "Prefabs/HitBox/HitBoxMonster_Prefab", false);
        Register(pool, "Prefabs/Items/Prefabs/Coin", true);
        Register(pool, "Prefabs/Items/Prefabs/BoxOfPandora", true);
        Register(pool, "Prefabs/Items/Prefabs/Bottle_Mana", true);
        Register(pool, "Prefabs/Items/Prefabs/Bottle_Health", true);
        Register(pool, "Prefabs/Effects/Slashes/Slash_Col_Base", true);
        Register(pool, "Prefabs/Effects/Slashes/Slash_Col_Blue", true);
        Register(pool, "Prefabs/Effects/Slashes/Slash_Dance_Green", true);
        Register(pool, "Prefabs/Effects/Slashes/Slash_Ink", true);
        Register(pool, "Prefabs/Effects/Slashes/Slash_Cosmos", true);
        Register(pool, "Prefabs/Effects/Slashes/Slash_Ink_verticalVariant", true);
        Register(pool, "Prefabs/Effects/Slashes/Slash_Multi_Blue", true);
        Register(pool, "Prefabs/Effects/Slashes/Slash_Row_Base", true);
        Register(pool, "Prefabs/Effects/Slashes/Slash_RowBlue", true);
        Register(pool, "Prefabs/Effects/Slashes/Slash_Water", true);
        Register(pool, "Prefabs/Effects/Sparks/Sparks_red", true);
        Register(pool, "Prefabs/Effects/Sparks/Sparks_blue", true);
        Register(pool, "Prefabs/Effects/Sparks/Sparks_explode_white", true);
        Register(pool, "Prefabs/Effects/Sparks/Holy_hit", true);
        Register(pool, "Prefabs/Effects/Skills/ChainLightning", true);
        Register(pool, "Prefabs/Effects/Hit/Electro_hit", true);
        Register(pool, "Prefabs/Projectiles/ChainLightningProj_Prefab", true);
        Register(pool, "Prefabs/Effects/Wolf/Wolf_GreenHit", true);
        Register(pool, "Prefabs/Effects/Wolf/Wolf_HealingCircle", true);
        Register(pool, "Prefabs/Effects/Wolf/Wolf_StoneSlash", true);
        Register(pool, "Prefabs/Effects/Monster/Minotaur/Slash_Row_Red", true);
        Register(pool, "Prefabs/Effects/Monster/Minotaur/Sparks_explode_red", true);
        Register(pool, "Prefabs/Effects/Monster/Alien/Lightning_Hit_Blue", true);
        Register(pool, "Prefabs/Effects/Monster/Alien/Lightning_aura", true);
        Register(pool, "Prefabs/Effects/Monster/Smaug/SmaugBiteEffect", true);
        Register(pool, "Prefabs/Effects/Monster/Smaug/SmaugBreath", true);
        Register(pool, "Prefabs/Effects/Monster/Smaug/SmaugMeteor", true);
        Register(pool, "Prefabs/Effects/Monster/Smaug/SmaugTornado", true);
        Register(pool, "Prefabs/Effects/Monster/Smaug/Sparks_explode_yellow", true);
        Register(pool, "Prefabs/PopupMarks/Mark_Completed", true);
        Register(pool, "Prefabs/PopupMarks/Mark_Quest", true);
        Register(pool, "Prefabs/PopupMarks/Mark_Reinforce", true);
        Register(pool, "Prefabs/PopupMarks/Mark_Shop", true);
    }

    private static void Register(PoolingManager pool, string path, bool worldPositionStays)
    {
        // Register templates before any scene Awake needs them; instantiate on demand.
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null) throw new System.InvalidOperationException("Missing pool prefab: " + path);
        pool.CreatePool(prefab, worldPositionStays, 0);
    }
}
