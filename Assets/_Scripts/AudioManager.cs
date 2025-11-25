using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SoundType
{
    // UI Sounds
    ButtonClick,
    Victory,
    Defeat,

    // Tower Sounds
    TowerPlace,
    TowerAttack_Hunter,
    TowerAttack_Jester,
    TowerAttack_Lumberjack,
    TowerAttack_Priest,
    TowerAttack_Alchemist,
    TowerAttack_Warrior,

    //Wave Sounds
    IncomingWaveAlert
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }


    [Header("Audio Manager Settings")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;



    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip defeatClip;
    [SerializeField] private AudioClip incomingWaveAlertClip;

    [Header("Tower Sound Effects")]
    [SerializeField] private AudioClip towerPlaceClip;
    [SerializeField] private AudioClip towerAttackHunterClip;
    [SerializeField] private AudioClip towerAttackJesterClip;
    [SerializeField] private AudioClip towerAttackLumberjackClip;
    [SerializeField] private AudioClip towerAttackPriestClip;
    [SerializeField] private AudioClip towerAttackAlchemistClip;
    [SerializeField] private AudioClip towerAttackWarriorClip;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void PlaySound(SoundType soundType)
    {
        AudioClip clip = soundType switch
        {
            // UI Sounds
            SoundType.ButtonClick => buttonClickClip,
            SoundType.Victory => victoryClip,
            SoundType.Defeat => defeatClip,

            // Tower Sounds
            SoundType.TowerPlace => towerPlaceClip,
            SoundType.TowerAttack_Hunter => towerAttackHunterClip,
            SoundType.TowerAttack_Jester => towerAttackJesterClip,
            SoundType.TowerAttack_Lumberjack => towerAttackLumberjackClip,
            SoundType.TowerAttack_Priest => towerAttackPriestClip,
            SoundType.TowerAttack_Alchemist => towerAttackAlchemistClip,
            SoundType.TowerAttack_Warrior => towerAttackWarriorClip,
            // Wave Sounds
            SoundType.IncomingWaveAlert => incomingWaveAlertClip,

            _ => null
        };

        if (clip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }

    // Music control methods
    public void SetMusicEnabled(bool enabled)
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.enabled = enabled;
        }
    }

    public bool GetMusicEnabled() // NEW: Get current music state
    {
        return musicAudioSource != null && musicAudioSource.enabled;
    }

    public void SetMusicVolume(float volume)
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = Mathf.Clamp01(volume);
        }
    }

    public float GetMusicVolume()
    {
        return musicAudioSource != null ? musicAudioSource.volume : 0f;
    }

    // Sound effects control methods
    public void SetSoundEffectsEnabled(bool enabled)
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.enabled = enabled;
        }
    }

    public bool GetSoundEffectsEnabled() // NEW: Get current sound effects state
    {
        return sfxAudioSource != null && sfxAudioSource.enabled;
    }

    public void SetSoundEffectsVolume(float volume)
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = Mathf.Clamp01(volume);
        }
    }

    public float GetSoundEffectsVolume()
    {
        return sfxAudioSource != null ? sfxAudioSource.volume : 0f;
    }


}