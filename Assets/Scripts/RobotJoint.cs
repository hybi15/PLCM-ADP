using UnityEngine;
 
public class RobotJoint : MonoBehaviour
{
    public Vector3 Axis;

    public Vector3 StartOffset;
    private Transform _transform;

    [HideInInspector]
    public char RotationAxis;

    public float MinAngle;
    public float MaxAngle;

    void Awake()
    {
        _transform = this.transform;
        StartOffset = _transform.localPosition;
        if (Axis.x == 1 && Axis.y == 0 && Axis.z == 0)
            RotationAxis = 'x';
        else if (Axis.x == 0 && Axis.y == 1 && Axis.z == 0)
            RotationAxis = 'y';
        else if (Axis.x == 0 && Axis.y == 0 && Axis.z == 1)
            RotationAxis = 'z';
    }
}