using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

public class Feeding : MonoBehaviour
{
    public GameObject feedPositionObj;
    public GameObject spherePrefab; // 表示させる球のプレハブ
    public int numberOfSpheres = 5; // 表示する球の数
    public float radius = 0.05f; // 手の上に球を配置する範囲（手の平の広さに合わせる）
    public float heightOffset = -0.03f; // 球が手の平からどの程度浮いているか

    public Vector3 feedPosition;
    private List<GameObject> spheres = new List<GameObject>();
    private List<Vector3> sphereOffsets = new List<Vector3>(); // 各球のオフセット（相対位置）

    float eatspeed = 0.0f;
    private float eatInterval = 4.2f;
    private float nextEatTime = 0.0f;

    // 手のトラッキング用
    private OVRHand rightHand;
    private OVRSkeleton rightSkeleton;
    private Transform palmTransform;
    public bool isHandDetected = false;
    private Vector3 rightHandPosition;
    private Vector3 rightHandRotation;
    private Vector3 rightHandNormal;

    public BoidsManager BM;

    void Start()
    {

    }


    void Update()
    {
        feedPosition = feedPositionObj.transform.position;

        if (sphereOffsets.Count != 0)
        {
            int i;
            for (i = 0; i < spheres.Count; i++)
            {
                // 手の平の動きに応じて球の位置を更新
                spheres[i].transform.position = feedPosition + sphereOffsets[i];
            }

            // 餌を食べる
            if (BM.CountFish() > 0 && Time.time > nextEatTime)
            {
                if (spheres.Count > 1)
                {
                    Destroy(spheres[i - 1]);
                    spheres.RemoveAt(i - 1);
                    eatspeed = eatInterval - BM.CountFish() * 0.1f;
                    if (eatspeed < 0.5) eatspeed = 0.5f;
                    nextEatTime = Time.time + eatspeed;
                }
                else
                {
                    RemoveSpheres();
                }
            }
        }
    }


    // 球を表示する関数
    public void DisplaySpheres()
    {
        isHandDetected = true;
        eatspeed = eatInterval - BM.CountFish() * 0.1f;
        if (eatspeed < 0.5) eatspeed = 0.5f;
        nextEatTime = Time.time + eatspeed;

        if (spheres.Count == 0) // まだ球が生成されていない場合
        {
            for (int i = 0; i < numberOfSpheres; i++)
            {
                // 手の平の範囲内にランダムに球を配置
                Vector3 randomOffset = new Vector3(
                    Random.Range(-radius, radius),  
                    0.05f,
                    Random.Range(-radius, radius)   
                    );                              

                Vector3 offset = randomOffset;
                GameObject sphere = Instantiate(spherePrefab, feedPosition + offset, Quaternion.identity);
                spheres.Add(sphere);
                sphereOffsets.Add(offset); // オフセットを保存
            }
        }
    }

    // 球を削除する関数
    public void RemoveSpheres()
    {
        isHandDetected = false; 

        foreach (GameObject sphere in spheres)
        {
            Destroy(sphere);
        }
        spheres.Clear();
        sphereOffsets.Clear();
    }
}
