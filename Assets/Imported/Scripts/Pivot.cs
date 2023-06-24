/*
 * Author: Shawn Hice
 * Description: This file is intended to keep track and debug the pivoting object
 * Notes:
 *     - Helper file for the Pivot Object to keep track of pivoting location
 * $GUID-073a5c99028742f884027478575078f3$
 */

using UnityEngine;

namespace Fusion
{
    public class Pivot : MonoBehaviour
    {
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.10f);

            foreach (var childRenderer in GetComponentsInChildren<Renderer>())
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(childRenderer.bounds.center, 0.25f);
            
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, childRenderer.bounds.center);
            }
        }
    }
}
