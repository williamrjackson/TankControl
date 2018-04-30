using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public Transform posTarget;
    public Transform lookTarget;
    public float followSpeed = .5f;

    private Vector3 velocity = Vector3.zero;

    // Update is called once per frame
    void LateUpdate () {
        transform.position = Vector3.SmoothDamp(transform.position, posTarget.position, ref velocity, followSpeed);
        transform.LookAt(lookTarget.position);
	}
}
