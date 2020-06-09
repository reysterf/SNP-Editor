using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RulePrefabController : MonoBehaviour
{
    private string rule;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendRule()
    {
        SendMessageUpwards("ChoiceMade",rule);
    }

    public void SetRule(string rule)
    {
        this.rule = rule;
        gameObject.transform.Find("Text").GetComponent<Text>().text = rule;
    } 
}
