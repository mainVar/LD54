using System;
using UnityEngine;

namespace LD54.Sound
{
    public class MenuSound : MonoBehaviour
    {
        [SerializeField] private SoundData soundData;

        private void Start()
        {
            SoundManager.Instance.PlaySound(soundData);
        }
    }
}