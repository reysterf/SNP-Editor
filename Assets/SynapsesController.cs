using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynapsesController : MonoBehaviour
{
    public EditorController ec;

    private bool deleteSynapseMode = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeleteSynapseMode(bool mode){
        print("Delete Synapse Mode: " + mode);
        deleteSynapseMode = true;
        BroadcastMessage("DeleteSynapseModeReceiver", mode);
    }

    public void DeleteSynapseTarget(string synapseName){
        print("DeleteSynapseTarget: " + synapseName);
        ec.DeleteSynapse(synapseName);
    }

    public bool GetDeleteSynapseMode(){
        return deleteSynapseMode;
    }

    public void Whaddup(){
        print("Whaddup");
    }
}
