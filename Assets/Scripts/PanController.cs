using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanController : MonoBehaviour
{
    Vector3 touchStart;

    bool panMode;
    private float panSensitivity = 1;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(panMode){
            if(Input.GetMouseButtonDown(0)){
                touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }        
            if(Input.GetMouseButton(0)){
                Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Camera.main.transform.position += direction;
            }
        }
    }

    public void SetPanMode(bool mode){
        panMode = mode;
    }

}
