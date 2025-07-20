using UnityEngine;

public class testButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnButton()
    {
        NPCManager.인스턴스.NPC소환("일반승객", new Vector3(-3, 1, -6));
    }

    public void OnButton02()
    {
        //NPCManager.인스턴스.NPC소환("샘플빌런");
    }
    public void OnButton03()
    {
        //NPCManager.인스턴스.NPC소환("방화범");
    }

    public void OnButton04()
    {
        //NPCManager.인스턴스.NPC소환("Saimin");
    }
}
