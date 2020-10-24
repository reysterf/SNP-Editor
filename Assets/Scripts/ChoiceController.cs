using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceController : MonoBehaviour
{
    public void ChoiceButtonPress()
    {
        EditorController ec = GameObject.Find("EditorController").GetComponent<EditorController>();
        ec.GoToChoice();
    }
}
