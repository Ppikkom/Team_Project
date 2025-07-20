using UnityEngine;
using TMPro; 

public class ChallengeEndUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI 생존시간텍스트;

    void Start()
    {
        float 생존시간 = GameManager.인스턴스.생존시간;

        // 시:분:초 형식으로 포맷
        int 시 = (int)(생존시간 / 3600);
        int 분 = (int)(생존시간 % 3600) / 60;
        int 초 = (int)(생존시간 % 60);
        생존시간텍스트.text = $"Survival Time: {시:D2}:{분:D2}:{초:D2}";
    }
}
