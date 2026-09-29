using JetBrains.Annotations;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx 
{
   public SceneBase CurrentScene //씬 베이스 객체는 씬 당 하나씩 할당
    {
        get
        {
            return GameObject.FindFirstObjectByType<SceneBase>();//베이스신 가져옿ㄴ다
        }
    }
    public string GetSceneName(Define.Scene sceneType)
    {
        string sceneName = System.Enum.GetName(typeof(Define.Scene),sceneType);

        return sceneName;

    }

    public AsyncOperation LoadScene(Define.Scene scene)
    {
        //Managers.Instance.Clear();
        //Managers.Instance.Initialize();

        return SceneManager.LoadSceneAsync(GetSceneName(scene));//자체제공 함수 
    }
   public void Clear()
    {
        //CurrentScene.Clear();
    }

}
