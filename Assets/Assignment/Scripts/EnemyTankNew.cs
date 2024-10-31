using UnityEngine;

public class EnemyTankNew : MonoBehaviour
{
    public float speed;
    public GameObject AI;
    private AITankAgent aiTankAgent;

    private Rigidbody rbody;

    private const float posThreshold = -37f;

    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        aiTankAgent = FindObjectOfType<AITankAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveVect = transform.forward * speed * Time.deltaTime;
        rbody.MovePosition(rbody.position + moveVect);

        if (transform.localPosition.z < posThreshold)
        {
            aiTankAgent.AddReward(-1.0f);
            aiTankAgent.score -= 1;
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
            aiTankAgent.AddReward(-5.0f);
            aiTankAgent.EndEpisode();
            Destroy(gameObject);
        }
    }
}
