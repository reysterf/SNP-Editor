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
    private bool editNeuronMode = false;
    private Vector3 synapseStart;
    private Vector3 synapseEnd;

    private List<int> neurons = new List<int>();
    private List<(int, int)> synapses = new List<(int, int)>();

    public GameObject editNeuronMenu;

    // Start is called before the first frame update
    void Start()
    {
        editNeuronMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Draw();
    }

    public void Draw(){
        foreach ((int i, int j) in synapses)
        {
            DrawLine(GameObject.Find("Neurons/"+i.ToString()).transform.position, GameObject.Find("Neurons/"+j.ToString()).transform.position);
        }
    }

    private void OnMouseDrag()
    {
        
    }

    public bool isNewSynapseMode()
    {
        return newSynapseMode;
    }

    public void NewNeuron()
    {
        GameObject newron = Instantiate(NeuronPrefab, new Vector3(neuronCount * 2f - 5, Random.Range(-5f, 5f), 0), Quaternion.identity);
        newron.name = neuronCount.ToString();
        newron.transform.parent = Neurons.transform;
        neurons.Add(neuronCount);
        neuronCount += 1;
    }

    public void EditNeuronStart(){
        editNeuronMode = true;
        Neurons.GetComponent<NeuronsController>().EditNeuronMode(true);
    }

    public void EditNeuron(GameObject neuron){
        editNeuronMenu.SetActive(true);
    }

    public void NewSynapseStart()
    {  
        newSynapseMode = true; 
        Neurons.GetComponent<NeuronsController>().NewSynapseMode(true);
    }

    public void NewSynapseEnd(Vector3 v1, Vector3 v2, string sourceNeuronName, string destNeuronName){
        synapseStart = v1;
        synapseEnd = v2;
        synapses.Add((int.Parse(sourceNeuronName), int.Parse(destNeuronName)));
        DrawLine(v1, v2);
        newSynapseMode = false;
    }

    public void testPrintSynapse(){
        foreach ((int i, int j) in synapses)
            {
                print(i.ToString() + " " + j.ToString());
                
            }
    }

    public void DrawLine(Vector3 start, Vector3 end, float duration = 0.03f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        // lr.SetColors(color, color);
        lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(myLine, duration);
        lineCount += 1;
    }
}
