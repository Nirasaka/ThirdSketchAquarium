using Meta.XR.MRUtilityKit;
using System.Collections.Generic;
using UnityEngine;

// サイズのカテゴリ
public enum SizeCategory
{
    Small,
    Medium,
    Large
}

public class BoidsManager : MonoBehaviour
{
    public static BoidsManager Instance;

    // 魚のリスト
    private HashSet<FishAgent> allFish = new HashSet<FishAgent>();
    private Dictionary<SizeCategory, HashSet<FishAgent>> fishGroups;

    [Header("サイズカテゴリの閾値")]
    [Range(0.0f, 1.0f)]
    public float mediumThreshold;

    [Range(0.0f, 1.0f)]
    public float largeThreshold;

    [Header("Boidsの設定")]
    [Range(0.0f, 2.0f)]
    public float separationWeight;

    [Range(0.0f, 2.0f)]
    public float alignmentWeight;

    [Range(0.0f, 2.0f)]
    public float cohesionWeight;

    [Range(1.0f, 10.0f)]
    public float neighborDistance;

    // 検知範囲
    [Range(1.0f, 5.0f)]
    public float detectionRange;

    [Range(0.0f,5.0f)]
    public float maxSpeed;
    [Range(0.0f, 5.0f)]
    public float minSpeed;
    [Range(1.0f, 5.0f)]
    public float rotationSpeed;

    [Header("障害物回避の設定")]
    [Range(0.0f, 2.0f)]
    public float obstacleAvoidanceWeight;

    [Range(0.0f, 2.0f)]
    public float obstacleDetectionDistance;

    public Bounds roomLimit;

    [Header("ターゲット追従設定")]
    public Feeding feed;

    [Range(0.0f, 2.0f)]
    public float targetFollowWeight;

    private void Awake()
    {
        Instance = this;

        fishGroups = new Dictionary<SizeCategory, HashSet<FishAgent>>
        {
            {SizeCategory.Small,    new HashSet<FishAgent>() },
            {SizeCategory.Medium,   new HashSet<FishAgent>() },
            {SizeCategory.Large,    new HashSet<FishAgent>() }
        };
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MRUK.Instance.RoomCreatedEvent.AddListener(SetRoomBounds);
    }

    // 部屋の境界を設定
    void SetRoomBounds(MRUKRoom room)
    {
        room = MRUK.Instance.GetCurrentRoom();
        roomLimit = room.GetRoomBounds();
        roomLimit.size *= 0.9f;
    }

    // Update is called once per frame
    void Update()
    {
        if (allFish.Count > 0)
        {
            foreach (FishAgent fish in allFish)
            {
                Vector3 direction = Vector3.zero;

                // ランダムな速度を確率で与える
                if(Random.Range(0, 100) < 10)
                {
                    fish.speed = Random.Range(minSpeed, maxSpeed);
                }

                // 部屋の大枠から外れたら戻す
                if (!roomLimit.Contains(fish.transform.position))
                {
                    direction = roomLimit.center - fish.transform.position;
                }
                // 部屋の中にいる場合は各カテゴリーごとに泳がせる
                else
                {
                    // 小さな魚は群れる
                    if (fish.Category == SizeCategory.Small)
                    {
                        if (Random.Range(0, 100) < 20)
                            direction = CalcBoid(fish);
                    }
                }

                // 餌があればそちらに向かう
                if (feed.isHandDetected)
                {
                    direction += TowardFeed(fish);
                }
                
                // 進行方向が定まっていれば滑らかに回転
                if (direction != Vector3.zero)
                {
                    fish.transform.rotation = Quaternion.Slerp(fish.transform.rotation,
                                            Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
                }
                // 現在の速度で前進
                fish.transform.Translate(0, 0, fish.speed * Time.deltaTime);
            }
        }
    }

    // Boidsの計算
    Vector3 CalcBoid(FishAgent fish)
    {
        Vector3 vCenter = Vector3.zero;
        Vector3 vAvoid = Vector3.zero;
        float gSpeed = 0.01f;
        Vector3 direction = Vector3.zero;
        int neighborCount = 0;

        foreach (FishAgent otherFish in fishGroups[fish.Category])
        {
            if(fish == otherFish) continue;

            float distance = Vector3.Distance(fish.transform.position, otherFish.transform.position);
            if(distance <= detectionRange)
            {
                vCenter += otherFish.transform.position;
                neighborCount++;

                if(distance < neighborDistance)
                {
                    vAvoid += (fish.transform.position - otherFish.transform.position);
                }
                gSpeed += otherFish.speed;
            }
        }

        if (neighborCount > 0)
        {
            vCenter = vCenter / neighborCount * cohesionWeight;
            vAvoid *= separationWeight;
            fish.speed = gSpeed / neighborCount;

            if(fish.speed > maxSpeed)
            {
                fish.speed = maxSpeed;
            }

            direction = ((vCenter + vAvoid) - fish.transform.position) * alignmentWeight;
        }
        return direction;
    }

    // 餌に近づく
    Vector3 TowardFeed(FishAgent fish)
    {

        Vector3 direction = Vector3.zero;
        Vector3 vAvoid = Vector3.zero;
        float neighborCount = 0;

        foreach(FishAgent otherFish in allFish)
        {
            if(fish == otherFish) continue;

            float distance = Vector3.Distance(fish.transform.position, otherFish.transform.position);
            if (distance <= detectionRange)
            {
                neighborCount++;
                if (distance < neighborDistance)
                {
                    vAvoid += (fish.transform.position - otherFish.transform.position);
                }
            }
        }



        Vector3 feedDirection = feed.transform.position - fish.transform.position;
        if (feedDirection.magnitude <= detectionRange)
            direction = (vAvoid * separationWeight - fish.transform.position) + feedDirection * targetFollowWeight;

        return direction;
    }

    // 魚の追加
    public void RegistFish(FishAgent fish)
    {
        allFish.Add(fish);
        fishGroups[fish.Category].Add(fish);
        fish.speed = Random.Range(minSpeed, maxSpeed);
    }

    // 魚をリストから削除
    public void UnregistFish(FishAgent fish)
    {
        allFish.Remove(fish);
        fishGroups[fish.Category].Remove(fish);
    }

    // 魚を消す
    public void DeleteFish(FishAgent fish)
    {
        UnregistFish(fish);
        Destroy(fish.gameObject);
    }

    // 全ての魚を消す
    public void DeleteAllFish()
    {
        foreach(FishAgent fish in allFish)
        {
            DeleteFish(fish);
        }
    }


    // カテゴリを変更
    public void ChangeCategory(FishAgent fish, SizeCategory oldCategory, SizeCategory newCategory)
    {
        if(oldCategory == newCategory) return;

        fishGroups[oldCategory].Remove(fish);
        fishGroups[newCategory].Add(fish);
        Debug.Log(fish.gameObject.name + ":" + fish.Category);
    }

    // 指定のカテゴリの魚群を取得
    public HashSet<FishAgent> GetGroup(SizeCategory category)
    {
        return fishGroups[category];
    }

    // サイズからカテゴリを選択
    public SizeCategory GetCategory(float size)
    {
        if (size < mediumThreshold)
            return SizeCategory.Small;
        if(size < largeThreshold) 
            return SizeCategory.Medium;
        return SizeCategory.Large;
    }

    public int CountFish()
    {
        return allFish.Count;
    }
}
