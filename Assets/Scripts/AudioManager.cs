using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource bgAudioSource;  // 背景音乐 AudioSource
    public AudioSource sfxAudioSource; // 音效 AudioSource

    [Header("Background Music (Music Folder)")]
    public AudioClip bgIntro;         // Assets/Audio/Music/BG_Intro.wav
    public AudioClip bgStartScene;    // Assets/Audio/Music/BG_StartScene.wav
    public AudioClip bgGhostNormal;   // Assets/Audio/Music/BG_GhostNormal.wav
    public AudioClip bgGhostScared;   // Assets/Audio/Music/BG_GhostScared.wav
    public AudioClip bgGhostDead;     // Assets/Audio/Music/BG_GhostDead.wav

    [Header("Sound Effects (SFX Folder)")]
    public AudioClip sfxMove;         // Assets/Audio/SFX/SFX_Move.wav
    public AudioClip sfxEatPellet;    // Assets/Audio/SFX/SFX_EatPellet.wav
    public AudioClip sfxWallHit;      // Assets/Audio/SFX/SFX_WallHit.wav
    public AudioClip sfxDeath;        // Assets/Audio/SFX/SFX_Death.wav

    void Start()
    {
        // 播放 Intro 背景音乐，然后切换到幽灵正常状态音乐
        StartCoroutine(PlayIntroThenNormal());
    }

    IEnumerator PlayIntroThenNormal()
    {
        bgAudioSource.clip = bgIntro;
        bgAudioSource.loop = false;  // Intro 只播放一次
        bgAudioSource.Play();

        // 等待音频结束或 3 秒，取最小值
        float waitTime = Mathf.Min(bgIntro.length, 3f);
        yield return new WaitForSeconds(waitTime);

        bgAudioSource.clip = bgGhostNormal;
        bgAudioSource.loop = true; // 循环播放幽灵正常状态音乐
        bgAudioSource.Play();
    }

    // 音效播放方法
    public void PlayMove() => sfxAudioSource.PlayOneShot(sfxMove);
    public void PlayEatPellet() => sfxAudioSource.PlayOneShot(sfxEatPellet);
    public void PlayWallHit() => sfxAudioSource.PlayOneShot(sfxWallHit);
    public void PlayDeath() => sfxAudioSource.PlayOneShot(sfxDeath);

    // 背景音乐切换方法
    public void PlayStartSceneMusic()
    {
        bgAudioSource.clip = bgStartScene;
        bgAudioSource.loop = true;
        bgAudioSource.Play();
    }

    public void PlayGhostScaredMusic()
    {
        bgAudioSource.clip = bgGhostScared;
        bgAudioSource.loop = true;
        bgAudioSource.Play();
    }

    public void PlayGhostDeadMusic()
    {
        bgAudioSource.clip = bgGhostDead;
        bgAudioSource.loop = true;
        bgAudioSource.Play();
    }
}
