using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuronsController : MonoBehaviour
{
    Vector3 v1;
    Vector3 v2;
    string sourceNeuronName;
    string destNeuronName;
    public GameObject EditorController;
    EditorController ec;
    // Start is called before the first frame update
    void Start()
    {
        ec = EditorController.GetComponent<EditorController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeleteNeuronMode(bool mode){
        print("Delete Neuron Mode");
        BroadcastMessage("DeleteNeuronModeReceiver", mode);
    }

    public void DeleteNeuronTarget(GameObject neuron){
        print("DeleteNeuronTarget: " + neuron.name);
        ec.DeleteNeuron(neuron);
    }

    public void EditNeuronMode(bool mode){
        print("Edit Neuron Mode");
        BroadcastMessage("EditNeuronModeReceiver", mode);
    }

    public void EditNeuronTarget(GameObject neuron){
        print("EditNeuronTarget: " + neuron.name);
        ec.EditNeuron(neuron);
    }

    public void NewSynapseMode(bool mode){
        BroadcastMessage("NewSynapseModeReceiver", mode);
        BroadcastMessage("SynapseV1ModeReceiver", true);
    }

    public void SynapseCoordinate1(GameObject neuron){
        v1 = neuron.transform.position;
        sourceNeuronName = neuron.name;
        BroadcastMessage("SynapseV1ModeReceiver", false);
        BroadcastMessage("SynapseV2ModeReceiver", true);
    }

    public void SynapseCoordinate2(GameObject neuron){
        v2 = neuron.transform.position;
        destNeuronName = neuron.name;
        BroadcastMessage("SynapseV2ModeReceiver", false);
        BroadcastMessage("NewSynapseModeReceiver", false);
        ec.NewSynapse(sourceNeuronName, destNeuronName);
    }

    public void ShowLabelMode(){
        BroadcastMessage("ShowLabel");
    }

    public void HideLabelMode(){
        BroadcastMessage("HideLabel");
    }

    public void ShowRulesMode(){
        BroadcastMessage("ShowRules");
    }

    public void HideRulesMode(){
        BroadcastMessage("HideRules");
    }

    public void Fire(GameObject shootingNeuron, GameObject targetNeuron)
    {
        shootingNeuron.GetComponent<NeuronController>().FireOneStep(targetNeuron);
    }
}
