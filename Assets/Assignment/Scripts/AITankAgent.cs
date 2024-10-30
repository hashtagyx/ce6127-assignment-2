using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections;

public class AITankAgent : Agent
{
    public float moveSpeed = 20f;
    public TankNewSpawn tankSpawner;
    private Rigidbody tankRigidbody;
    private LineRenderer lineRenderer;

    private void Start()
    {
        // Ensure tankRigidbody is assigned at the start
        tankRigidbody = GetComponent<Rigidbody>();
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = Color.red;
        lineRenderer.widthMultiplier = 0.1f; // Adjust thickness as needed
        lineRenderer.positionCount = 2; // Set the number of points in the line
        lineRenderer.enabled = false;
    }

    // Called when the episode begins or resets
    public override void OnEpisodeBegin()
    {
        SetReward(0.0f);
        // Reset tank position and velocity
        tankRigidbody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 0, -35);
        if (tankSpawner != null)
        {
            tankSpawner.DestroyAllTanks();
        }

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // AI Tank position
        sensor.AddObservation(transform.localPosition);

        // Detect nearby enemy and friendly tanks (using transform)
        foreach (Transform child in tankSpawner.transform) // Iterate through children of TankNewSpawn
        {
            if (child.CompareTag("EnemyAI"))
            {
                sensor.AddObservation(child.localPosition); // Add enemy position
            }
            else if (child.CompareTag("Friendly"))
            {
                sensor.AddObservation(child.localPosition); // Add friendly position
            }
        }
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float moveInput = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1, 1);  // Movement left/right, limited to -1 to 1

        // Move the tank horizontally within speed cap
        Vector3 move = new Vector3(moveInput * moveSpeed * Time.deltaTime, 0, 0);
        transform.Translate(move);


        // Handle shooting action
        if (actionBuffers.DiscreteActions[0] == 1)
        {
            ShootRaycast();
        }

        if (transform.localPosition.x < -30.0f | transform.localPosition.x > 30.0f)
        {
            AddReward(-20.0f);
            EndEpisode();
        }

        if (GetCumulativeReward() >= 20f)
        {
            EndEpisode();
        }
    }


    // Define manual player controls for debugging or testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Debug.Log("Heuristic method called");
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");

        var discreteActions = actionsOut.DiscreteActions;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            discreteActions[0] = 1;  // Shoot when space is pressed
        } else {
            discreteActions[0] = 0;
        }
    }

    // Shoot function using Raycast for instant bullet
    private void ShootRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        // Draw the ray in the Scene view to visualize the bullet's path
        if (Physics.Raycast(ray, out hit, 30f))  // Max range of 30 units
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hit.point);
            lineRenderer.enabled = true; // Make sure it's enabled
            StartCoroutine(HideLineRendererAfterDelay(0.1f)); // Hide after 0.1 second
            if (hit.collider.CompareTag("EnemyAI"))
            {
                AddReward(2.0f);  // Reward for hitting an enemy
                Destroy(hit.collider.gameObject);  // Destroy enemy

            }
            else if (hit.collider.CompareTag("Friendly"))
            {
                AddReward(-1.0f);  // Penalty for hitting a friendly
                Destroy(hit.collider.gameObject);  // Destroy friendly
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
