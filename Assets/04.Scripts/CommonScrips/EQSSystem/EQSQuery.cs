using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using static EQSManager;
using UnityEngine.Analytics;
/*쿼리 테스트는 외부에서 받는다. (몬스터마다), 몬스터는 스포너로 프리팹 */
[Serializable]public class EQSQuery : MonoBehaviour
{
    /*QUERY 를 Serializeable로 받기 위해서 감싼다.*/
    [SerializeField] private List<TestConfig> _testsList = new List<TestConfig>();
    [SerializeField] private List<TestConfig> _actionTestList = new List<TestConfig>(); 

    Dictionary<Type,TestConfig>_dicTestConfig = new Dictionary<Type,TestConfig>();

    [SerializeField] private EQSQuerier _querier;

    private List<TextMeshPro> _textMeshProDebug = new List<TextMeshPro>();
    [SerializeField] GameObject _TextMeshProDebugPrefab;
  

    [SerializeField] private SCORING_POLICY _scoringPolicy = SCORING_POLICY.eWeightedSum;

    [SerializeField] private int _topResultCount=1; //상위 N개 중 랜덤 선택 

    List<ScoredPoint> scoredPoints;
    #region Debug
    private bool _debugColor = false;
    private Vector3 _debugSphereDir = Vector3.zero;
    private Vector3 _debugSphereCentre = Vector3.zero;
    private float _debugSphereRadius = 0;
    private float _debugRayDist = 0f;
    private void ClearDebugSetting()
    {
        _debugColor = false;
        _debugSphereDir= Vector3.zero;
        _debugSphereCentre = Vector3.zero;
        _debugSphereRadius = 0;
        _debugRayDist =0;
    }
    #endregion


    #region INNER_CLASSES
    [Serializable] public class TestConfig
    {
        [Header("테스트 이름")]
        public string sz_testName;

        [SerializeField] private  ScriptableObject m_Test; //쿼리 테스트 원본
        public IEQSTest _testCopy; //쿼리 테스트 사용

        public bool enabled = true;
        public IEQSTest GetTest()
        {
            return _testCopy;
        }
        public void SetTest(IEQSTest test)
        {
            _testCopy = test;
        }

        public void SaveOrigin()
        {

           if (null == m_Test) return;
            _testCopy = ScriptableObject.Instantiate(m_Test) as IEQSTest;
        }
        public  IEQSTest  GetOriginTest() 
        {
            /*ReadOnly*/
            return m_Test as IEQSTest;
        }

    }
    private class ScoredPoint
    {
        public EQSPoint point;
        public float score; 
    }

    public enum SCORING_POLICY
    { 
        eWeightedSum,
        eWeighted_Dot,
        eMinScore,
    }
    #endregion
    private void Awake()
    {
        //UpdateTextMeshPro();
        _querier = GetComponent<EQSQuerier>();

        _dicTestConfig =new Dictionary<Type, TestConfig>();
        InitializeTests2Dictionary(_testsList);
        InitializeTests2Dictionary(_actionTestList);
    }
    public void LowerBaseTestWeights(float per)
    {
        foreach(var testConfig in _testsList)
        {
            TestConfig config = _dicTestConfig[testConfig.GetOriginTest().GetType()];
                config.GetTest().Weight = config.GetTest().Weight/per;
        }
    }
    public  void RestoreBaseTestWeights()
    {
        foreach (var testConfig in _testsList)
        {
            TestConfig config = _dicTestConfig[testConfig.GetOriginTest().GetType()];
            config.GetTest().Weight = config.GetOriginTest().Weight;
        }
    }
    public void GetTestConfigByType(Type testType, out TestConfig testConfig)
    {
        _dicTestConfig.TryGetValue(testType, out TestConfig config);
        testConfig= config;
    }
    private void InitializeTests2Dictionary(List<TestConfig> _list)
    {
        foreach (var config in _list)
        {
            if (null==config.GetOriginTest()) continue;
            Type type = config.GetOriginTest().GetType();
            if (_dicTestConfig.ContainsKey(type)) continue;

            config.SaveOrigin();
            _dicTestConfig.Add(type, config);
        }
    }
    void UpdateTextMeshPro()
    {
        float numText= EQSManager.GetInstance()._numPoints*EQSManager.GetInstance()._numRings; //점 개수가 동적이라면 매번 받아와야한다. Query 부를때마다
        for (int i=0;i<numText;++i)
        {
            _textMeshProDebug.Add(Instantiate(_TextMeshProDebugPrefab.GetComponent<TextMeshPro>()));
        }
    }
    public float CalculatePoint(EQSPoint point, EQSQuerier querier, EQSContext context)
    {
        

        if ( 0  == _dicTestConfig.Count) return 0f;
        float totalWeight = _dicTestConfig.Sum(t => t.Value.GetTest().Weight); 
        
        switch (_scoringPolicy)
        {
            case SCORING_POLICY.eWeightedSum:
            {
                    float sum = 0f; 
                    foreach(var testWrapped in _dicTestConfig)
                    {
                        var test = testWrapped.Value.GetTest();
                        float score = test.Execute(point, querier, context);
                        sum+= score* (test.Weight / totalWeight); //0~1
                    }
                    return sum; 
               
            }
            case SCORING_POLICY.eWeighted_Dot:
            {

                    float product = 1; 
                    foreach(var testWrapped in _dicTestConfig)
                    {
                        var test = testWrapped.Value.GetTest();
                        float score = test.Execute(point, querier, context);
                        product *=Mathf.Pow(score, test.Weight); //감소 함수(밑이 항상 0~1), 0 
                    }

                return product; // 작을 수록 높은 우선 순위 
            }
            case SCORING_POLICY.eMinScore:
            {
                    float minScore = float.MaxValue;
                    foreach (var testWrapped in _dicTestConfig)
                    {
                        var test = testWrapped.Value.GetTest();
                        float score = test.Execute(point, querier, context);
                        minScore = Mathf.Min(minScore, score);
                    }
                    //그냥 최소인 것
                    return minScore;
            }
            default:
                return 0f;
        }
    }

    public EQSPoint ExecuteQuery(EQSQuerier querier, EQSPoints points,EQSContext context)
    {

        
        /*1. 점수 측정전에 점수를 측정하고 난 후에 포인트와 점수를 같이 저장할 구조체를 만든다. Setting한다*/
        scoredPoints = new List<ScoredPoint>();
        /*Points 로부터 각각의 포인트를 가져오고 
         * Execute 함수를 통해서 부여된 점수를 가져오고 합산한다.
         * 정책에 따라서 다르게 구현*/
        foreach(var point in points?.GetAllPoints())//모든 리스트를 가지고 온다. 
        {
              
            float finalScore= CalculatePoint(point, querier, context);
            //if(finalScore<=0) continue;
            

            ScoredPoint scoredPoint =new ScoredPoint { point = point, score = finalScore }; 
            scoredPoints.Add(scoredPoint);
        }
        scoredPoints.Sort((first,second) => second.score.CompareTo(first.score)); //내림차순
        int numCandidates = Mathf.Min(_topResultCount, scoredPoints.Count);//0,1,2 <=

        //계속 수행하면 지터링 생기나 후보가 충분히 많다면 ?
        if (numCandidates  > 0)
        {
            int idx = UnityEngine.Random.Range(0, numCandidates);
            return scoredPoints[idx].point;//포인트 반환하면 쿼리어가 등록과 점유 
        }
        return null;

    }
    #region 디버그 드로우
    public void OnDrawGizmos()
    {
        if (null ==scoredPoints) return;

        int _textMeshProIdx = 0;
        foreach (var point in scoredPoints)
        {
            float score = point.score; //0~1
            if (0f >=score)
                Gizmos.color = new Color(0f, 0f, 1f);
            else
                Gizmos.color = new Color(Mathf.Clamp01(1 - score), Mathf.Clamp01(score), 0);
            Gizmos.DrawWireSphere(point.point.pos, 0.1f);


            //_textMeshProDebug[_textMeshProIdx].text = 0.ToString();
            //_textMeshProDebug[_textMeshProIdx].font = TMP_Settings.defaultFontAsset; // 폰트 보장
            //_textMeshProDebug[_textMeshProIdx].alignment = TextAlignmentOptions.Center;
            //_textMeshProDebug[_textMeshProIdx].fontSize  = 2.5f;
            //_textMeshProDebug[_textMeshProIdx].text      = score.ToString("F3");



            //_textMeshProDebug[_textMeshProIdx].transform.rotation =  /*Quaternion.Euler(0f, 180f, 0f) **/ CameraUtil.GetCameraRotation(point.point.pos);

            //_textMeshProDebug[_textMeshProIdx++].transform.position = point.point.pos +Vector3.up*0.1f;

            if (point.point.ringIndex ==0) continue;
            /*Trace Test Result*/
            ClearDebugSetting();
            Vector3 dirToTarget = (_querier.Target.transform.position - point.point.pos).normalized;
            LayerMask obstacleLayer = LayerMask.GetMask("Obstacles", "Enemies");
            if (Physics.SphereCast(
          _debugSphereCentre =  point.point.pos+Vector3.up*0.1f,//Origin
          _debugSphereRadius =  0.3f,                        //Radius 
          _debugSphereDir = dirToTarget,                //Dir
           out RaycastHit hit,          //RaycastHit
          _debugRayDist =  1f,                       //RayDistance
           obstacleLayer,
           QueryTriggerInteraction.Ignore
           ))
            {
                Gizmos.color =new UnityEngine.Color(1f, 0f, 0, 0.3f);
                if (true == _debugColor)
                    Gizmos.color =new UnityEngine.Color(0f, 1f, 0f, 0.3f);

                // 끝 지점
                Vector3 end = _debugSphereCentre + _debugSphereDir*_debugRayDist;
                // 중심선
                Debug.DrawLine(_debugSphereCentre, end, Gizmos.color, 1);
                Gizmos.DrawSphere(_debugSphereCentre +_debugSphereDir*_debugRayDist, _debugSphereRadius);

            }



        }

    }
    #endregion

}
