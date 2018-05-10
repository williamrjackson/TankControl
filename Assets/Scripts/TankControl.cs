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
    public float projectileForce = 50f;
    public float fireRecoilForce = 5f;
    public LineRenderer line;
    public Transform cameraTarget;

    private float m_determinedVelocityMultiplier = 0f;
    private float m_CanonAngle = 0f;
    private Vector3 m_GroundNormal;
    private Rigidbody rBody;

    private void Start () {
        rBody = GetComponent<Rigidbody>();
        line.useWorldSpace = true;
	}
	
	private void Update ()
    {
#if false
        if (Input.GetButton("LeftShoulder"))
        {
            leftSpeedMultiplier = -1f;
        }
        else
        {
            leftSpeedMultiplier = Input.GetAxis("LeftTrigger");
        }
        if (Input.GetButton("RightShoulder"))
        {
            rightSpeedMultiplier = -1f;
        }
        else
        {
            rightSpeedMultiplier = Input.GetAxis("RightTrigger");
        }

        turretRotationControl = Input.GetAxis("Horizontal");
        canonElevationControl = Input.GetAxis("Vertical");
#endif
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1"))
        {
            FireProjectile();
        }
    }

    private void LateUpdate()
    {
        HandleTurretRotation();
        HandleCanonElevation();
        int maxLineSegments = 50;
        Vector3 lastLinePos = projectile.transform.position;
        for (int i = 0; i < maxLineSegments; i++)
        {
            Vector3 nextLinePos = PlotProjectileTrajectory(projectile.transform.position, canonRotator.forward * projectileForce, Remap(i, 0, maxLineSegments, 0, 5));
            line.positionCount = i + 1;
            line.SetPosition(i, nextLinePos);
            RaycastHit hit;
            Physics.Raycast(lastLinePos, nextLinePos - lastLinePos, out hit, Vector3.Distance(nextLinePos, lastLinePos));
            if (hit.collider != null)
            {
                //print(hit.collider.name);
                break;
            }
            lastLinePos = nextLinePos;
        }
    }

    private void FixedUpdate()
    {
        CalculateMovement();

        // Move the tank
        if (IsGrounded() && m_determinedVelocityMultiplier != 0)
        {
            rBody.velocity = Vector3.Cross(transform.right, m_GroundNormal) * m_determinedVelocityMultiplier * speed;
            float sideSpeed = transform.InverseTransformDirection(rBody.velocity).x;
            rBody.AddForce(Vector3.Cross(transform.right, Vector3.up) * -sideSpeed * 100);
        }
    }

    private void CalculateMovement()
    {
        // Tank velocity is the average value between the two speed controls.
        m_determinedVelocityMultiplier = (leftSpeedMultiplier + rightSpeedMultiplier) / 2;

        // Only force rotation if we're on the ground. Otherwise physics is in charge.
        if (IsGrounded())
        {
            // Tank angle is the difference between the two speed controls, remapped from 4 degree to 180 degree range.
            float rotateAngle = Remap(leftSpeedMultiplier - rightSpeedMultiplier, -2f, 2f, -90f, 90f);
            // Apply rotation
            transform.Rotate(transform.up, rotateAngle * Time.fixedDeltaTime);
        }
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
        bool bIsGrounded = false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 1.5f))
        {
            m_GroundNormal = hit.normal;
            bIsGrounded = true;
        }

        return bIsGrounded;
    }
    private float Remap(float sourceValue, float sourceMin, float sourceMax, float destMin, float destMax)
    {
        return Mathf.Lerp(destMin, destMax, Mathf.InverseLerp(sourceMin, sourceMax, sourceValue));
    }
}
