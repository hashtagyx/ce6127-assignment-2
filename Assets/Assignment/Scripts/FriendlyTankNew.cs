using UnityEngine;

public class FriendlyTankNew : MonoBehaviour
{
    public float speed;
    public GameObject AI;
    private AITankAgent aiTankAgent;

    // private Rigidbody rbody;
    // private GameObject enemy;

    private const float posThreshold = -37f;

    void Start()
    {
    //     rbody = GetComponent<Rigidbody>();
    //     enemy = GetComponent<GameObject>();
        aiTankAgent = FindObjectOfType<AITankAgent>();
        // aiTankAgent = AI.GetComponent<AITankAgent>();
    }

    void Update()
    {
        transform.localPosition += transform.forward * Time.deltaTime * speed;

        if (transform.localPosition.z < posThreshold)
        {
            aiTankAgent.AddReward(2.0f);
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
            Destroy(gameObject);
        }
    }
}
