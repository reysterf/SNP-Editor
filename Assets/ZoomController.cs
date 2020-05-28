using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomController : MonoBehaviour
{
    public Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ZoomIn(){
        if(camera.orthographicSize >= 3){
            camera.orthographicSize -= 1;
        }
    }

    public void ZoomOut(){
        if(camera.orthographicSize <= 19){
            camera.orthographicSize += 1;
        }
    }
}
