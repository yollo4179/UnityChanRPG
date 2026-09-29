#if UNITY_EDITOR
using UnityEditor.SearchService;
#endif
using UnityEngine;

public static class Define
{
    /*Scene¡§¿«*/
    public  enum Scene
    {
        Static,
        LoginScene,
        LoadingScene,
        GamePlayScene,
        End,
    }
    public enum UIEvent
    {
        Click,
        Drag,
    }
    public enum MouseEvent
    {
        Press,
        Click,
    }
    public enum CameraMode
    {
        Main,
        CutScene,
        NPC, 
        BackView,
    }
}
