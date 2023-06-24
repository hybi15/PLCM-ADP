using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;


public class SpawnAndGet : MonoBehaviour
{
    public Transform SpawnPoint;
    public GameObject SpawnedObject;
    public IKManager IKManager;
    
    private GameObject PrintObject()
    {
        GameObject obj = Instantiate(SpawnedObject, SpawnPoint.position, SpawnPoint.rotation);
        return obj;
    }

    public void GetObject(GameObject Target)
    {
        IKManager.TargetObject = Target.transform;
    }

    public void ButtonTrigger()
    {
        GameObject obj = PrintObject();
        GetObject(obj);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject obj = PrintObject();
            GetObject(obj);
        }
    }
}
