using UnityEngine;

public class ItemPrefabScript : MonoBehaviour
{
    private int _amount = 1;
    public int Amount { get => _amount; set => _amount = Mathf.Max(1, value); }
    [SerializeField] private eITEMTYPE _itemType;
    [SerializeField] private int _itemID;
    private int _dropMoney;
    private bool _ready;
    private bool _collected;
    private bool _hasLanded;
    private const float PickupDelayAfterLanding = 0.3f;
    private float _pickupAllowedAt;

    private void OnEnable()
    {
        _amount = 1;
        _dropMoney = 0;
        _ready = false;
        _collected = false;
        _hasLanded = false;
        _pickupAllowedAt = float.PositiveInfinity;
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null && !body.isKinematic)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    public void SetOwner(GameObject owner)
    {
        StatusScript status = owner != null ? owner.GetComponent<StatusScript>() : null;
        if (_itemType == eITEMTYPE.MONEY && status != null)
        {
            MonsterInfo monster = Managers.Monster.GetMonsterInfo(status.CharacterID);
            if (monster != null) _dropMoney = monster.DropMoney;
        }
        _ready = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCollect(other);
    }

    private void OnCollisionEnter(Collision collision) => CheckLanding(collision);
    private void OnCollisionStay(Collision collision) => CheckLanding(collision);

    private void CheckLanding(Collision collision)
    {
        if (!_ready || _hasLanded) return;
        int groundMask = LayerMask.GetMask("Default", "Terrain", "Obstacles");
        if ((groundMask & (1 << collision.gameObject.layer)) == 0) return;
        if (collision.collider.GetComponentInParent<StatusScript>() != null) return;
        for (int i = 0; i < collision.contactCount; i++)
        {
            // Side contacts with walls do not count as landing on the floor.
            if (Vector3.Dot(collision.GetContact(i).normal, Vector3.up) < 0.5f) continue;
            _hasLanded = true;
            _pickupAllowedAt = Time.time + PickupDelayAfterLanding;
            return;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Also collect drops that spawned while the player was already overlapping.
        TryCollect(other);
    }

    private void TryCollect(Collider other)
    {
        if (!_ready || !_hasLanded || _collected || other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (Time.time < _pickupAllowedAt) return;
        if (_itemType != eITEMTYPE.MONEY && _itemType != eITEMTYPE.CONSUMABLE &&
            _itemType != eITEMTYPE.EQUIPMENT && _itemType != eITEMTYPE.QUEST &&
            _itemType != eITEMTYPE.INGREDIENT) return;

        // Multiple player colliders must not award the same drop more than once.
        _collected = true;
        if (_itemType == eITEMTYPE.MONEY)
        {
            int money = _dropMoney > 0 ? Random.Range(_dropMoney, _dropMoney * 2) : _amount;
            Managers.Player.AddMoney(money);
        }
        else
        {
            Managers.Inventory.TryAddItem(_itemType, _itemID, _amount);
        }
        _ready = false;
        Managers.Resource.Destroy(gameObject);
    }
}
