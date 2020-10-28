using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class DropdownController : MonoBehaviour, IPointerDownHandler
{
    public LoadMenuController loadMenuController;

    //Updates the list of files to load upon clicking the dropdown in the Load Menu
    public void OnPointerDown(PointerEventData eventData)
    {
        loadMenuController.UpdateOptions();
    }
}
