using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorController : MonoBehaviour
{
    private int neuronCount = 0;
    private int lineCount = 0;


    public GameObject NeuronPrefab;
    public GameObject Neurons;

    private bool newSynapseMode = false;
    private bool editNeuronMode = false;
    private bool editRulesMode = false;

    private Vector3 synapseStart;
    private Vector3 synapseEnd;

    private List<int> neurons = new List<int>();
    private List<(int, int)> synapses = new List<(int, int)>();

    public GameObject editNeuronMenu;
    public GameObject editRulesMenu;

    private GameObject activeNeuronForEditing;

    private string initialString;
    private string modifiedString;

    private GameObject initialGameObject;
    private GameObject modifiedGameObject;

    // Start is called before the first frame update
    void Start()
    {
        editNeuronMenu.SetActive(false);
        editRulesMenu.SetActive(false);
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

        activeNeuronForEditing = neuron;

        Text spikesText = editNeuronMenu.transform.Find("Spikes").transform.Find("Spikes Text").GetComponent<Text>();
        spikesText.text = "Spikes: " + neuron.GetComponent<NeuronController>().GetSpikes().ToString();
        
        List<string> rules = neuron.GetComponent<NeuronController>().GetRules();
        string rulesString = string.Join("\n", rules.ToArray());

        Text rulesText = editNeuronMenu.transform.Find("Rules").transform.Find("Rules Text").GetComponent<Text>();
        rulesText.text = rulesString;
    }

    public void EditNeuronCancel(){
        //// Add Functionality: "DON'T SAVE CHANGES"

        editNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);
        editNeuronMenu.SetActive(false);
    }

    public void EditNeuronSave(){
        //// Add Functionality: "SAVE CHANGES"

        editNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);
        editNeuronMenu.SetActive(false);
    }

    public void EditRulesStart(){
        editRulesMode = true;
        editRulesMenu.SetActive(true);

        EditRules();
    }

    public void EditRules(){
        GameObject initialGameObject = Instantiate(activeNeuronForEditing);
        initialGameObject.SetActive(false);
        GameObject neuron = activeNeuronForEditing;

        InputField rulesInputField = editRulesMenu.transform.Find("Rules").transform.Find("Rules InputField").GetComponent<InputField>();

        List<string> rules = neuron.GetComponent<NeuronController>().GetRules();
        string rulesString = string.Join("\n", rules.ToArray());

        initialString = rulesString;

        rulesInputField.text = rulesString;
    }

    public void EditRulesSave(){
        editRulesMode = false;
        editRulesMenu.SetActive(false);

        InputField rulesInputField = editRulesMenu.transform.Find("Rules").transform.Find("Rules InputField").GetComponent<InputField>();

        activeNeuronForEditing.GetComponent<NeuronController>().SetRules(rulesInputField.text);
    }

    public void EditRulesCancel(){
        editRulesMode = false;
        editRulesMenu.SetActive(false);

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

    public void StartFire()
    {
        foreach ((int i, int j) in synapses)
        {
            Neurons.GetComponent<NeuronsController>().Fire(GameObject.Find("Neurons/" + i.ToString()), GameObject.Find("Neurons/" + j.ToString()));
        }
    }
}
