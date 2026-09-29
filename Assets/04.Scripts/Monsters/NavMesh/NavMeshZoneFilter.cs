using UnityEngine;


public enum eNavMeshZoneID
{
        ZONE_WOLF=1000,
        ZONE_MINOTAUR = 1001,
        ZONE_ALIAN = 1002, 
        ZONE_SMAUG = 1003,
        ZONE_TEST,
        ZONE_END
}

public static class NavMeshZoneFilter 
{

    static readonly Collider[] _navBuf = new Collider[10/*+(int)eNavMeshZoneID.ZONE_END*/];
    static public int GetZoneIDAt(Vector3 vPoint )
    {
        
        int numCols = Physics.OverlapSphereNonAlloc(
            vPoint,
            1f,
            _navBuf);
        for(int i=0;i<numCols;++i)
        {
            if (_navBuf[i]&& _navBuf[i].TryGetComponent( out NavMeshKeys ZoneID))
            {
                return (int)ZoneID.zoneID;
            }
        }

        

        return -1; 
    } 
}
