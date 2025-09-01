using System;
using System.Collections.Generic;
using UnityEngine;

public class InjuredManager : MonoBehaviour
{
    public static InjuredManager Instance { get; private set; }

    public event Action<InjuredNPC> OnInjuredSpawned;
    public event Action<InjuredNPC> OnInjuredHelped;

    private readonly List<InjuredNPC> _all = new();
    private int _helpedCount = 0;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(InjuredNPC npc)
    {
        if (!_all.Contains(npc)) { _all.Add(npc); OnInjuredSpawned?.Invoke(npc); }
    }

    public void NotifyHelped(InjuredNPC npc)
    {
        
        _helpedCount++;

      
        OnInjuredHelped?.Invoke(npc);
}


    public int Total => _all.Count;
    public int Helped => _helpedCount;
    public bool AllHelped() => _all.Count > 0 && _helpedCount >= _all.Count;
}
