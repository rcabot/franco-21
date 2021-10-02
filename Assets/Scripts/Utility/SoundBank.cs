using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using UnityRandom = UnityEngine.Random; //System.Random exists...

[CreateAssetMenu(menuName = "Franco Jam/Sound Bank")]
public class SoundBank : ScriptableObject
{
    [SerializeField] private List<AudioClip> m_AudioClips = new List<AudioClip>();

    public IReadOnlyList<AudioClip> AudioClips => m_AudioClips;
    public AudioClip                RandomSound => m_AudioClips[UnityRandom.Range(0, m_AudioClips.Count - 1)];
    public List<AudioClip>          ShuffledSequence => m_AudioClips.OrderBy(a => UnityRandom.value).ToList();
}
