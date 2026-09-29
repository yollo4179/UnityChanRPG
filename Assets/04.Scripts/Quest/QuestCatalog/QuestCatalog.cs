using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuestCatalog", menuName = "Scriptable Objects/QuestCatalog")]
public class QuestCatalog:ScriptableObject 
{
   
 [SerializeField]public List<QuestData> _quests;
}
