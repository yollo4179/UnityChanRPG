using UnityEngine;

public class PlayerProjectile : Projectile
{
    protected PlayerControllerCom _playerController;
   public virtual void Init(PlayerControllerCom playerController) {
        _playerController = playerController;  
    }
    

}
