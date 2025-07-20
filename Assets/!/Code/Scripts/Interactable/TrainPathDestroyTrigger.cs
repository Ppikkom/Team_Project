using UnityEngine;

public class TrainPathDestroyTrigger : MonoBehaviour
{
    public string npcTag = "NPC";
    [Header("플레이어 리스폰 포인트")]
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        // 열차는 무시
        if (other.CompareTag("Train") || (other.transform.parent != null && other.transform.parent.CompareTag("Train")))
            return;

        // 플레이어는 삭제하지 않고 리스폰 포인트로 이동
        if (other.CompareTag("Player") && respawnPoint != null)
        {
            // 부모 해제 (혹시 열차의 자식이면)
            other.transform.SetParent(null, true);

            // 위치/회전 이동
            other.transform.position = respawnPoint.position;
            other.transform.rotation = respawnPoint.rotation;

            // Rigidbody가 있다면 속도도 0으로
            var rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return;
        }

        // NPC는 풀로 반환
        if (other.CompareTag(npcTag))
        {
            var npc = other.gameObject;
            if (npc != null)
            {
                NPCManager.인스턴스.NPC반환(other.gameObject);
                return;
            }
        }

        // tool 태그: 초기 위치로 이동
        if (other.CompareTag("Tool"))
        {
            var item = other.GetComponent<BaseItem>();
            if (item == null)
                item = other.GetComponentInParent<BaseItem>();

            if (item != null && item.초기위치 != null)
            {
                item.transform.position = item.초기위치.position;
                item.transform.rotation = item.초기위치.rotation;
                var rb = item.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                return;
            }
        }

        // 그 외 오브젝트는 삭제
        Destroy(other.gameObject);
    }
}
