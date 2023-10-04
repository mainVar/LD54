using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LD54.Sound
{
    public class SoundManager : MonoBehaviour {
        private static SoundManager instance;

        public static SoundManager Instance {
            get {
                if (instance == null) {
                    instance = new GameObject("SoundManager").AddComponent<SoundManager>();
                }

                return instance;
            }
        }
        public Slider effectSlider;
        public Slider musicSlider;

        private List<AudioSource> musicSources = new List<AudioSource>();
        private List<AudioSource> soundEffectSources = new List<AudioSource>();

        private float effectsV =- 1;
        private float effectsVolume {
            get {
                if (effectsV < 0) {
                    if (!PlayerPrefs.HasKey("effectsVolume")) {
                        PlayerPrefs.SetFloat("effectsVolume", 0.5f);
                    }
                    effectsV = PlayerPrefs.GetFloat("effectsVolume");
                }
                return effectsV;
            }
            set {
                effectsV = value;
                PlayerPrefs.SetFloat("effectsVolume", effectsV);
            }
        }

        private float musicV =- 1;
        private float musicVolume {
            get {
                if (musicV < 0) {
                    if (!PlayerPrefs.HasKey("musicVolume")) {
                        PlayerPrefs.SetFloat("musicVolume", 0.5f);
                    }
                    musicV = PlayerPrefs.GetFloat("musicVolume");
                }
                return musicV;
            }
            set {
                musicV = value;
                PlayerPrefs.SetFloat("musicVolume", musicV);
            }
        }

        private void Awake()
        {
            musicSlider.value = musicVolume;
            effectSlider.value = effectsVolume;

            if (instance != null && instance != this) {
                Destroy(gameObject);
            }
            else {
                instance = this;
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

        private void KillMusic()
        {
            foreach (var musicSource in musicSources)
            {
                Destroy(musicSource);
            }
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
                KillMusic();
                musicSources.Clear();
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
            if (instance == this) {
                instance = null;
            }
        }

        private void OnApplicationQuit() {
            instance = null;
        }
    }
}