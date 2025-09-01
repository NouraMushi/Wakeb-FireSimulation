using UnityEngine;
using UnityEngine.Rendering;

public class FlashingVolumeController : MonoBehaviour
{
    [Header("Refs")]
    public Volume volume;          

    [Header("Flash Settings")]
    [Range(0f, 1f)] public float minWeight = 0.0f;
    [Range(0f, 1f)] public float maxWeight = 1.0f;
    public float speed = 2.0f;     

    [Header("Optional: Pulse Intensity")]
    public bool alsoPulseIntensity = false;
    [Range(0f, 1f)] public float minIntensity = 0.2f; 
    [Range(0f, 1f)] public float maxIntensity = 0.7f;

    bool flashing = false;
    float t0;

    void Reset()
    {
        volume = GetComponent<Volume>();
    }

    void OnEnable()
    {
        if (volume) volume.weight = 0f;
    }

    void Update()
    {
        if (!flashing || !volume) return;

       
        float s = Mathf.Sin((Time.time - t0) * Mathf.PI * 2f * speed) * 0.5f + 0.5f;
        volume.weight = Mathf.Lerp(minWeight, maxWeight, s);

      
    }

    public void StartFlashing()
    {
        if (!volume) return;
        flashing = true;
        t0 = Time.time;
 
        volume.weight = maxWeight;
    }

    public void StopFlashing()
    {
        flashing = false;
        if (volume) volume.weight = 0f;
    }
}
