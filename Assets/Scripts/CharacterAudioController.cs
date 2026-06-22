using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CharacterAudioController : MonoBehaviour
{
    [Header("SFX")]
    public AudioSource sfx_AS;
    public List<AudioClip> MovementAudioClips = new List<AudioClip>();
    public List<AudioClip> AttackAudioClips = new List<AudioClip>();

    private Coroutine soundCoroutine;

    public void PlaySound(AudioClip clip)
    {
        if(sfx_AS.clip != null)
        {
            sfx_AS.Stop();
        }

        sfx_AS.clip = clip;
        sfx_AS.Play();
    }

    public void PlayMovementClip()
    {
        if (sfx_AS.clip != null) return;
        int randClip = Random.Range(0, MovementAudioClips.Count);
        soundCoroutine = StartCoroutine(RemoveSound(MovementAudioClips[randClip].length));
        PlaySound(MovementAudioClips[randClip]);
    }

    public void PlayAttackClip()
    {
        int randClip = AttackAudioClips.Count > 1 ? Random.Range(0, AttackAudioClips.Count) : 0;
        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
            soundCoroutine = null;
        }
        PlaySound(AttackAudioClips[randClip]);
    }

    public IEnumerator RemoveSound(float waitTime)
    {
        float delayIncrease = 0.3f;
        waitTime += delayIncrease;
        yield return new WaitForSeconds(waitTime);
        sfx_AS.clip = null;
        soundCoroutine = null;
    }
}
