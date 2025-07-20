using UnityEngine;

public class MainMenuPassenger : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        NPCManager.인스턴스.NPC반환(col.gameObject);
    }
}
