using UnityEngine;
using UnityEditor;

public class SortChildrenByName : MonoBehaviour
{
    [MenuItem("GameObject/자식 오브젝트 이름순 정렬", false, 0)]
    static void SortSelectedChildren()
    {
        if (Selection.activeTransform == null) return;

        var parent = Selection.activeTransform;
        var children = new Transform[parent.childCount];
        for (int i = 0; i < parent.childCount; i++)
            children[i] = parent.GetChild(i);

        System.Array.Sort(children, (a, b) => string.Compare(a.name, b.name));

        for (int i = 0; i < children.Length; i++)
            children[i].SetSiblingIndex(i);
    }
}
