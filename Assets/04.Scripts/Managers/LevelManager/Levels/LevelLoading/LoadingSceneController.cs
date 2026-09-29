using UnityEngine;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField]
    Define.Scene _nextScene; 
    public void GameStartEvent()
    {
        UnityNote.SceneLoader.Instance.LoadScene(_nextScene);
    }
    public void GameExitEvent()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit(); 
        #endif
    }
}
