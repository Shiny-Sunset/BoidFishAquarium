// This code is based on Boids by Sebastian Lague
// Copyright (c) 2019 Sebastian Lague
// Released under the MIT license

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {

    public BoidSettings settings;
    [Tooltip("旧GPU実装用。WebGL は Compute Shader 非対応のため現在は未使用（近傍集計は CPU で実行）。")]
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
        if (boids == null) return;

        int numBoids = boids.Length;

        // --- 近傍集計を CPU で実行 ---
        // WebGL は Compute Shader 非対応のため、BoidCompute.computeを
        // CPU の総当たり O(N^2) 集計へ置き換え。i<j の対称ループで計算量を半減している。
        // 各 Boid に渡す集計値（合計の向き・重心・分離ベクトル・仲間数）の意味は GPU 版と同一。
        float sqrViewRadius  = settings.perceptionRadius * settings.perceptionRadius;
        float sqrAvoidRadius = settings.avoidanceRadius  * settings.avoidanceRadius;

        // 集計値をリセット
        for (int i = 0; i < numBoids; i++) {
            boids[i].numPerceivedFlockmates = 0;
            boids[i].avgFlockHeading        = Vector3.zero;
            boids[i].centreOfFlockmates     = Vector3.zero;
            boids[i].avgAvoidanceHeading    = Vector3.zero;
        }

        // 総当たりで近傍を集計（sqrDst のまま比較して √ を省略）
        for (int i = 0; i < numBoids; i++) {
            for (int j = i + 1; j < numBoids; j++) {
                Vector3 offset = boids[j].position - boids[i].position; // j - i
                float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                if (sqrDst < sqrViewRadius) {
                    // 視界内：互いを仲間として集計（対称）
                    boids[i].numPerceivedFlockmates += 1;
                    boids[i].avgFlockHeading    += boids[j].forward;
                    boids[i].centreOfFlockmates += boids[j].position;

                    boids[j].numPerceivedFlockmates += 1;
                    boids[j].avgFlockHeading    += boids[i].forward;
                    boids[j].centreOfFlockmates += boids[i].position;

                    if (sqrDst < sqrAvoidRadius) {
                        // 分離：逆二乗で反発。offset=(j-i) なので i は -offset、j は +offset で互いに離れる
                        Vector3 sep = offset / sqrDst;
                        boids[i].avgAvoidanceHeading -= sep;
                        boids[j].avgAvoidanceHeading += sep;
                    }
                }
            }
        }

        // --- 各個体のステアリング更新 ---
        for (int i = 0; i < numBoids; i++) {
            // 最寄りの餌を検索
            Vector3 closestFoodPos = Vector3.zero;
            float minFoodDist = float.MaxValue;
            foreach (GameObject food in foodObjects) {
                if (food == null) continue; // 破棄済みはスキップ
                float dist = Vector3.Distance(boids[i].position, food.transform.position);
                if (dist < minFoodDist) {
                    minFoodDist = dist;
                    closestFoodPos = food.transform.position;
                }
            }
            boids[i].closestFood = closestFoodPos;

            boids[i].UpdateBoid ();
        }
    }
}