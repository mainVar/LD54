using System;
using UnityEngine;

namespace LD54.Sound
{
    public class MenuSound : MonoBehaviour
    {
        [SerializeField] private SoundData soundData;
        private static bool hasStarted = false;
        private void Start()
        {
            if (!hasStarted)
            {
                SoundManager.Instance.PlaySound(soundData);
                hasStarted = true;
            }
        }

        private void OnApplicationQuit() {
            hasStarted = false;
        }
    }
}