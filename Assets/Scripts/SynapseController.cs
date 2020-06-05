using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class SynapseController : MonoBehaviour
{
    private bool deleteSynapseMode = false;

    private Vector3 screenPoint;
    private Vector3 offset;

    private string sourceNeuronName = "";
    private string destNeuronName = "";

    GameObject sourceNeuron;
    GameObject destNeuron;

    public GameObject deleteButton;

    float distance = 0;

    // Start is called before the first frame update
    void Start()
    {
        // if(sourceNeuronName != null || sourceNeuronName != ""){
        //     sourceNeuron =  GameObject.Find(sourceNeuronName);
        // }
        // if(destNeuronName != null || destNeuronName != ""){
        //     destNeuron = GameObject.Find(destNeuronName);
        // }
    }

    // Update is called once per frame
    void Update()
    {
        if(sourceNeuronName != null || sourceNeuronName != "" || sourceNeuron == null){
            // print("SN: " + sourceNeuronName);
            sourceNeuron =  GameObject.Find(sourceNeuronName);
        }
        if(destNeuronName != null || destNeuronName != "" || destNeuron == null){
            // print("DN: " + destNeuronName);
            destNeuron = GameObject.Find(destNeuronName);

        //Set Rotation
        if(gameObject.GetComponent<AimConstraint>().GetSource(0).sourceTransform == null){
            print("Walang source yung AimConstraint");
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = destNeuron.transform;
            source.weight = 1;
            gameObject.GetComponent<AimConstraint>().AddSource(source);
        }

        }
        // if(sourceNeuron && destNeuron){s
        // print(sourceNeuron.transform.position);
        distance = Vector3.Distance(sourceNeuron.transform.localPosition, destNeuron.transform.localPosition);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(distance, 30);
        transform.position = sourceNeuron.transform.position;
        // }
    }

    void OnMouseDown(){
        // screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        // offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

        // print("Oh Synapse");
        // if(deleteSynapseMode){
        //     SendMessageUpwards("DeleteSynapseTarget", gameObject);
        // }
    }

    public void SetSourceNeuronName(string neuronName){
        sourceNeuronName = neuronName;
    }

    public void SetDestNeuronName(string neuronName){
        destNeuronName = neuronName;
    }

    public void SetSourceNeuron(string neuronName){
        sourceNeuronName = neuronName;
        sourceNeuron =  GameObject.Find(neuronName);
    }

    public void SetSourceNeuron(GameObject neuron){
        sourceNeuron = neuron;
    }

    public void SetDestNeuron(string neuronName){
        destNeuronName = neuronName;
        destNeuron = GameObject.Find(neuronName);
    }

    public void SetDestNeuron(GameObject neuron){
        destNeuron = neuron;
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
        if(deleteSynapseMode){
            GameObject delbut = Instantiate(deleteButton, (sourceNeuron.transform.position * 0.6f + destNeuron.transform.position * 0.4f), Quaternion.identity);
            delbut.transform.localScale = new Vector3(.015f, .015f, 0);
            delbut.transform.SetParent(gameObject.transform.parent.transform);
            delbut.GetComponent<DeleteButtonController>().SetSynapse(gameObject);
            delbut.transform.tag = "Delete Button";
        }
        else if(!deleteSynapseMode){

        }
    }

    public void FetchDeleteSynapseMode(){
        // GameObject.Find("Synapses").GetComponent<SynapsesController>().
    }

    public void DeleteSynapseTarget(){
        SendMessageUpwards("DeleteSynapseTarget", gameObject.name);
    }

}
