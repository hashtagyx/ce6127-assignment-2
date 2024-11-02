using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnityEngine.UIElements;

public class AITankAgent : Agent
{
    public float moveSpeed = 20f;
    public TankNewSpawn tankSpawner;
    private Rigidbody tankRigidbody;
    private LineRenderer lineRenderer;
    public int friendlyCount = 0;
    public int killCount = 0;
    public int score = 0;
    public Stopwatch stopwatch;

    private void Start()
    {
        // Ensure tankRigidbody is assigned at the start
        tankRigidbody = GetComponent<Rigidbody>();
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = Color.red;
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.positionCount = 2; // Set the number of points in the line
        lineRenderer.enabled = false;
    }

    // Called when the episode begins or resets
    public override void OnEpisodeBegin()
    {
        SetReward(0.0f);
        friendlyCount = 0;
        killCount = 0;
        score = 0;
        // Reset tank position and velocity
        tankRigidbody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 0, -35);
        transform.localRotation = Quaternion.identity;
        stopwatch = new Stopwatch(); // Initialize the stopwatch
        stopwatch.Start(); // Start the stopwatch
        if (tankSpawner != null)
        {
            tankSpawner.DestroyAllTanks();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // AI Tank position
        sensor.AddObservation(transform.localPosition);

        float closestEnemyX = 0f;  // Initialize with a default value
        float closestFriendlyX = 0f; // Initialize with a default value
        float closestEnemyZ = float.MaxValue;
        float closestFriendlyZ = float.MaxValue;
        Transform closestEnemy = null;
        Transform closestFriendly = null;



        foreach (Transform child in tankSpawner.transform)
        {
            float zVal = child.position.z;

            if (child.CompareTag("EnemyAI"))
            {
                if (zVal < closestEnemyZ)
                {
                    closestEnemyZ = zVal;
                    closestEnemyX = child.position.x;
                    closestEnemy = child;
                }
            }
            else if (child.CompareTag("Friendly"))
            {
                if (zVal < closestFriendlyZ)
                {
                    closestFriendlyZ = zVal;
                    closestFriendlyX = child.position.x;
                    closestFriendly = child;
                }
            }
        }

        sensor.AddObservation(closestEnemyX);
        sensor.AddObservation(closestFriendlyX);

        if (closestEnemy != null)
        {
            float zDiff = Mathf.Abs(transform.position.z - closestEnemy.position.z); // Calculate z-difference

            // Check both distance and z-difference thresholds
            if (zDiff <= 30f)
            {
                float xDiff = Mathf.Abs(transform.position.x - closestEnemy.position.x);
                float alignmentReward = 0.001f / (xDiff + 1f); // Avoid division by zero
                AddReward(alignmentReward);
            }
        }
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveInput = actionBuffers.DiscreteActions[0]; // 0 for left, 1 for right

        if (moveInput == 0)
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }
        else if (moveInput == 1)
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }

        if (friendlyCount >= 20 || killCount >= 40)
        {
            AddReward(20.0f);
            EndEpisode();
            return;
        }

        if (score >= 20)
        {

            AddReward(20.0f);
            EndEpisode();
            return;
        }

        // Reward given each step to encourage agent to not die for no reason.
        AddReward(0.001f);

        // Handle shooting action
        if (actionBuffers.DiscreteActions[1] == 1)
        {
            ShootRaycast();
        }

        if (transform.localPosition.y < -0.1f)
        {
            UnityEngine.Debug.Log("We are here for some reason, " + transform.localPosition.y);
            AddReward(-5.0f);
            EndEpisode();
            return;
        }

    }


    // Define manual player controls for debugging or testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActions[0] = 0; // Move Left
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActions[0] = 1; // Move Right
        }
        else
        {
            discreteActions[0] = 2; // Do Nothing
        }

        if (Input.GetKey(KeyCode.Space))
        {
            discreteActions[1] = 1;  // Shoot when space is pressed
        }
        else
        {
            discreteActions[1] = 0;
        }
    }

    // Shoot function using Raycast for instant bullet
    private void ShootRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        AddReward(-0.001f); // Penalize spamming of shooting

        // Draw the ray in the Scene view to visualize the bullet's path
        if (Physics.Raycast(ray, out hit, 30f))  // Max range of 30 units
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hit.point);
            lineRenderer.enabled = true; // Make sure it's enabled
            StartCoroutine(HideLineRendererAfterDelay(0.1f)); // Hide after 0.1 second
            if (hit.collider.CompareTag("EnemyAI"))
            {
                AddReward(5.0f);  // Reward for hitting an enemy
                Destroy(hit.collider.gameObject);  // Destroy enemy
                score += 2;

            }
            else if (hit.collider.CompareTag("Friendly"))
            {
                AddReward(-2.0f);  // Penalty for hitting a friendly
                Destroy(hit.collider.gameObject);  // Destroy friendly
                score -= 1;
            }
        }
        else
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + transform.forward * 30f);
            lineRenderer.enabled = true; // Make sure it's enabled
            StartCoroutine(HideLineRendererAfterDelay(0.1f)); // Hide after 0.1 second
        }
    }
    private IEnumerator HideLineRendererAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lineRenderer.enabled = false;
    }

}
