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
    private int                    m_LastPickupThreshold = int.MinValue;
    private int                    m_CurrentBankPriority = int.MinValue;
    private float                  m_MonsterNearCooldownRemaining = 0f;
    private bool                   m_MonsterDetected = false;
    private bool                   m_MonsterWasNear = false;

    [SerializeField, Header("Audio Sources"), Tooltip("Audio source for the handler")]
    private AudioSource            m_HandlerAudioSource = default;

    [SerializeField, Tooltip("Audio source for the EVA computer voice")]
    private AudioSource            m_EVAAudioSource = default;

    [SerializeField, Header("Handler Voice Over"), Tooltip("Handler VO Sounds played for the intro")]
    private VOSoundBank            m_IntroVO = default;

    [SerializeField, Range(0f, 100f), Tooltip("Delay (seconds) on game start before the intro VO will play")]
    private float                  m_IntroDelay = 0f;

    [SerializeField, Tooltip("Handler VO played when the game is over")]
    private VOSoundBank            m_GameoverVO = default;

    [SerializeField, Tooltip("Handler VO played on victory")]
    private VOSoundBank            m_VictoryVO = default;

    [SerializeField, Tooltip("Handler VO played when the victory portal opens")]
    private VOSoundBank            m_ObjectiveCompleteVO = default;

    [SerializeField, Header("EVA Voice Over"), Tooltip("EVA VO Sounds played for the intro")]
    private VOSoundBank            m_EVAIntroVO = default;

    [SerializeField, Range(0f, 100f), Tooltip("Delay (seconds) on game start before the EVA intro VO will play")]
    private float                  m_EVAIntroDelay = 0f;

    [SerializeField, Tooltip("EVA Sounds played on taking damage. Index is the number of hitpoints remaining")]
    private List<VOSoundBank>      m_DamageVO = new List<VOSoundBank>();

    [SerializeField, Tooltip("EVA Audio bank played when the creature comes out from backstage")]
    private VOSoundBank            m_MonsterDetectedVO = default;

    [SerializeField, Tooltip("EVA Audio bank played when the creature gets close")]
    private VOSoundBank            m_MonsterNearVO = default;

    [SerializeField, Range(0f, 1000f), Tooltip("Range at which to trigger the near VO")]
    private float                  m_MonsterNearRange = 30f;

    [SerializeField, Range(1f, 1000f), Tooltip("Time delay before the monster near VO may fire again")]
    private float                  m_MonsterNearCooldown = 5f;

    [SerializeField, Tooltip("EVA VO played when the player has collected trash. Index is a divisor. If there are two sounds one will be played at 50% and one at 100%")]
    private List<VOSoundBank>      m_PickupsVO = new List<VOSoundBank>();

    public event Action            OnIntroVOComplete;
    public event Action            OnOutroVOComplete;

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
                //Wait for the clip to finish
                yield return new WaitWhile(() => audio_source.isPlaying && audio_source.clip == single_clip);
            }
            yield break;
        }

        if (bank.SoundBank)
        {
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
    }

    private IEnumerator CoPlayBankDelayed(VOSoundBank bank, AudioSource audio_source, float delay_seconds)
    {
        if (delay_seconds > 0f)
            yield return new WaitForSeconds(delay_seconds);

        yield return StartCoroutine(CoPlayBank(bank, audio_source));
    }

    private IEnumerator CoPlayIntro()
    {
        Coroutine eva_audio = StartCoroutine(CoPlayBankDelayed(m_EVAIntroVO, m_EVAAudioSource, m_EVAIntroDelay));
        Coroutine handler_audio = StartCoroutine(CoPlayBankDelayed(m_IntroVO, m_HandlerAudioSource, m_IntroDelay));

        //Wait for all audio to finish
        yield return eva_audio;
        yield return handler_audio;

        //Notify that intro is finished
        OnIntroVOComplete?.Invoke();
    }

    private IEnumerator CoPlayOutro(VOSoundBank bank, AudioSource audio_source)
    {
        yield return StartCoroutine(CoPlayBank(bank, m_HandlerAudioSource));
        OnOutroVOComplete?.Invoke();
    }

    private void UpdateMonsterNear()
    {
        if (m_Creature == null)
            return;

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

    void OnPlayerHitAnimFinished()
    {
        int health = PlayerState.Instance?.Health ?? 0;
        if (health >= 0 && health < m_DamageVO.Count)
        {
            StartCoroutine(CoPlayBank(m_DamageVO[health], m_EVAAudioSource));
        }
    }

    private void OnItemCollected(PlayerState player)
    {
        int total_thresholds = m_PickupsVO.Count;
        if (total_thresholds > 0 && total_thresholds > m_LastPickupThreshold)
        {
            int per_threshold = player.TotalCollectables / total_thresholds;
            int collected = player.TotalCollectables - player.CalculateLeftToCollect;

            int threshold = (collected / per_threshold) - 1;
            if (threshold >= 0 && threshold > m_LastPickupThreshold)
            {
                m_LastPickupThreshold = threshold;

                if (m_LastPickupThreshold < total_thresholds)
                {
                    StartCoroutine(CoPlayBank(m_PickupsVO[m_LastPickupThreshold], m_EVAAudioSource));
                }
            }
        }
    }

    private void OnGameStateChanged(PlayerState.State prev, PlayerState.State next)
    {
        Debug.Log(next);
        if (prev == next)
            return;

        switch (next)
        {
            case PlayerState.State.Defeat:
                StartCoroutine(CoPlayOutro(m_GameoverVO, m_HandlerAudioSource));
                break;
            case PlayerState.State.Victory:
                StartCoroutine(CoPlayOutro(m_VictoryVO, m_HandlerAudioSource));
                break;
            case PlayerState.State.ObjectiveComplete:
                StartCoroutine(CoPlayBank(m_ObjectiveCompleteVO, m_HandlerAudioSource));
                break;
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
                m_PlayerSub.OnHitAnimFinished += OnPlayerHitAnimFinished;
            }
            StartCoroutine(CoPlayIntro());
        }
        else
        {
            Debug.LogError($"Error: More than one VO manager is present in the scene. Objects: [{this}] and [{Instance}]");
            Destroy(this);
        }
    }

    private void Start()
    {
        m_Creature = HunterBehaviour.Instance;
        if (m_Creature)
            m_Creature.OnStateChanged += OnMonsterChangeState;

        PlayerState.Instance.OnItemCollected += OnItemCollected;
        PlayerState.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    private void FixedUpdate()
    {
        UpdateMonsterNear();
    }

    private void OnDestroy()
    {
        if (m_PlayerSub)
            m_PlayerSub.OnHitAnimFinished -= OnPlayerHitAnimFinished;

        if (HunterBehaviour.Instance)
            HunterBehaviour.Instance.OnStateChanged -= OnMonsterChangeState;

        if (PlayerState.Instance)
            PlayerState.Instance.OnItemCollected -= OnItemCollected;

        if (Instance == this)
            Instance = null;
    }

    #endregion
}
