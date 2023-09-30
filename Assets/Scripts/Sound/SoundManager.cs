using System.Collections.Generic;
using UnityEngine;

namespace LD54.Sound
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        private List<AudioSource> audioSources = new List<AudioSource>();

        private bool isMusicEnabled = true;
        private bool isSoundEffectsEnabled = true;

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

        public void ToggleMusic()
        {
            isMusicEnabled = !isMusicEnabled;
        }

        public void ToggleSoundEffects()
        {
            isSoundEffectsEnabled = !isSoundEffectsEnabled;
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
            }

            source.Play();
            audioSources.Add(source);
            
        }

        private void Update()
        {
            for (int i = audioSources.Count - 1; i >= 0; i--)
            {
                if (!audioSources[i].isPlaying)
                {
                    Destroy(audioSources[i]);
                    audioSources.RemoveAt(i);
                }
            }
        }
    }
}