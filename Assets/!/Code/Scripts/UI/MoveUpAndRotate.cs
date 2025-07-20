using UnityEngine;

public class MoveUpAndRotate : MonoBehaviour
{
    [Header("이동 속도 (Y축)")]
    public float moveSpeed = 2f;
    [Header("회전 속도 (초당 각도)")]
    public float rotationSpeed = 180f;

    [Header("시작 위치와 멈추는 위치")]
    public Vector3 startPosition;
    public Vector3 endPosition;

    [Header("등장까지의 대기 시간(초)")]
    public float appearDelay = 0f;

    [Header("회전 시작까지 대기 시간(초)")]
    public float rotateDelay = 1f;

    [Header("회전 여부")]
    public bool shouldRotate = true;

    private bool isMoving = false;
    private bool canRotate = false;

    void Start()
    {
        // 시작 위치로 이동
        transform.position = startPosition;
        StartCoroutine(AppearDelayCoroutine());
    }

    void Update()
    {
        if (isMoving)
        {
            // Y축으로 위로 이동 (endPosition까지)
            transform.position = Vector3.MoveTowards(transform.position, endPosition, moveSpeed * Time.deltaTime);

            // 도착하면 이동 멈춤 및 회전 대기 코루틴 시작
            if (Vector3.Distance(transform.position, endPosition) < 0.01f)
            {
                isMoving = false;
                if (shouldRotate)
                    StartCoroutine(RotateDelayCoroutine());
            }
        }
        else if (canRotate && shouldRotate)
        {
            // 대기 시간 후에만 회전
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private System.Collections.IEnumerator AppearDelayCoroutine()
    {
        yield return new WaitForSeconds(appearDelay);
        isMoving = true;
    }

    private System.Collections.IEnumerator RotateDelayCoroutine()
    {
        yield return new WaitForSeconds(rotateDelay);
        canRotate = true;
    }
}
