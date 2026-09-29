using UnityEngine;

public class SpawnerTileIndex : MonoBehaviour
{
    private int _tileIndex;
    public void SetTileIndex(int _idx) { _tileIndex=_idx; }
    public int TileIndex{ get => _tileIndex;  }
}
