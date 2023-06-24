/*
 * Author: Shawn Hice
 * Description: This is the main IK Controller for a kinematic assembly, to be placed on grounded axis
 * Notes:
 *     - This really should be discoverable by the top level Assembly
 *     - Needs to have a set of interfaces that are discoverable and usable
 *     - Needs more Editor script functionality
 * $GUID-4d273c1742714f478d59eb03bffb1d36$
 */

using System;
using UnityEditor;
using UnityEngine;

namespace Fusion
{
    // Should seperate this out into a debug class along with Gizmo functionality
    [CustomEditor(typeof(Base))]
    public class IkEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var generateAxisConnections = (Base)target;
            
#if UNITY_2019_1_OR_NEWER
            
            if (GUILayout.Button("Generate Axis Connections"))
            {
                generateAxisConnections.GenerateAllAxis();
            }
#endif
        }

    }
    
    [CustomEditor(typeof(Base))]
    public class TargetPositionHandle : Editor
    {
        protected virtual void OnSceneGUI()
        {
            var baseController = (Base)target;

            EditorGUI.BeginChangeCheck();
            
            var newTargetPosition = Handles.PositionHandle(baseController.target, Quaternion.identity);
            
            if (!EditorGUI.EndChangeCheck()) return;
            
            Undo.RecordObject(baseController, $"Moved {baseController.gameObject.name} target position");
            baseController.target = newTargetPosition;
            // baseController.Update();
        }
    }

    [SelectionBase]
    public class Base : MonoBehaviour
    {
        [Header("Display")] 
        public bool DebugDisplay = true;
        [Space]
        
        [Header("Kinematics Controller")] 
        public bool Enabled = true;
        public Axis root;
        public Axis end;
        [Space]
        
        public int steps = 0;
        public float k_stepsize = 1.25f;
        public float k_threshold = 1.0f;
        [Space]
        
        public bool onTarget = false;
        public float thetaSize = 0.01f;
        public Vector3 target = new Vector3(0.0f, 50.0f, 0.0f);
        public GameObject activeTarget;
        
        
#if UNITY_2019_1_OR_NEWER

        public void GenerateAllAxis()
        {
            var foundRoot = false;
            
            foreach(Transform child in transform)
            {
                var childAxis = child.GetComponent<Axis>();
                if (!childAxis) continue;
                
                if (!foundRoot)
                {
                    root = childAxis;
                    foundRoot = true;
                }
                
                childAxis.GenerateChildAxis(this);
            }
        }


        private float GetDistance(Vector3 origin, Vector3 target_)
        {
            return Vector3.Distance(origin, target_);
        }

        private float GetDistanceFromTarget(Vector3 a)
        {
            return activeTarget == null ? (a - target).magnitude : (a - activeTarget.transform.position).magnitude;
        }

        private float CalculateDerivative(Axis _axis)
        {
            var distance1 = GetDistanceFromTarget(end.Position());
            _axis.RotateByAngle(thetaSize);
            
            var distance2 = GetDistanceFromTarget(end.Position());
            _axis.RotateByAngle(-thetaSize);
            
            return ((distance2 - distance1) / thetaSize);
        }
        
        public void Update()
        {
            if (!Enabled) return;
            if (!(GetDistanceFromTarget(end.Position()) > k_threshold)) return;
            
            var current = root;
            while (current != null)
            {
                var slope = CalculateDerivative(current);
                current.RotateByAngle(-slope * k_stepsize);
                current = current.connectedAxis.Count > 0 ? current.connectedAxis[0] : null;
            }
            
        }

        private void OnDrawGizmosSelected()
        {
            // This is just a temporary measure
            if (root == null)
            {
                GenerateAllAxis();
            }
            
            if (!DebugDisplay) return;

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(activeTarget == null ? target : activeTarget.transform.position, 2.0f);
        }

        private void Start()
        {
            // Also temporary
            GenerateAllAxis();
        }
#else
        private void Start()
        {
            Debug.LogWarning("Unity 2019 or above is required for Fusion 360 Kinematics modeling");
        }
#endif
        
        
        
        // ________________________ PUBLIC INTERFACE ___________________
        
        /// <summary>
        /// Sets the Target Gameobject which runs off of it's base transform 
        /// </summary>
        /// <param name="nextTarget"></param>
        public void SetTarget(GameObject nextTarget)
        {
            // Do some validation check to see if it's within the limitations of the system
            // Do validation on the positions and rotation
            activeTarget = nextTarget;
        }

        /// <summary>
        /// Sets the Target Transform for the IK interface.
        /// </summary>
        /// <param name="_target"></param>
        /// <returns></returns>
        public void SetTarget(Transform _target)
        {
            // Check to see if activeTarget is currently within bounds by using extents of Axis chain.
            // Return successful operation to activeTarget
            // This may need to have the applied target matrix to get unit differences
            target = _target.position;
        }
        
        /// <summary>
        /// Sets the Target Vector for the IK Interface if it is within the bounds of the solution.
        /// </summary>
        /// <param name="_target"></param>
        /// <returns></returns>
        public void SetTarget(Vector3 _target)
        {
            // Check to see if activeTarget is currently within bounds by using extents of Axis chain.
            // Return successful operation to activeTarget
            target = _target;
        }

        /// <summary>
        /// Gets the number of available Axis currently in the chain.
        /// </summary>
        /// <returns></returns>
        public int GetAxisCount()
        {
            var num = 0;
            var current = root;
            
            while (current != null)
            {
                num++;
                current = current.connectedAxis.Count > 0 ? current.connectedAxis[0] : null;
            }
            
            return num;
        }

        /// <summary>
        /// Gets the Extents of the interaction box that the IK system is capable of.
        /// Not currently implemented.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetInteractionBoxSize()
        {
            throw new NotImplementedException("The Interaction Box size is not currently implemented");
        }

        /// <summary>
        /// Changes the speed of all axis in change to the specified degrees per second listed.
        /// </summary>
        /// <param name="_degPerSecond"></param>
        public void SetSpeed(float _degPerSecond)
        {
            throw new NotImplementedException("The Set Speed function is not currently implemented, if you want this feature please contact me I may have a working version that uses Matrices instead and lerps.");
            // loop through and use the axis speed specifiers
        }

        /// <summary>
        /// Turn on the System
        /// </summary>
        public void On()
        {
            Enabled = true;
        }

        /// <summary>
        /// Turn off the System
        /// </summary>
        public void Off()
        {
            Enabled = false;
        }
        
    }

}
