using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GripTillCollide : MonoBehaviour
{
    public GameObject leftGripper;
    public GameObject rightGripper;
    public float displacementRate;
    public bool closed;

    private FingerController leftController;
    private FingerController rightController;
    [SerializeField]
    private float displacement;
    private IKManager iKManager;

    // Start is called before the first frame update
    void Start()
    {
        leftController = leftGripper.GetComponent<FingerController>();
        rightController = rightGripper.GetComponent<FingerController>();
        displacement = 0.0f;
        closed = false;
        iKManager = FindFirstObjectByType<IKManager>().GetComponent<IKManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((displacement >= leftController.maxDisplacement) || (displacement >= rightController.maxDisplacement)) // stop closing when reaching max displacement, assume gripped
        {
            displacement = 0.0f;
            iKManager.TargetObjectGripped();
        }
        if (closed == false)
        {
            leftGripper.transform.localPosition += leftController.axis * displacementRate;
            rightGripper.transform.localPosition += rightController.axis * displacementRate;
            displacement += displacementRate;
            
        }
        else // Open the fingers to initial position
        {
            leftGripper.transform.localPosition = leftController.initialPosition;
            rightGripper.transform.localPosition = rightController.initialPosition;
            closed = false;
            enabled = false;
        }
    }
}
