using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class AITankAgent : Agent
{
    public float moveSpeed = 20f;
    private Rigidbody tankRigidbody;

    private void Start()
    {
        // Ensure tankRigidbody is assigned at the start
        tankRigidbody = GetComponent<Rigidbody>();
    }

    // Called when the episode begins or resets
    public override void OnEpisodeBegin()
    {
        // Reset tank position and velocity
        tankRigidbody.velocity = Vector3.zero;
        // transform.localPosition = new Vector3(0, 0, -35);

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // AI Tank position
        sensor.AddObservation(transform.localPosition);

        // Detect nearby enemy and friendly tanks
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 30f);  // Adjust the radius as needed
        foreach (var obj in nearbyObjects)
        {
            if (obj.CompareTag("EnemyAI"))
            {
                sensor.AddObservation(obj.transform.localPosition); // Add enemy position
            }
            else if (obj.CompareTag("Friendly"))
            {
                sensor.AddObservation(obj.transform.localPosition); // Add friendly position
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
        }
    }

    // Shoot function using Raycast for instant bullet
    private void ShootRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        // Draw the ray in the Scene view to visualize the bullet's path
        Debug.DrawRay(ray.origin, ray.direction * 30f, Color.red, 1.0f); 
        if (Physics.Raycast(ray, out hit, 30f))  // Max range of 30 units
        {
            if (hit.collider.CompareTag("EnemyAI"))
            {
                SetReward(2.0f);  // Reward for hitting an enemy
                Destroy(hit.collider.gameObject);  // Destroy enemy
            }
            else if (hit.collider.CompareTag("Friendly"))
            {
                SetReward(-1.0f);  // Penalty for hitting a friendly
                Destroy(hit.collider.gameObject);  // Destroy friendly
            }
        }
    }
}
