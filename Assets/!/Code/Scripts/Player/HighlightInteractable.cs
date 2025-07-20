using UnityEngine;

public class HighlightInteractable : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float highlightRadius = 0.4f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 6f;

    private float highlightDistance; // 동적으로 할당
    private QuickOutline lastOutline;

    void Start()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        if (playerController == null)
            Debug.LogWarning("PlayerController를 찾을 수 없습니다.");

        // PlayerController의 상호작용_거리 값을 가져옴
        highlightDistance = GetInteractionDistance();
    }

    void Update()
    {
        // 혹시 런타임에 상호작용_거리 값이 바뀔 수 있다면 매 프레임 동기화
        highlightDistance = GetInteractionDistance();
        HighlightObject();
    }

    float GetInteractionDistance()
    {
        // PlayerController의 상호작용_거리는 private이므로, public 프로퍼티로 노출하거나 [SerializeField]를 public으로 바꿔야 함
        // 예시: public float 상호작용_거리 => 상호작용_거리;
        // 또는 [SerializeField] public float 상호작용_거리;
        return playerController != null ? playerController.상호작용_거리 : 2f;
    }

    void HighlightObject()
    {
        if (playerController == null || playerController.카메라 == null)
            return;

        Ray ray = new Ray(playerController.카메라.transform.position, playerController.카메라.transform.forward);
        RaycastHit hit;

        if (Physics.SphereCast(ray, highlightRadius, out hit, highlightDistance, interactableLayer))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                QuickOutline outline = hit.collider.GetComponent<QuickOutline>();
                if (outline == null)
                    outline = hit.collider.GetComponentInParent<QuickOutline>();

                if (outline != null)
                {
                    if (lastOutline != null && lastOutline != outline)
                        lastOutline.enabled = false;

                    outline.OutlineColor = outlineColor;
                    outline.OutlineWidth = outlineWidth;
                    outline.enabled = true;
                    lastOutline = outline;
                    return;
                }
            }
        }

        if (lastOutline != null)
        {
            lastOutline.enabled = false;
            lastOutline = null;
        }
    }
}
