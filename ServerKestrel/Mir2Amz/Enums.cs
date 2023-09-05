namespace ServerKestrel.Mir2Amz
{
    public enum GameStage
    {
        None = 0,
        Login = 1,
        Select = 2,
        Game = 3,
        Observer = 4,
        Disconnected = 5
    }

    public enum DelayedType
    {
        Magic,
        /// <summary>
        /// Param0 MapObject (Target) | Param1 Damage | Param2 Defence | Param3 damageWeapon | Param4 UserMagic | Param5 FinalHit
        /// </summary>
        Damage,
        RangeDamage,        
        Spawn,
        Die,
        Recall,
        MapMovement,
        Mine,
        NPC,
        Poison,
        DamageIndicator,
        Quest,

        // Sanjian
        SpellEffect,
    }
}
