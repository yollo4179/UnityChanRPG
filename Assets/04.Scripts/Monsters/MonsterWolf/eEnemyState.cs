using System;
using Unity.Behavior;

[BlackboardEnum]
public enum eEnemyState
{
    IDLE,
	WANDER,
    PATROL,
	CHASE,
	ATTACK,
	HIT,
	DEAD,
    SLEEP,
    FLY,
	CUTSCENE,
    RESPAWN,
	
}
public static class BlackboardKeys
{
	public static readonly string  SKILL_NO				="SkillNO";
    public static readonly string  FLY_SKILL_NO			= "FlySkillNO";
    public static readonly string  ENEMY_STATE			="EnemyState"; 
    public static readonly string  SPAWNER				="Spawner";
	public static readonly string  CELL_INDEX			="CellIndex";
	public static readonly string  IS_ON_SKILL			= "IsOnSkill";
	public static readonly string  IS_TARGET_DETECTED   ="IsTargetDetected";
	public static readonly string  ANIMATOR			    ="Animator";
	public static readonly string  DEAD_DURATION		="DeadDuration";
	public static readonly string  WAY_POINTS			= "WayPoints";
}