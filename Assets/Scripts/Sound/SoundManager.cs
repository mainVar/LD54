using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LD54.Sound
{
    public class SoundManager : MonoBehaviour {
        public static SoundManager Instance;
        public Slider effectSlider;
        public Slider musicSlider;

        private List<AudioSource> musicSources = new List<AudioSource>();
        private List<AudioSource> soundEffectSources = new List<AudioSource>();

        private float effectsVolume {
            get {
                if (!PlayerPrefs.HasKey("effectsVolume")) {
                    PlayerPrefs.SetFloat("effectsVolume", 0.5f);
                }
                return PlayerPrefs.GetFloat("effectsVolume");
            }
            set => PlayerPrefs.SetFloat("effectsVolume", value);
        }

        private float musicVolume {
            get {
                if (!PlayerPrefs.HasKey("musicValue")) {
                    PlayerPrefs.SetFloat("musicVolume", 0.5f);
                }
                return PlayerPrefs.GetFloat("musicVolume");
            }
            set => PlayerPrefs.SetFloat("musicVolume", value);
        }

        private void Awake()
        {
            musicSlider.value = musicVolume;
            effectSlider.value = effectsVolume;

            if (Instance != null && Instance != this) {
                Destroy(gameObject);
            }
            else {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        public void SetMusicVolume()
        {
            foreach (AudioSource musicSource in musicSources)
            {
                musicSource.volume = musicSlider.value;
            }
            
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
        
        public void PlaySound(SoundData soundData)
        {
            if (soundData == null)
            {
                Debug.LogWarning("Trying to play a null SoundData.");
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

        private void OnDestroy() {
            if (Instance == this) {
                Instance = null;
            }
        }

        private void OnApplicationQuit() {
            Instance = null;
        }
    }
}