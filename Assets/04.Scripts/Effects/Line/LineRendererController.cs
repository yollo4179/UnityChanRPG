using UnityEngine;
using System.Collections.Generic;

public class LineRendererController : MonoBehaviour
{
    [SerializeField] List<LineRenderer> lineRenderers = new List<LineRenderer>();

    public void SetPosition(Transform startPos, Transform endPos)
    {
        if (0<lineRenderers.Count)
        {
            for (int i = 0; i<lineRenderers.Count; ++i)
            {
                if (lineRenderers[i].positionCount>=2)
                {

                    lineRenderers[i].SetPosition(0, startPos.position);
                    lineRenderers[i].SetPosition(1, endPos.position);
                }
                else
                {
                    Debug.Log($"<Color=#ff0000>라인 ㄹ렌더러는 적어도 2개 이상의 위치 포함</color>");
                }
            }
        }
        else
        {
            Debug.Log($"<Color=#ff0000>라인 렌더러가 비었다</color>");
        }


    }

    public void SetPosition(Vector3 startPos, Vector3 endPos)
    {
        if (0<lineRenderers.Count)
        {
            for (int i = 0; i<lineRenderers.Count; ++i)
            {
                if (lineRenderers[i].positionCount>=2)
                {

                    lineRenderers[i].SetPosition(0, startPos);
                    lineRenderers[i].SetPosition(1, endPos);
                }
                else
                {
                    Debug.Log($"<Color=#ff0000>라인 ㄹ렌더러는 적어도 2개 이상의 위치 포함</color>");
                }
            }
        }
        else
        {
            Debug.Log($"<Color=#ff0000>라인 렌더러가 비었다</color>");
        }
    }
}
