using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RulePrefabController : MonoBehaviour
{
    private string rule;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //Calls choicemade in GuidedMenuController, passing the rule chosen
    public void SendRule()
    {
        SendMessageUpwards("ChoiceMade",rule);
    }

    //Sets the rule to be displayed for the prefab
    public void SetRule(string rule)
    {
        this.rule = rule;
        gameObject.transform.Find("Text").GetComponent<Text>().text = rule;
    } 
}
