using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public enum GizmoType { Never, SelectedOnly, Always }

    public Boid prefab;
    public float spawnRadius = 10;
    public GizmoType showSpawnRegion;

    public List<Boid> SpawnBoids (int count, Boid prefabToSpawn, Color colourToSet, BoidSettings settings) {
        List<Boid> spawnedBoids = new List<Boid>();
        for (int i = 0; i < count; i++) {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Boid boid = Instantiate (prefabToSpawn);
            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere;

            boid.SetColour (colourToSet);
            boid.Initialize(settings, null); // Initialize boid with settings
            spawnedBoids.Add(boid);
        }
        return spawnedBoids;
    }

    private void OnDrawGizmos () {
        if (showSpawnRegion == GizmoType.Always) {
            DrawGizmos ();
        }
    }

    void OnDrawGizmosSelected () {
        if (showSpawnRegion == GizmoType.SelectedOnly) {
            DrawGizmos ();
        }
    }

    void DrawGizmos () {

        Gizmos.color = new Color (0, 1, 0, 0.3f); // Green color for gizmo
        Gizmos.DrawSphere (transform.position, spawnRadius);
    }

}