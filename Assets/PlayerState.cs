using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerState : MonoBehaviour
{
    private int m_totalCollectables;
    private VictoryPortal m_victoryPortal;
    public int Health = 3;
    public int Collected = 0;
    public State GameState;
    public float PercentageCollected => ((float)Collected) / m_totalCollectables;

    public static PlayerState Instance = null;
    //Unity Events
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError($"Error: Multiple shake managers exist. Objects: {Instance.name} | {name}");
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_totalCollectables = FindObjectsOfType<GameObject>().Count(o => o.tag == "Collectable");
        m_victoryPortal = FindObjectOfType<VictoryPortal>(true);
    }

    private void Update()
    {
        if(Collected >= m_totalCollectables)
        {
            GameState = State.ObjectiveComplete;
        }
        else if (m_victoryPortal.TouchedByPlayer)
        {
            GameState = State.Victory;
        }
        else if (Health <= 0)
        {
            GameState = State.Defeat;
        }
        else
        {
            GameState = State.Main;
        }
    }

    public enum State
    {
        Main,
        ObjectiveComplete,
        Victory,
        Defeat
    }
}
