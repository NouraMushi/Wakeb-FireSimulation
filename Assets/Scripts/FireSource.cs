using UnityEngine;
using System.Collections;

public class FireSource : MonoBehaviour, IInteractable
{
    public ParticleSystem fireParticles;
    public float spreadRadius = 10f;
    public float spreadDelay = 6f;

    private bool isExtinguished = false;
    public bool delayOnStart = false;


    // void Start()
    // {
    //     if (fireParticles == null) fireParticles = GetComponentInChildren<ParticleSystem>();

    //     FireManager.Instance?.RegisterFire(this);

    //     if (!isExtinguished) StartCoroutine(FireSpreadLoop());
    // }
    void Start()
    {
        if (fireParticles == null)
            fireParticles = GetComponentInChildren<ParticleSystem>();

        if (delayOnStart)
            StartCoroutine(DelayedStart());
        else
            Ignite();
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(5f);
        Ignite();
    }

    void Ignite()
    {
        isExtinguished = false;
        if (fireParticles) fireParticles.Play();
        FireManager.Instance?.RegisterFire(this);
        StartCoroutine(FireSpreadLoop());
    }


    public void OnInteract()
    {
        if (isExtinguished) return;

        isExtinguished = true;
        if (fireParticles) fireParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        StopAllCoroutines();


        FireManager.Instance?.NotifyExtinguished(this);

        Debug.Log("Fire extinguished!");
    }

    IEnumerator FireSpreadLoop()
    {
        while (!isExtinguished)
        {
            yield return new WaitForSeconds(spreadDelay);
            SpreadFire();
        }
    }

    void SpreadFire()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, spreadRadius);
        foreach (var hit in hitColliders)
        {
            if (hit.gameObject == this.gameObject) continue;

            if (hit.CompareTag("Flammable") && hit.GetComponent<FireSource>() == null)
            {
                
                var newFire = hit.gameObject.AddComponent<FireSource>();
                
                ParticleSystem newParticles = Instantiate(fireParticles, hit.transform);
                newParticles.transform.localPosition = new Vector3(0, 0, -3);
                newParticles.transform.localScale    = new Vector3(3, 3, 3);
                newFire.fireParticles = newParticles;
                newParticles.Play();

                
                FireManager.Instance?.RegisterFire(newFire);

                Debug.Log("Fire spread to: " + hit.name);
                break; 
            }
        }
    }

    public bool IsExtinguished() => isExtinguished;

    public Vector3 GetWorldFirePos()
    {
        if (fireParticles) return fireParticles.transform.position;
        return transform.position;
    }
}
