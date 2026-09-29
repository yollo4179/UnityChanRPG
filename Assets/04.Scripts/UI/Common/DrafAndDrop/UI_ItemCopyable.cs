using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ItemCopyable : MonoBehaviour, IPointerDownHandler
{


    public void OnPointerDown(PointerEventData eventData)
    {
        SkillSO skillSO = GetComponent<ItemDataStorage>().GetSkillSO();
        GameObject iconGO = Managers.Resource.Instantiate("UI/Part/ItemIcon_Prefab", null);
        iconGO.GetComponent<ItemDataStorage>().UpdateSkillSOData(skillSO);
        iconGO.AddComponent<UI_Draggable_Move>();
    }
}
