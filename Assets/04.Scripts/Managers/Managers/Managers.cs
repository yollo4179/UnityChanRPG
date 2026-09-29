using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor.PackageManager;
#endif
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers _instance;
    public static Managers Instance { get { Init();return _instance;} }


     ResourceManager _resource = new ResourceManager();
     PoolingManager _pool = new PoolingManager();
     SceneManagerEx _scene = new SceneManagerEx();
     UIManager _ui = new UIManager();  
    InventoryManager _inventory = new InventoryManager();
    DataManager _data = new DataManager();
    MonsterManager _monster = new MonsterManager();
    ShopManager _shop = new ShopManager();
    PlayerManager _player= new PlayerManager();
    DialogueManager _dialogue = new DialogueManager();
    EventManager _event = new EventManager();
    EquipmentManager _equipment = new EquipmentManager();
    QuestManager _quest = new QuestManager();  
    WeaponEffectManager _weaponEffect = new WeaponEffectManager();
    PrefabManager _prefab = new PrefabManager();

    public static ResourceManager Resource {  get { return Instance._resource; } }
    public static PoolingManager Pool { get { return Instance._pool; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }

    public static UIManager UI { get { return Instance._ui; } }

    public static InventoryManager Inventory { get { return Instance._inventory; } }
    public static DataManager Data { get { return Instance._data; } }
    public static MonsterManager Monster { get { return Instance._monster; } }
    public static ShopManager Shop { get { return Instance._shop; } }   
    public static PlayerManager Player { get { return Instance._player; } }

    public static DialogueManager Dialogue { get { return Instance._dialogue; } } 
    public static EventManager Event { get { return Instance._event; } }  
    
    public static EquipmentManager Equipment { get { return Instance._equipment; } }

    public static QuestManager Quest { get {return Instance._quest; } }

    public static WeaponEffectManager WeaponEffect { get { return Instance._weaponEffect; } }

    public static PrefabManager Prefab {  get { return Instance._prefab; } }
    private void Awake()
    {
        Initialize();
    }
    public void Initialize()
    {
        Init();
        Pool.Init();

        Data.Init();//아이템 데이터
        Shop.Init(); //상점 데이터``
        UI.Init();

        Monster.Init(); //몬스터 데이터
        Inventory.Init();//인벤토리 데이터
        Player.Init(); //플레이어 데이터
        Dialogue.Init(); //다이얼로그 데이터 
        Equipment.Init();
        Quest.Init();
        WeaponEffect.Init();
        Prefab.Init();
    }
    static void Init()
    {
        if (null ==_instance)
        {
            GameObject go = GameObject.Find("@Managers");
            if (null==go)
            {
                go = new GameObject { name ="@Managers" };
                go.AddComponent<Managers>();
            }
            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();
        }
        //씬이동 시 매니저는 파괴하지 않는다 . 그 안의 내용물 까지도  
       
    }
    public void Clear()
    {
       // Pool.Clear();
        //Resource.Clear();
        Scene.Clear();
        Player.Clear();
        Equipment.Clear();  // 먼저 인벤에 옮기기 장비들 
        Inventory.SaveInfo();
        Quest.Clear();
        Event.Clear();
        
    }
    public void OnApplicationQuit()
    {
        Clear();
    }
}
