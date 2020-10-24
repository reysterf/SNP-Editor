using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynapsesController : MonoBehaviour
{
    public EditorController ec;

    private bool deleteSynapseMode = true;

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
}
