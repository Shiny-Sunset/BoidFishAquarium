using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {

    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    Boid[] boids;
    List<GameObject> foodObjects;

    void Awake() {
        foodObjects = new List<GameObject>();
    }

    void Start () {
        // Boids are now set by GameManager via SetBoids method
    }

    public void SetBoids(List<Boid> newBoids) {
        boids = newBoids.ToArray();
    }

    public void ScareBoidsAt(Vector3 position, float radius, float force) {
        if (boids == null) return;

        foreach (Boid boid in boids) {
            float dist = Vector3.Distance(boid.position, position);
            if (dist < radius) {
                Vector3 dir = (boid.position - position).normalized;
                // Apply force to boid's velocity or acceleration
                // This is a simplified example, you might want to adjust how force is applied
                boid.velocity += dir * force * (1 - dist / radius);
            }
        }
    }

    public void RegisterFood(GameObject food) {
        foodObjects.Add(food);
    }

    public void RemoveFood(GameObject food) {
        foodObjects.Remove(food);
    }

    public void ClearBoids() {
        if (boids != null) {
            foreach (Boid boid in boids) {
                if (boid != null) {
                    Destroy(boid.gameObject);
                }
            }
            boids = null;
        }
    }

    void Update () {
        if (boids != null) {

            int numBoids = boids.Length;
            var boidData = new BoidData[numBoids];

            for (int i = 0; i < boids.Length; i++) {
                boidData[i].position = boids[i].position;
                boidData[i].direction = boids[i].forward;
            }

            var boidBuffer = new ComputeBuffer (numBoids, BoidData.Size);
            boidBuffer.SetData (boidData);

            compute.SetBuffer (0, "boids", boidBuffer);
            compute.SetInt ("numBoids", boids.Length);
            compute.SetFloat ("viewRadius", settings.perceptionRadius);
            compute.SetFloat ("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt (numBoids / (float) threadGroupSize);
            compute.Dispatch (0, threadGroups, 1, 1);

            boidBuffer.GetData (boidData);

            for (int i = 0; i < boids.Length; i++) {
                boids[i].avgFlockHeading = boidData[i].flockHeading;
                boids[i].centreOfFlockmates = boidData[i].flockCentre;
                boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
                boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;

                // Find closest food
                Vector3 closestFoodPos = Vector3.zero;
                float minFoodDist = float.MaxValue;
                foreach (GameObject food in foodObjects) {
                    if (food == null) continue; // Skip if food object is destroyed
                    float dist = Vector3.Distance(boids[i].position, food.transform.position);
                    if (dist < minFoodDist) {
                        minFoodDist = dist;
                        closestFoodPos = food.transform.position;
                    }
                }
                boids[i].closestFood = closestFoodPos;

                boids[i].UpdateBoid ();
            }

            boidBuffer.Release ();
        }
    }

    public struct BoidData {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size {
            get {
                return sizeof (float) * 3 * 5 + sizeof (int);
            }
        }
    }
}