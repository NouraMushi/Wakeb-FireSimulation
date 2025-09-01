using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource alarmSource;

    public AudioClip fireAlarmClip;   // plays when fire starts

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Create if missing
        if (!alarmSource)  alarmSource  = gameObject.AddComponent<AudioSource>();


        // Force correct config even if assigned in Inspector
        alarmSource.loop         = true;
        alarmSource.playOnAwake  = false;
        alarmSource.spatialBlend = 0f;   // 2D

    }

    void OnEnable()
    {
        if (FireManager.Instance != null)
        {
            FireManager.Instance.OnFireStarted      += HandleFireStarted;
            FireManager.Instance.OnFireExtinguished += HandleFireExtinguished;
        }
    }

    void OnDisable()
    {
        if (FireManager.Instance != null)
        {
            FireManager.Instance.OnFireStarted      -= HandleFireStarted;
            FireManager.Instance.OnFireExtinguished -= HandleFireExtinguished;
        }
    }


    public void PlayAlarm()
    {
        if (!alarmSource || !fireAlarmClip) return;
        if (alarmSource.isPlaying) return;

        alarmSource.clip = fireAlarmClip;
        alarmSource.Play();
    }

    public void StopAlarm()
    {
        if (alarmSource && alarmSource.isPlaying)
            alarmSource.Stop();
    }


    void HandleFireStarted(FireSource fire)        => PlayAlarm();
    void HandleFireExtinguished(FireSource fire)
    {
        
        if (FireManager.Instance != null && FireManager.Instance.AllFiresOut())
            StopAlarm();
    }
}
