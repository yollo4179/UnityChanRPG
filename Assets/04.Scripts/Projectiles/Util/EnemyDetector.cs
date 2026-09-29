using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.GraphicsBuffer;

public class EnemyDetector : MonoBehaviour
{
    List<GameObject> _enemiesInRange = new List<GameObject>();
    StatusScript _status;

    public void Awake()
    {
        _status = GetComponent<StatusScript>();
        StartCoroutine(CheckDeadEnemies());
    }
    

    public void ClearDeadEnemy()
    {
        List<GameObject> enemyToClear = new();
        foreach (var enemy in _enemiesInRange)
        {
            if (enemy.GetComponent<StatusScript>().IsDead)
                enemyToClear.Add(enemy);
        }
        foreach (var deadEnemy in enemyToClear)
        {
            _enemiesInRange.Remove(deadEnemy);
        }
    }
    public void ClearAllEnemies()
    {
        _enemiesInRange.Clear();
        
    }
    public List<GameObject> GetClosestEnemySortedList()
    {

        //ClearDeadEnemy();
        if (_enemiesInRange.Count > 0)
        {
            Vector3 currentPos = transform.position;
            _enemiesInRange.Sort((a, b) =>
            {
                if (a == null) return (b == null) ? 0 : 1; // null은 뒤로
                if (b == null) return -1;

                float da = (a.transform.position - currentPos).sqrMagnitude;
                float db = (b.transform.position - currentPos).sqrMagnitude;

                return da.CompareTo(db); // 오름차순: 가까운 것 먼저

            });
            return _enemiesInRange;
        }
        else
        {
            return null;
        }
    }
    IEnumerator CheckDeadEnemies()
    {

        while (true)
        {
            if (_status.IsDead) 
                yield return null;

            ClearDeadEnemy();
            yield return new WaitForSeconds(1.0f);
        }
    }
    public List<GameObject> GetEnemiesInRange()
    {
       // ClearDeadEnemy();
        return _enemiesInRange; 
    }
    private void OnTriggerStay(Collider col)
    {

        if(col.CompareTag("Enemies"))
        {
            if (0 == _enemiesInRange.Count)
            {
                _enemiesInRange.Add(col.gameObject);//에네미는 트리거 콜라이더를 가진다.
            }
            else if (!_enemiesInRange.Contains(col.gameObject))
            {
                _enemiesInRange.Add(col.gameObject);
            }

        }
    }
    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Enemies"))
        {
            if (0 < _enemiesInRange.Count)
            {
                _enemiesInRange.Remove(col.gameObject);
            }
        }
    }


  }
