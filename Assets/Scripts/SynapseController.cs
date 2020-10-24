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


    // Update is called once per frame
    void Update()
    {
        if(sourceNeuronName != null || sourceNeuronName != "" || sourceNeuron == null){
            sourceNeuron =  GameObject.Find(sourceNeuronName);
        }
        if(destNeuronName != null || destNeuronName != "" || destNeuron == null){
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
        distance = Vector3.Distance(sourceNeuron.transform.localPosition, destNeuron.transform.localPosition);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(distance, 30);
        transform.position = sourceNeuron.transform.position;
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

    public void DeleteSynapseTarget(){
        SendMessageUpwards("DeleteSynapseTarget", gameObject.name);
    }

}
