#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// CustomEditor 속성을 사용하여 ItemSO 클래스에 이 에디터를 연결합니다.
[CustomEditor(typeof(SkillSO))]
public class ItemSOEditor : Editor
{
    private SkillSO skillSO;
    private Texture2D spritePreviewTexture;

    // Inspector 창이 활성화될 때 호출됩니다.
    private void OnEnable()
    {
        // 현재 검사 중인(inspecting) 객체를 ItemSO 타입으로 가져옵니다.
        skillSO = target as SkillSO;
    }

    public override void OnInspectorGUI()
    {
        // ScriptableObject의 기본 필드들을 그립니다. (itemName, itemSprite, itemValue 등)
        base.OnInspectorGUI();

        // Sprite 필드가 null이 아닌지 확인합니다.
        if (skillSO.SkillIcon != null)
        {
            // AssetPreview.GetAssetPreview를 사용하여 Sprite로부터 Texture2D 미리보기를 가져옵니다.
            // 이 함수는 에디터 전용 기능입니다.
            Texture2D texture = AssetPreview.GetAssetPreview(skillSO.SkillIcon);

            // 미리보기가 성공적으로 생성되었는지 확인합니다.
            if (texture != null)
            {
                // 미리보기 영역을 위한 레이블을 정의하고 크기를 설정합니다.
                // GUILayout.Label("", ...)을 사용하여 빈 공간을 확보합니다.
                GUILayout.Label("", GUILayout.Height(100), GUILayout.Width(100));

                // GUILayoutUtility.GetLastRect()를 사용하여 방금 정의한 레이블의 Rect 정보를 가져옵니다.
                Rect rect = GUILayoutUtility.GetLastRect();

                // GUI.DrawTexture를 사용하여 해당 영역에 Texture2D를 그립니다.
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            }
        }

        // Inspector 창에 변화가 생겼다면 변경 사항을 저장합니다. (ScriptableObject를 수정할 때 필요)
        if (GUI.changed)
        {
            EditorUtility.SetDirty(skillSO);
        }
    }
}
#endif