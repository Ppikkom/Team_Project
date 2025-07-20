using System.Collections.Generic;
using UnityEngine;

public class ManagerRoomCollider : MonoBehaviour
{
    // 관리실 내부에 있는 쓰레기 오브젝트 목록
    public static HashSet<GameObject> InsideTrashObjects = new HashSet<GameObject>();

    [Header("디버그")]
    public bool 디버그출력요청 = false;

    // 유틸리티 메서드만 남김 (필요시)
    public static void PrintInsideTrashObjects()
    {
        string list = "[ManagerRoomCollider] 현재 내부 쓰레기 목록: ";
        foreach (var obj in InsideTrashObjects)
        {
            list += obj.name + ", ";
        }
        Debug.Log(list);
    }

    private void Update()
    {
        if (디버그출력요청)
        {
            PrintInsideTrashObjects();
            디버그출력요청 = false;
        }
    }
}
