using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
public class SmaugSpawnTile : SpawnTile
{
    [SerializeField]private List<GameObject> _smaugWayPoints;
    public override void SetMonster(GameObject _enemyObject, MonsterDesc monsterDesc)
    {
        _monsterEnemy =_enemyObject;
        _monsterEnemy.transform.position = _cellPosition;

        int a = -1;
        bool isSucceeded = _enemyObject.GetComponent<BehaviorGraphAgent>().SetVariableValue(BlackboardKeys.SPAWNER, monsterDesc.Spawner.gameObject);
        _enemyObject.GetComponent<BehaviorGraphAgent>().SetVariableValue(BlackboardKeys.CELL_INDEX, _cellIndex);
        _enemyObject.GetComponent<BehaviorGraphAgent>().SetVariableValue(BlackboardKeys.ANIMATOR, _enemyObject.GetComponentInChildren<Animator>());
        _enemyObject.GetComponent<BehaviorGraphAgent>().SetVariableValue(BlackboardKeys.SKILL_NO, a=UnityEngine.Random.Range(0, monsterDesc.NumSkills)); //0,1,2
        _enemyObject.GetComponent<BehaviorGraphAgent>().SetVariableValue(BlackboardKeys.WAY_POINTS, _smaugWayPoints);
        _enemyObject.GetComponent<BehaviorGraphAgent>().BlackboardReference.GetVariableValue<int>(BlackboardKeys.SKILL_NO, out a);



        /*네브 매시 내부 트랜스폼*/
        _monsterEnemy.GetComponent<NavMeshAgent>().Warp(_cellPosition+Vector3.up*0.5f);


    }
}
