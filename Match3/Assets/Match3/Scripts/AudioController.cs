using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SfxEffect { Select, Clear, CantSwap, MixBoard, Help }
public class AudioController : MonoBehaviour
{

    private AudioSource[] audioEffect;
    private bool mute;

    private void OnEnable()
    {
        mute = false;
        BoardController.SfxPlayHandler += PlayEffect;
    }
    
    private void OnDisable()
    {
        BoardController.SfxPlayHandler -= PlayEffect;
    }

    public delegate void OnMuteChange(bool isMute);
    public static event OnMuteChange MuteChangeHandler;

    void Awake()
    {
        audioEffect = GetComponents<AudioSource>();
    }

    public void PlayEffect(SfxEffect effect)
    {
        audioEffect[(int)effect].Play();
    }

    public void MuteChange()
    {
        if (!mute)
        {
            AudioListener.volume = 0;
            mute = true;
            MuteChangeHandler(mute);
        }
        else
        {
            AudioListener.volume = 1;
            mute = false;
            MuteChangeHandler(mute);
        }
    }
}
