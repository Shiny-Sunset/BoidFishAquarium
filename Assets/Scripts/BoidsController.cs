using UnityEngine;
using System.Collections.Generic;

public class BoidsController : MonoBehaviour
{
    [Header("Boids Settings")]
    public BoidSettings settings;

    [Header("Setup")]
    public GameObject boidPrefab;
    public Transform waterTank;
    public int agentCount = 100;
    public float spawnRadius = 4.0f;

    [HideInInspector]
    public List<BoidAgent> agents = new List<BoidAgent>();

    void Start()
    {
        // 指定した数だけエージェント（魚）を生成する
        for (int i = 0; i < agentCount; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            GameObject newAgentObj = Instantiate(boidPrefab, randomPos, Quaternion.identity, transform);
            BoidAgent newAgent = newAgentObj.GetComponent<BoidAgent>();
            newAgent.Initialize(this, settings);
            agents.Add(newAgent);
        }
    }
}
