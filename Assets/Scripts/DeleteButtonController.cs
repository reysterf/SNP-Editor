using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteButtonController : MonoBehaviour
{
    private GameObject linkedSynapse;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(linkedSynapse == null){
            Destroy(gameObject);
        }
    }

    public void SetSynapse(GameObject synapse){
        linkedSynapse = synapse;
    }

    public void DeleteButtonPress(){
        //Synapses have a child button that will be used to delete the synapses. This will activate when the button is pressed
        // print(gameObject.name);
        // print(gameObject.transform.parent.gameObject.name);
        print("Delete Button Pressed");

        EditorController ec = GameObject.Find("EditorController").GetComponent<EditorController>();
        ec.DeleteSynapse(linkedSynapse.name);
        gameObject.SetActive(false);
        print("Name: " + linkedSynapse.name);
        print("Destroy button");
    }

}
