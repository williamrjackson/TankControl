using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankControl : MonoBehaviour {

    [Range(-1, 1f)]
    public float leftSpeedMultiplier = 0f;
    [Range(-1, 1f)]
    public float rightSpeedMultiplier = 0f;
    public float speed = 10f;
    [Range(-1, 1f)]
    public float turretRotationControl = 0f;
    public float turretRotationSpeed = 45f;
    public Transform turretRotator;
    [Range(-1, 1f)]
    public float canonElevationControl = 0f;
    public float canonElevationSpeed = 5f;
    public Transform canonRotator;
    public Transform canon;
    public GameObject projectile;
    public float projectileForce = 50;
    public float fireRecoilForce = 5;

    float m_determinedVelocityMultiplier = 0f;
    float m_CanonAngle = 0;

    Rigidbody rb;

    void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	void Update ()
    {
        CalculateMovement();
        HandleTurretRotation();
        HandleCanonElevation();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireProjectile();
        }
    }

    void FixedUpdate()
    {
        // Move the tank
        if (IsGrounded())
            rb.velocity = transform.forward * m_determinedVelocityMultiplier * speed;
    }

    void CalculateMovement()
    {
        // Tank velocity is the average value between the two speed controls.
        m_determinedVelocityMultiplier = (leftSpeedMultiplier + rightSpeedMultiplier) / 2;

        // Tank angle is the difference between the two speed controls, remapped from 4 degree to 180 degree range.
        float rotateAngle = Remap(leftSpeedMultiplier - rightSpeedMultiplier, -2f, 2f, -90f, 90f);
        // Apply rotation
        transform.Rotate(transform.up, rotateAngle * Time.deltaTime);
    }

    void HandleTurretRotation()
    {
        // Rotate the turret at based on turret rotation control input
        turretRotator.Rotate(turretRotator.up, turretRotationSpeed * turretRotationControl * Time.deltaTime);
    }

    void HandleCanonElevation()
    {
        // Adjust canon angle based on input. Limit to 25 degrees.
        m_CanonAngle = Mathf.Clamp(m_CanonAngle + (canonElevationSpeed * -canonElevationControl * Time.deltaTime), -25.0f, 0f);
        canonRotator.localEulerAngles = new Vector3(m_CanonAngle, 0, 0);
    }

    void FireProjectile()
    {
        // Instantiate copy of projectile template
        GameObject newProjectile = Instantiate(projectile);
        // Set position
        newProjectile.transform.position = projectile.transform.position;
        // Enable it
        newProjectile.SetActive(true);
        // Shoot it forward, along the canon barrel
        newProjectile.GetComponent<Rigidbody>().AddForce(canonRotator.forward * projectileForce, ForceMode.Impulse);
        // add reverse force for recoil
        rb.AddForce(canonRotator.forward * -fireRecoilForce, ForceMode.Impulse);
    }

    public bool IsGrounded()
    {
        bool result = false;
        if (Physics.Raycast(transform.position, -transform.up, 1.5f))
        { 
            result = true;
        }
        return result;
    }
    float Remap(float sourceValue, float sourceMin, float sourceMax, float destMin, float destMax)
    {
        return Mathf.Lerp(destMin, destMax, Mathf.InverseLerp(sourceMin, sourceMax, sourceValue));
    }
}
