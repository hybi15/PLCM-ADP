using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RotateJoint : MonoBehaviour
{
    public RobotJoint[] Joints;
    private List<int> Angles = new List<int>();
    public bool Loop;
    private List<int> AnglesNeg = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        Angles.Clear();
        AnglesNeg.Clear();
        Joints[0].transform.localEulerAngles = new Vector3(0,0,0);
        Joints[1].transform.localEulerAngles = new Vector3(-90, 0, 0);
        Joints[2].transform.localEulerAngles = new Vector3(0, 180, 0);
        Joints[3].transform.localEulerAngles = new Vector3(-90, 0, 0);
        Joints[4].transform.localEulerAngles = new Vector3(0, 90, 180);
        Joints[5].transform.localEulerAngles = new Vector3(0, 0, 0);

        // Only positive int
        Angles.Add(180);
        Angles.Add(90);
        Angles.Add(90);
        Angles.Add(180);
        Angles.Add(90);
        Angles.Add(180);

        AnglesNeg = new List<int>(Angles.Count); // Create a new list with the same size as Angles
        for (int i = 0; i < Angles.Count; i++)
        {
            AnglesNeg.Add(-Angles[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = Joints.Length - 1; i >= 0; i--)
        {
            if (Angles[i] != 0)
            {
                switch (Joints[i].RotationAxis)
                {
                    case 'x':
                        Joints[i].transform.localEulerAngles = new Vector3((Joints[i].transform.localRotation.eulerAngles.x + 1), Joints[i].transform.localRotation.eulerAngles.y, Joints[i].transform.localRotation.eulerAngles.z);
                        break;
                    case 'y':
                        Joints[i].transform.localEulerAngles = new Vector3(Joints[i].transform.localRotation.eulerAngles.x, (Joints[i].transform.localRotation.eulerAngles.y + 1), Joints[i].transform.localRotation.eulerAngles.z);
                        break;
                    case 'z':
                        Joints[i].transform.localEulerAngles = new Vector3(Joints[i].transform.localRotation.eulerAngles.x, Joints[i].transform.localRotation.eulerAngles.y, (Joints[i].transform.localRotation.eulerAngles.z + 1));
                        break;
                }
                Angles[i] -= 1;
            }
        }
        if (Loop && Angles.Max()==0)
        {
            for (int i = 0; i <= Joints.Length - 1; i++)
            {
                if (AnglesNeg[i] != 0)
                {
                    switch (Joints[i].RotationAxis)
                    {
                        case 'x':
                            Joints[i].transform.localEulerAngles = new Vector3((Joints[i].transform.localRotation.eulerAngles.x - 1), Joints[i].transform.localRotation.eulerAngles.y, Joints[i].transform.localRotation.eulerAngles.z);
                            break;
                        case 'y':
                            Joints[i].transform.localEulerAngles = new Vector3(Joints[i].transform.localRotation.eulerAngles.x, (Joints[i].transform.localRotation.eulerAngles.y - 1), Joints[i].transform.localRotation.eulerAngles.z);
                            break;
                        case 'z':
                            Joints[i].transform.localEulerAngles = new Vector3(Joints[i].transform.localRotation.eulerAngles.x, Joints[i].transform.localRotation.eulerAngles.y, (Joints[i].transform.localRotation.eulerAngles.z - 1));
                            break;
                    }
                    AnglesNeg[i] += 1;
                }
            }
        }
        if (AnglesNeg.Min() == 0)
            Start();
    }
}

    