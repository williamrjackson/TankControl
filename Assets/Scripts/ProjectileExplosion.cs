using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileExplosion : MonoBehaviour {
    public ParticleSystem particles;

    void OnCollisionEnter()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        particles.Play();
        StartCoroutine(KillMe(particles.main.duration));
    }

    IEnumerator KillMe (float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
