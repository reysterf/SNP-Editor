using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownController : MonoBehaviour
{
    void Start()
    {
        List<string> options = new List<string>();
        options.Add("Woho");
        options.Add("Wooho");
        options.Add("Wohoo");
        //Add Item
        Dropdown dd = GetComponent<Dropdown>();
        dd.AddOptions(options);
    }

}
