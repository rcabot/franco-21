using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

[Serializable]
public struct VOSoundBank
{
    public SoundBank SoundBank;
    public AudioClip SingleClip;

    [Tooltip("Sounds with a higher priority will interrupt those that are lower")]
    public int       Priority;
}

public class VoiceoverManager : MonoBehaviour
{
    public static VoiceoverManager Instance = null;
    private SubmarineController    m_PlayerSub;
    private int                    m_CurrentBankPriority = int.MinValue;

    [SerializeField, Header("Intro"), Tooltip("Sounds played sequentially for the intro")]
    private VOSoundBank            m_IntroVO = default;

    [SerializeField, Range(0f, 100f), Tooltip("Delay (seconds) on game start before the into VO will play")]
    private float                  m_IntroDelay = 0f;

    [SerializeField]
    private AudioSource            m_IntroAudioSource = default;

    [SerializeField, Header("Damage"), Tooltip("Sounds played on taking damage. Index is the number of hitpoints remaining")]
    private List<VOSoundBank>      m_DamageVO = new List<VOSoundBank>();
    [SerializeField]
    private AudioSource            m_DamageAudioSource = default;

    public event Action            OnIntroVOComplete;

    private IEnumerator CoPlaySequence(VOSoundBank bank, AudioSource audio_source)
    {
        if (audio_source == null)
            yield break;

        //Play single clip if there is one
        if (bank.SingleClip)
        {
            if (!audio_source.isPlaying || bank.Priority >= m_CurrentBankPriority)
            {
                audio_source.clip = bank.SingleClip;
                audio_source.Play();
            }
            yield break;
        }

        audio_source.clip = null;
        AudioClip last = null;
        foreach (AudioClip clip in bank.SoundBank.AudioClips)
        {
            //Our clip may have been replaced. So only play the next in the sequence if that hasn't happened or we're higher priority
            if (audio_source.clip == last || bank.Priority > m_CurrentBankPriority)
            {
                last = clip;
                m_CurrentBankPriority = bank.Priority;
                audio_source.clip = clip;
                audio_source.Play();

                //Wait for the clip to finish
                yield return new WaitWhile(() => audio_source.isPlaying && audio_source.clip == clip);
            }
        }
    }

    private IEnumerator CoPlayIntro()
    {
        yield return new WaitForSeconds(m_IntroDelay);
        yield return StartCoroutine(CoPlaySequence(m_IntroVO, m_IntroAudioSource));
        OnIntroVOComplete?.Invoke();
    }

    void OnPlayerHit()
    {
        int health = PlayerState.Instance?.Health ?? 0;
        if (health >= 0 && health < m_DamageVO.Count)
        {
            StartCoroutine(CoPlaySequence(m_DamageVO[health], m_DamageAudioSource));
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            m_PlayerSub = FindObjectOfType<SubmarineController>();
            if (m_PlayerSub)
            {
                m_PlayerSub.OnTakeHit += OnPlayerHit;
            }

            StartCoroutine(CoPlayIntro());
        }
        else
        {
            Debug.LogError($"Error: More than one VO manager is present in the scene. Objects: [{this}] and [{Instance}]");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (m_PlayerSub)
        {
            m_PlayerSub.OnTakeHit -= OnPlayerHit;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
