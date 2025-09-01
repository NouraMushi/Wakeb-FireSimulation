using UnityEngine;
using TMPro;

public class MissionManager : MonoBehaviour
{
    [Header("UI (HUD)")]
    public TMP_Text alertText;
    public TMP_Text firesText;
    public TMP_Text injuredText;
    public TMP_Text hudTimerText;        
    [Header("Mission Complete UI")]
    public GameObject missionCompletePanel; 
    public TMP_Text missionCompleteText;   

    [Header("Visuals")]
    public FlashingVolumeController alertFlasher; 

    private bool missionStarted = false;
    private bool missionEnded   = false;
    private float startTimeUnscaled;

    void OnEnable()
    {
        if (FireManager.Instance != null)
        {
            FireManager.Instance.OnFireStarted      += HandleFireStarted;
            FireManager.Instance.OnFireExtinguished += HandleFireExtinguished;
        }
        if (InjuredManager.Instance != null)
        {
            InjuredManager.Instance.OnInjuredHelped += HandleInjuredHelped;
        }
    }

    void OnDisable()
    {
        if (FireManager.Instance != null)
        {
            FireManager.Instance.OnFireStarted      -= HandleFireStarted;
            FireManager.Instance.OnFireExtinguished -= HandleFireExtinguished;
        }
        if (InjuredManager.Instance != null)
        {
            InjuredManager.Instance.OnInjuredHelped -= HandleInjuredHelped;
        }
    }

    void Start()
    {
        SetText(alertText,   "");
        SetText(firesText,   "");
        SetText(injuredText, "");
        SetText(hudTimerText, "");
        SetText(missionCompleteText, "");

        if (missionCompletePanel) missionCompletePanel.SetActive(false);
        if (alertFlasher) alertFlasher.StopFlashing();
        Time.timeScale = 1f; 
    }

    void Update()
    {
        if (missionStarted && !missionEnded)
        {
            float elapsed = Time.unscaledTime - startTimeUnscaled; 
            SetText(hudTimerText, $" {FormatTime(elapsed)}");
        }
    }
    void HandleFireStarted(FireSource fire)
    {
        if (!missionStarted)
        {
            missionStarted = true;
            startTimeUnscaled = Time.unscaledTime;
        }

        SetText(alertText, "There is a fire in the area!");
        SetText(firesText, "");

        if (alertFlasher) alertFlasher.StartFlashing();
        SoundController.Instance?.PlayAlarm();

    }

  
    void HandleFireExtinguished(FireSource fire)
    {

        if (FireManager.Instance != null && FireManager.Instance.AllFiresOut())
        {
            SoundController.Instance?.StopAlarm();
        }

        if (FireManager.Instance != null && FireManager.Instance.AllFiresOut())
        {
            SetText(firesText, "All fires have been extinguished");
            SetText(alertText, "");
            SetText(hudTimerText, "");
            if (alertFlasher) alertFlasher.StopFlashing();
            TryComplete();
        }
    }
    void HandleInjuredHelped(InjuredNPC npc)
    {
        if (InjuredManager.Instance != null && InjuredManager.Instance.AllHelped())
        {
            SetText(injuredText, "All injured civilians have been helped!");
            TryComplete();
        }
    }

    void TryComplete()
    {
        if (missionEnded) return;

        bool firesDone   = FireManager.Instance != null && FireManager.Instance.AllFiresOut();
        bool injuredDone = InjuredManager.Instance != null && InjuredManager.Instance.AllHelped();

        if (firesDone && injuredDone && missionStarted)
        {
            missionEnded = true;

            float elapsed = Time.unscaledTime - startTimeUnscaled;

            if (missionCompletePanel) missionCompletePanel.SetActive(true);
            SetText(missionCompleteText, "Mission Complete!");

         
            Time.timeScale = 0f;
        }
    }

    string FormatTime(float t)
    {
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        // int ms = Mathf.FloorToInt((t * 1000f) % 1000f);
        return $"{m:00}:{s:00}";
    }

    void SetText(TMP_Text t, string msg) { if (t) t.text = msg; }
}
