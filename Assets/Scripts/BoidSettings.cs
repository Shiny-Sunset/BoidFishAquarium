using UnityEngine;

[CreateAssetMenu(fileName = "BoidSettings", menuName = "Boids/Settings")]
public class BoidSettings : ScriptableObject
{
    [Header("Movement")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float maxSteerForce = 3f;

    [Header("Perception")]
    public float perceptionRadius = 2.5f;
    public float avoidanceRadius = 1f;

    [Header("Behavior Weights")]
    [Range(0, 10)] public float cohesionWeight = 1f;
    [Range(0, 10)] public float alignmentWeight = 1f;
    [Range(0, 10)] public float separationWeight = 1.5f;
    [Range(0, 10)] public float targetWeight = 1f;
    [Range(0, 10)] public float foodWeight = 2f;

    [Header("Bounds")]
    [Range(0, 10)] public float boundsWeight = 10f;
    public float boundsOffset = 1.5f;

    [Header("Obstacles")]
    public LayerMask obstacleLayer;
    [Range(0, 10)] public float obstacleWeight = 10f;

    [Header("Collisions")]
    public LayerMask obstacleMask;
    public float boundsRadius = .27f;
    public float avoidCollisionWeight = 10;
    public float collisionAvoidDst = 5;
}
