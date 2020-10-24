using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomController : MonoBehaviour
{
    public Camera camera;

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
