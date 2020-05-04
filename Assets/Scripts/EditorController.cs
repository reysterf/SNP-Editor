using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class EditorController : MonoBehaviour
{
    private int neuronCount = 0;
    private int lineCount = 0;


    public GameObject NeuronPrefab;
    public GameObject NeuronWithRules;
    public GameObject NeuronWithoutRules;
    public GameObject Neurons;
    public GameObject Synapses;

    private bool newSynapseMode = false;
    private bool editNeuronMode = false;
    private bool editRulesMode = false;
    private bool editSpikesMode = false;
    private bool deleteNeuronMode = false;
    private bool deleteSynapseMode = false;

    private Vector3 synapseStart;
    private Vector3 synapseEnd;

    private List<int> neurons = new List<int>();
    private List<(int, int)> synapses = new List<(int, int)>();

    public GameObject editNeuronMenu;
    public GameObject editRulesMenu;
    public GameObject editSpikesMenu;
    public GameObject neuronLabel;

    private GameObject activeNeuronForEditing;

    private string initialString;
    private string modifiedString;

    private GameObject initialGameObject;
    private GameObject modifiedGameObject;

    private bool showRules = true;
    private bool showLabels = true;
    private bool showModeChanged = false;

    public GameObject deleteSynapseButton;

    // Start is called before the first frame update
    void Start()
    {
        editNeuronMenu.SetActive(false);
        editRulesMenu.SetActive(false);


        foreach ((int i, int j) in synapses)
        {
            print(i.ToString() + ", " + j.ToString());
        }
    }

    // Update is called once per frame
    void Update()
    {

        if(!deleteSynapseMode){
            Draw();
        }
        if(showModeChanged){
            if(showLabels){
                //Broadcast show labels
                Neurons.GetComponent<NeuronsController>().ShowLabelMode();
                showModeChanged = false;
            }
            else if(!showLabels){
                //Broadcast hide labels
                Neurons.GetComponent<NeuronsController>().HideLabelMode();
                showModeChanged = false;
            }
            if(showRules){
                //Broadcast show rules
                Neurons.GetComponent<NeuronsController>().ShowRulesMode();
                showModeChanged = false;
            }
            else if(!showRules){
                //Broadcast hide rules
                Neurons.GetComponent<NeuronsController>().HideRulesMode();
                showModeChanged = false;
            }
        }
    }

    public void Draw(){
        foreach ((int i, int j) in synapses)
        {
            DrawLine(i.ToString(), j.ToString());
        }
    }

    private void BlankSlate(){
        //Reset the program state into a blank slate
        DeleteAllNeurons();
    }

    public void ChangeShowLabelMode(){
        showModeChanged = true;
        showLabels = !showLabels;
    }

    public void ChangeShowRulesMode(){
        showModeChanged = true;
        showRules = !showRules;
    }

    public bool isShowRulesMode(){
        return showRules;
    }

    public bool isShowLabelsMode(){
        return showLabels;
    }    

    public bool isNewSynapseMode()
    {
        return newSynapseMode;
    }

    public bool isDeleteSynapseMode(){
        return deleteSynapseMode;
    }

    public void neuronsRefresh(){
        Neurons.SetActive(false);
        Neurons.SetActive(true);
    }


    public void NewNeuron()
    {
        GameObject newron = Instantiate(NeuronPrefab, new Vector3(neuronCount * 2f - 5, UnityEngine.Random.Range(-5f, 5f), 0), Quaternion.identity);
        newron.name = neuronCount.ToString();
        newron.transform.SetParent(Neurons.transform);
        newron.transform.tag = "Neuron";
        neurons.Add(neuronCount); //neuronCount
        neuronCount += 1;

        neuronsRefresh();
        // if(showRules){
        //     GameObject newron = Instantiate(NeuronWithRules, new Vector3(neuronCount * 2f - 5, UnityEngine.Random.Range(-5f, 5f), 0), Quaternion.identity);
        //     newron.name = neuronCount.ToString();
        //     newron.transform.SetParent(Neurons.transform, true);
        //     newron.transform.localScale = new Vector3(.03f, .03f, .1f);
        //     newron.transform.tag = "Neuron";
        //     neurons.Add(neuronCount);
        //     neuronCount += 1;
        // }
        // if(!showRules){
        //     GameObject newron = Instantiate(NeuronWithoutRules, new Vector3(neuronCount * 2f - 5, UnityEngine.Random.Range(-5f, 5f), 0), Quaternion.identity);
        //     newron.name = neuronCount.ToString();
        //     newron.transform.parent = Neurons.transform;
        //     newron.transform.tag = "Neuron";
        //     neurons.Add(neuronCount);
        //     neuronCount += 1;
        // }


        // GameObject newronLabel = Instantiate(neuronLabel, newron.transform.position * 2, Quaternion.identity);
        // newronLabel.transform.SetParent(newron.transform);
        // newronLabel.transform.localScale = new Vector3(.03f, .03f, 0);
        // newronLabel.transform.GetChild(0).gameObject.GetComponent<Text>().text = newron.name;
    }

    public GameObject NewNeuron(int number, bool setActive)
    {
        GameObject newron = Instantiate(NeuronPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        newron.SetActive(setActive);
        newron.name = number.ToString();
        newron.transform.parent = Neurons.transform;
        newron.transform.tag = "Neuron";
        neurons.Add(number); //neuronCount
        neuronCount += 1;

        // GameObject newron = null;

        // if(showRules){
        //     newron = Instantiate(NeuronWithRules, new Vector3(neuronCount * 2f - 5, UnityEngine.Random.Range(-5f, 5f), 0), Quaternion.identity);
        //     newron.SetActive(setActive);
        //     newron.name = number.ToString();
        //     newron.transform.SetParent(Neurons.transform);
        //     newron.transform.tag = "Neuron";
        //     neurons.Add(number);
        //     neuronCount += 1;
        // }
        // else if(!showRules){
        //     newron = Instantiate(NeuronWithoutRules, new Vector3(neuronCount * 2f - 5, UnityEngine.Random.Range(-5f, 5f), 0), Quaternion.identity);
        //     newron.SetActive(setActive);
        //     newron.name = number.ToString();
        //     newron.transform.parent = Neurons.transform;
        //     newron.transform.tag = "Neuron";
        //     neurons.Add(number);
        //     neuronCount += 1;
        // }

        // GameObject newronLabel = Instantiate(neuronLabel, newron.transform.position * 2, Quaternion.identity);
        // newronLabel.transform.SetParent(newron.transform);
        // newronLabel.transform.localScale = new Vector3(.03f, .03f, 0);
        // newronLabel.transform.GetChild(0).gameObject.GetComponent<Text>().text = newron.name;

        return newron;
    }

    public void DeleteSynapseStart(){
        deleteSynapseMode = true;
        Draw();
        Synapses.GetComponent<SynapsesController>().DeleteSynapseMode(true);
    }

    public void DeleteSynapse(string synapseName){
        print("Deleting synapse: " + synapseName);
        GameObject targetSynapse = GameObject.Find(synapseName);

        string sourceNeuron = targetSynapse.GetComponent<SynapseController>().GetSourceNeuron();
        string destNeuron = targetSynapse.GetComponent<SynapseController>().GetDestNeuron();

        synapses.Remove((int.Parse(sourceNeuron), int.Parse(destNeuron)));
        
        GameObject.Find(sourceNeuron).GetComponent<NeuronController>().DeleteOutSynapse(int.Parse(destNeuron));

        Destroy(targetSynapse);

        deleteSynapseMode = false;
        Synapses.GetComponent<SynapsesController>().DeleteSynapseMode(false);

        GameObject[] deleteButtons = GameObject.FindGameObjectsWithTag("Delete Button");
        foreach(GameObject delBut in deleteButtons){
            Destroy(delBut);
        }

        //Synapse reset
        GameObject[] synapsesDelete = GameObject.FindGameObjectsWithTag("Synapse");
        foreach(GameObject synapse in synapsesDelete){
            Destroy(synapse);
        }
    }

    public void DeleteNeuronStart(){
        deleteNeuronMode = true;
        Neurons.GetComponent<NeuronsController>().DeleteNeuronMode(true);
    }

    public void DeleteNeuron(GameObject neuron){
        removeConnectedSynapses(neuron);
        neurons.Remove(int.Parse(neuron.name));
        Destroy(neuron);
        DeleteNeuronEnd();
    }

    public void DeleteNeuronEnd(){
        deleteNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().DeleteNeuronMode(false);        
    }

    public void DeleteAllNeurons(){
        GameObject[] neurons = GameObject.FindGameObjectsWithTag("Neuron");
        foreach(GameObject neuron in neurons){
            DeleteNeuron(neuron);
        }
    }

    private void removeConnectedSynapses(GameObject neuron){
        int n = int.Parse(neuron.name);
        int k = 0;

        List<int> indexToRemove = new List<int>();

        foreach ((int i, int j) in synapses)
        {
            if (i == n || j == n){
                indexToRemove.Add(k);
            }
            k += 1;
        }
        
        int indexOffset = 0;
        foreach (int i in indexToRemove){
            synapses.RemoveAt(i - indexOffset);
            indexOffset += 1;
        }
    }

    public void EditNeuronStart(){
        editNeuronMode = true;
        Neurons.GetComponent<NeuronsController>().EditNeuronMode(true);
    }

    public void EditNeuron(GameObject neuron){
        editNeuronMenu.SetActive(true);

        activeNeuronForEditing = neuron;

        /// EDIT NEURON WILL BE REPURPOSED TO JUST GET THE ACTIVE NEURON FOR EDITING
        /// UNCOMMENT BELOW TO RETURN TO OLD FUNCTIONALITY

        // Text spikesText = editNeuronMenu.transform.Find("Spikes").transform.Find("Spikes Text").GetComponent<Text>();
        // spikesText.text = "Spikes: " + neuron.GetComponent<NeuronController>().GetSpikes().ToString();
        
        // List<string> rules = neuron.GetComponent<NeuronController>().GetRules();
        // string rulesString = string.Join("\n", rules.ToArray());

        // Text rulesText = editNeuronMenu.transform.Find("Rules").transform.Find("Rules Text").GetComponent<Text>();
        // rulesText.text = rulesString;
    }

    public void EditNeuron(GameObject neuron, string mode){
        activeNeuronForEditing = neuron;
        if(mode == "spikes"){
            EditSpikesStart();
        }  
        else if(mode == "rules"){
            EditRulesStart();
        }
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
        editRulesMenu.transform.position = activeNeuronForEditing.transform.position;

        EditRules();
    }

    public void EditRules(){
        GameObject initialGameObject = Instantiate(activeNeuronForEditing);
        initialGameObject.SetActive(false);
        GameObject neuron = activeNeuronForEditing;

        InputField rulesInputField = editRulesMenu.transform.Find("Rules InputField").GetComponent<InputField>();

        List<string> rules = neuron.GetComponent<NeuronController>().GetRules();
        string rulesString = string.Join("\n", rules.ToArray());

        initialString = rulesString;

        rulesInputField.text = rulesString;
    }

    public void EditRulesConfirm(){
        editRulesMode = false;
        editRulesMenu.SetActive(false);

        InputField rulesInputField = editRulesMenu.transform.Find("Rules InputField").GetComponent<InputField>();

        activeNeuronForEditing.GetComponent<NeuronController>().SetRules(rulesInputField.text);
    }

    public void EditRulesCancel(){
        editRulesMode = false;
        editRulesMenu.SetActive(false);
    }

    public void EditSpikesStart(){
        editSpikesMode = true;
        editSpikesMenu.SetActive(true);
        editSpikesMenu.transform.position = activeNeuronForEditing.transform.position;

        EditSpikes();
    }

    public void EditSpikes(){
        print("EditorController EditSpikes");
        InputField spikesInputField = editSpikesMenu.transform.Find("Spikes InputField").GetComponent<InputField>();

        // List<string> rules = neuron.GetComponent<NeuronController>().GetRules();
        // string rulesString = string.Join("\n", rules.ToArray());

        spikesInputField.text = activeNeuronForEditing.GetComponent<NeuronController>().GetSpikesNum().ToString();
        print(spikesInputField.text);
    }

    public void EditSpikesConfirm(){
        InputField spikesInputField = editSpikesMenu.transform.Find("Spikes InputField").GetComponent<InputField>();

        editSpikesMode = false;
        editSpikesMenu.SetActive(false);

        activeNeuronForEditing.GetComponent<NeuronController>().SetSpikes(int.Parse(spikesInputField.text));
    }

    public void EditSpikesCancel(){
        editSpikesMode = false;
        editSpikesMenu.SetActive(false);

    }

    public void NewSynapseStart()
    {  
        newSynapseMode = true; 
        Neurons.GetComponent<NeuronsController>().NewSynapseMode(true);
    }

    

    public void NewSynapse(string sourceNeuronName, string destNeuronName){
        // synapseStart = v1;
        // synapseEnd = v2;
        int sourceNeuron = int.Parse(sourceNeuronName);
        int destNeuron = int.Parse(destNeuronName);

        bool synapseExists = false;
        foreach ((int i, int j) in synapses)
        {
            if(i == sourceNeuron && j == destNeuron){
                synapseExists = true;
            }
        }

        if(!synapseExists){
            synapses.Add((sourceNeuron, destNeuron));
        }

        //add outSynapse to source neuron
        GameObject.Find(sourceNeuronName).GetComponent<NeuronController>().AddOutSynapse(int.Parse(destNeuronName));

        DrawLine(sourceNeuronName, destNeuronName);
        NewSynapseEnd();
    }

    public void NewSynapseEnd(){
        newSynapseMode = false;
    }

    public void testPrintSynapse(){
        print("[Synapses]");
        foreach ((int i, int j) in synapses)
        {
            print(i.ToString() + " " + j.ToString());            
        }
    }

    public void testPrintNeurons(){
        print("[Neurons]");
        foreach(int i in neurons){
            print(i.ToString());
        }
    }

    public void DrawLine(string sourceNeuronName, string destNeuronName, float duration = 0.03f)
    {
        Vector3 start = GameObject.Find(sourceNeuronName).transform.position;
        Vector3 end = GameObject.Find(destNeuronName).transform.position;

        GameObject newSynapse = new GameObject();
        newSynapse.transform.position = start;
        newSynapse.transform.parent = Synapses.transform;
        newSynapse.transform.tag = "Synapse";
        newSynapse.name = sourceNeuronName + destNeuronName;
        newSynapse.AddComponent<LineRenderer>();
        newSynapse.AddComponent<SynapseController>();
        newSynapse.GetComponent<SynapseController>().SetSourceNeuron(sourceNeuronName);
        newSynapse.GetComponent<SynapseController>().SetDestNeuron(destNeuronName);


        LineRenderer lr = newSynapse.GetComponent<LineRenderer>();
        // lr.SetColors(color, color);
        // lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = 0.5f;
        lr.endWidth = 0.5f;
        if(deleteSynapseMode){
            GameObject delbut = Instantiate(deleteSynapseButton, (start + end) * 0.5f, Quaternion.identity);
            delbut.transform.localScale = new Vector3(.03f, .03f, 0);
            delbut.transform.SetParent(newSynapse.transform);
            delbut.transform.tag = "Delete Button";
        }
        if(!deleteSynapseMode){
            GameObject.Destroy(newSynapse, duration);
        }
        lineCount += 1;
    }

    public void StartFire()
    {
        foreach ((int i, int j) in synapses)
        {
            Neurons.GetComponent<NeuronsController>().Fire(GameObject.Find("Neurons/" + i.ToString()), GameObject.Find("Neurons/" + j.ToString()));
        }
    }

    public void Save(){
        var path = EditorUtility.SaveFilePanel(
        "Save as",
        "",
        "Untitled.snapse",
        "snapse");

        if (path.Length != 0)
        {
            var configData = System.Text.Encoding.UTF8.GetBytes(EncodeToFormat());
            if (configData != null)
                File.WriteAllBytes(path, configData);
        }
    }

    public void Load(){
        string path = EditorUtility.OpenFilePanel("Load snapse file", "", "snapse");
        if (path.Length != 0)
        {
            var fileContent = File.ReadAllBytes(path);
            string formatData = System.Text.Encoding.UTF8.GetString(fileContent);
            DecodeFromFormat(formatData);
        }
    }


    public string EncodeToFormat(){
        string lineEnder = ":";
        string format = "";
        
        string neuronsDeclaration = "neurons = [";
        int i = 0;
        foreach (int neuron in neurons){
            neuronsDeclaration += "N" + neuron.ToString();
            i += 1;
            if (i < neurons.Count){
                neuronsDeclaration += ", ";
            }
        }
        neuronsDeclaration += "]";

        format += neuronsDeclaration + lineEnder;

        format += "\n";

        string neuronsDefinition = "";

        foreach (int neuron in neurons){
            string neuronDefinition = "";
            GameObject neuronToEncode = GameObject.Find(neuron.ToString());

            neuronDefinition += "N" + neuron.ToString() + "{\n";

            //spikes
            int neuronSpikes = neuronToEncode.GetComponent<NeuronController>().GetSpikesNum();
            neuronDefinition += "\tspikes = " + neuronSpikes.ToString() + lineEnder;

            neuronDefinition += "\n";

            //rules
            string neuronRules = "{";
            List<string> rules = neuronToEncode.GetComponent<NeuronController>().GetRules();

            i = 0;
            foreach (string rule in rules){
                neuronRules += "[" + rule + "]";
                i+=1;
                if (i < rules.Count){
                    neuronRules += ", ";
                }
            }
            neuronRules += "}";

            neuronDefinition += "\trules = " + neuronRules + lineEnder;

            neuronDefinition += "\n";

            //outsynapses
            List<int> outSynapses = neuronToEncode.GetComponent<NeuronController>().GetOutSynapses();
            string neuronOutSynapses = "[";

            i = 0;
            foreach (int outSynapse in outSynapses){
                neuronOutSynapses += "N" + outSynapse.ToString();
                i += 1;
                if (i < outSynapses.Count){
                    neuronOutSynapses += ", ";
                }
            }

            neuronOutSynapses += "]";

            neuronDefinition += "\toutsynapses = " + neuronOutSynapses + lineEnder;

            neuronDefinition += "\n";

            neuronDefinition += "}";

            neuronDefinition += "\n";

            //add neuron definition to neurons definition
            neuronsDefinition += neuronDefinition;

            // print(neuronDefinition);
        }

        format += neuronsDefinition;

        return format;
    }

    public void testEncodeToFormat(){
        print(EncodeToFormat());        
    }

    public void DecodeFromFormat(string formatData){
        BlankSlate();
        formatData = Regex.Replace(formatData, @"\s+", "");

        char[] separators = {'{', '}', '=', ':' };
        string[] strValues = formatData.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        int i = 0;
        foreach(string str in strValues)
        {
            print(i.ToString() + ": " + str);
            i+=1;
        }

        //Parse neuron declaration
        string[] neuronDeclarations = strValues[1].Split(new char[] {'[', ']', ',', 'N'}, StringSplitOptions.RemoveEmptyEntries);

        List<GameObject> neurons = new List<GameObject>();

        foreach(string neuronDeclaration in neuronDeclarations){
            neurons.Add(NewNeuron(int.Parse(neuronDeclaration), true));
            print(neuronDeclaration);
        }

        int n = 0;
        foreach(GameObject neuron in neurons){
            neuron.GetComponent<NeuronController>().SetSpikes(int.Parse(strValues[4 + 7*n]));

            //parse rules
            string rules = strValues[6 + 7*n];
            rules = string.Join("\n", rules.Split(new char[] {'[', ']', ','}, StringSplitOptions.RemoveEmptyEntries));
            neuron.GetComponent<NeuronController>().SetRules(rules);

            //parse outsynapses
            string outSynapses = strValues[8 + 7*n];
            string[] outSynapsesArray = outSynapses.Split(new char[] {'[', ']', ',', 'N'}, StringSplitOptions.RemoveEmptyEntries);
            List<int> outSynapsesList = new List<int>();
            foreach(string outSynapse in outSynapsesArray){
                NewSynapse(neuron.name, outSynapse);
                outSynapsesList.Add(int.Parse(outSynapse));
            }
            neuron.GetComponent<NeuronController>().SetOutSynapses(outSynapsesList);

            n+=1;
        }
    }

    public void testDecodeFromFormat(){
        Load();
    }
}
