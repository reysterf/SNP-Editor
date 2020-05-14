using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public void LoadedFile() {
        Destroy(GetComponent<GridLayoutGroup>()); // For auto layout;
    }

    public void DeleteNeuronMode(bool mode) {
        print("Delete Neuron Mode");
        BroadcastMessage("DeleteNeuronModeReceiver", mode);
    }

    public void DeleteNeuronTarget(GameObject neuron) {
        print("DeleteNeuronTarget: " + neuron.name);
        ec.DeleteNeuron(neuron);
    }

    public void EditNeuronMode(bool mode) {
        print("Edit Neuron Mode");
        BroadcastMessage("EditNeuronModeReceiver", mode);
    }

    public void EditNeuronTarget(GameObject neuron) {
        print("EditNeuronTarget: " + neuron.name);
        ec.EditNeuron(neuron);
    }

    public void NewSynapseMode(bool mode) {
        BroadcastMessage("NewSynapseModeReceiver", mode);
        BroadcastMessage("SynapseV1ModeReceiver", true);
        if(mode){
            ec.SetStatusText("New Synapse: Click source neuron");
        }
    }

    public void SynapseCoordinate1(GameObject neuron) {
        v1 = neuron.transform.position;
        sourceNeuronName = neuron.name;
        BroadcastMessage("SynapseV1ModeReceiver", false);
        BroadcastMessage("SynapseV2ModeReceiver", true);
        ec.SetStatusText("New Synapse: Click destination neuron");
    }

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

    public (List<string>, string) Fire(GameObject shootingNeuron, List<GameObject> targetNeurons)
    {
        return shootingNeuron.GetComponent<NeuronController>().FireOneStep(targetNeurons);
    }

    public string EndFire(GameObject outputNeuron){
        return outputNeuron.GetComponent<NeuronController>().SignalEnd();
    }
}
