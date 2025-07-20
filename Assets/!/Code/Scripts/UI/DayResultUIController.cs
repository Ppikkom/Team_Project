using UnityEngine;
using UnityEngine.UI;

public class DayResultUIController : MonoBehaviour
{
    public Text 결과텍스트;

    public void 요약표시(float 점수)
    {
        if (점수 >= 100)
            결과텍스트.text = "해피엔딩!";
        else if (점수 <= 0)
            결과텍스트.text = "배드엔딩...";
        else
            결과텍스트.text = "평범한 하루";
    }
}
