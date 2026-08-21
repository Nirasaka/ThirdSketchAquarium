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
    private List<FishAgent> allFish = new List<FishAgent>();
    private Dictionary<SizeCategory, List<FishAgent>> fishGroups;

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

    [Range(0.0f, 10.0f)]
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
    public Feeding Feeding;

    public GameObject target;

    [Range(0.0f, 2.0f)]
    public float targetFollowWeight;

    public float feed_distance = 1.0f;

    private void Awake()
    {
        Instance = this;

        fishGroups = new Dictionary<SizeCategory, List<FishAgent>>
        {
            {SizeCategory.Small,    new List<FishAgent>() },
            {SizeCategory.Medium,   new List<FishAgent>() },
            {SizeCategory.Large,    new List<FishAgent>() }
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
        if (allFish.Count <= 0) return;

        foreach (FishAgent fish in allFish)
        {
            Vector3 direction = Vector3.zero;

            // ランダムな速度を確率で与える
            if(Random.Range(0, 100) < 5)
            {
                fish.SetSpeed(Random.Range(minSpeed, maxSpeed));
            }

            // 部屋の大枠から外れたら戻す
            if (!roomLimit.Contains(fish.transform.position))
            {
                direction = roomLimit.center - fish.transform.position;
            }
            // 部屋の中にいる場合は各カテゴリーごとに泳がせる
            else
            {
                // 小さな魚
                if (fish.Category == SizeCategory.Small)
                {
                    // そこそこ群れる
                    if (Random.Range(0, 100) < 40)
                        direction = CalcBoid(fish);

                    // 中くらいの魚から逃げる
                    if(Random.Range(0,100) < 30 && !Feeding.isHandDetected)
                        direction = AwayFromFishInCategory(fish, SizeCategory.Medium);
                }

                // 中くらいの魚
                if(fish.Category == SizeCategory.Medium)
                {
                    // 少し群れる
                    if(Random.Range(0,100) < 20)
                        direction = CalcBoid(fish);

                    // 小さい魚を襲う
                    if(Random.Range(0,100) < 50 && !Feeding.isHandDetected)
                        direction += TowardFishInCategory(fish, SizeCategory.Small);
                }

                // 大きな魚
                if(fish.Category == SizeCategory.Large)
                {
                    if(Random.Range(0,100) < 10)
                        direction = CalcBoid(fish);
                }
            }

            // 餌があればそちらに向かう
            if (Feeding.isHandDetected)
            {
                direction += TowardFeed(fish);
            }

            // カメラに近すぎる場合は避ける
            if(Vector3.Distance(fish.transform.position, Camera.main.transform.position) <= 0.5)
            {
                direction += (fish.transform.position - Camera.main.transform.position);
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

    // Boidsの計算
    Vector3 CalcBoid(FishAgent fish)
    {
        Vector3 vCenter = Vector3.zero;
        Vector3 vAvoid = Vector3.zero;
        float gSpeed = 0.01f;
        Vector3 direction = Vector3.zero;
        int neighborCount = 0;

        // fishと同じカテゴリの魚のみ参照される
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
            fish.SetSpeed(gSpeed / neighborCount);

            if(fish.speed > maxSpeed)
            {
                fish.SetSpeed(maxSpeed);
            }

            direction = ((vCenter + vAvoid) - fish.transform.position) * alignmentWeight;
        }
        return direction;
    }

    // 餌に近づく
    Vector3 TowardFeed(FishAgent fish)
    {

        Vector3 direction = Vector3.zero;

        Vector3 feedDirection = target.transform.position - fish.transform.position;
        if (feedDirection.magnitude >= feed_distance)
            direction = feedDirection * targetFollowWeight;

        return direction;
    }

    // 特定のカテゴリから逃げる
    Vector3 AwayFromFishInCategory(FishAgent fish, SizeCategory category)
    {
        Vector3 direction = Vector3.zero;

        foreach (FishAgent targetFish in fishGroups[category])
        {
            Vector3 awayDirection = (fish.transform.position - targetFish.transform.position);
            if (awayDirection.magnitude <= detectionRange)
            {
                direction += awayDirection * (1.0f - awayDirection.magnitude / detectionRange);
                fish.SetSpeed(Random.Range(minSpeed + 0.7f, maxSpeed + 0.7f));
            }
        }

        return direction;
    }

    // 特定のカテゴリの魚を追う
    Vector3 TowardFishInCategory(FishAgent fish, SizeCategory category)
    {
        Vector3 direction = Vector3.zero;

        foreach(FishAgent targetFish in fishGroups[category])
        {
            Vector3 targetDirection = (targetFish.transform.position - fish.transform.position);
            if(targetDirection.magnitude <= detectionRange)
            {
                direction += targetDirection * targetFollowWeight;
                fish.SetSpeed(Random.Range(minSpeed+0.5f, maxSpeed+0.5f));
            }
        }

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
        //foreach(FishAgent fish in allFish)
        //{
        //    DeleteFish(fish);
        //}

        int i = allFish.Count - 1;

        for(;  i >= 0; i--)
        {
            DeleteFish(allFish[i]);
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
    public List<FishAgent> GetGroup(SizeCategory category)
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
