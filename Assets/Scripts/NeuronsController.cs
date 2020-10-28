using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Handles all NeuronControllers
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

    public void LoadedFile() {
        Destroy(GetComponent<GridLayoutGroup>()); // For auto layout;
    }

    // Broadcasts delete neuron mode messages
    public void DeleteNeuronMode(bool mode) {
        print("Delete Neuron Mode");
        BroadcastMessage("DeleteNeuronModeReceiver", mode);
    }

    public void DeleteNeuronTarget(GameObject neuron) {
        print("DeleteNeuronTarget: " + neuron.name);
        ec.DeleteNeuron(neuron);
    }

    // Broadcasts edit neuron mode messages
    public void EditNeuronMode(bool mode) {
        print("Edit Neuron Mode");
        BroadcastMessage("EditNeuronModeReceiver", mode);
    }

    // Broadcasts new synapse mode messages
    public void NewSynapseMode(bool mode) {
        BroadcastMessage("NewSynapseModeReceiver", mode);
        BroadcastMessage("SynapseV1ModeReceiver", true);
        if(mode){
            ec.SetStatusText("New Synapse: Click source neuron");
        }
    }

    // Handles and broadcasts synapse source neuron messages
    public void SynapseCoordinate1(GameObject neuron) {
        v1 = neuron.transform.position;
        sourceNeuronName = neuron.name;
        BroadcastMessage("SynapseV1ModeReceiver", false);
        BroadcastMessage("SynapseV2ModeReceiver", true);
        ec.SetStatusText("New Synapse: Click destination neuron");
    }

    // Handles and broadcasts synapse destination neuron messages
    public void SynapseCoordinate2(GameObject neuron) {
        v2 = neuron.transform.position;
        destNeuronName = neuron.name;
        BroadcastMessage("SynapseV2ModeReceiver", false);
        BroadcastMessage("NewSynapseModeReceiver", false);
        ec.NewSynapse(sourceNeuronName, destNeuronName);
    }

    public void ShowLabelMode() {
        BroadcastMessage("ShowLabel");
    }

    public void HideLabelMode() {
        BroadcastMessage("HideLabel");
    }

    public void ShowRulesMode() {
        BroadcastMessage("ShowRules");
    }

    public void HideRulesMode() {
        BroadcastMessage("HideRules");
    }

    //Function that tells a neuron to start firing (check rules)
    public (List<string>, string) Fire(GameObject shootingNeuron)
    {
        return shootingNeuron.GetComponent<NeuronController>().StartFire();
    }

    //Function that tells a neuron to end firing
    public void EndFireNeurons(GameObject shootingNeuron)
    {
        shootingNeuron.GetComponent<NeuronController>().EndFire();
    }

    //Function that tells a neuron to set the chosen rule after guided choosing
    public string UpdateOutputNeurons(GameObject outputNeuron){
        return outputNeuron.GetComponent<NeuronController>().UpdateOutput();
    }

    //Function that tells a neuron to delete the last digit of the output bitstring
    //Used in going back one time step
    public string RetractOutput(GameObject outputNeuron)
    {
        return outputNeuron.GetComponent<NeuronController>().Retract();
    }

    //Function that clears the output bitstring of a neuron
    public void ClearOutput(GameObject outputNeuron)
    {
        outputNeuron.GetComponent<NeuronController>().ClearOutput();
    }

    //Function that tells a neuron to set the chosen rule after guided choosing
    public void SetChosenRule(GameObject neuron, string chosenRule)
    {
        neuron.GetComponent<NeuronController>().ProcessRule(chosenRule);
    }
}
