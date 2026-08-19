using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using static Oculus.Interaction.TransformerUtils;

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
        rb.isKinematic = true;

        if (rb != null)
        {
            Grabbable gb = obj.AddComponent<Grabbable>();
            gb.InjectOptionalRigidbody(rb);
            gb.InjectOptionalThrowWhenUnselected(false);

            //HandGrabInteractable hgb = obj.AddComponent<HandGrabInteractable>();
            //hgb.InjectRigidbody(rb);
            //hgb.InjectOptionalPointableElement(gb);

            DistanceHandGrabInteractable dhgb = obj.AddComponent<DistanceHandGrabInteractable>();
            dhgb.InjectRigidbody(rb);
            dhgb.InjectOptionalPointableElement(gb);
            dhgb.ResetGrabOnGrabsUpdated = false;
            dhgb.HandAlignment = HandAlignType.None;


            GrabFreeTransformer gftf = obj.AddComponent<GrabFreeTransformer>();
            TransformerUtils.ScaleConstraints constraints = new TransformerUtils.ScaleConstraints();
            constraints.ConstraintsAreRelative = true;
            
            TransformerUtils.ConstrainedAxis axis = new TransformerUtils.ConstrainedAxis();
            axis.ConstrainAxis = false;
            constraints.XAxis = axis;
            constraints.YAxis = axis;
            constraints.ZAxis = axis;

            gftf.InjectOptionalScaleConstraints(constraints);
            gb.InjectOptionalOneGrabTransformer(gftf);
            gb.InjectOptionalTwoGrabTransformer(gftf);


        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
