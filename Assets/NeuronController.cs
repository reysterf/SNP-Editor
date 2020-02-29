using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuronController : MonoBehaviour
{
    public GameObject neuron;
    public GameObject EditorController;
    public EditorController ec;
    private bool newSynapseMode = false;
    private bool synapseV1Mode = false;
    private bool synapseV2Mode = false;

    private bool editNeuronMode = false;

    private Vector3 screenPoint;
    private Vector3 offset;

    private int spikes = 0;
    private List<string> rules = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        EditorController = GameObject.Find("EditorController");
        ec = EditorController.GetComponent<EditorController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void EditNeuronModeReceiver(bool mode){
        editNeuronMode = mode;
    }

    void NewSynapseModeReceiver(bool mode)
    {
        newSynapseMode = mode;
    }

    void SynapseV1ModeReceiver(bool mode)
    {
        synapseV1Mode = mode;
    }

    void SynapseV2ModeReceiver(bool mode)
    {
        synapseV2Mode = mode;
    }

    void OnMouseDrag()
    {
        Vector3 cursorScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint (cursorScreenPoint) + offset;
        transform.position = cursorPosition;
        // ec.Draw();
    }



    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

        // EditorController ec = EditorController.GetComponent<EditorController>();
        // Debug.Log("OOF");
        // ec.nemu();
        if(newSynapseMode && synapseV1Mode){
            Debug.Log("UNO");
            var x = new {position = gameObject.transform.position, name = gameObject.name};
            SendMessageUpwards("SynapseCoordinate1", gameObject);

        }
        else if(newSynapseMode && synapseV2Mode){
            Debug.Log("DOS");
            var x = new {position = gameObject.transform.position, name = gameObject.name};
            SendMessageUpwards("SynapseCoordinate2", gameObject);
        }
        else if(editNeuronMode){
            SendMessageUpwards("EditNeuronTarget", gameObject);
        }
    }

    void OnMouseOver()
    {
        //If your mouse hovers over the GameObject with the script attached, output this message
        // Debug.Log("Mouse is over GameObject." + neuron.name);
        // Debug.Log();
    }

    void OnMouseExit()
    {
        //The mouse is no longer hovering over the GameObject so output this message each frame
        // Debug.Log("Mouse is no longer on GameObject.");
    }
}
