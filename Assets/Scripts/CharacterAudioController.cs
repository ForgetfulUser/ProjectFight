using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;

public enum AudioType
{
    Move,
    Attack,
    TakeDamage,
    Death
}

public class CharacterAudioController : MonoBehaviour
{
    [Header("SFX")]
    public AudioSource sfx_AS;
    public List<AudioClip> MovementAudioClips = new List<AudioClip>();
    public List<AudioClip> AttackAudioClips = new List<AudioClip>();
    public List<AudioClip> TakeDamageAudioClips = new List<AudioClip>();
    public List<AudioClip> DeathAudioClips = new List<AudioClip>();

    private Coroutine soundCoroutine;

    public void PlayClipByType(AudioType audioType)
    {
        int randClip = 0;
        AudioClip clip = null;
        switch (audioType)
        {
            case AudioType.Move:
                randClip = MovementAudioClips.Count > 1 ? Random.Range(0, MovementAudioClips.Count) : 0;
                clip = MovementAudioClips[randClip];
                break;
            case AudioType.Attack:
                randClip = AttackAudioClips.Count > 1 ? Random.Range(0, AttackAudioClips.Count) : 0;
                clip = AttackAudioClips[randClip];
                break;
            case AudioType.TakeDamage:
                randClip = TakeDamageAudioClips.Count > 1 ? Random.Range(0, TakeDamageAudioClips.Count) : 0;
                clip = TakeDamageAudioClips[randClip];
                break;
            case AudioType.Death:
                randClip = DeathAudioClips.Count > 1 ? Random.Range(0, DeathAudioClips.Count) : 0;
                clip = DeathAudioClips[randClip];
                break;
        }

        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
            soundCoroutine = null;
        }
        soundCoroutine = StartCoroutine(RemoveSound(clip.length));

        sfx_AS.clip = clip;
        sfx_AS.Play();
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
