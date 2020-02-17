using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorController : MonoBehaviour
{
    private int neuronCount = 0;
    private int lineCount = 0;
    public GameObject NeuronPrefab;
    public GameObject Neurons;
    private bool newSynapseMode = false;
    private Vector3 synapseStart;
    private Vector3 synapseEnd;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void nemu()
    {
        Debug.Log("DOG");
    }

    public bool isNewSynapseMode()
    {
        return newSynapseMode;
    }

    public void NewNeuron()
    {
        GameObject newron = Instantiate(NeuronPrefab, new Vector3(neuronCount * 2f - 5, Random.Range(-5f, 5f), 0), Quaternion.identity);
        newron.transform.parent = Neurons.transform;
        neuronCount += 1;
    }


    public void NewSynapseStart()
    {  
        newSynapseMode = true; 
        Neurons.GetComponent<NeuronsController>().NewSynapseMode(true);
    }

    public void NewSynapseEnd(Vector3 v1, Vector3 v2){
        synapseStart = v1;
        synapseEnd = v2;
        DrawLine(v1, v2);
    }

    public void DrawLine(Vector3 start, Vector3 end, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        // lr.SetColors(color, color);
        lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        // GameObject.Destroy(myLine, duration);
        lineCount += 1;
    }
}
