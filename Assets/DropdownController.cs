using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class DropdownController : MonoBehaviour, IPointerDownHandler
{
    public LoadMenuController loadMenuController;

    public void OnPointerDown(PointerEventData eventData)
    {
        loadMenuController.UpdateOptions();
    }
}
