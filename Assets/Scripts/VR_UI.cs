using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VR_UI : MonoBehaviour
{
    public static VR_UI instance;

    //VR right controller will function as mouse.
    public Transform rightHand;
    // Dot will be our pointer.
    public Transform dot;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Activate dot (pointer) using Ray
        Ray ray = new Ray(rightHand.position, rightHand.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                dot.gameObject.SetActive(true);
                dot.position = hit.point;
            }
            else
            {
                dot.gameObject.SetActive(false);
            }

            //Enable "click" when dot is colliding
            if (dot.gameObject.activeSelf)
            {
                Button btn = hit.transform.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.Invoke();
                }
            }
        }
        else
        {
            dot.gameObject.SetActive(false);
        }
    }
}