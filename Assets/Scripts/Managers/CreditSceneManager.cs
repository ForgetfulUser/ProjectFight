using TMPro;
using UnityEngine;

public class CreditSceneManager : MonoBehaviour
{
    public TMP_Text timer_TXT;
    private float timer = 8f;

    void Update()
    {
        timer_TXT.text = ((int)timer).ToString();
        timer -= Time.deltaTime;
        if (timer < 0)
            LevelManager.Instance.LoadScene(0);
    }
}
