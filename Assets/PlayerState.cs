using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerState : MonoBehaviour
{
    private int m_totalCollectables;
    private VictoryPortal m_victoryPortal;
    public int Health = 3;
    private int LeftToCollect => collectablesRegistries.Sum(c => c.SpawnedCollectables.Count(s => s != null));
    public State GameState;

    public static PlayerState Instance = null;
    private CollectablesDistributor[] collectablesRegistries;

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
        collectablesRegistries = FindObjectsOfType<CollectablesDistributor>();
        foreach (var distributor in collectablesRegistries)
        {
            m_totalCollectables += distributor.AmountToDistribute * distributor.TilesToLitter;
        }
        m_victoryPortal = FindObjectOfType<VictoryPortal>(true);
    }

    private void Update()
    {
        if (m_victoryPortal.TouchedByPlayer)
        {
            GameState = State.Victory;
        }
        else if(Health <= 0)
        {
            GameState = State.Defeat;
        }
        else if (LeftToCollect == 0)
        {
            GameState = State.ObjectiveComplete;
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
