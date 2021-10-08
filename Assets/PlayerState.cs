using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    private int m_MaxHealth;
    private int m_totalCollectables;
    private VictoryPortal m_victoryPortal;
    public int  Health = 3;
    public int  MaxHealth => m_MaxHealth;
    public int  TotalCollectables => m_totalCollectables;
    public int  CalculateLeftToCollect => collectablesRegistries.Sum(c => c.SpawnedCollectables.Count);
    public State GameState;

    public static PlayerState Instance = null;
    private CollectablesDistributor[] collectablesRegistries;

    public event Action<PlayerState> OnItemCollected;
    public event Action<State, State> OnGameStateChanged;

    public void CollecedItem()
    {
        OnItemCollected?.Invoke(this);
    }

    //Unity Events
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            m_MaxHealth = Health;
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
        collectablesRegistries = FindObjectsOfType<CollectablesDistributor>();
        foreach (var distributor in collectablesRegistries)
        {
            m_totalCollectables += distributor.AmountToDistribute * distributor.TilesToLitter;
        }
        m_victoryPortal = FindObjectOfType<VictoryPortal>(true);
    }

    private void Update()
    {
        State prev_state = GameState;
        if (m_victoryPortal.TouchedByPlayer)
        {
            GameState = State.Victory;
        }
        else if(Health <= 0)
        {
            GameState = State.Defeat;
        }
        else if (CalculateLeftToCollect == 0)
        {
            GameState = State.ObjectiveComplete;
        }
        else
        {
            GameState = State.Main;
        }

        if (prev_state != GameState)
        {
            OnGameStateChanged?.Invoke(prev_state, GameState);
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
