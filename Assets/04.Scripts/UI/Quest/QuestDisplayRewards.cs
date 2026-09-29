using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using UnityEditor;
public class QuestDisplayRewards : UI_Base
{
    List<Poolable> _frameHandles = new List<Poolable>();
    public override void Init()
    {
        if (true == _hasInitialized) return;
        _hasInitialized = true;
        Bind<GameObject>(typeof(GameObjects));
        _parent = Get<GameObject>((int)GameObjects.RewardCellView_Contents);
    }

    public enum GameObjects
    {
        RewardCellView_Contents

    }
    private bool _hasInitialized = false;
    private  GameObject _parent;
    private  QuestData _questData; 

    public void SetRewards(QuestData questData)
    {
        Init();
        _questData = questData;
        ClearHandles();
        if (null ==_questData) return;
        foreach(var reward in questData.QuestReward.RewardItemSets)
        {
            Poolable handle= Managers.Pool.LendPoolableTo("QuestItem_Slot",_parent.transform);
            _frameHandles.Add(handle);
            ItemData itemData = Managers.Data.GetItemData(reward.ItemID);
            QuestImageSetter imgSetter = handle.GetComponentInChildren<QuestImageSetter>();
            imgSetter.SetImage(itemData);



        }
    }

    public void ClearHandles()
    {
        foreach(var handle  in  _frameHandles)
        {
            Managers.Pool.GetBack(handle);
           
        }
        _frameHandles.Clear();
    }
}
