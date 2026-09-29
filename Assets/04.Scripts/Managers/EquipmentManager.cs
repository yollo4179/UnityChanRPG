using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class EquipmentManager 
{
    public void Init() { }

    Transform playerWeaponPoint;
    public List<ItemInfo> _nowEquipItems =new List<ItemInfo>(); 
    public  List<ItemInfo> GetEquipmentItemList()
    {
        return _nowEquipItems;
    }
    public void EquipBaseWeapon()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (null ==playerWeaponPoint)
            playerWeaponPoint = playerGO.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == "WeaponPoint");

        Managers.Prefab.ActivatePrefab(0, playerWeaponPoint);
    }
    public void UnEquipBaseWeapon()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (null ==playerWeaponPoint)
            playerWeaponPoint = playerGO.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == "WeaponPoint");

        Managers.Prefab.InActivatePrefab();
    }
    public bool TryAddEquipment(ItemInfo itemInfo)
    {
        if (!_nowEquipItems.Contains(itemInfo))
        {
            _nowEquipItems.Add(itemInfo);

            eEQUIPMENTTYPE type =  Managers.Data.GetItemData(itemInfo.ID).EquipmentType;


            if (type ==eEQUIPMENTTYPE.WEAPON)
            {
                GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
                if (null ==playerWeaponPoint)
                    playerWeaponPoint = playerGO.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == "WeaponPoint");

                InstanceItemInfo instance = (itemInfo as InstanceItemInfo);

                

                Managers.Prefab.ActivatePrefab(instance.ID, playerWeaponPoint, instance.InstanceID);

                
                MeshRenderer mr = playerWeaponPoint.GetComponentInChildren<MeshRenderer>();
                Managers.WeaponEffect.ActivateEffect(ELEMENT.BASE, instance.CurrentReinforce, instance.InstanceID, mr, playerWeaponPoint);
            }
           

            return true; 
        }
        return false; 
    }
    public bool TryRemoveEquipment(ItemInfo itemInfo)
    {
        if (_nowEquipItems.Contains(itemInfo))
        {
            _nowEquipItems.Remove(itemInfo);


            eEQUIPMENTTYPE type = Managers.Data.GetItemData(itemInfo.ID).EquipmentType;
            

            if (type ==eEQUIPMENTTYPE.WEAPON)
            {
                InstanceItemInfo instance = (itemInfo as InstanceItemInfo);

                Managers.Prefab.InActivatePrefab(instance.InstanceID);
                Managers.WeaponEffect.InActivateEffect(instance.InstanceID);
            }

                return true;
        }
        return false;
    }
    public bool TryMoveToInven(ItemInfo itemInfo)
    {
        if (null ==itemInfo) return false;
        InstanceItemInfo instanceInfo = (itemInfo as InstanceItemInfo);
        if (null == instanceInfo) return false;
        if (eITEMTYPE.EQUIPMENT != itemInfo.Type) return false;


        if (!_nowEquipItems.Contains(itemInfo)) return false;
        if (Managers.Inventory.InvenInfo.InfoList.Contains(itemInfo)) return false; 

        _nowEquipItems.Remove(itemInfo); 
        instanceInfo.IsEquipped = false;
        Managers.Inventory.InvenInfo.InfoList.Add(itemInfo);

        Managers.Inventory.RefreshInventory();
        return true;
         
    }
    public void  Clear()
    {
        foreach( var item in _nowEquipItems)
        {
            if (item == null) continue;
            bool isInInvent = Managers.Inventory.InvenInfo.InfoList.Contains(item);
            if (true  ==isInInvent) continue;
            Managers.Inventory.InvenInfo.InfoList.Add(item);
        }

        //저장하기 전에 보관 
       
    }
    

}
