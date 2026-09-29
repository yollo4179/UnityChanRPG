using System.Collections;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine;



public class ItemPrefabScript : MonoBehaviour
{
    Collider _trigger;
    int _amount;
    public int Amount { get {return _amount; } set { _amount = value;}}
    [SerializeField] eITEMTYPE _itemType;
    [SerializeField] int _itemID;
    StatusScript _itemOwnerStatus; 
    MonsterInfo _monsterInfo;
    Poolable _poolable;
    Coroutine _co;
    private void Awake()
    {
        _poolable = GetComponent<Poolable>();
        _trigger =  GetComponentsInChildren<Collider>(true).FirstOrDefault((x)=>(x.isTrigger ==true));        
    }
    public void SetOwner(GameObject owner)
    {
        _itemOwnerStatus = owner.GetComponent<StatusScript>();
    }
    private void OnTriggerStay(Collider other)
    {
        if(null == _itemOwnerStatus)
        {
            return;
        }
        
       MonsterInfo _monsterInfo =  Managers.Monster.GetMonsterInfo(_itemOwnerStatus.CharacterID);

        int mask = LayerMask.GetMask("Player");
        int extraAmount = UnityEngine.Random.Range(1,2); 
        if ( 0 < (other.includeLayers.value &mask))
        {
           switch(_itemType)
            {
                    case eITEMTYPE.MONEY:
                    {
                    int finalMoney =UnityEngine.Random.Range(_monsterInfo.DropMoney, _monsterInfo.DropMoney*2);
                    Managers.Player.AddMoney(finalMoney);
                    break;
                    }
                    //아니면 디폴트로 같은 처리,
                    case eITEMTYPE.CONSUMABLE:
                    {
                        Debug.Log($"<color=#00ff00>ItemPrefabScript OnTriggerEnter CONSUMABLE </color>");
                        Managers.Inventory.TryAddItem(_itemType, _itemID, extraAmount);
                        break;
                    }
                    case eITEMTYPE.EQUIPMENT:
                    {
                        Debug.Log($"<color=#00ff00>ItemPrefabScript OnTriggerEnter EQUIPMENT </color>");
                        Managers.Inventory.TryAddItem(_itemType, _itemID, 1);
                        break;
                    }
                    case eITEMTYPE.QUEST:
                    {
                        Debug.Log($"<color=#00ff00>ItemPrefabScript OnTriggerEnter QUEST </color>");
                        Managers.Inventory.TryAddItem(_itemType, _itemID, 1);
                        break;
                    }
                    case eITEMTYPE.INGREDIENT:
                    {
                        Debug.Log($"<color=#00ff00>ItemPrefabScript OnTriggerEnter INGREDIENT </color>");
                        Managers.Inventory.TryAddItem(_itemType, _itemID, extraAmount);
                        break;
                    }

            }
            _itemOwnerStatus=null;

            ShaderEffects shaderEffects = GetComponent<ShaderEffects>();
            if (null!= shaderEffects)
            {
                if(null == _co)
                _co=  StartCoroutine(DisolveAndGetBack(shaderEffects));
            }
            else
            {
                Managers.Pool.GetBack(_poolable);
            }
            
        }
    }
    IEnumerator DisolveAndGetBack(ShaderEffects shader)
    {
        shader.SetNowMarerial(eShaderEffect.DISOLVE);
        shader.DoFade(1f, -0.3f, 0.5f, eFadeMode.FADE_OUT);
        yield return new WaitUntil(() => shader.IsFadeEffectDone == true);

        Managers.Pool.GetBack(_poolable);
        shader.SetNowMarerial(eShaderEffect.ORIGIN);
        _co=null;
    }
}
