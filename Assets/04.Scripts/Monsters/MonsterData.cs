using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
public class MonsterData
{
    public List<MonsterInfo>MonsterInfoList = new();

    public List<MonsterInfo> GetList()
    {
        return MonsterInfoList;
    }


}
