using System.Collections.Generic;
using UnityEngine;

namespace LD54.Sound
{
    [CreateAssetMenu(fileName = "SoundData", menuName = "ScriptableObjects/SoundData")]
    public class SoundData : ScriptableObject
    {
        public AudioClip audioClip;
        public SoundType soundType;
    }
}