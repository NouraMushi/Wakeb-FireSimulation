using System;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    public static FireManager Instance { get; private set; }

    public event Action<FireSource> OnFireStarted;
    public event Action<FireSource> OnFireExtinguished;

    public readonly List<FireSource> ActiveFires = new();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterFire(FireSource fire)
    {
        if (fire == null || fire.IsExtinguished()) return;
        if (!ActiveFires.Contains(fire))
        {
            ActiveFires.Add(fire);
            OnFireStarted?.Invoke(fire);
        }
    }

    public void NotifyExtinguished(FireSource fire)
    {
        if (fire == null) return;
        if (ActiveFires.Remove(fire))
            OnFireExtinguished?.Invoke(fire);
    }

    public bool AllFiresOut() => ActiveFires.Count == 0;
    public FireSource FindNearestActiveFire(Vector3 fromPosition, float range = Mathf.Infinity)
    {
        FireSource best = null;
        float bestDist = range;
        foreach (var fire in ActiveFires)
        {
            if (!fire || fire.IsExtinguished()) continue;
            float d = Vector3.Distance(fromPosition, fire.transform.position);
            if (d < bestDist) { best = fire; bestDist = d; }
        }
        return best;
    }

}
