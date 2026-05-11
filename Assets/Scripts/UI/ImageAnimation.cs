using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ImageAnimation : MonoBehaviour
{
    public Image targetImage; // assign in Inspector
    public Sprite[] frames;
    public float fps = 12f;

    private Coroutine playCoroutine;

    void OnEnable()
    {
        if (targetImage != null && frames != null && frames.Length > 0)
            playCoroutine = StartCoroutine(PlayAtFPS());
    }

    void OnDisable()
    {
        if (playCoroutine != null) StopCoroutine(playCoroutine);
    }

    IEnumerator PlayAtFPS()
    {
        int i = 0;
        float wait = 1f / Mathf.Max(0.0001f, fps);
        while (true)
        {
            targetImage.sprite = frames[i];
            i = (i + 1) % frames.Length;
            yield return new WaitForSecondsRealtime(wait);
        }
    }

}