/*
 * Author: Shawn Hice
 * Description: This is the file that contains generic properties for a given Fusion 360 Object.
 * $GUID-4c55ef30ca914a4bb68e48c2eb1b7e79$
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Fusion
{
    public class FusionObject : MonoBehaviour
    {
        [Header("Information Display")] 
        public bool show;
        [Space]
        
        [Header("Physical Information")]
        [Tooltip("Center of Mass, in cm")]
        public Vector3 CenterOfMass;
        [Tooltip("Mass, in kg")]
        public float Mass;
        [Tooltip("Density, in kg/cm^3")]
        public float Density;
        [Tooltip("Area, in cm^2")]
        public float Area;
        [Tooltip("Volume, in cm^3")]
        public float Volume;
        [Space]

        [Header("Export Information")]
        public string Version;
        public int MaterialShader;
        public bool RigidBodies;
        public bool Kinematics;
        public int Accuracy;
        public string Name;
        public bool ConvexMeshes;

#if UNITY_EDITOR && UNITY_2019_1_OR_NEWER
        private void OnDrawGizmosSelected()
        {
            if (!show) return;
            
            if (!((IList) Selection.gameObjects).Contains(transform.gameObject)) { return; }

            var o = this.gameObject;
            
            var text = $"Name: {o.name}\n" +
                       $" - Mass: {this.Mass} kg\n" + 
                       $" - Density: {this.Density} kg/cm^3\n"+
                       $" - Volume: {this.Volume} cm^3\n"+
                       $" - Area: {this.Area} cm^2\n"+
                       $" - Center of Mass: \n \t {this.CenterOfMass.x}, {this.CenterOfMass.y}, {this.CenterOfMass.z}";

            var localToWorldMatrix = o.transform.localToWorldMatrix;
            
            Handles.matrix = localToWorldMatrix;
            Handles.color = Color.black;
            
            var newPos = new Vector3 (1, 5, 1);
            Handles.Label (this.CenterOfMass + newPos, text);

            Gizmos.color = Color.magenta;
            Gizmos.matrix = localToWorldMatrix;
            // Gizmos.DrawSphere(this.CenterOfMass, 1.0f);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Vector3.zero, this.CenterOfMass);
        }
#endif

    }
}
