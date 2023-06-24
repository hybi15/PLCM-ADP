using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class IKManager : MonoBehaviour
{
    public RobotJoint[] Joints;
    public Transform target;
    public Transform TargetObject;
    public GameObject[] Buttons;
    public BoxCollider[] ButtonColliders;
    public Color[] ButtonColors;

    public float SamplingDistance;
    public float LearningRate;
    public float DistanceThreshold;

    public bool TargetAttached;

    public float[] Angles;

    public void Start()
    {
        float[] angles = new float[Joints.Length];

        for (int i = 0; i < Joints.Length; i++)
        {
            if (Joints[i].RotationAxis == 'x')
                angles[i] = Joints[i].transform.localRotation.eulerAngles.x;
            else if (Joints[i].RotationAxis == 'y')
                angles[i] = Joints[i].transform.localRotation.eulerAngles.y;
            else if (Joints[i].RotationAxis == 'z')
                angles[i] = Joints[i].transform.localRotation.eulerAngles.z;                
        }
        Angles = angles;
}

    public void Update()
{
    if (TargetObject)
    {
        Vector3 localTargetPosition = transform.InverseTransformPoint(TargetObject.position);
        InverseKinematics(localTargetPosition, Angles);
        if (DistanceFromTarget(localTargetPosition, Angles) < DistanceThreshold)
        {
            TargetObject.parent = Joints[Joints.Length-1].transform; //parent the spawned object to last joint
            TargetAttached = true;
            TargetObject = null;
        }
    }
    else
    {
        Vector3 localTargetPosition = transform.InverseTransformPoint(target.position);
        InverseKinematics(localTargetPosition, Angles);
    }
}

public Vector3 ForwardKinematics(float[] angles)
{
    Vector3 prevPoint = Vector3.zero;
    Quaternion rotation = Quaternion.identity;
    for (int i = 1; i < Joints.Length; i++)
    {
        // Rotates around a new axis
        rotation *= Quaternion.AngleAxis(angles[i - 1], Joints[i - 1].Axis);
        Vector3 nextPoint = prevPoint + rotation * Joints[i].StartOffset;
        prevPoint = nextPoint;
    }
    return prevPoint;
}

    public float DistanceFromTarget(Vector3 target, float[] angles)
    {
        Vector3 point = ForwardKinematics (angles);
        return Vector3.Distance(point, target);
    }

    public float PartialGradient(Vector3 target, float[] angles, int i)
    {
        // Saves the angle,
        // it will be restored later
        float angle = angles[i];

        // Gradient : [F(x+SamplingDistance) - F(x)] / h
        float f_x = DistanceFromTarget(target, angles);

        angles[i] += SamplingDistance;
        float f_x_plus_d = DistanceFromTarget(target, angles);

        float gradient = (f_x_plus_d - f_x) / SamplingDistance;

        // Restores
        angles[i] = angle;

        return gradient;
    }

    public void InverseKinematics(Vector3 localTarget, float[] angles)
    {
        if (DistanceFromTarget(localTarget, angles) < DistanceThreshold)
        {
            if (TargetAttached == true)
            {
                for (int i = 0; i < ButtonColliders.Length ; i++)
                {
                    ButtonColliders[i].enabled = true;
                }
                for (int i = 0; i < Buttons.Length; i++)
                {
                    Buttons[i].GetComponent<Renderer>().material.color = ButtonColors[i];
                }
                TargetAttached = false;
            }
            return;
        }

        for (int j = Joints.Length - 1; j >= 0; j--)
        {
            // Gradient descent
            // Update : Solution -= LearningRate * Gradient
            float gradient = PartialGradient(localTarget, angles, j);
            angles[j] -= LearningRate * gradient;

            // Clamp
            if ((Joints[j].MinAngle != 0) && (Joints[j].MaxAngle != 0))
            {
                angles[j] = Mathf.Clamp(angles[j], Joints[j].MinAngle, Joints[j].MaxAngle);
            }


            // Early termination
            if (DistanceFromTarget(localTarget, angles) < DistanceThreshold) if (TargetAttached == true)
            {
                if (TargetAttached == true)
                {
                    for (int i = 0; i < ButtonColliders.Length; i++)
                    {
                        ButtonColliders[i].enabled = true;
                    }
                    TargetAttached = false;
                    for (int i = 0; i < Buttons.Length; i++)
                    {
                        Buttons[i].GetComponent<Renderer>().material.color = ButtonColors[i];
                    }
                }
                return;
            }

            // Move joints
            Vector3 localEulerAngles = Joints[j].transform.localEulerAngles;
            Vector3 globalAxis = transform.TransformDirection(Joints[j].Axis);
            Quaternion deltaRotation = Quaternion.AngleAxis(angles[j], globalAxis);
            Joints[j].transform.rotation = transform.rotation * deltaRotation * Quaternion.Euler(localEulerAngles);
            /*switch (Joints[j].RotationAxis)
            {
                case 'x':
                    Joints[j].transform.localEulerAngles = new Vector3(angles[j], Joints[j].transform.localRotation.eulerAngles.y, Joints[j].transform.localRotation.eulerAngles.z);
                    break;
                case 'y':
                    Joints[j].transform.localEulerAngles = new Vector3(Joints[j].transform.localRotation.eulerAngles.x, angles[j], Joints[j].transform.localRotation.eulerAngles.z);
                    break;
                case 'z':
                    Joints[j].transform.localEulerAngles = new Vector3(Joints[j].transform.localRotation.eulerAngles.x, Joints[j].transform.localRotation.eulerAngles.y, angles[j]);
                    break;
            }*/
        }
    }
}