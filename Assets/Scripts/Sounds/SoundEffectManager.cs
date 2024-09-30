using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SoundEffectManager : SingletonMonobehaviour<SoundEffectManager>
{
    public int soundsVolume = 8;

    private void Start()
    {
        if (PlayerPrefs.HasKey("soundsVolume"))
        {
            soundsVolume = PlayerPrefs.GetInt("soundsVolume");
        }

        SetSoundsVolume(soundsVolume);
    }

    private void OnDisable()
    {
        // Save volume settings in playerprefs
        PlayerPrefs.SetInt("soundsVolume", soundsVolume);
    }


    private void SetSoundsVolume(int volume)
    {
        float muteDecibels = -80f;

        if (volume == 0) {
            GameResources.Instance.soundMasterMixerGroup.audioMixer.SetFloat("soundsVolume",muteDecibels);
        }
        else {
            GameResources.Instance.soundMasterMixerGroup.audioMixer.SetFloat("soundsVolume", 
                HelperUtilities.LinearToDicibels(volume));
        }
    }

    public void PlaySoundEffect(SoundEffectSO soundEffect)
    {
        SoundEffect sound = (SoundEffect)PoolManager.Instance.ReuseComponent(soundEffect.soundPrefab, Vector3.zero, Quaternion.identity);
        sound.SetSound(soundEffect);
        sound.gameObject.SetActive(true);
        StartCoroutine(DisableSound(sound, soundEffect.soundEffectClip.length));
    }
    private IEnumerator DisableSound(SoundEffect sound, float soundDuration)
    {
        yield return new WaitForSeconds(soundDuration);
        sound.gameObject.SetActive(false);
    }


    public void IncreaseSoundVolume()
    {
        int maxSoundVolume = 20;

        if (soundsVolume >= maxSoundVolume) return;

        soundsVolume += 1;

        SetSoundsVolume(soundsVolume);
    }

    /// <summary>
    /// Decrease music volume
    /// </summary>
    public void DecreaseSoundVolume()
    {
        if (soundsVolume == 0) return;

        soundsVolume -= 1;

        SetSoundsVolume(soundsVolume);
    }
}
