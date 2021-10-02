using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceSoundBankTwiddler : MonoBehaviour
{
    // Start is called before the first frame update
    public SoundBank soundBank;
    AudioSource audioSource;
    float timer;
    public float triggerThresholdTimer;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if( timer >= triggerThresholdTimer)
        {
            AudioClip clip = soundBank.RandomSound;
            audioSource.clip = clip;
            audioSource.Play();
            timer -= triggerThresholdTimer;
        }
    }
}
