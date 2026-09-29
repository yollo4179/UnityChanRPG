using System;
using UnityEngine;
public enum eDialogueEffect
{
    DEFAULT,
    DECREASE_HEALTH,
    END
}
public static class DialogueEffectEvent
{

    private static  Action<int>[] _actions = new Action<int>[(int)eDialogueEffect.END];
   
    public static void Init()
    {
        _actions[(int)eDialogueEffect.DECREASE_HEALTH]=
            (x) =>
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");

                DamageInfo damageInfo = new DamageInfo();
                damageInfo.attackerID = EntityId.None;
                damageInfo.hitLayerMask=LayerMask.GetMask("NPC");
                damageInfo.attackID =1;
                damageInfo.baseDamage = x;
                player.GetComponent<DamageReceiver>().TakeDamage(damageInfo);
            };
    }
    public static void Invoke(eDialogueEffect effect,int  amount)
    {
        if(null != _actions[(int)effect])
            _actions[(int)effect](amount);
    }
}
