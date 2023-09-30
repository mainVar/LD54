using System.Collections;
using System.Collections.Generic;
using LD54.Sound;
using UnityEngine;

public class SoundTest : MonoBehaviour
{
    [SerializeField] private SoundData soundData;

    public void MakeNoise()
    {
        SoundManager.Instance.PlaySound(soundData);
    }
}