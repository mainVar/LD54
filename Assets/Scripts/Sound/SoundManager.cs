using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LD54.Sound
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;
        public Slider effectSlider;
        public Slider musicSlider;

        private List<AudioSource> musicSources = new List<AudioSource>();
        private List<AudioSource> soundEffectSources = new List<AudioSource>();

        private bool isMusicEnabled = true;
        private bool isSoundEffectsEnabled = true;
        private float effectsVolume;
        private float musicVolume;

        private void Awake()
        {
            musicVolume = musicSlider.value;
            effectsVolume = effectSlider.value;

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

        public void ToggleMusic()
        {
            isMusicEnabled = !isMusicEnabled;

            if (isMusicEnabled)
            {
                musicVolume = GetMusicVolume();
                SetMusicVolume(musicVolume);
            }
            else
            {
                musicVolume = GetMusicVolume();
                SetMusicVolume(0.0f);
            }
        }

        public void SetMusicVolume(float volume)
        {
            foreach (AudioSource musicSource in musicSources)
            {
                musicSource.volume = volume;
            }

            musicSlider.value = volume;
            musicVolume = musicSlider.value;
        }

        public void SetEffectVolume()
        {
            foreach (AudioSource soundEffect in soundEffectSources)
            {
                soundEffect.volume = effectSlider.value;
            }

            effectsVolume = effectSlider.value;
        }

        /// <summary>
        /// Get sound volume 1 sound source
        /// </summary>
        /// <returns></returns>
        public float GetMusicVolume()
        {
            if (musicSources.Count > 0)
            {
                return musicSources[0].volume;
            }

            return 0.0f;
        }

        public void ToggleSoundEffects()
        {
            isSoundEffectsEnabled = !isSoundEffectsEnabled;

            if (!isSoundEffectsEnabled)
            {
                for (int i = soundEffectSources.Count - 1; i >= 0; i--)
                {
                    Destroy(soundEffectSources[i]);
                    soundEffectSources.RemoveAt(i);
                }
            }
        }

        public void PlaySound(SoundData soundData)
        {
            if (soundData == null)
            {
                Debug.LogWarning("Trying to play a null SoundData.");
                return;
            }

            if ((soundData.soundType == SoundType.Music && !isMusicEnabled) ||
                (soundData.soundType == SoundType.SoundEffect && !isSoundEffectsEnabled))
            {
                return;
            }

            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = soundData.audioClip;

            if (soundData.soundType == SoundType.Music)
            {
                source.loop = true;
                source.volume = musicVolume;
                musicSources.Add(source);
            }
            else
            {
                source.volume = effectsVolume;
                soundEffectSources.Add(source);
            }

            source.Play();
        }

        private void Update()
        {
            for (int i = musicSources.Count - 1; i >= 0; i--)
            {
                if (!musicSources[i].isPlaying)
                {
                    Destroy(musicSources[i]);
                    musicSources.RemoveAt(i);
                }
            }

            for (int i = soundEffectSources.Count - 1; i >= 0; i--)
            {
                if (!soundEffectSources[i].isPlaying)
                {
                    Destroy(soundEffectSources[i]);
                    soundEffectSources.RemoveAt(i);
                }
            }
        }
    }
}