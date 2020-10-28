using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynapsesController : MonoBehaviour
{
    public EditorController ec;

    private bool deleteSynapseMode = true;
    
    // Handles delete synapse message
    public void DeleteSynapseMode(bool mode){
        print("Delete Synapse Mode: " + mode);
        deleteSynapseMode = true;
        BroadcastMessage("DeleteSynapseModeReceiver", mode);
    }

    // Handles delete synapse target message
    public void DeleteSynapseTarget(string synapseName){
        print("DeleteSynapseTarget: " + synapseName);
        ec.DeleteSynapse(synapseName);
    }

    public bool GetDeleteSynapseMode(){
        return deleteSynapseMode;
    }
}
