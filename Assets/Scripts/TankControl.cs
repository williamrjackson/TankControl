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
    public LineRenderer line;

    private float m_determinedVelocityMultiplier = 0f;
    private float m_CanonAngle = 0;

    private Rigidbody rBody;

    private void Start () {
        rBody = GetComponent<Rigidbody>();
	}
	
	private void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireProjectile();
        }
    }

    private void LateUpdate()
    {
        CalculateMovement();
        HandleTurretRotation();
        HandleCanonElevation();
        line.positionCount = 50;
        for (int i = 0; i < 50; i++)
        {
            line.SetPosition(i, PlotProjectileTrajectory(projectile.transform.position, canonRotator.forward * projectileForce, Remap(i, 0, 50, 0, 5)));
        }
    }

    private void FixedUpdate()
    {
        // Move the tank
        if (IsGrounded())
            rBody.velocity = transform.forward * m_determinedVelocityMultiplier * speed;
    }

    private void CalculateMovement()
    {
        // Tank velocity is the average value between the two speed controls.
        m_determinedVelocityMultiplier = (leftSpeedMultiplier + rightSpeedMultiplier) / 2;

        // Tank angle is the difference between the two speed controls, remapped from 4 degree to 180 degree range.
        float rotateAngle = Remap(leftSpeedMultiplier - rightSpeedMultiplier, -2f, 2f, -90f, 90f);
        // Apply rotation
        transform.Rotate(transform.up, rotateAngle * Time.deltaTime);
    }

    private void HandleTurretRotation()
    {
        // Rotate the turret based on turret rotation control input
        turretRotator.Rotate(turretRotator.up, turretRotationSpeed * turretRotationControl * Time.deltaTime);
    }

    private void HandleCanonElevation()
    {
        // Adjust canon angle based on input. Limit to 25 degrees.
        m_CanonAngle = Mathf.Clamp(m_CanonAngle + (canonElevationSpeed * -canonElevationControl * Time.deltaTime), -25.0f, 0f);
        canonRotator.localEulerAngles = new Vector3(m_CanonAngle, 0, 0);
    }

    private void FireProjectile()
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
        rBody.AddForce(canonRotator.forward * -fireRecoilForce, ForceMode.Impulse);
    }

    private Vector3 PlotProjectileTrajectory(Vector3 startPos, Vector3 initialVel, float time)
    {
        return startPos + initialVel * time + Physics.gravity * Mathf.Pow(time, 2) * 0.5f;
    }

    private bool IsGrounded()
    {
        bool result = false;
        if (Physics.Raycast(transform.position, -transform.up, 1.5f))
        { 
            result = true;
        }
        return result;
    }
    private float Remap(float sourceValue, float sourceMin, float sourceMax, float destMin, float destMax)
    {
        return Mathf.Lerp(destMin, destMax, Mathf.InverseLerp(sourceMin, sourceMax, sourceValue));
    }
}
