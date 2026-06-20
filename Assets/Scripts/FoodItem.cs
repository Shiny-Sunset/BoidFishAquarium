using UnityEngine;

public class FoodItem : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 衝突したオブジェクトがBoidであるかを確認
        // Boidスクリプトがアタッチされているオブジェクトと衝突した場合
        Boid boid = other.GetComponent<Boid>();
        if (boid != null)
        {
            // BoidManagerからこの餌の登録を解除
            if (GameManager.Instance != null && GameManager.Instance.boidManager != null)
            {
                GameManager.Instance.boidManager.RemoveFood(this.gameObject);
            }
            // 餌オブジェクトを削除
            Destroy(this.gameObject);
        }
    }
}