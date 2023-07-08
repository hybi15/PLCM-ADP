using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    public RobotJoint[] Joints;
    public Transform target;
    public Transform TargetObject;
    public Transform baseParent;
    public GripTillCollide gripperManager;

    public float SamplingDistance;
    public float LearningRate;
    public float DistanceThreshold;

    public bool TargetAttached;

    public float[] Angles;

    private Transform storedObject;

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
            InverseKinematics(TargetObject.position, Angles);
            if (DistanceFromTarget(TargetObject.position, Angles) < DistanceThreshold)
            {
                TargetObject.parent = Joints[Joints.Length - 1].transform; //parent the spawned object to last joint
                gripperManager.enabled = true;
            }
        }
        else
        {
            InverseKinematics(target.position, Angles);
            if ((DistanceFromTarget(target.position, Angles) < DistanceThreshold) && storedObject.parent)
            {
                gripperManager.closed = true;
                gripperManager.enabled = true;
                storedObject.parent = null;
            }
        }
    }

    public void TargetObjectGripped() // called by GripTillCollide after gripper closed in to target object
    {
        gripperManager.enabled = false;
        TargetAttached = true;
        storedObject = TargetObject; // to be used to unparent object
        TargetObject = null;
    }

    public Vector3 ForwardKinematics(float[] angles)
    {
        Vector3 prevPoint = Joints[0].transform.position;
        Quaternion rotation = Quaternion.identity;
        rotation *= baseParent.transform.rotation;
        for (int i = 1; i < Joints.Length; i++)
        {
            // Rotates around a new axis
            rotation *= Quaternion.AngleAxis(angles[i - 1], Joints[i - 1].Axis);
            Vector3 nextPoint = prevPoint + rotation * MultiplyMatricesByRows(Joints[i].StartOffset, baseParent.transform.lossyScale);
            prevPoint = nextPoint;
            Debug.Log(i + "rotation: " + rotation.eulerAngles);
            Debug.Log(i + "position: " + prevPoint);
        }
        Debug.Log(prevPoint);
        return prevPoint;
    }

    // Multiply two matrices by rows
    public Vector3 MultiplyMatricesByRows(Vector3 matA, Vector3 matB)
    {
        float resultX = matA.x * matB.x;
        float resultY = matA.y * matB.y;
        float resultZ = matA.z * matB.z;

        return new Vector3(resultX, resultY, resultZ);
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

    public void InverseKinematics(Vector3 target, float[] angles)
    {
        if (DistanceFromTarget(target, angles) < DistanceThreshold)
        {
            return;
        }

        for (int j = Joints.Length - 1; j >= 0; j--)
        {
            // Gradient descent
            // Update : Solution -= LearningRate * Gradient
            float gradient = PartialGradient(target, angles, j);
            angles[j] -= LearningRate * gradient;

            // Clamp
            if ((Joints[j].MinAngle != 0) && (Joints[j].MaxAngle != 0))
            {
                angles[j] = Mathf.Clamp(angles[j], Joints[j].MinAngle, Joints[j].MaxAngle);
            }


            // Early termination
            if (DistanceFromTarget(target, angles) < DistanceThreshold)
                return;

            // Move joints
            switch (Joints[j].RotationAxis)
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
            }
        }
    }
}
