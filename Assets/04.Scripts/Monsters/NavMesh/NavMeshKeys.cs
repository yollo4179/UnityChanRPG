using UnityEngine;

public class NavMeshKeys : MonoBehaviour
{
    [SerializeField] public eNavMeshZoneID zoneID;

    bool _isPlayerIn = false; 
    /*cellIndex*/
    private int _cellIndex;
    public void SetCellID(int id) { _cellIndex = id; }
    public int CellIndxe { get => _cellIndex; }

    public void OnTriggerEnter(Collider other)
    {
        if (true ==_isPlayerIn) return; 

        if (other.CompareTag("Player"))
        {
            _isPlayerIn = true; 
            Event_EnterArea evt = new Event_EnterArea((int)zoneID);
            Managers.Event.Publish(evt);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _isPlayerIn = false; 
        
    }

}
