using UnityEngine;
using System.Collections.Generic;

public class BoidsManager : MonoBehaviour
{
    // 魚のリスト
    private List<FishAgent> fishAgents = new List<FishAgent>();

    [Header("Boidsの設定")]
    public float separationWeight = 1.5f;

    public float alignmentWeight = 1.0f;

    public float cohesionWeight = 1.0f;

    public float neighborDistance = 1.0f;

    public float maxSteerForce = 1.0f;


    [Header("障害物回避の設定")]
    public float obstacleAvoidanceWeight = 10.0f;

    public float obstacleDetectionDistance = 1.0f;


    [Header("ランダムウォーク設定")]
    public float wanderWeight = 0.5f;

    [Header("ターゲット追従設定")]
    public GameObject target;

    public float targetFollowWeight = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 初めから配置されている魚を取得
        foreach (FishAgent fish in FindObjectsByType<FishAgent>(FindObjectsSortMode.None))
        {
            RegistFish(fish);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Boids計算
        foreach (FishAgent fish in fishAgents)
        {
            fish.velocity += CalcBoids(fish);
        }

        // 移動処理
        foreach(FishAgent fish in fishAgents)
        {
            fish.MoveAgent(Time.deltaTime);
        }
    }

    // Boids計算
    Vector3 CalcBoids(FishAgent fish)
    {
        Vector3 directionToTarget = Vector3.zero;
        Vector3 separation = Vector3.zero;          // 分離成分
        Vector3 alignment = Vector3.zero;           // 整列成分
        Vector3 cohesion = Vector3.zero;            // 結合成分
        Vector3 obstacleAvoidance = Vector3.zero;   // 障害物回避成分
        Vector3 wander = Vector3.zero;               // ランダムウォーク成分

        int neighborCount = 0;                      // 近くの魚の数
        Vector3 averagePosition = Vector3.zero;     // 群れの平均位置
        Vector3 averageHeading = Vector3.zero;      // 群れの平均進行方向

        // 追いかける対象があれば
        if(target != null)
        {
            Vector3 offset = target.transform.position - fish.transform.position;
            if(offset.magnitude > fish.detectionRange)
            {
                directionToTarget = SteerTowards(fish, offset);
            }
        }

        // 近くの魚を検出 & 分離成分の算出
        foreach (FishAgent otherFish in fishAgents)
        {
            // 自身を除く
            if (otherFish == fish) continue;

            float distance = Vector3.Distance(fish.transform.position, otherFish.transform.position);

            // 検知範囲でなければ無視
            if (distance < fish.detectionRange) continue;

            if (distance < neighborDistance)
            {
                // 閾値より近いなら分離
                separation += SteerTowards(fish, (fish.transform.position - otherFish.transform.position) / distance);
            }

            // 整列と結合のための計算
            neighborCount++;
            averageHeading += SteerTowards(fish, otherFish.velocity.normalized);
            averagePosition += SteerTowards(fish, otherFish.transform.position);

        }

        if (neighborCount > 0)
        {
            averagePosition /= neighborCount;
            averageHeading /= neighborCount;

            // 平均進行方向から整列成分を算出
            alignment = averageHeading;

            // 平均位置から結合成分を算出
            cohesion = (averagePosition - fish.transform.position).normalized;
        }
        // 周囲に仲間がいない場合，放浪する
        else
        {
            Vector3 wanderDirection = (fish.velocity.normalized + Random.onUnitSphere * 0.2f).normalized;
            wander = SteerTowards(fish, wanderDirection);
        }


        //// 前方に障害物があれば回避
        //RaycastHit hit;
        //if (Physics.Raycast(fish.transform.position, fish.transform.forward, out hit, obstacleDetectionDistance))
        //{
        //    obstacleAvoidance = SteerTowards(fish, Vector3.Reflect(fish.transform.forward, hit.normal).normalized);
        //}

        // 全ての成分を合成
        Vector3 acceleration = directionToTarget * targetFollowWeight +
                               separation * separationWeight +
                               alignment * alignmentWeight +
                               cohesion * cohesionWeight +
                               wander * wanderWeight +
                               obstacleAvoidance * obstacleAvoidanceWeight;

        Vector3 velocity = acceleration * Time.deltaTime;

        // 速度を制限
        float speed = Mathf.Clamp(velocity.magnitude, fish.minSpeed, fish.maxSpeed);

        velocity = velocity.normalized * speed;

        return velocity;
    }

    // 回転制限
    private Vector3 SteerTowards(FishAgent fish, Vector3 targetDirection)
    {
        if(targetDirection == Vector3.zero) return Vector3.zero;

        Vector3 steer = targetDirection.normalized * fish.maxSpeed - fish.velocity;
        return Vector3.ClampMagnitude(steer, maxSteerForce);
    }


    // 魚の追加
    public void RegistFish(FishAgent fish)
    {
        if (!fishAgents.Contains(fish))
        {
            fishAgents.Add(fish);
            fish.velocity = fish.transform.forward * Random.Range(fish.minSpeed, fish.maxSpeed);
            //fish.velocity = Random.onUnitSphere * Random.Range(fish.minSpeed, fish.maxSpeed);
        }
    }
}
