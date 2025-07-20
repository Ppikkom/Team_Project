using System.Collections.Generic;
using UnityEngine;

public class SubwayDoor : MonoBehaviour
{
    public enum MoveAxis { X, Z }

    [Header("왼쪽 문들")]
    public List<Transform> leftDoors = new List<Transform>();
    [Header("오른쪽 문들")]
    public List<Transform> rightDoors = new List<Transform>();

    [Header("문 이동 설정")]
    public float openDistance = 2f;
    public float moveSpeed = 2f;
    public MoveAxis moveAxis = MoveAxis.X; // 인스펙터에서 선택
    public bool reverseOpenDirection = false; // ← 추가: 문 열림 방향 반전
//
//
//

    [Header("문 상태")]
    public bool isOpen = false;

    private List<Vector3> leftClosedPos = new List<Vector3>();
    private List<Vector3> rightClosedPos = new List<Vector3>();
    private List<Vector3> leftOpenPos = new List<Vector3>();
    private List<Vector3> rightOpenPos = new List<Vector3>();

    void Start()
    {
        Vector3 dirLeft, dirRight;
        if (moveAxis == MoveAxis.X)
        {
            dirLeft = Vector3.left;
            dirRight = Vector3.right;
        }
        else // Z축
        {
            dirLeft = Vector3.forward;
            dirRight = Vector3.back;
        }

        // 방향 반전 옵션 적용
        if (reverseOpenDirection)
        {
            // 방향을 서로 바꿔줌
            var temp = dirLeft;
            dirLeft = dirRight;
            dirRight = temp;
        }

        // 왼쪽 문들 위치 저장
        foreach (var door in leftDoors)
        {
            leftClosedPos.Add(door.position);
            leftOpenPos.Add(door.position + dirLeft * openDistance);
        }
        // 오른쪽 문들 위치 저장
        foreach (var door in rightDoors)
        {
            rightClosedPos.Add(door.position);
            rightOpenPos.Add(door.position + dirRight * openDistance);
        }
    }

    void Update()
    {
        // 왼쪽 문들 이동
        for (int i = 0; i < leftDoors.Count; i++)
        {
            if (isOpen)
                leftDoors[i].position = Vector3.MoveTowards(leftDoors[i].position, leftOpenPos[i], moveSpeed * Time.deltaTime);
            else
                leftDoors[i].position = Vector3.MoveTowards(leftDoors[i].position, leftClosedPos[i], moveSpeed * Time.deltaTime);
        }
        // 오른쪽 문들 이동
        for (int i = 0; i < rightDoors.Count; i++)
        {
            if (isOpen)
                rightDoors[i].position = Vector3.MoveTowards(rightDoors[i].position, rightOpenPos[i], moveSpeed * Time.deltaTime);
            else
                rightDoors[i].position = Vector3.MoveTowards(rightDoors[i].position, rightClosedPos[i], moveSpeed * Time.deltaTime);
        }
    }

    public void Open() => isOpen = true;
    public void Close() => isOpen = false;
    public void Toggle() => isOpen = !isOpen;
}
