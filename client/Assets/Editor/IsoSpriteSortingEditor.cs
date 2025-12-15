/*
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IsoSpriteSorting))]
public class IsoSpriteSortingEditor : Editor
{
    private void OnSceneGUI()
    {
        IsoSpriteSorting sorter = (IsoSpriteSorting)target;

        // 캐시 갱신 (씬 뷰에서도 최신 점 위치 반영)
        sorter.RefreshCache();

        // 색상 설정
        Color lineColor = Color.yellow;
        Color pointColor = new Color(1f, 0.8f, 0f, 0.9f);

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        if (sorter.sortType == IsoSpriteSorting.SortType.Point)
        {
            // 단일 포인트 표시
            Handles.color = pointColor;
            Handles.SphereHandleCap(0, sorter.SortingPoint1, Quaternion.identity,
                HandleUtility.GetHandleSize(sorter.SortingPoint1) * 0.05f, EventType.Repaint);
        }
        else if (sorter.sortType == IsoSpriteSorting.SortType.Line)
        {
            // 두 점과 라인 표시
            Handles.color = lineColor;
            Handles.DrawLine(sorter.SortingPoint1, sorter.SortingPoint2);

            Handles.color = pointColor;
            Handles.SphereHandleCap(0, sorter.SortingPoint1, Quaternion.identity,
                HandleUtility.GetHandleSize(sorter.SortingPoint1) * 0.05f, EventType.Repaint);
            Handles.SphereHandleCap(0, sorter.SortingPoint2, Quaternion.identity,
                HandleUtility.GetHandleSize(sorter.SortingPoint2) * 0.05f, EventType.Repaint);
        }

        // 중점에도 살짝 표시해주면 보기 좋음
        Handles.color = new Color(0f, 1f, 1f, 0.4f);
        Handles.SphereHandleCap(0, sorter.AsPoint, Quaternion.identity,
            HandleUtility.GetHandleSize(sorter.AsPoint) * 0.04f, EventType.Repaint);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        IsoSpriteSorting sorter = (IsoSpriteSorting)target;

        GUILayout.Space(6);
        EditorGUILayout.HelpBox(
            "Scene 뷰에서 노란 점/선은 스프라이트의 기준선(깊이 계산용)을 나타냅니다.\n" +
            "이 라인은 SpriteDepthDatabase에서 정의된 localA, localB를 기준으로 표시됩니다.",
            MessageType.Info);

        if (GUILayout.Button("Sort Visible Scene"))
        {
            sorter.SortScene();
        }
    }
}
#endif
*/