using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public enum VOSoundBankMode : byte
{
    Sequence,
    Random
}

[Serializable]
public struct VOSoundBank
{
    public SoundBank       SoundBank;
    public AudioClip       SingleClip;

    [Tooltip("Sounds with a higher priority will interrupt those that are lower")]
    public int             Priority;

    public VOSoundBankMode Mode;
}

public class VoiceoverManager : MonoBehaviour
{
    public static VoiceoverManager Instance = default;
    private HunterBehaviour        m_Creature = default;
    private SubmarineController    m_PlayerSub = default;
    private int                    m_CurrentBankPriority = int.MinValue;
    private bool                   m_MonsterDetected = false;
    private bool                   m_MonsterWasNear = false;
    private float                  m_MonsterNearCooldownRemaining = 0f;


    [SerializeField, Header("Audio Sources"), Tooltip("Audio source for the handler")]
    private AudioSource            m_HandlerAudioSource = default;
    [SerializeField, Tooltip("Audio source for the EVA computer voice")]
    private AudioSource            m_EVAAudioSource = default;

    [SerializeField, Header("Intro"), Tooltip("Sounds played sequentially for the intro")]
    private VOSoundBank            m_IntroVO = default;

    [SerializeField, Range(0f, 100f), Tooltip("Delay (seconds) on game start before the into VO will play")]
    private float                  m_IntroDelay = 0f;

    [SerializeField, Header("Creature"), Tooltip("Sounds played on taking damage. Index is the number of hitpoints remaining")]
    private List<VOSoundBank>      m_DamageVO = new List<VOSoundBank>();

    [SerializeField, Tooltip("Audio bank played when the creature comes out from backstage")]
    private VOSoundBank            m_MonsterDetectedVO = default;

    [SerializeField, Tooltip("Audio bank played when the creature gets close")]
    private VOSoundBank            m_MonsterNearVO = default;

    [SerializeField, Range(0f, 1000f), Tooltip("Range at which to trigger the near VO")]
    private float                  m_MonsterNearRange = 30f;

    [SerializeField, Range(1f, 1000f), Tooltip("Time delay before the monster near VO may fire again")]
    private float                  m_MonsterNearCooldown = 5f;

    public event Action            OnIntroVOComplete;

    private IEnumerator CoPlayBank(VOSoundBank bank, AudioSource audio_source)
    {
        if (audio_source == null)
            yield break;

        //Play single clip or random if set
        AudioClip single_clip = bank.SingleClip ?? (bank.Mode == VOSoundBankMode.Random ? bank.SoundBank.RandomSound : null);
        if (single_clip)
        {
            if (!audio_source.isPlaying || bank.Priority >= m_CurrentBankPriority)
            {
                audio_source.clip = single_clip;
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
        if (m_IntroDelay > 0f)
            yield return new WaitForSeconds(m_IntroDelay);

        yield return StartCoroutine(CoPlayBank(m_IntroVO, m_HandlerAudioSource));
        OnIntroVOComplete?.Invoke();
    }

    private void UpdateMonsterNear()
    {
        if (m_MonsterNearCooldownRemaining > 0f)
        {
            m_MonsterNearCooldownRemaining -= Time.deltaTime;
        }
        else if (m_MonsterNearCooldownRemaining <= 0f)
        {
            float sqr_distance = (m_Creature.transform.position - m_PlayerSub.transform.position).sqrMagnitude;
            if (sqr_distance < (m_MonsterNearRange * m_MonsterNearRange))
            {
                //Is near, wasn't before. Play audio
                if (!m_MonsterWasNear)
                {
                    m_MonsterNearCooldownRemaining = m_MonsterNearCooldown;
                    StartCoroutine(CoPlayBank(m_MonsterNearVO, m_EVAAudioSource));
                }
                m_MonsterWasNear = true;
            }
            else
            {
                m_MonsterWasNear = false;
            }
        }
    }

    #region Event Listeners
 
    void OnMonsterChangeState(HunterState prev_state, HunterState new_state)
    {
        if (!m_MonsterDetected && prev_state == HunterState.Backstage)
        {
            m_MonsterDetected = true;
            StartCoroutine(CoPlayBank(m_MonsterDetectedVO, m_EVAAudioSource));
        }
    }

    void OnPlayerHit()
    {
        int health = PlayerState.Instance?.Health ?? 0;
        if (health >= 0 && health < m_DamageVO.Count)
        {
            StartCoroutine(CoPlayBank(m_DamageVO[health], m_EVAAudioSource));
        }
    }

    #endregion

    #region Unit Events

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

            m_Creature = HunterBehaviour.Instance;
            m_Creature.OnStateChanged += OnMonsterChangeState;

            StartCoroutine(CoPlayIntro());
        }
        else
        {
            Debug.LogError($"Error: More than one VO manager is present in the scene. Objects: [{this}] and [{Instance}]");
            Destroy(this);
        }
    }

    private void FixedUpdate()
    {
        UpdateMonsterNear();
    }

    private void OnDestroy()
    {
        if (m_PlayerSub)
            m_PlayerSub.OnTakeHit -= OnPlayerHit;

        if (HunterBehaviour.Instance)
            HunterBehaviour.Instance.OnStateChanged -= OnMonsterChangeState;

        if (Instance == this)
            Instance = null;
    }

    #endregion
}
