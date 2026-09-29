using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace UnityNote
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance;


        [SerializeField] private GameObject _loadingScene;
        [SerializeField] private Image _loadingBackground;
        [SerializeField] private Sprite[] _loadingSprites;
        [SerializeField] private Slider _loadingProgress;
        [SerializeField] private TextMeshProUGUI _textProgress;
        private WaitForSeconds _waitChangeDelay;
        private float _waitChangeDelayTime = 2.5f; 
        [SerializeField] private Define.Scene _nextScene;


        [SerializeField] private float _speed;
        

        private void Awake()
        {
            if (null != Instance &&this != Instance)
                Destroy(Instance);

            else
            {
                Instance = this;
                _waitChangeDelay = new WaitForSeconds(_waitChangeDelayTime);
                DontDestroyOnLoad(gameObject); //나 파괴하지마 
            }
            LoadScene(_nextScene);
        }
        public void LoadScene(Define.Scene nextScene)
        {
            //int idx = 0;
            //_loadingBackground.sprite = _loadingSprites[idx];
            _loadingProgress.value= 0;
            _loadingScene.SetActive(true);
            StartCoroutine(LoadSceneAsync(nextScene));
        }
        public IEnumerator LoadSceneAsync(Define.Scene nextScene)
        {
            AsyncOperation oper = Managers.Scene.LoadScene(nextScene);
            // float accTime = 0f;

            oper.allowSceneActivation =false;

            float timer = 0f; 
            while (false == oper.isDone)
            {
                if (oper.progress<0.9f)
                {
                    _loadingProgress.value = oper.progress;
                }
                else
                {/*.fake Loading*/
                    timer +=Time.unscaledDeltaTime/_waitChangeDelayTime;
                    _loadingProgress.value = Mathf.Lerp(0.9f, 1f, timer);
                    if(_loadingProgress.value>=1f)
                    {
                        _loadingScene.SetActive(false);
                        oper.allowSceneActivation=true;
                        yield break;
                    }
                }
                _textProgress.text= $"{ (_loadingProgress.value*100):0.00}%";
                yield return null; 
            }


        }


    }
}