using UnityEngine;

public class Ladder : MonoBehaviour
{
    [Header("사다리 이동 속도")]
    [SerializeField] private float ladderMoveSpeed = 3f;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetLadderMode(true, ladderMoveSpeed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetLadderMode(false, 0f);
        }
    }
}