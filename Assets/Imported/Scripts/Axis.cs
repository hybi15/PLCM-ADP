/*
 * Author: Shawn Hice
 * Description: This file is intended to control a given access in a series of nested Joints
 * Notes:
 *     - Needs the following correct data
 *         - Renderer Bounds center for Pivot
 *         - Axis aligned with Local Space for the given connection item
 *         - Connection offset from position
 * $GUID-e8addc725fef43d5998e89bcabe21ae7$
 */

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Fusion
{
    [CustomEditor(typeof(Axis))]
    public class AxisEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var axis = (Axis)target;
            
#if UNITY_2019_1_OR_NEWER

            if (GUILayout.Button("Rotate Axis By Angle"))
            {
                axis.RotateByAngle(0.69f);
            }
            
            if (GUILayout.Button("Rotate Axis To Angle"))
            {
                axis.RotateToAngle(0.69f);
            }
            
            if (GUILayout.Button("Test Set Predicted"))
            {
                axis.SetPredictedByAngle(0.69f);
            }
            
#endif
        }

    }
    
    [SelectionBase]
    public class Axis : MonoBehaviour
    {
        
        [Header("Rotational Joint")]
        public float current = 0.0f;
        public float target = 0.0f;
        [SerializeField] public float max = 360.0f;
        [SerializeField] public float min = 0.0f;
        public bool lowerLimit;
        public bool upperLimit;
        public GameObject endEffector;

        [Space] [Header("Motor")]
        // number of steps per rotation
        public bool on = true;
        public float resolution = 1200.0f;
        public float dampening = 0.05f;
        [Space]
        
        [Header("Visual")]
        public bool drawLines = true;
        public float drawThreshold = 0.5f;
        private float _currentDrawAngle = 0.0f;
        [Space]
        
        [Header("System Information")]
        public Base controller;
        public Vector3 axisVector = Vector3.forward;
        public List<Axis> connectedAxis;
        public GameObject pivot;
        [Space]
        
        [Header("DEBUG")]
        public float rotateAngle = 0.0f;
        public Vector3 predictedPosition;
        
        
        // Internal fields
        private LineRenderer _lr;
        
#if UNITY_2019_1_OR_NEWER

        void Start()
        {
            {
                _lr ??= gameObject.AddComponent<LineRenderer>();

                _currentDrawAngle = current;

                _lr.startWidth = 0.12f;
                _lr.endWidth = 0.12f;
                // this may not always be the case
                _lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
                _lr.numCornerVertices = 1;

                const float alpha = 1.0f;
                var gradient = new Gradient();
                gradient.SetKeys(
                    new[] {new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.blue, 1.0f)},
                    new[] {new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f)}
                );
                _lr.colorGradient = gradient;
                _lr.positionCount = 1;
                _lr.SetPosition(0, this.pivot.transform.position);
            }

            connectedAxis ??= new List<Axis>();

            if (connectedAxis.Count > 0) return;
            
            foreach(Transform child in transform)
            {
                var childAxis = child.GetComponent<Axis>();
                if(childAxis)
                {                   
                    connectedAxis.Add(childAxis);
                }
            }

        }

        public void GenerateChildAxis([NotNull] Base baseController)
        {
            this.controller ??= baseController ? baseController : throw new ArgumentNullException(nameof(baseController));
            connectedAxis ??= new List<Axis>();
            connectedAxis.Clear();
            
            // reform tree just in case
            foreach(Transform child in transform)
            {
                var childAxis = child.GetComponent<Axis>();
                if (!childAxis) continue;
                
                connectedAxis.Add(childAxis);
                childAxis.GenerateChildAxis(baseController);
            }

            if (connectedAxis.Count == 0)
            {
                this.controller.end = this;
            }
        }

        private float RotateAxis(float kAngle)
        {
            if (!on) return 0.0f;
            
            var futureAngle = (kAngle + current);

            if (futureAngle >= max)
            {
                return 0.0f;
            }

            if (futureAngle <= min)
            {
                return 0.0f;
            }

            gameObject.transform.RotateAround(pivot.transform.position, transform.TransformDirection(axisVector), kAngle);

            current = (current + kAngle);

            // half a degree max
            upperLimit = max - current < 0.5f;
            lowerLimit = current - min < 0.5f;
            
            DrawHistoryLines();

            return kAngle;
        }
        
        /// <summary>
        /// Rotate the Axis by a given delta angle.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns>Total Degrees Rotated by operation.</returns>
        public float RotateByAngle(float angle)
        {
            if (!pivot) return 0.0f;
            // find the angle left over from 0->360

            if (Math.Abs(angle - 0.69f) < .001f && Math.Abs(angle - 0.69f) > -.001f)
            {
                angle = rotateAngle;
            }
            
            // Basically clamp the angle to the maximum rotation
            // RotateAxis(angle % 360);

            return RotateAxis(angle % max);;
        }

        /// <summary>
        /// Rotates the axis to the given angle between 0-360
        /// </summary>
        /// <param name="angle"></param>
        /// <returns>Total Degrees Rotated by operation.</returns>
        public float RotateToAngle(float angle)
        {
            if (!pivot) return 0.0f;
            
            if (Math.Abs(angle - 0.69f) < .001f && Math.Abs(angle - 0.69f) > -.001f)
            {
                angle = rotateAngle;
            }
            
            angle %= max;
            if (angle <= min) return 0.0f;
            return angle >= max ? 0.0f : RotateAxis(angle - current);
        }
        
        private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }

        private Vector3 GetPositionFromRotation(float angle, Vector3 previousPosition)
        {
            return RotatePointAroundPivot(previousPosition, pivot.transform.position, transform.TransformDirection(axisVector) * angle);
        }
        
        public void SetPredictedByAngle(float angle)
        {
            if (Math.Abs(angle - 0.69f) < .001f && Math.Abs(angle - 0.69f) > -.001f)
            {
                angle = rotateAngle;
            }
            
            predictedPosition = GetPositionFromRotation(angle, this.connectedAxis[0].pivot.transform.position);
        }

        public Vector3 Position()
        {
            if (endEffector)
            {
                return endEffector.transform.position;
            }

            return !pivot ? transform.position : pivot.transform.position;
        }

        private void OnDrawGizmosSelected()
        {
            if (!pivot) return;
            
            if (controller)
            {
                if (!controller.DebugDisplay) return;
            }

            var pivotTransform = pivot.transform;
            var position1 = pivotTransform.position;

            if (connectedAxis.Count != 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(position1, connectedAxis[0].pivot.transform.position);
            }
            else if (controller != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(endEffector.transform.position, controller.activeTarget ? controller.activeTarget.transform.position : controller.target);
            }
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(predictedPosition, 1.0f);

            var axis = axisVector;

            if (axis == Vector3.up)
            {
                axis = pivotTransform.up;
            }else if (axis == Vector3.forward)
            {
                axis = pivotTransform.forward;
            }else if (axis == Vector3.right)
            {
                axis = pivotTransform.right;
            }
            else
            {
                axis *= 365;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(position1, 1.0f);

            var rotationMatrix = Matrix4x4.TRS(position1, this.transform.rotation * Quaternion.LookRotation(axis), pivotTransform.lossyScale);
            Gizmos.matrix = rotationMatrix;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, 20));
            
            // Should get renderer bounds extents
            Gizmos.color = new Color(1, 0, 0, 0.25f);
            Gizmos.DrawCube(Vector3.zero, new Vector3(10.0f, 10.0f, 0.0001f));
        }

        private void DrawHistoryLines()
        {
            if (!drawLines || !controller.DebugDisplay)
            {
                _lr.positionCount = 0;
            }

            if (Math.Abs(current - _currentDrawAngle) < drawThreshold) return;

            var positionCount = _lr.positionCount + 1;
            
            _lr.positionCount = positionCount;

            _lr.SetPosition((positionCount - 1), pivot.transform.position);

            _currentDrawAngle = current;
        }

        // ________________________ PUBLIC INTERFACE ___________________

        /// <summary>
        /// Sets the Axis Speed in Degrees per second.
        /// Uses a lerp that runs in fixed time intervals to rotate the individual axis.
        /// </summary>
        /// <param name="degPerSecond"></param>
        public void SetSpeed(float degPerSecond)
        {
            throw new NotImplementedException("The Set Speed function is not currently implemented, if you want this feature please contact me I may have a working version that uses Matrices instead and lerps.");
        }

        /// <summary>
        /// Disable the motor in the given assembly by disabling the ability for it to rotate.
        /// </summary>
        public void Off()
        {
            on = false;
        }

        /// <summary>
        /// Move to the Maximum Angle this Axis can rotate to.
        /// </summary>
        public void MoveMinimum()
        {
            RotateToAngle(min);
        }

        /// <summary>
        /// Move to the Minimum angle this axis can rotate to.
        /// </summary>
        public void MoveMaximum()
        {
            RotateToAngle(max);
        }
        
#else

        private void Start()
        {
            Debug.LogWarning("Unity 2019 or above is required for Fusion 360 Kinematics modeling");
        }

#endif
    }
}