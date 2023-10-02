using System.Collections;
using System.Collections.Generic;
using LD54.Sound;
using UnityEngine;

public class SoundInScene : MonoBehaviour
{
    [SerializeField] private SoundData soundDataPak1;
    [SerializeField] private SoundData soundDataPak2;

    private void Start()
    {
        if (Random.Range(0, 2) == 0)
        {
            SoundManager.Instance.PlaySound(soundDataPak1);
        }
        else
        {
            SoundManager.Instance.PlaySound(soundDataPak2);
        }
    }
}
