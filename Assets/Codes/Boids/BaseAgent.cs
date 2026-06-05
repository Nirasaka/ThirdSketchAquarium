using UnityEngine;

public abstract class BaseAgent : MonoBehaviour
{
    // 速度ベクトル
    public Vector3 velocity;
    // 最大・最低速度
    public float maxSpeed = 3f;
    public float minSpeed = 1f;
    // 検知範囲
    public float detectionRange = 2f;

    private void Start()
    {
    }

    public void MoveAgent(float deltaTime)
    {
        if (velocity.sqrMagnitude > 0)
        {
            transform.forward = velocity.normalized;
            transform.position += velocity * deltaTime;
        }
    }
}
