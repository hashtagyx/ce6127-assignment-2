using UnityEngine;
using TMPro;
using System.Diagnostics;

public class Scorecount : MonoBehaviour
{
    [SerializeField] private TMP_Text score;
    public GameObject AI;
    private AITankAgent AItank;
    // Start is called before the first frame update
    void Start()
    {
        AItank = AI.GetComponent<AITankAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        float elapsedTime = (float)AItank.stopwatch.ElapsedMilliseconds / 1000;

        score.text = $"Time Elapsed: {elapsedTime:F2}s\nScore: {AItank.score}\nCumulative Reward: {AItank.GetCumulativeReward().ToString()}";
    }
}
