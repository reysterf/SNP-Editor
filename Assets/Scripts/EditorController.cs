using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class EditorController : MonoBehaviour
{
    private int neuronCount = 0;
    private int lineCount = 0;
    private int globalTime = 0;

    public GameObject MainCamera;
    public GameObject cameraCenterArea;

    private Vector3 screenPoint;
    private Vector3 offset;
    private Vector3 prevPosition;
    private float panSensitivity = 1;

    public GameObject Buttons;

    public GameObject choiceButtonPrefab;
    public GameObject NeuronPrefab;
    public GameObject NeuronWithRules;
    public GameObject NeuronWithoutRules;
    public GameObject Neurons;
    public GameObject Synapses;

    private bool freeMode = true;
    private bool freeModeChanged = false;
    private bool newSynapseMode = false;
    private bool editNeuronMode = false;
    private bool editRulesMode = false;
    private bool editSpikesMode = false;
    private bool deleteNeuronMode = false;
    private bool deleteSynapseMode = false;

    private bool panMode = false;

    private Vector3 synapseStart;
    private Vector3 synapseEnd;

    private List<int> neurons = new List<int>();
    private List<(int, int)> synapses = new List<(int, int)>();


    public GameObject statusBar;
    public GameObject ChoiceMenu;
    public GameObject choiceContent;
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

    public Text showRulesText;
    public Text showLabelsText;

    public GameObject deleteSynapseButton;

    public Material white;

    private string lastData;
    public List<(List<string>, string, string)> appliedRulesStorage;
    public ChoiceNode root;
    public ChoiceNode last;
    public List<int> choiceTimes;
    public List<List<int>> configHistory;

    // Start is called before the first frame update
    void Start()
    {
        editNeuronMenu.SetActive(false);
        editRulesMenu.SetActive(false);
        editSpikesMenu.SetActive(false);
        lastData = null;
        root = null;
        configHistory = new List<List<int>>();
        choiceTimes = new List<int>();
        appliedRulesStorage = new List<(List<string>, string, string)>();

        showLabelsText.text = "Hide Labels";
        showRulesText.text = "Hide Rules";

        foreach ((int i, int j) in synapses)
        {
            print(i.ToString() + ", " + j.ToString());
        }

        SetStatusText("");
    }

    // Update is called once per frame
    void Update()
    {
        if(freeModeChanged){
            if(!freeMode){
                DisableButtons();
            }
            else if(freeMode){
                EnableButtons();
            }
        }
        if(!deleteSynapseMode){
            Draw();
        }
        if(showModeChanged){
            if(showLabels){
                //Broadcast show labels
                Neurons.GetComponent<NeuronsController>().ShowLabelMode();
                showLabelsText.text = "Hide Labels";
                showModeChanged = false;
            }
            else if(!showLabels){
                //Broadcast hide labels
                Neurons.GetComponent<NeuronsController>().HideLabelMode();
                showLabelsText.text = "Show Labels";
                showModeChanged = false;
            }
            if(showRules){
                //Broadcast show rules
                Neurons.GetComponent<NeuronsController>().ShowRulesMode();
                showRulesText.text = "Hide Rules";
                showModeChanged = false;
            }
            else if(!showRules){
                //Broadcast hide rules
                Neurons.GetComponent<NeuronsController>().HideRulesMode();
                showRulesText.text = "Show Rules";
                showModeChanged = false;
            }
        }
    }

    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z)) ;
    }

    void OnMouseDrag()
    {
        if(panMode){
            Vector3 cursorScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 cursorPosition = Camera.main.ScreenToWorldPoint (cursorScreenPoint) + offset;
            MainCamera.transform.position = new Vector3(prevPosition.x-cursorPosition.x/panSensitivity, prevPosition.y-cursorPosition.y/panSensitivity, -10);
            prevPosition = MainCamera.transform.position;
        }

    }

    void OnMouseUp(){
        if(panMode){
            prevPosition = MainCamera.transform.position;
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

    public void SetStatusText(string statusText){
        statusBar.transform.GetChild(0).GetComponent<Text>().text = statusText;
    }

    public bool isFreeMode(){
        return freeMode;
    }

    public void SetFreeMode(bool mode){
        freeMode = mode;
        freeModeChanged = true;
    }

    public void DisableButtons(){
        Button[] buttons = Buttons.transform.GetComponentsInChildren<Button>();

        foreach(Button button in buttons){
            button.interactable = false;
        }
    }

    public void EnableButtons(){
        Button[] buttons = Buttons.transform.GetComponentsInChildren<Button>();

        foreach(Button button in buttons){
            button.interactable = true;
        }
    }

    public void ChangePanMode(){
        SetFreeMode(!freeMode);
        panMode = !panMode;
        if(panMode){
            SetStatusText("Pan Mode");
        }
    }

    public void ChangeShowLabelMode(){
        // if(freeMode){
            showModeChanged = true;
            showLabels = !showLabels;
        // }
    }

    public void ChangeShowRulesMode(){
        // if(freeMode){
            showModeChanged = true;
            showRules = !showRules;
        // }
    }

    public void ShowChoiceMenu()
    {
        ChoiceMenu.transform.position = new Vector3(0, 0, 0);
        ChoiceMenu.GetComponent<CanvasGroup>().alpha = 1;
    }

    public void HideChoiceMenu()
    {
        ChoiceMenu.transform.position = new Vector3(10000, 0, 0);
        ChoiceMenu.GetComponent<CanvasGroup>().alpha = 0;
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
        if(freeMode){
            Vector3[] neuronsBounds = new Vector3[4];
            cameraCenterArea.GetComponent<RectTransform>().GetWorldCorners(neuronsBounds);

            Vector3 initialPosition = new Vector3(UnityEngine.Random.Range(neuronsBounds[0].x, neuronsBounds[3].x), UnityEngine.Random.Range(neuronsBounds[0].y, neuronsBounds[1].y), 0);
            GameObject newron = Instantiate(NeuronPrefab, initialPosition, Quaternion.identity);
            newron.name = neuronCount.ToString();
            newron.transform.SetParent(Neurons.transform);
            newron.transform.tag = "Neuron";
            neurons.Add(neuronCount); //neuronCount
            neuronCount += 1;
            SetStatusText("New neuron created");
        }
        // neuronsRefresh();
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

    private int GetAvailableNeuronName(){
        //Returns the lowest available neuron int
        for(int i = 0; ; i++){
            if(!neurons.Contains(i)){
                return i;
            }
        }
    }

    public GameObject NewNeuron(int number, bool setActive) //Used by load function
    {
        GameObject newron = Instantiate(NeuronPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        newron.transform.localScale = new Vector3(1, 1, 1);
        newron.SetActive(setActive);
        newron.name = number.ToString();
        newron.transform.SetParent(Neurons.transform);
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
        if(freeMode){
            SetFreeMode(false);
            deleteSynapseMode = true;
            Draw();
            Synapses.GetComponent<SynapsesController>().DeleteSynapseMode(true);
        }
    }

    public void DeleteSynapse(string synapseName){
        print("Deleting synapse: " + synapseName);
        GameObject targetSynapse = GameObject.Find(synapseName);

        string sourceNeuron = targetSynapse.GetComponent<SynapseController>().GetSourceNeuron();
        string destNeuron = targetSynapse.GetComponent<SynapseController>().GetDestNeuron();

        synapses.Remove((int.Parse(sourceNeuron), int.Parse(destNeuron)));
        
        GameObject.Find(sourceNeuron).GetComponent<NeuronController>().DeleteOutSynapse(int.Parse(destNeuron));

        Destroy(targetSynapse);

        SetFreeMode(true);
        deleteSynapseMode = false;
        Synapses.GetComponent<SynapsesController>().DeleteSynapseMode(false);

        SetStatusText("Synapse deleted");

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
        if(freeMode){
            SetStatusText("Delete Neuron: Select a neuron to delete");
            SetFreeMode(false);
            deleteNeuronMode = true;
            Neurons.GetComponent<NeuronsController>().DeleteNeuronMode(true);
        }
    }

    public void DeleteNeuron(GameObject neuron){
        removeConnectedSynapses(neuron);
        neurons.Remove(int.Parse(neuron.name));
        Destroy(neuron);
        DeleteNeuronEnd();
    }

    public void DeleteNeuronEnd(){
        SetFreeMode(true);
        deleteNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().DeleteNeuronMode(false);   
        SetStatusText("Neuron deleted");     
    }

    public void DeleteAllNeurons(){
        GameObject[] neuronsToDelete = GameObject.FindGameObjectsWithTag("Neuron");
        foreach(GameObject neuron in neuronsToDelete){
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
        SetFreeMode(false);
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
        SetFreeMode(true);
        editRulesMode = false;
        editRulesMenu.SetActive(false);

        InputField rulesInputField = editRulesMenu.transform.Find("Rules InputField").GetComponent<InputField>();

        activeNeuronForEditing.GetComponent<NeuronController>().SetRules(rulesInputField.text);

        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);
        SetStatusText("Rules successfully edited");
    }

    public void EditRulesCancel(){
        SetFreeMode(true);
        editRulesMode = false;
        editRulesMenu.SetActive(false);

        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);

        SetStatusText("Rules edit cancelled");
    }

    public void EditSpikesStart(){
        SetFreeMode(false);
        // freeMode = false;
        editSpikesMode = true;
        editSpikesMenu.SetActive(true);
        editSpikesMenu.transform.position = activeNeuronForEditing.transform.position;

        EditSpikes();
    }

    public void EditSpikes(){
        InputField spikesInputField = editSpikesMenu.transform.Find("Spikes InputField").GetComponent<InputField>();

        // List<string> rules = neuron.GetComponent<NeuronController>().GetRules();
        // string rulesString = string.Join("\n", rules.ToArray());

        spikesInputField.text = activeNeuronForEditing.GetComponent<NeuronController>().GetSpikesNum().ToString();
        print(spikesInputField.text);
    }

    public void EditSpikesConfirm(){
        SetFreeMode(true);
        // freeMode = true;

        InputField spikesInputField = editSpikesMenu.transform.Find("Spikes InputField").GetComponent<InputField>();

        editSpikesMode = false;
        editSpikesMenu.SetActive(false);

        activeNeuronForEditing.GetComponent<NeuronController>().SetSpikes(int.Parse(spikesInputField.text));

        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);
        SetStatusText("Spikes successfully edited");
    }

    public void EditSpikesCancel(){
        SetFreeMode(true);
        // freeMode = true;

        editSpikesMode = false;
        editSpikesMenu.SetActive(false);

        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);
        SetStatusText("Spikes edit cancelled");
    }

    public void NewSynapseStart(){
        if(freeMode){
            SetStatusText("New Synapse");
            newSynapseMode = true;
            SetFreeMode(false);
            // freeMode = false;
            Neurons.GetComponent<NeuronsController>().NewSynapseMode(true);
        }
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
        SetFreeMode(true);
        SetStatusText("Synapse successfully created");
        // freeMode = true;
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
        // newSynapse.AddComponent<LineRenderer>();
        newSynapse.AddComponent<SynapseController>();
        newSynapse.GetComponent<SynapseController>().SetSourceNeuron(sourceNeuronName);
        newSynapse.GetComponent<SynapseController>().SetDestNeuron(destNeuronName);

        GameObject line1 = new GameObject();
        line1.transform.position = start;
        line1.transform.SetParent(newSynapse.transform);
        line1.name = "Line 1";
        line1.AddComponent<LineRenderer>();

        Vector3 segmentPosition = (0.4f * start) + (.6f * end);

        LineRenderer lr1 = line1.GetComponent<LineRenderer>();
        // lr.SetColors(color, color);
        // lr.SetWidth(0.1f, 0.1f);
        lr1.SetPosition(0, start);
        lr1.SetPosition(1, segmentPosition);
        lr1.startWidth = 0.01f;
        lr1.endWidth = 0.01f;
        lr1.material = new Material(white);
        lr1.startColor = Color.white;

        GameObject line2 = new GameObject();
        line2.transform.position = segmentPosition;
        line2.transform.SetParent(newSynapse.transform);
        line2.name = "Line 2";
        line2.AddComponent<LineRenderer>();

        LineRenderer lr2 = line2.GetComponent<LineRenderer>();
        // lr.SetColors(color, color);
        // lr.SetWidth(0.1f, 0.1f);
        lr2.SetPosition(0, segmentPosition);
        lr2.SetPosition(1, end);
        lr2.startWidth = 0.2f;
        lr2.endWidth = 0.05f;
        lr2.material = new Material(white);
        lr2.startColor = Color.white;

        if(deleteSynapseMode){
            GameObject delbut = Instantiate(deleteSynapseButton, (start + end) * 0.5f, Quaternion.identity);
            delbut.transform.localScale = new Vector3(.015f, .015f, 0);
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
        //create Root (ie. the first configuration)
        if (root == null)
        {
            root = new ChoiceNode(root, GetAllSpikes());
            GameObject newChoiceButton = Instantiate(choiceButtonPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), 
                Quaternion.identity, choiceContent.transform);
            newChoiceButton.transform.localScale = new Vector3(1, 1, 1);
            newChoiceButton.name = "root";
            newChoiceButton.GetComponentInChildren<Text>().text = "Go to Root";
            last = root;
            choiceTimes.Add(globalTime);
        }
        appliedRulesStorage.Clear();
        List<(List<string>, string ,int)> nondeterministicList = new List<(List<string>, string, int)>();
        (List<string>, string) determinismCheck = (new List<string>(), "");
        configHistory.Add(GetAllSpikes());
        synapses.Sort();
        List<GameObject> receivingNeurons = new List<GameObject>();
        int shootingNeuron = 0;
        foreach ((int i, int j) in synapses)
        {
            print(i.ToString() + j.ToString());
            if (shootingNeuron != i && receivingNeurons != null)
            {
                print("Firing " + shootingNeuron.ToString());
                //determinismCheck receives a tuple of a list of applicable rules and the chosen rules, respectively
                determinismCheck = Neurons.GetComponent<NeuronsController>().Fire(GameObject.Find("Neurons/" + shootingNeuron.ToString()), receivingNeurons);
                if (determinismCheck.Item1.Count > 1)
                {
                    nondeterministicList.Add((determinismCheck.Item1, determinismCheck.Item2, i));
                    print((determinismCheck.Item1, determinismCheck.Item2, i)); 
                }   
                receivingNeurons.Clear();
            }
            receivingNeurons.Add(GameObject.Find("Neurons/" + j.ToString()));
            shootingNeuron = i;
        }
        //Takes the last neuron and fires
        var lastElement = synapses[synapses.Count - 1];
        int lastNeuron = lastElement.Item1;
        determinismCheck = Neurons.GetComponent<NeuronsController>().Fire(GameObject.Find("Neurons/" + lastNeuron.ToString()), receivingNeurons);
        globalTime++;
        if (determinismCheck.Item1.Count > 1)
        {
            nondeterministicList.Add((determinismCheck.Item1, determinismCheck.Item2, lastNeuron));
            print((determinismCheck.Item1, determinismCheck.Item2, lastNeuron));
        }

        LogAppliedRules();

        if (nondeterministicList.Count > 0)
        {
            ChoiceNode newChoice = new ChoiceNode(root, GetAllSpikes(), nondeterministicList, globalTime);
            newChoice.SetFather(last);
            choiceTimes.Add(globalTime);
            choiceContent.GetComponent<RectTransform>().sizeDelta = new Vector2(600, (choiceTimes.Count+1)* 80);

            GameObject newChoiceButton = Instantiate(choiceButtonPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), 
                Quaternion.identity, choiceContent.transform);
            newChoiceButton.GetComponent<Button>().interactable = false;
            newChoiceButton.transform.localScale = new Vector3(1, 1, 1);
            newChoiceButton.GetComponentInChildren<Text>().text = newChoice.GetChosen();
            newChoiceButton.name = "Choice" + newChoice.time.ToString();

            last = newChoice;
            last.PrintNondetRules();
        }
    }

    public void GoBackOne()
    {
        if(configHistory.Count > 0)
        {
            SetAllSpikes(configHistory[configHistory.Count - 1]);
            configHistory.RemoveAt(configHistory.Count - 1);
            globalTime--;
        }    
    }

    public void GoToChoice()
    {
        if(EventSystem.current.currentSelectedGameObject.name == "root")
        {
            if (configHistory.Count > 0)
            {
                SetAllSpikes(configHistory[0]);
                globalTime = 0;
            }       
        }
    }

    public void LogAppliedRules()
    {
        string appliedRules = "Matched Rules: ";
        foreach ((List<string> matchedRules, string chosen, string name) in appliedRulesStorage)
        {
            foreach(string rule in matchedRules)
            {
                appliedRules += rule + ", ";
            }
            appliedRules += "|| The Chosen: " + chosen;
            appliedRules += "|| NeuronNo: " + name;
            appliedRules += "\n";
        }
        print(appliedRules);
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
            
            SetStatusText("Saved!");
        }
    }

    public void Load(){
        string path = EditorUtility.OpenFilePanel("Load snapse file", "", "snapse");
        if (path.Length != 0)
        {
            var fileContent = File.ReadAllBytes(path);
            string formatData = System.Text.Encoding.UTF8.GetString(fileContent);
            DecodeFromFormat(formatData);

            //Auto Layout
            Neurons.AddComponent<GridLayoutGroup>().cellSize = new Vector2(200, 200);
            Neurons.GetComponent<GridLayoutGroup>().enabled = true;
            Neurons.GetComponent<GridLayoutGroup>().CalculateLayoutInputHorizontal();
            Neurons.GetComponent<GridLayoutGroup>().CalculateLayoutInputVertical();
            Neurons.GetComponent<GridLayoutGroup>().SetLayoutHorizontal();
            Neurons.GetComponent<GridLayoutGroup>().SetLayoutVertical();
            Neurons.GetComponent<GridLayoutGroup>().enabled = false;

            SetStatusText("Loaded");
        }
    }
    
    public List<int> GetAllSpikes()
    {
        List<int> config = new List<int>();
        foreach (int i in neurons)
        {
            GameObject neuronObject = GameObject.Find(i.ToString());
            int spike = neuronObject.GetComponent<NeuronController>().GetSpikesNum();
            config.Add(spike);
        }
        return config;
    }

    public void SetAllSpikes(List<int> config)
    {
        foreach (int i in neurons)
        {
            if (i < config.Count)
            {
                GameObject neuronObject = GameObject.Find(i.ToString());
                neuronObject.GetComponent<NeuronController>().SetSpikes(config[i]);
            }               
        }
    }

    public bool ValidateRules(string rules)
    {
        string[] newLine = new string[1] { "\n" };
        string[] rulesArr = rules.Split(newLine, StringSplitOptions.RemoveEmptyEntries);
        foreach (string rule in rulesArr)
        {
            string[] separators = new string[3] { "/", "->", ";" };
            string[] parts = rule.Split(separators, 4, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                if(!Regex.Match("a", parts[0]).Success)
                    return false;
                if(!Regex.Match(parts[1], " *a+ *").Success)
                    return false;
                if(!Regex.Match(parts[2], " *a+ *").Success)
                    return false;
                if(!Regex.Match(parts[3], " *[0-9]+ *").Success)
                    return false;
            }
            catch(ArgumentException)
            {
                return false;
            }           
        }
        return true;
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

            if(rules.Count == 0){
                neuronRules += "[]";
            }
            else{
            i = 0;
                foreach (string rule in rules){
                    neuronRules += "[" + rule + "]";
                    i+=1;
                    if (i < rules.Count){
                        neuronRules += ", ";
                    }
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
        print(formatData);
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
            // print(neuronDeclaration);
        }

        int n = 0;
        foreach(GameObject neuron in neurons){
            neuron.GetComponent<NeuronController>().SetSpikes(int.Parse(strValues[4 + 7*n]));

            //parse rules
            string rules = strValues[6 + 7*n];
            rules = string.Join("\n", rules.Split(new char[] {'[', ']', ','}, StringSplitOptions.RemoveEmptyEntries));
            neuron.GetComponent<NeuronController>().SetRules(rules);
            print(rules);

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
