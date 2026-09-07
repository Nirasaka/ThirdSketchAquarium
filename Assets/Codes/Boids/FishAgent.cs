using Oculus.Interaction;
using Unity.VisualScripting;
using UnityEngine;

public class FishAgent : MonoBehaviour
{
    public SizeCategory Category = SizeCategory.Small;
    public Animator animator;

    private Grabbable gb;
    private bool isTwoGrabbed;

    // 速度ベクトル
    public float speed;

    private void Start()
    {
        Categorize();

        BoidsManager.Instance.RegistFish(this);

        animator = GetComponent<Animator>();

        gb = GetComponent<Grabbable>(); 
    }

    void Update()
    {
        if (isTwoGrabbed)
        {
            if(gb.PointsCount < 2)
            {
                Categorize();
                isTwoGrabbed = false;
            }
        }
        else
        {
            if(gb.PointsCount >= 2)
            {
                isTwoGrabbed = true;
            }
        }
    }


    public void Categorize()
    {
        // サイズの閾値のギャップにいる場合は調整
        this.gameObject.transform.localScale *= BoidsManager.Instance.BridgeSizeGap(GetSize());

        SizeCategory newCategory = BoidsManager.Instance.GetCategory(GetSize());

        if (Category != newCategory)
        {
            SizeCategory oldCategory = Category;

            Category = newCategory;

            BoidsManager.Instance.ChangeCategory(this, oldCategory, newCategory);
        }
    }

    public float GetSize()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();

        Bounds bounds = renderer.bounds;

        return Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
        this.animator.SetFloat("TailSpeed", speed);
    }

}
