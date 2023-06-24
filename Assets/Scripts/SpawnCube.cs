using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCube : MonoBehaviour
{
    public Transform SpawnPoint;
    public GameObject SpawnedObject;

    // Start is called before the first frame update
    public GameObject PrintObject()
    {
        GameObject obj = Instantiate(SpawnedObject, SpawnPoint.position, SpawnPoint.rotation);
        return obj;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
