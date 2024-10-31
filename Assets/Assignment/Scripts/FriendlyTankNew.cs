using UnityEngine;

public class FriendlyTankNew : MonoBehaviour
{
    public float speed;
    public GameObject AI;
    private AITankAgent aiTankAgent;


    private const float posThreshold = -37f;

    void Start()
    {
        aiTankAgent = FindObjectOfType<AITankAgent>();
    }

    void Update()
    {
        transform.localPosition += transform.forward * Time.deltaTime * speed;

        if (transform.localPosition.z < posThreshold)
        {
            aiTankAgent.AddReward(2.0f);
            aiTankAgent.friendlyCount++;
            aiTankAgent.score += 2;
            Destroy(gameObject);
        }
    }

    public void Hit()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Tank")
        {
            aiTankAgent.AddReward(-1.0f);
            aiTankAgent.score -= 1;
            Destroy(gameObject);
        }
    }
}
