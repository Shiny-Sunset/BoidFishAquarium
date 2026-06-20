using UnityEngine;

public class BoidAgent : MonoBehaviour
{
    private BoidsController controller;
    private BoidSettings settings;

    // 現在の速度
    private Vector3 currentVelocity;

    public void Initialize(BoidsController controller, BoidSettings settings)
    {
        this.controller = controller;
        this.settings = settings;

        // 初期の速度をランダムに設定
        float speed = (settings.minSpeed + settings.maxSpeed) / 2f;
        currentVelocity = transform.forward * speed;
    }

    void Update()
    {
        if (controller == null) return;

        Vector3 acceleration = Vector3.zero;

        // --- Boidsの3原則を計算 ---
        Vector3 cohesion = CalculateCohesion();
        Vector3 alignment = CalculateAlignment();
        Vector3 separation = CalculateSeparation();

        // --- 境界から出ないようにする力を計算 ---
        Vector3 bounds = CalculateBounds();

        // 各ルールに重みを付けて合算
        acceleration += cohesion * settings.cohesionWeight;
        acceleration += alignment * settings.alignmentWeight;
        acceleration += separation * settings.separationWeight;
        acceleration += bounds; // 境界の力は直接加算

        // 速度を更新
        currentVelocity += acceleration * Time.deltaTime;

        // 速度を制限
        float speed = currentVelocity.magnitude;
        Vector3 direction = currentVelocity.normalized;
        speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
        currentVelocity = direction * speed;

        // 進行方向を向く
        if (currentVelocity != Vector3.zero)
        {
            transform.forward = currentVelocity;
        }

        // 位置を更新
        transform.position += currentVelocity * Time.deltaTime;
    }

    // 周りの仲間の中心に向かう力（結合）
    Vector3 CalculateCohesion()
    {
        Vector3 centerOfMass = Vector3.zero;
        int count = 0;
        foreach (var other in controller.agents)
        {
            if (other != this)
            {
                float dist = Vector3.Distance(transform.position, other.transform.position);
                if (dist > 0 && dist < settings.perceptionRadius)
                {
                    centerOfMass += other.transform.position;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            centerOfMass /= count;
            return (centerOfMass - transform.position).normalized;
        }
        return Vector3.zero;
    }

    // 周りの仲間と平均速度を合わせる力（整列）
    Vector3 CalculateAlignment()
    {
        Vector3 averageVelocity = Vector3.zero;
        int count = 0;
        foreach (var other in controller.agents)
        {
            if (other != this)
            {
                float dist = Vector3.Distance(transform.position, other.transform.position);
                if (dist > 0 && dist < settings.perceptionRadius)
                {
                    averageVelocity += other.currentVelocity;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            averageVelocity /= count;
            return (averageVelocity - currentVelocity).normalized;
        }
        return Vector3.zero;
    }

    // 周りの仲間とぶつからないように離れる力（分離）
    Vector3 CalculateSeparation()
    {
        Vector3 separationMove = Vector3.zero;
        foreach (var other in controller.agents)
        {
            if (other != this)
            {
                float dist = Vector3.Distance(transform.position, other.transform.position);
                if (dist > 0 && dist < settings.avoidanceRadius)
                {
                    separationMove += (transform.position - other.transform.position) / (dist * dist);
                }
            }
        }
        return separationMove.normalized;
    }

    // 境界から出ないようにする力
    Vector3 CalculateBounds()
    {
        Vector3 force = Vector3.zero;
        if (controller.waterTank == null) return force;

        Transform tank = controller.waterTank;
        Vector3 center = tank.position;
        Vector3 halfSize = tank.localScale / 2.0f;

        // 境界に近づきすぎたら、中心に向かう力を発生させる
        if (Mathf.Abs(transform.position.x - center.x) > halfSize.x) {
            force.x = (center.x - transform.position.x);
        }
        if (Mathf.Abs(transform.position.y - center.y) > halfSize.y) {
            force.y = (center.y - transform.position.y);
        }
        if (Mathf.Abs(transform.position.z - center.z) > halfSize.z) {
            force.z = (center.z - transform.position.z);
        }

        return force.normalized;
    }
}
