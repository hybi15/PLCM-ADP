using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerController : MonoBehaviour
{
    public float maxDisplacement;
    public Vector3 axis;
    public Vector3 initialPosition;
    private IKManager iKManager = FindObjectOfType<IKManager>().GetComponent<IKManager>();
    private GripTillCollide gripManager = FindObjectOfType<GripTillCollide>().GetComponent<GripTillCollide>();

    // Start is called before the first frame update
    public void Start()
    {
        initialPosition = transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        iKManager.TargetObjectGripped();
    }
}
