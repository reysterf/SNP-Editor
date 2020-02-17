using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuronsController : MonoBehaviour
{
    Vector3 v1;
    Vector3 v2;
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

    public void NewSynapseMode(bool mode){
        BroadcastMessage("NewSynapseModeReceiver", mode);
        BroadcastMessage("SynapseV1ModeReceiver", true);
    }

    public void SynapseCoordinate1(Vector3 vector){
        v1 = vector;
        BroadcastMessage("SynapseV1ModeReceiver", false);
        BroadcastMessage("SynapseV2ModeReceiver", true);
    }

    public void SynapseCoordinate2(Vector3 vector){
        v2 = vector;
        BroadcastMessage("SynapseV2ModeReceiver", false);
        BroadcastMessage("NewSynapseModeReceiver", false);
        ec.NewSynapseEnd(v1, v2);
    }
}
