using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class AddGrabbable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var obj = this.gameObject;

        // Rigidbody付与
        obj.AddComponent<Rigidbody>();

        // つかみ判定関係のコンポーネントを追加
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        rb.useGravity = false;

        if (rb != null)
        {
            Grabbable gb = obj.AddComponent<Grabbable>();
            gb.InjectOptionalRigidbody(rb);

            //HandGrabInteractable hgb = obj.AddComponent<HandGrabInteractable>();
            //hgb.InjectRigidbody(rb);
            //hgb.InjectOptionalPointableElement(gb);

            DistanceHandGrabInteractable dhgb = obj.AddComponent<DistanceHandGrabInteractable>();
            dhgb.InjectRigidbody(rb);
            dhgb.InjectOptionalPointableElement(gb);

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
