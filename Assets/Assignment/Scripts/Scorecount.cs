using UnityEngine;
using TMPro;

public class Scorecount : MonoBehaviour
{
    [SerializeField] private TMP_Text score;
    public GameObject AI;
    private AITankAgent AItank; 
    // Start is called before the first frame update
    void Start()
    {
        AItank = AI.GetComponent<AITankAgent>();
        // AITankAgent AItank = FindObjectOfType<AITankAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        score.text = AItank.GetCumulativeReward().ToString();
    }
}
