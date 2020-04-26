using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynapseController : MonoBehaviour
{
    private bool deleteSynapseMode = false;

    private Vector3 screenPoint;
    private Vector3 offset;

    private string sourceNeuronName;
    private string destNeuronName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown(){
        // screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        // offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

        // print("Oh Synapse");
        // if(deleteSynapseMode){
        //     SendMessageUpwards("DeleteSynapseTarget", gameObject);
        // }
    }

    public void SetSourceNeuron(string neuronName){
        sourceNeuronName = neuronName;
    }

    public void SetDestNeuron(string neuronName){
        destNeuronName = neuronName;
    }

    public string GetSourceNeuron(){
        return sourceNeuronName;
    }

    public string GetDestNeuron(){
        return destNeuronName;
    }

    public void DeleteSynapseModeReceiver(bool mode){
        print("Delete Synapse Received: " + mode);
        deleteSynapseMode = mode;
    }

    public void FetchDeleteSynapseMode(){
        // GameObject.Find("Synapses").GetComponent<SynapsesController>().
    }

    public void DeleteSynapseTarget(){
        print("WHUUUUT");
        SendMessageUpwards("DeleteSynapseTarget", gameObject.name);
    }

}
