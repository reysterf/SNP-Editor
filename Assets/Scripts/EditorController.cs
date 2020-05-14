using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;
using SFB;

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

    public GameObject NeuronPrefab;
    public GameObject OutputNeuronPrefab;
    public GameObject NeuronWithRules;
    public GameObject NeuronWithoutRules;
    public GameObject Neurons;
    public GameObject Synapses;

    public PanController panController;

    private bool freeMode = true;
    private bool freeModeChanged = false;
    private bool newSynapseMode = false;
    private bool editNeuronMode = false;
    private bool editRulesMode = false;
    private bool editSpikesMode = false;
    private bool deleteNeuronMode = false;
    private bool deleteSynapseMode = false;
    private bool changeOutputMode = false;

    private bool panMode = false;

    private Vector3 synapseStart;
    private Vector3 synapseEnd;

    private List<int> neurons = new List<int>();
    private List<int> outputneurons = new List<int>();
    private List<(int, int)> synapses = new List<(int, int)>();


    public GameObject statusBar;
    public GameObject ChoiceMenu;
    public GameObject choiceContent;
    public GameObject choiceElement;
    public GameObject choicePerNeuron;
    public GameObject editNeuronMenu;
    public GameObject editRulesMenu;
    public GameObject editSpikesMenu;
    public GameObject neuronLabel;
    public GameObject cancelButton;

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
    public List<List<int>> delayHistory;
    private string outputPath;
    List<string> outputBitstrings = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        editNeuronMenu.SetActive(false);
        editRulesMenu.SetActive(false);
        editSpikesMenu.SetActive(false);
        cancelButton.SetActive(false);

        lastData = null;
        root = null;
        configHistory = new List<List<int>>();
        delayHistory = new List<List<int>>();
        choiceTimes = new List<int>();
        appliedRulesStorage = new List<(List<string>, string, string)>();
        outputPath = Application.dataPath + "/output.txt";

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
        if (freeModeChanged) {
            if (!freeMode) {
                DisableButtons();
            }
            else if (freeMode) {
                EnableButtons();
            }
        }
        if (!deleteSynapseMode) {
            Draw();
        }
        if (showModeChanged) {
            if (showLabels) {
                //Broadcast show labels
                Neurons.GetComponent<NeuronsController>().ShowLabelMode();
                showLabelsText.text = "Hide Labels";
                showModeChanged = false;
            }
            else if (!showLabels) {
                //Broadcast hide labels
                Neurons.GetComponent<NeuronsController>().HideLabelMode();
                showLabelsText.text = "Show Labels";
                showModeChanged = false;
            }
            if (showRules) {
                //Broadcast show rules
                Neurons.GetComponent<NeuronsController>().ShowRulesMode();
                showRulesText.text = "Hide Rules";
                showModeChanged = false;
            }
            else if (!showRules) {
                //Broadcast hide rules
                Neurons.GetComponent<NeuronsController>().HideRulesMode();
                showRulesText.text = "Show Rules";
                showModeChanged = false;
            }
        }
    }

    // void OnMouseDown()
    // {
    //     screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
    //     offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    // }

    // void OnMouseDrag()
    // {
    //     if (panMode) {
    //         Vector3 cursorScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
    //         Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorScreenPoint) + offset;
    //         MainCamera.transform.position = new Vector3(prevPosition.x - cursorPosition.x / panSensitivity, prevPosition.y - cursorPosition.y / panSensitivity, -10);
    //         prevPosition = MainCamera.transform.position;
    //     }

    // }

    // void OnMouseUp() {
    //     if (panMode) {
    //         prevPosition = MainCamera.transform.position;
    //     }
    // }

    public void Draw() {
        foreach ((int i, int j) in synapses)
        {
            DrawLine(i.ToString(), j.ToString());
        }
    }

    private void BlankSlate() {
        //Reset the program state into a blank slate
        DeleteAllNeurons();
    }

    public void SetStatusText(string statusText) {
        statusBar.transform.GetChild(0).GetComponent<Text>().text = statusText;
    }

    public bool isFreeMode() {
        return freeMode;
    }

    public void SetFreeMode(bool mode) {
        freeMode = mode;
        freeModeChanged = true;
    }

    public void DisableButtons() {
        Button[] buttons = Buttons.transform.GetComponentsInChildren<Button>();

        foreach (Button button in buttons) {
            button.interactable = false;
        }
    }

    public void EnableButtons() {
        Button[] buttons = Buttons.transform.GetComponentsInChildren<Button>();

        foreach (Button button in buttons) {
            button.interactable = true;
        }
    }

    public void ChangePanMode() {
        SetFreeMode(!freeMode);
        panMode = !panMode;
        panController.SetPanMode(panMode);
        if (panMode) {
            SetStatusText("Pan Mode");
        }
    }

    public void ChangeShowLabelMode() {
        // if(freeMode){
        showModeChanged = true;
        showLabels = !showLabels;
        // }
    }

    public void ChangeShowRulesMode() {
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

    public bool isShowRulesMode() {
        return showRules;
    }

    public bool isShowLabelsMode() {
        return showLabels;
    }

    public bool isNewSynapseMode()
    {
        return newSynapseMode;
    }

    public bool isDeleteSynapseMode() {
        return deleteSynapseMode;
    }

    public void StartMode(){
        cancelButton.SetActive(true);
    }

    public void CancelCurrentMode(){
        if(newSynapseMode){
            Neurons.GetComponent<NeuronsController>().NewSynapseMode(false);
            NewSynapseCancel();
            EndMode();
        }
        if(editNeuronMode){
            EditNeuronCancel();
            EndMode();
        }
        if(deleteNeuronMode){
            DeleteNeuronCancel();
            EndMode();
        }
        if(deleteSynapseMode){
            DeleteSynapseCancel();
            EndMode();
        }
    }

    public void EndMode(){
        cancelButton.SetActive(false);
    }

    public void neuronsRefresh() {
        Neurons.SetActive(false);
        Neurons.SetActive(true);
    }


    public void NewNeuron()
    {
        if (freeMode) {
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

    public void NewOutputNeuron()
    {
        if (freeMode)
        {
            Vector3[] neuronsBounds = new Vector3[4];
            cameraCenterArea.GetComponent<RectTransform>().GetWorldCorners(neuronsBounds);

            Vector3 initialPosition = new Vector3(UnityEngine.Random.Range(neuronsBounds[0].x, neuronsBounds[3].x), UnityEngine.Random.Range(neuronsBounds[0].y, neuronsBounds[1].y), 0);
            GameObject newron = Instantiate(OutputNeuronPrefab, initialPosition, Quaternion.identity);
            newron.name = neuronCount.ToString();
            newron.transform.SetParent(Neurons.transform);
            newron.transform.tag = "OutputNeuron";
            neurons.Add(neuronCount); //neuronCount
            outputneurons.Add(neuronCount);
            neuronCount += 1;
            SetStatusText("New output neuron created");
        }
    }

    public GameObject NewOutputNeuron(int neuronInt)
    {
        Vector3[] neuronsBounds = new Vector3[4];
        cameraCenterArea.GetComponent<RectTransform>().GetWorldCorners(neuronsBounds);

        Vector3 initialPosition = new Vector3(UnityEngine.Random.Range(neuronsBounds[0].x, neuronsBounds[3].x), UnityEngine.Random.Range(neuronsBounds[0].y, neuronsBounds[1].y), 0);
        GameObject newron = Instantiate(OutputNeuronPrefab, initialPosition, Quaternion.identity);
        newron.name = neuronInt.ToString();
        newron.transform.SetParent(Neurons.transform);
        newron.transform.tag = "OutputNeuron";
        neurons.Add(neuronInt); //neuronCount
        outputneurons.Add(neuronInt);
        neuronCount += 1;

        return newron;
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
            StartMode();
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

        DeleteSynapseEnd();
    }

    public void DeleteSynapseEnd(){
        SetFreeMode(true);
        deleteSynapseMode = false;
        Synapses.GetComponent<SynapsesController>().DeleteSynapseMode(false);
        EndMode();

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

    public void DeleteSynapseCancel(){
        SetFreeMode(true);
        deleteSynapseMode = false;
        Synapses.GetComponent<SynapsesController>().DeleteSynapseMode(false);

        SetStatusText("Synapse deletion cancelled");

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
            StartMode();
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
        EndMode();
    }

    public void DeleteNeuronCancel(){
        SetFreeMode(true);
        deleteNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().DeleteNeuronMode(false);   
        SetStatusText("Neuron deletion cancelled");     
    }

    public void DeleteAllNeurons(){
        GameObject[] neuronsToDelete = GameObject.FindGameObjectsWithTag("Neuron");
        GameObject[] outputNeuronsToDelete = GameObject.FindGameObjectsWithTag("OutputNeuron");
        foreach(GameObject neuron in neuronsToDelete){
            DeleteNeuron(neuron);
        }
        foreach(GameObject neuron in outputNeuronsToDelete){
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
        StartMode();
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
        SetStatusText("Neuron editting cancelled");
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

        if(activeNeuronForEditing.GetComponent<NeuronController>().SetRules(rulesInputField.text))
            SetStatusText("Rules successfully edited");
        else
            SetStatusText("Invalid rule format");

        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);       
        EndMode();
    }

    public void EditRulesCancel(){
        SetFreeMode(true);
        editRulesMode = false;
        editRulesMenu.SetActive(false);

        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);

        SetStatusText("Rules edit cancelled");
        EndMode();
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
        EndMode();
    }

    public void EditSpikesCancel(){
        SetFreeMode(true);
        // freeMode = true;

        editSpikesMode = false;
        editSpikesMenu.SetActive(false);

        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);
        SetStatusText("Spikes edit cancelled");
        EndMode();
    }

    public void NewSynapseStart(){
        if(freeMode){
            SetStatusText("New Synapse");
            newSynapseMode = true;
            SetFreeMode(false);
            // freeMode = false;
            Neurons.GetComponent<NeuronsController>().NewSynapseMode(true);
            StartMode();
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

        bool invalidSynapse = false;
        if(sourceNeuron == destNeuron){
            SetStatusText("Can't create synapse with the same source and destination");
            NewSynapseError();
            invalidSynapse = true;
        }

        if(!synapseExists && !invalidSynapse){
            synapses.Add((sourceNeuron, destNeuron));

            //add outSynapse to source neuron
            GameObject.Find(sourceNeuronName).GetComponent<NeuronController>().AddOutSynapse(int.Parse(destNeuronName));

            DrawLine(sourceNeuronName, destNeuronName);
            NewSynapseEnd();
        }
    }

    public void NewSynapseEnd(){
        newSynapseMode = false;
        SetFreeMode(true);
        SetStatusText("Synapse successfully created");
        EndMode();
        // freeMode = true;
    }

    public void NewSynapseError(){
        newSynapseMode = false;
        SetFreeMode(true);
        // SetStatusText("Synapse successfully created");
        EndMode();
    }

    public void NewSynapseCancel(){
        newSynapseMode = false;
        SetFreeMode(true);
        SetStatusText("Synapse creation cancelled");
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
            last = root;
            choiceTimes.Add(globalTime);
        }
        appliedRulesStorage.Clear();
        List<(List<string>, string ,int)> nondeterministicList = new List<(List<string>, string, int)>();
        (List<string>, string) determinismCheck = (new List<string>(), "");
        configHistory.Add(GetAllSpikes());
        delayHistory.Add(GetAllDelay());
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
                    nondeterministicList.Add((determinismCheck.Item1, determinismCheck.Item2, shootingNeuron));
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
        EndFire();

        if (nondeterministicList.Count > 0)
        {
            AddChoiceElement(nondeterministicList);
        }
    }

    public void EndFire()
    {
        outputBitstrings.Clear();
        if (outputneurons.Count > 0)
        {
            foreach (int i in outputneurons)
            {
                string outputBitsring = i.ToString() + ":" + 
                    Neurons.GetComponent<NeuronsController>().EndFire(GameObject.Find("Neurons/" + i.ToString()));
                outputBitstrings.Add(outputBitsring);
            }
        }
        if(outputBitstrings.Count > 0)
            SaveOutput(outputBitstrings);
        SetStatusText("Fired at t = " + globalTime);
    }

    public void GoBackOne()
    {
        if(configHistory.Count > 0)
        {
            SetAllSpikes(configHistory[configHistory.Count - 1]);
            configHistory.RemoveAt(configHistory.Count - 1);
            SetAllDelays(delayHistory[delayHistory.Count - 1]);
            delayHistory.RemoveAt(delayHistory.Count - 1);
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
                SetStatusText("t = " + globalTime);
            }       
        }
    }

    public void AddChoiceElement(List<(List<string>, string, int)> nondeterministicList)
    {
        ChoiceNode newChoice = new ChoiceNode(root, GetAllSpikes(), nondeterministicList, globalTime);
        newChoice.SetFather(last);
        choiceTimes.Add(globalTime);
                
        GameObject newChoiceElement = Instantiate(choiceElement, new Vector3(transform.position.x, transform.position.y, transform.position.z),
               Quaternion.identity, choiceContent.transform);
        //newChoiceButton.GetComponent<Button>().interactable = false;
        //newChoiceButton.transform.localScale = new Vector3(1, 1, 1);
        //newChoiceButton.GetComponentInChildren<Text>().text = newChoice.GetChosen();
        //newChoiceButton.name = "Choice" + newChoice.time.ToString();

        newChoiceElement.transform.Find("Time").Find("TimeText").gameObject.GetComponent<Text>().text = "t=" + globalTime.ToString();
        Transform perNeuronContainer = newChoiceElement.transform.Find("PerNeuron Container");

        foreach ((List<string> matched, string chosen, int neuronNo) in nondeterministicList)
        {
            matched.Remove(chosen);
            GameObject newChoicePerNeuron = Instantiate(choicePerNeuron, new Vector3(transform.position.x, transform.position.y, transform.position.z),
               Quaternion.identity, perNeuronContainer.transform);
            newChoicePerNeuron.transform.Find("NeuronNo").Find("NeuronNoText").GetComponent<Text>().text = "N" + neuronNo.ToString();
            newChoicePerNeuron.transform.Find("Chosen").Find("ChosenText").GetComponent<Text>().text = chosen;
            string ignoredRules = "";
            foreach(string rule in matched)
            {
                ignoredRules += rule + "\n";
            }
            ignoredRules.TrimEnd((new char[] { '\n' }));
            newChoicePerNeuron.transform.Find("Ignored").Find("IgnoredText").GetComponent<Text>().text = ignoredRules;
        }     
        //Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(newChoiceElement.GetComponent<RectTransform>());

        last = newChoice;
        last.PrintNondetRules();

        newChoiceElement.GetComponent<HorizontalLayoutGroup>().enabled = false;
        newChoiceElement.GetComponent<HorizontalLayoutGroup>().CalculateLayoutInputHorizontal();
        newChoiceElement.GetComponent<HorizontalLayoutGroup>().CalculateLayoutInputVertical();
        newChoiceElement.GetComponent<HorizontalLayoutGroup>().SetLayoutHorizontal();
        newChoiceElement.GetComponent<HorizontalLayoutGroup>().SetLayoutVertical();
        newChoiceElement.GetComponent<HorizontalLayoutGroup>().enabled = true;
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

    public void ChangeOutputPath()
    {
        if (!changeOutputMode)
        {
            changeOutputMode = true;
            EventSystem.current.currentSelectedGameObject.transform.Find("Text").GetComponent<Text>().text = "Confirm";
            EventSystem.current.currentSelectedGameObject.transform.parent.Find("InputField").gameObject.SetActive(true);
            EventSystem.current.currentSelectedGameObject.transform.parent.Find("Cancel").gameObject.SetActive(true);
        }
        else
        {
            try
            {

                string newPath = Application.dataPath + "/../";                  
                string submittedPath = EventSystem.current.currentSelectedGameObject.transform.parent.Find("InputField").Find("Text").
                    GetComponent<Text>().text;
                if (submittedPath.StartsWith("/"))
                    submittedPath = submittedPath.TrimStart((new char[] { '/' }));
                if (submittedPath.EndsWith("/"))
                    submittedPath = submittedPath.TrimEnd((new char[] { '/' }));

                newPath += submittedPath;
                print("1: "+ newPath);
                if (String.Equals(newPath, Application.dataPath + "/../root", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(newPath, Application.dataPath + "/../", StringComparison.OrdinalIgnoreCase))
                    newPath = Application.dataPath;
                print("2: "+newPath);
                print(Application.dataPath);
                newPath += "/output.txt";
                try
                {
                    File.Delete(outputPath);
                }
                catch (DirectoryNotFoundException e)
                {
                    ;
                }
                catch (UnauthorizedAccessException e)
                {
                    ;
                }
                outputPath = newPath;
                SaveOutput(outputBitstrings);
                CloseChangeOutput();
                SetStatusText("Saved to " + newPath);
            }
            catch (DirectoryNotFoundException e)
            {
                SetStatusText("No such directory");
                print(e.ToString());
            }
            catch (UnauthorizedAccessException e)
            {
                SetStatusText("Unauthorized Access to directory");
                print(e.ToString());
            }           
        } 
    }

    public void CloseChangeOutput()
    {
        
        EventSystem.current.currentSelectedGameObject.transform.parent.Find("InputField").gameObject.SetActive(false);
        EventSystem.current.currentSelectedGameObject.transform.parent.Find("InputField").Find("Text").
                GetComponent<Text>().text = "";
        EventSystem.current.currentSelectedGameObject.transform.parent.Find("Change Output Button").
                Find("Text").GetComponent<Text>().text = "Change Output Path";

        if (EventSystem.current.currentSelectedGameObject.name == "Change Output Button")
            EventSystem.current.currentSelectedGameObject.transform.parent.Find("Cancel").gameObject.SetActive(false);
        else if(EventSystem.current.currentSelectedGameObject.name == "Cancel")
            EventSystem.current.currentSelectedGameObject.SetActive(false);

        changeOutputMode = false;
    }

    public void SaveOutput(List<string> outputBitstrings)
    {
        StreamWriter writer = new StreamWriter(outputPath, false);
        foreach(string bitstring in outputBitstrings)
            writer.WriteLine(bitstring);
        writer.Close();
    }

    public void Save(){
        var path = StandaloneFileBrowser.SaveFilePanel(
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
        string[] path = StandaloneFileBrowser.OpenFilePanel("Load snapse file", "", "snapse", false);

        foreach (string p in path)
            print(p);
        
        if (path.Length != 0)
        {
            var fileContent = File.ReadAllBytes(path[0]);
            string formatData = System.Text.Encoding.UTF8.GetString(fileContent);

            bool hasPositionData = false;
            hasPositionData = DecodeFromFormat(formatData);

            if(!hasPositionData){
                //Auto Layout
                print("NO POSITION DATA");
                Neurons.GetComponent<GridLayoutGroup>().cellSize = new Vector2(200, 200);
                Neurons.GetComponent<GridLayoutGroup>().enabled = true;
                Neurons.GetComponent<GridLayoutGroup>().CalculateLayoutInputHorizontal();
                Neurons.GetComponent<GridLayoutGroup>().CalculateLayoutInputVertical();
                Neurons.GetComponent<GridLayoutGroup>().SetLayoutHorizontal();
                Neurons.GetComponent<GridLayoutGroup>().SetLayoutVertical();
                Neurons.GetComponent<GridLayoutGroup>().enabled = false;
            }
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

    public List<int> GetAllDelay()
    {
        List<int> delayList = new List<int>();
        foreach (int i in neurons)
        {
            GameObject neuronObject = GameObject.Find(i.ToString());
            int spike = neuronObject.GetComponent<NeuronController>().GetDelay();
            delayList.Add(spike);
        }
        return delayList;
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

    public void SetAllDelays(List<int> delayList)
    {
        string delayString = "";
        foreach (int i in neurons)
        {
            if (i < delayList.Count)
            {
                GameObject neuronObject = GameObject.Find(i.ToString());
                neuronObject.GetComponent<NeuronController>().SetDelay(delayList[i]);
                delayString += delayList[i].ToString() + ", ";
            }
        }
        print(delayString);
    }

    public bool ValidateRules(string rules)
    {
        if(rules == ""){
            return true;
        }
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
            catch(ArgumentException e)
            {
                print(e);
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
            if(GameObject.Find(neuron.ToString()).GetComponent<NeuronController>().IsOutputNeuron()){
                neuronsDeclaration += "O" + neuron.ToString();
            }
            else{
                neuronsDeclaration += "N" + neuron.ToString();
            }
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
                if(GameObject.Find(outSynapse.ToString()).GetComponent<NeuronController>().IsOutputNeuron()){
                    neuronOutSynapses += "O" + outSynapse.ToString();
                }
                else{
                    neuronOutSynapses += "N" + outSynapse.ToString();
                }
                i += 1;
                if (i < outSynapses.Count){
                    neuronOutSynapses += ", ";
                }
            }

            neuronOutSynapses += "]";

            neuronDefinition += "\toutsynapses = " + neuronOutSynapses + lineEnder;

            neuronDefinition += "\n";

            //delay
            string neuronDelay = neuronToEncode.GetComponent<NeuronController>().GetDelay().ToString();
            neuronDefinition +="\tdelay = " + neuronDelay + lineEnder;
            neuronDefinition += "\n";

            //storedGive
            string storedGive = neuronToEncode.GetComponent<NeuronController>().GetStoredGive().ToString();
            neuronDefinition += "\tstoredGive = " + storedGive + lineEnder;
            neuronDefinition += "\n";

            //storedConsume
            string storedConsume = neuronToEncode.GetComponent<NeuronController>().GetStoredConsume().ToString();
            neuronDefinition += "\tstoredConsume = " + storedConsume + lineEnder;
            neuronDefinition += "\n";

            //outputNeuron
            string outputNeuron = neuronToEncode.GetComponent<NeuronController>().IsOutputNeuron().ToString();
            neuronDefinition += "\toutputNeuron = " + outputNeuron + lineEnder;
            neuronDefinition += "\n";

            //storedReceived
            if(bool.Parse(outputNeuron)){
                string storedReceived = neuronToEncode.GetComponent<NeuronController>().GetStoredReceived().ToString();
                neuronDefinition += "\tstoredReceived = " + storedReceived + lineEnder;
                neuronDefinition += "\n";
            }

            //bitString
            if(bool.Parse(outputNeuron)){
                string bitString = neuronToEncode.GetComponent<NeuronController>().GetBitString();
                if(bitString == ""){
                    bitString = "null";
                }
                neuronDefinition += "\tbitString = " + bitString + lineEnder;
                neuronDefinition += "\n";
            }

            //positions
            string neuronPosition = "(";
            neuronPosition += neuronToEncode.transform.position.x.ToString();
            neuronPosition += ",";
            neuronPosition += neuronToEncode.transform.position.y.ToString();
            neuronPosition += ",";
            neuronPosition += neuronToEncode.transform.position.z.ToString();
            neuronPosition += ")";

            neuronDefinition += "\tposition = " + neuronPosition + lineEnder;

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

    public bool DecodeFromFormat(string formatData){
        BlankSlate();
        print(formatData);
        formatData = Regex.Replace(formatData, @"\s+", ""); //Remove all whitespace

        bool hasPositionData = false;


        char[] separators = {'{', '}', '=', ':' }; //Remove delimiters
        string[] strValues = formatData.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        int i = 0;
        foreach(string str in strValues)
        {
            print(i.ToString() + ": " + str);
            i+=1;
        }

        print("PARSING START");

        //Parse neuron declaration
        string[] neuronDeclarations = strValues[1].Split(new char[] {'[', ']', ','}, StringSplitOptions.RemoveEmptyEntries);

        List<GameObject> neurons = new List<GameObject>();

        foreach(string neuronDeclaration in neuronDeclarations){
            if(neuronDeclaration[0] == 'N'){                
                neurons.Add(NewNeuron(int.Parse(neuronDeclaration.Substring(1, neuronDeclaration.Length-1)), true));
            }
            else if(neuronDeclaration[0] == 'O'){
                neurons.Add(NewOutputNeuron(int.Parse(neuronDeclaration.Substring(1, neuronDeclaration.Length-1))));
            }
            // print(neuronDeclaration);
        }

        int n = 0;

        int searchArea = 6;

        int spikesIndex = 0;
        int spikesSearchIndex = 0;

        int rulesIndex = 0;
        int rulesSearchIndex = 0;

        int outSynapsesIndex = 0;
        int outSynapsesSearchIndex = 0;

        int delayIndex = 0;
        int delaySearchIndex = 0;

        int storedGiveIndex = 0;
        int storedGiveSearchIndex = 0;

        int storedConsumeIndex = 0;
        int storedConsumeSearchIndex = 0;

        int outputNeuronIndex = 0;
        int outputNeuronSearchIndex = 0;

        int storedReceivedIndex = 0;
        int storedReceivedSearchIndex = 0;

        int bitStringIndex = 0;
        int bitStringSearchIndex = 0;

        int positionIndex = 0;
        int positionSearchIndex = 0;

        foreach(GameObject neuron in neurons){
            //parse spikes
            print("Parsing Spikes");
            spikesIndex = Array.IndexOf(strValues, "spikes", spikesSearchIndex, Mathf.Min(searchArea, strValues.Length-spikesSearchIndex));
            neuron.GetComponent<NeuronController>().SetSpikes(int.Parse(strValues[spikesIndex + 1]));
            spikesSearchIndex = spikesIndex + 1;
            rulesSearchIndex = spikesIndex;

            //parse rules
            print("Parsing Rules");
            rulesIndex = Array.IndexOf(strValues, "rules", rulesSearchIndex, Mathf.Min(searchArea, strValues.Length-rulesSearchIndex));
            rulesSearchIndex = rulesIndex + 1;
            outSynapsesSearchIndex = rulesIndex;

            rulesSearchIndex = rulesIndex + 1;

            string rules = strValues[rulesIndex + 1];
            rules = string.Join("\n", rules.Split(new char[] {'[', ']', ','}, StringSplitOptions.RemoveEmptyEntries));
            print(rules);
            if(rules != ""){
                neuron.GetComponent<NeuronController>().SetRules(rules);
            }
            // print(rules);

            //parse outsynapses
            print("Parsing OutSynapses");
            outSynapsesIndex = Array.IndexOf(strValues, "outsynapses", outSynapsesSearchIndex, Mathf.Min(searchArea, strValues.Length-outSynapsesSearchIndex));
            outSynapsesSearchIndex = outSynapsesIndex + 1;
            delaySearchIndex = outSynapsesIndex;
            positionSearchIndex = outSynapsesIndex;
            spikesSearchIndex = outSynapsesIndex;

            string outSynapses = strValues[outSynapsesIndex + 1];
            string[] outSynapsesArray = outSynapses.Split(new char[] {'[', ']', ',', 'N', 'O'}, StringSplitOptions.RemoveEmptyEntries);
            List<int> outSynapsesList = new List<int>();
            foreach(string outSynapse in outSynapsesArray){
                NewSynapse(neuron.name, outSynapse);
                outSynapsesList.Add(int.Parse(outSynapse));
            }
            neuron.GetComponent<NeuronController>().SetOutSynapses(outSynapsesList);

            //parse delay
            delayIndex = Array.IndexOf(strValues, "delay", delaySearchIndex, Mathf.Min(searchArea, strValues.Length-delaySearchIndex));
            if(delayIndex >= 0){
                print("Parsing Delay");
                positionSearchIndex = delayIndex;
                storedGiveSearchIndex = delayIndex;
                spikesSearchIndex = delayIndex;
                neuron.GetComponent<NeuronController>().SetDelay(int.Parse(strValues[delayIndex + 1]));
            }

            //parse storedGive & storedConsume
            storedGiveIndex = Array.IndexOf(strValues, "storedGive", storedGiveSearchIndex, Mathf.Min(searchArea, strValues.Length-storedGiveSearchIndex));
            if(storedGiveIndex >= 0){
                print("Parsing StoredGive");
                positionSearchIndex = storedGiveIndex;
                spikesSearchIndex = storedGiveIndex;
                storedConsumeSearchIndex = storedGiveIndex;
                neuron.GetComponent<NeuronController>().SetStoredGive(int.Parse(strValues[storedGiveIndex + 1]));
            }

            //storedConsume
            storedConsumeIndex = Array.IndexOf(strValues, "storedConsume", storedConsumeSearchIndex, Mathf.Min(searchArea, strValues.Length-storedConsumeSearchIndex));
            if(storedConsumeIndex >= 0){
                print("Parsing StoredConsume");
                positionSearchIndex = storedConsumeIndex;
                spikesSearchIndex = storedConsumeIndex;
                outputNeuronSearchIndex = storedConsumeIndex;
                neuron.GetComponent<NeuronController>().SetStoredConsume(int.Parse(strValues[storedConsumeIndex + 1]));
            }

            //outputNeuron
            outputNeuronIndex = Array.IndexOf(strValues, "outputNeuron", outputNeuronSearchIndex, Mathf.Min(searchArea, strValues.Length-outputNeuronSearchIndex));
            if(outputNeuronIndex >= 0){
                print("Parsing OutputNeuron");
                positionSearchIndex = outputNeuronIndex;
                spikesSearchIndex = outputNeuronIndex;
                storedReceivedSearchIndex = outputNeuronIndex;
                if(bool.Parse(strValues[outputNeuronIndex+1])){
                    neuron.GetComponent<NeuronController>().SetToOutputNeuron();
                }
            }

            storedReceivedIndex = Array.IndexOf(strValues, "storedReceived", storedReceivedSearchIndex, Mathf.Min(searchArea, strValues.Length-storedReceivedSearchIndex));
            if(storedReceivedIndex >= 0){
                print("Parsing StoredReceived");
                positionSearchIndex = storedReceivedIndex;
                spikesSearchIndex = storedReceivedIndex;
                bitStringSearchIndex = storedReceivedIndex;
                neuron.GetComponent<NeuronController>().SetStoredReceived(int.Parse(strValues[storedReceivedIndex + 1]));
            }

            bitStringIndex = Array.IndexOf(strValues, "bitString", bitStringSearchIndex, Mathf.Min(searchArea, strValues.Length-bitStringSearchIndex));
            if(bitStringIndex >= 0){
                print("Parsing BitString");
                positionSearchIndex = bitStringIndex;
                spikesSearchIndex = bitStringIndex;
                if(strValues[bitStringIndex+1] == "null"){
                    neuron.GetComponent<NeuronController>().SetBitString("");
                }
                else{
                    neuron.GetComponent<NeuronController>().SetBitString(strValues[bitStringIndex+1]);
                }
            }

            //parse positions
            positionIndex = Array.IndexOf(strValues, "position", positionSearchIndex, Mathf.Min(searchArea, strValues.Length-positionSearchIndex));
            if(positionIndex >= 0){
                print("Parsing Positions");
                hasPositionData = true;
                positionSearchIndex = positionIndex + 1;
                string[] values = strValues[positionIndex + 1].Split(new char[] {',', '(', ')'}, StringSplitOptions.RemoveEmptyEntries);
                neuron.transform.position = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                print(strValues[positionIndex + 1]);
                spikesSearchIndex = positionIndex;
            }

            // //parse spikes
            // spikesIndex = Array.IndexOf(strValues, "spikes", spikesSearchIndex);
            // neuron.GetComponent<NeuronController>().SetSpikes(int.Parse(strValues[4 + 7*n]));
            // spikesSearchIndex = spikesIndex + 1;

            // //parse rules
            // rulesIndex = Array.IndexOf(strValues, "rules", rulesSearchIndex);
            // rulesSearchIndex = rulesIndex + 1;
            // string rules = strValues[6 + 7*n];
            // rules = string.Join("\n", rules.Split(new char[] {'[', ']', ','}, StringSplitOptions.RemoveEmptyEntries));
            // neuron.GetComponent<NeuronController>().SetRules(rules);
            // print(rules);

            // //parse outsynapses
            // outSynapsesIndex = Array.IndexOf(strValues, "outsynapses", outSynapsesSearchIndex);
            // outSynapsesSearchIndex = outSynapsesIndex + 1;

            // string outSynapses = strValues[8 + 7*n];
            // string[] outSynapsesArray = outSynapses.Split(new char[] {'[', ']', ',', 'N'}, StringSplitOptions.RemoveEmptyEntries);
            // List<int> outSynapsesList = new List<int>();
            // foreach(string outSynapse in outSynapsesArray){
            //     NewSynapse(neuron.name, outSynapse);
            //     outSynapsesList.Add(int.Parse(outSynapse));
            // }
            // neuron.GetComponent<NeuronController>().SetOutSynapses(outSynapsesList);

            // n+=1;
        }

        print("PARSING END");

        return hasPositionData;
    }

    public void testDecodeFromFormat(){
        Load();
    }
}
