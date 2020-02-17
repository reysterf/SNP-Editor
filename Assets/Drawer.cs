using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drawer : MonoBehaviour
{
    public Button someButton;
    private int lineCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        //Register Button Events
        // someButton.onClick.AddListener(() => DrawLine(Vector3.zero + new Vector3(0, lineCount, 0), Vector3.one));
    }

    private void buttonCallBack(string myStringValue, int myIntValue)
    {
        Debug.Log("Button Clicked. Received string: " + myStringValue + " with int: " + myIntValue);
    }

    void OnDisable()
    {
        //Un-Register Button Events
        someButton.onClick.RemoveAllListeners();
    }

    public void DrawLine(Vector3 start, Vector3 end, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        // lr.SetColors(color, color);
        lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        // GameObject.Destroy(myLine, duration);
        lineCount += 1;
    }
}
