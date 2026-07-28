using Unity.VisualScripting;
using UnityEngine;

public class FishAgent : MonoBehaviour
{
    public SizeCategory Category = SizeCategory.Small;
    public Animator animator;

    // 速度ベクトル
    public float speed;

    private void Start()
    {
        Categorize();

        BoidsManager.Instance.RegistFish(this);

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Categorize();
    }

    public void Categorize()
    {
        float size = GetSize();

        SizeCategory newCategory = BoidsManager.Instance.GetCategory(size);

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

}
