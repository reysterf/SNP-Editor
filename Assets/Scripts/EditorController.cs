using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Animations;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;
using SFB;
using SWF = System.Windows.Forms;

public class EditorController : MonoBehaviour
{
    public ZoomController zc;

    private int neuronCount = 0;
    private int lineCount = 0;
    private int globalTime = 0;

    public GameObject MainCamera;
    public GameObject cameraCenterArea;

    public Canvas NeuronsSynapsesCanvas;
    public GameObject SynapsesPanel;

    private Vector3 screenPoint;
    private Vector3 offset;
    private Vector3 prevPosition;
    private float panSensitivity = 1;

    public GameObject guidedMenu;
    public GameObject NeuronPrefab;
    public GameObject OutputNeuronPrefab;
    public GameObject NeuronWithRules;
    public GameObject NeuronWithoutRules;
    public GameObject Neurons;
    public GameObject guidedMenusContainer;
    public GameObject Synapses;
    public GameObject SynapsePrefab;
    public GameObject rulePrefab;

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
    public bool enableAnimationMode = true;

    public bool guidedMode = false;

    private bool settingsMenuMode = false;

    private bool panMode = false;

    private Vector3 synapseStart;
    private Vector3 synapseEnd;

    private List<int> neurons = new List<int>();
    private List<int> outputneurons = new List<int>();
    private List<(int, int)> synapses = new List<(int, int)>();

    public GameObject settingsMenu;
    public GameObject helpMenu;
    public GameObject statusBar;
    public GameObject ChoiceMenu;
    public GameObject choiceContent;
    public GameObject choiceElement;
    public GameObject choicePerNeuron;
    public GameObject editNeuronMenu;
    public GameObject editRulesMenu;
    public GameObject editSpikesMenu;
    public GameObject neuronLabel;
    // public GameObject cancelButton;
    public GameObject nextButton;
    public GameObject playButton;
    public GameObject backButton;
    public Sprite playImage;
    public Sprite pauseImage;

    public GameObject newSynapseModeIndicator;
    public Button newSynapseButton;
    public GameObject editNeuronModeIndicator;
    public Button editNeuronButton;
    public GameObject deleteNeuronModeIndicator;
    public Button deleteNeuronButton;
    public GameObject deleteSynapseModeIndicator;
    public Button deleteSynapseButton;

    public GameObject panModeIndicator;

    public GameObject viewButtons;
    public GameObject controlButtons;
    public GameObject titleText;
    public GameObject showButtonsButton;
    public GameObject settingsButton;
    public GameObject helpButton;

    private bool showButtonsMode = false;

    private GameObject activeNeuronForEditing;

    private string initialString;
    private string modifiedString;

    private GameObject initialGameObject;
    private GameObject modifiedGameObject;

    private bool showRules = true;
    private bool showLabels = true;
    private bool showModeChanged = false;
    private int fireState = 0;

    public Text showRulesText;
    public Text showLabelsText;

    public GameObject deleteSynapseInstanceButton;

    public GameObject pseudorandomToggleIndicator;
    public GameObject guidedToggleIndicator;
    public Text outputPathText;


    public Material white;

    public int waitTime;
    private string lastData;
    public List<(List<string>, string, int)> appliedRulesStorage;
    public ChoiceNode root;
    public ChoiceNode last;
    public List<int> choiceTimes;
    public List<List<int>> configHistory;
    public List<List<int>> delayHistory;
    private string outputPath;
    List<string> outputBitstrings = new List<string>();
    private bool guidedCreated = false;

    public static bool quitConfirmation = false;
    public static GameObject quitTest;

    public SaveMenuController saveMenuController;
    public string autoSavePath;
    public float autoSaveInterval;
    public GameObject autoSaveNotif;

    void Awake(){
        CreateSavesFolder();
        autoSavePath = Application.dataPath + "/saves/autosave.snapse";
    }

    // Start is called before the first frame update
    void Start()
    {
        editNeuronMenu.SetActive(false);
        editRulesMenu.SetActive(false);
        editSpikesMenu.SetActive(false);
        // cancelButton.SetActive(false);
        settingsMenu.SetActive(false);
        helpMenu.SetActive(false);

        newSynapseModeIndicator.SetActive(false);
        editNeuronModeIndicator.SetActive(false);
        deleteNeuronModeIndicator.SetActive(false);
        deleteSynapseModeIndicator.SetActive(false);

        panModeIndicator.SetActive(false);

        lastData = null;
        root = null;
        configHistory = new List<List<int>>();
        delayHistory = new List<List<int>>();
        choiceTimes = new List<int>();
        appliedRulesStorage = new List<(List<string>, string, int)>();
        outputPath = Application.dataPath + "/output.txt";
        outputPathText.text = outputPath;


        showLabelsText.text = "Hide Labels";
        showRulesText.text = "Hide Rules";

        foreach ((int i, int j) in synapses)
        {
            print(i.ToString() + ", " + j.ToString());
        }

        SetStatusText("");

        AutoSave();
        
        //temp
        autoSaveInterval = 60f;
        //temp

        InvokeRepeating("AutoSave", autoSaveInterval, autoSaveInterval);
    }

    // Update is called once per frame
    void Update()
    {
        // print(outputPath);

        if (freeModeChanged) {
            if (!freeMode) {
                DisableButtons();
                print("Disable Buttons");
                freeModeChanged = false;
            }
            else if (freeMode) {
                EnableButtons();
                print("Enable Buttons");
                freeModeChanged = false;
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
        if(newSynapseMode){
            newSynapseButton.interactable = true;
        }
        if(editNeuronMode){
            editNeuronButton.interactable = true;
        }
        if(deleteNeuronMode){
            deleteNeuronButton.interactable = true;
        }
        if(deleteSynapseMode){
            deleteSynapseButton.interactable = true;
        }
        // if(Input.GetKeyDown("1")){
        //     if(freeMode){
        //         NewNeuron();
        //     }
        // }
        // if(Input.GetKeyDown("2")){
        //     if(freeMode){
        //         NewOutputNeuron();
        //     }
        // }
        // if(Input.GetKeyDown("3")){
        //     if(freeMode || newSynapseMode){
        //         NewSynapseToggle();
        //     }
        // }
        // if(Input.GetKeyDown("4")){
        //     if(freeMode || editNeuronMode){
        //         EditNeuronToggle();
        //     }
        // }
        // if(Input.GetKeyDown("5")){
        //     if(freeMode || deleteNeuronMode){
        //         DeleteNeuronToggle();
        //     }
        // }
        // if(Input.GetKeyDown("6")){
        //     if(freeMode || deleteSynapseMode){
        //         DeleteSynapseToggle();  
        //     }
        // }
        if(Input.GetKey(KeyCode.RightControl) ||
                Input.GetKey(KeyCode.LeftControl) ||
                Input.GetKey(KeyCode.LeftApple) ||
                Input.GetKey(KeyCode.RightApple)){
            if(Input.GetKeyDown("s")){
                if(autoSavePath == Application.dataPath + "/saves/autosave.snapse"){ //First time saving
                    saveMenuController.OpenSaveMenu();
                }else{
                    AutoSave();
                }
            }
            if(Input.GetKeyDown("=")){
                zc.ZoomIn();
            }
            if(Input.GetKeyDown("-")){
                zc.ZoomOut();
            }
        }
        if(Input.GetKeyDown("p")){
            ChangePanMode();
        }
    }

    void CreateSavesFolder(){
        // Specify a name for your top-level folder.
        string folderName = Application.dataPath;

        // To create a string that specifies the path to a subfolder under your
        // top-level folder, add a name for the subfolder to folderName.
        string pathString = System.IO.Path.Combine(folderName, "saves");

        System.IO.Directory.CreateDirectory(pathString);
    }

    [RuntimeInitializeOnLoadMethod]
    static void RunOnStart()
    {
        // Application.wantsToQuit += WantsToQuit;
    }

    static bool WantsToQuit()
    {
        if (quitConfirmation)
        {
            return true;
        }
        else
        {
            RequestQuitConfirmation();
        }
        return false;
    }

    static void RequestQuitConfirmation()
    {
        SWF.DialogResult result = SWF.MessageBox.Show(
            "Are you sure you want to exit?",
            "Question",
            SWF.MessageBoxButtons.YesNo,
            SWF.MessageBoxIcon.Question);
        if (result == SWF.DialogResult.Yes)
        {
            quitConfirmation = true;
            Application.Quit();
        }
    }



    public void HideButtonsToggle(){
        if(!showButtonsMode){
            viewButtons.SetActive(true);
            controlButtons.SetActive(true);
            titleText.SetActive(true);
            showButtonsMode = true;
        }
        else if(showButtonsMode){
            viewButtons.SetActive(false);
            controlButtons.SetActive(false);
            titleText.SetActive(false);
            showButtonsMode = false;
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
        // foreach ((int i, int j) in synapses)
        // {
        //     DrawLine(i.ToString(), j.ToString());
        // }
    }

    private void BlankSlate() {
        //Reset the program state into a blank slate
        DeleteAllNeurons();
    }

    public void HelpMenuOpen(){
        SetFreeMode(false);
        helpMenu.SetActive(true);
    }

    public void HelpMenuClose(){
        SetFreeMode(true);
        helpMenu.SetActive(false);
    }


    public void SettingsMenuToggle(){
        if(!settingsMenuMode){
            settingsMenuMode = true;
            SettingsMenuOpen();
        }
        else if(settingsMenuMode){
            settingsMenuMode = false;
            SettingsMenuClose();
        }
    }

    public void SettingsMenuOpen(){
        SetFreeMode(false);
        settingsMenu.SetActive(true);        
    }

    public void SettingsMenuClose(){
        SetFreeMode(true);
        settingsMenu.SetActive(false);
    }

    public void SetToPseudorandomMode(){
        //Change nondeterminism mode to pseudorandom
        if(guidedMode == true){
            guidedMode = false;
            pseudorandomToggleIndicator.SetActive(true);
            guidedToggleIndicator.SetActive(false);
        }
    }

    public void SetToGuidedMode(){
        if(guidedMode == false){
            guidedMode = true;
            pseudorandomToggleIndicator.SetActive(false);
            guidedToggleIndicator.SetActive(true);
        }
    }

    public void SetStatusText(string statusText) {
        statusBar.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = statusText;
    }

    public bool isFreeMode() {
        return freeMode;
    }

    public void SetFreeMode(bool mode) {
        freeMode = mode;
        freeModeChanged = true;
    }

    public void DisableButtons() {
        Button[] buttons = controlButtons.transform.GetComponentsInChildren<Button>();

        foreach (Button button in buttons) {
            button.interactable = false;
        }

        settingsButton.GetComponent<Button>().interactable = false;
        showButtonsButton.GetComponent<Button>().interactable = false;
        helpButton.GetComponent<Button>().interactable = false;

    }

    public void EnableButtons() {
        Button[] buttons = controlButtons.transform.GetComponentsInChildren<Button>();

        foreach (Button button in buttons) {
            button.interactable = true;
        }

        settingsButton.GetComponent<Button>().interactable = true;
        showButtonsButton.GetComponent<Button>().interactable = true;
        helpButton.GetComponent<Button>().interactable = true;
    }

    public void ChangePanMode() {
        // SetFreeMode(!freeMode);
        panMode = !panMode;
        panModeIndicator.SetActive(panMode);
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

    public void ChangeEnableAnimationMode(){
        enableAnimationMode = !enableAnimationMode;
    }

    public bool isEnableAnimationMode(){
        return enableAnimationMode;
    }

    public void ShowChoiceMenu()
    {
        ChoiceMenu.transform.position = cameraCenterArea.transform.position;
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
        // cancelButton.SetActive(true);
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
        // cancelButton.SetActive(false);
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
        Vector3[] neuronsBounds = new Vector3[4];
        cameraCenterArea.GetComponent<RectTransform>().GetWorldCorners(neuronsBounds);

        Vector3 initialPosition = new Vector3(UnityEngine.Random.Range(neuronsBounds[0].x, neuronsBounds[3].x), UnityEngine.Random.Range(neuronsBounds[0].y, neuronsBounds[1].y), 0);
        GameObject newron = Instantiate(NeuronPrefab, initialPosition, Quaternion.identity);
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

    public void DeleteSynapseToggle(){
        if(!deleteSynapseMode){
            DeleteSynapseOn();
        }
        else if(deleteSynapseMode){
            DeleteSynapseOff();
        }
    }

    public void DeleteSynapseOn(){
        if(freeMode){
            SetFreeMode(false);
            deleteSynapseMode = true;
            deleteSynapseModeIndicator.SetActive(true);
            // Draw();
            // Synapses.GetComponent<SynapsesController>().DeleteSynapseMode(true);
            StartMode();
            DeleteSynapseStart();
        }
    }

    public void DeleteSynapseOff(){
        deleteSynapseModeIndicator.SetActive(false);
        SetFreeMode(true);
        deleteSynapseMode = false;
        Synapses.GetComponent<SynapsesController>().DeleteSynapseMode(false);
        EndMode();

        GameObject[] deleteButtons = GameObject.FindGameObjectsWithTag("Delete Button");
        foreach(GameObject delBut in deleteButtons){
            Destroy(delBut);
        }
    }

    public void DeleteSynapseStart(){

        // Draw();
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

        DeleteSynapseEnd();
    }

    public void DeleteSynapseEnd(){
        SetStatusText("Synapse deleted");


        //Synapse reset
        // GameObject[] synapsesDelete = GameObject.FindGameObjectsWithTag("Synapse");
        // foreach(GameObject synapse in synapsesDelete){
        //     Destroy(synapse);
        // }

        DeleteSynapseStart();
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
        // GameObject[] synapsesDelete = GameObject.FindGameObjectsWithTag("Synapse");
        // foreach(GameObject synapse in synapsesDelete){
        //     Destroy(synapse);
        // }
        
    }

    public void DeleteNeuronToggle(){
        if(!deleteNeuronMode){
            DeleteNeuronOn();
        }
        else if(deleteNeuronMode){
            DeleteNeuronOff();
        }
    }

    public void DeleteNeuronOn(){
        if(freeMode){
            SetStatusText("Delete Neuron: Select a neuron to delete");
            SetFreeMode(false);
            deleteNeuronMode = true;
            deleteNeuronModeIndicator.SetActive(true);
            // Neurons.GetComponent<NeuronsController>().DeleteNeuronMode(true);
            StartMode();
            DeleteNeuronStart();
        }
    }

    public void DeleteNeuronOff(){
        SetFreeMode(true);
        deleteNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().DeleteNeuronMode(false);   
        deleteNeuronModeIndicator.SetActive(false);
        EndMode();
    }

    public void DeleteNeuronStart(){
        Neurons.GetComponent<NeuronsController>().DeleteNeuronMode(true);
    }

    public void DeleteNeuron(GameObject neuron){
        removeConnectedSynapses(neuron);
        neurons.Remove(int.Parse(neuron.name));
        Destroy(neuron);
        DeleteNeuronEnd();
    }

    public void DeleteNeuronEnd(){
        // SetFreeMode(true);
        // deleteNeuronMode = false;
        // Neurons.GetComponent<NeuronsController>().DeleteNeuronMode(false);   
        SetStatusText("Neuron deleted");     
        // EndMode();
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
        List<(int,int)> synapsesToDelete = new List<(int,int)>();

        foreach ((int i, int j) in synapses)
        {
            if (i == n || j == n){
                indexToRemove.Add(k);
                synapsesToDelete.Add((i, j));
            }
            k += 1;
        }
        
        int indexOffset = 0;
        foreach (int i in indexToRemove){
            synapses.RemoveAt(i - indexOffset);
            indexOffset += 1;
        }

        foreach ((int i, int j) in synapsesToDelete){
            GameObject synapseToDelete = GameObject.Find(i.ToString() + "-" + j.ToString());
            Destroy(synapseToDelete);
        }
    }

    public void EditNeuronToggle(){
        if(!editNeuronMode){
            EditNeuronOn();
        }
        else if(editNeuronMode){
            EditNeuronOff();
        }
    }

    public void EditNeuronOn(){
        if(freeMode){
            editNeuronModeIndicator.SetActive(true);
            SetFreeMode(false);
            EditNeuronStart();
        }
    }

    public void EditNeuronOff(){
        editNeuronModeIndicator.SetActive(false);
        SetFreeMode(true);
        EditNeuronEnd();
    }

    public void EditNeuronEnd(){
        editNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);       
        EndMode();        
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

        editNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);
        editNeuronMenu.SetActive(false);
        SetStatusText("Neuron editting cancelled");
    }

    public void EditNeuronSave(){

        editNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);
        editNeuronMenu.SetActive(false);
    }

    public void EditRulesStart(){
        // SetFreeMode(false);
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
        // SetFreeMode(true);
        editRulesMode = false;
        editRulesMenu.SetActive(false);

        InputField rulesInputField = editRulesMenu.transform.Find("Rules InputField").GetComponent<InputField>();

        if(activeNeuronForEditing.GetComponent<NeuronController>().SetRules(rulesInputField.text))
            SetStatusText("Rules successfully edited");
        else
            SetStatusText("Invalid rule format");

        // Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);       
        // EndMode();
    }

    public void EditRulesCancel(){
        // SetFreeMode(true);
        editRulesMode = false;
        editRulesMenu.SetActive(false);

        // Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);

        SetStatusText("Rules edit cancelled");
        // EndMode();
    }

    public void EditSpikesStart(){
        // SetFreeMode(false);
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
        // print(spikesInputField.text);
    }

    public void EditSpikesConfirm(){
        // SetFreeMode(true);
        // freeMode = true;

        InputField spikesInputField = editSpikesMenu.transform.Find("Spikes InputField").GetComponent<InputField>();

        editSpikesMode = false;
        editSpikesMenu.SetActive(false);

        activeNeuronForEditing.GetComponent<NeuronController>().SetSpikes(int.Parse(spikesInputField.text));

        SetStatusText("Spikes successfully edited");
        // Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);
        // EndMode();
    }

    public void EditSpikesCancel(){
        // SetFreeMode(true);
        // freeMode = true;

        editSpikesMode = false;
        editSpikesMenu.SetActive(false);

        // Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);
        // SetStatusText("Spikes edit cancelled");
        // EndMode();
    }



    public void NewSynapseToggle(){

        if(!newSynapseMode){
            NewSynapseOn();
        }
        else if(newSynapseMode){
            NewSynapseOff();
        }
    }

    void NewSynapseOn(){
        if(freeMode){
            SetStatusText("Entered New Synapse Mode");
            newSynapseModeIndicator.SetActive(true);
            newSynapseMode = true;
            SetFreeMode(false);
            // newSynapseButton.interactable = true;
            // freeMode = false;
            StartMode();
            NewSynapseStart();
            // Neurons.GetComponent<NeuronsController>().NewSynapseMode(true);
        }
    }

    public void NewSynapseStart(){
        // if(freeMode){
        //     SetStatusText("New Synapse");
        //     newSynapseMode = true;
        //     SetFreeMode(false);
        //     // freeMode = false;
        //     StartMode();
        Neurons.GetComponent<NeuronsController>().NewSynapseMode(true);
        // }
    }

    public void NewSynapse(string sourceNeuronName, string destNeuronName, bool loadMode = false){
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
            // NewSynapseError();
            invalidSynapse = true;
            NewSynapseError();
        }

        if(!synapseExists && !invalidSynapse){
            synapses.Add((sourceNeuron, destNeuron));

            //add outSynapse to source neuron
            GameObject.Find(sourceNeuronName).GetComponent<NeuronController>().AddOutSynapse(int.Parse(destNeuronName));

            // DrawLine(sourceNeuronName, destNeuronName);
            InstantiateSynapse(sourceNeuronName, destNeuronName);
            if(!loadMode){
                NewSynapseEnd();
            }
        }
    }

    public void NewSynapseEnd(){ //Possibly depracated in favor of NewSynapseOff
        // newSynapseMode = false;
        // SetFreeMode(true);
        SetStatusText("Synapse successfully created");
        NewSynapseStart();
        // EndMode();
        // freeMode = true;
    }

    public void NewSynapseOff(){
        newSynapseMode = false;
        newSynapseModeIndicator.SetActive(false);
        SetStatusText("Exited New Synapse Mode");
        Neurons.GetComponent<NeuronsController>().NewSynapseMode(false);
        SetFreeMode(true);
        EndMode();
    }

    public void NewSynapseError(){
        NewSynapseStart();
        // newSynapseMode = false;
        // SetFreeMode(true);
        // SetStatusText("Synapse successfully created");
        // EndMode();
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

    public void InstantiateSynapse(string sourceNeuronName, string destNeuronName){
        print("Instantiating synapse " + sourceNeuronName + "-" + destNeuronName);
        GameObject sourceNeuron = GameObject.Find(sourceNeuronName);
        print(sourceNeuron.transform.position);
        if(sourceNeuron == null){
            print("Oopsie no sourceNeuron");
        }
        GameObject destNeuron = GameObject.Find(destNeuronName);
        if(destNeuron == null){
            print("Oopsie no destNeuron");
        }
        GameObject synapse = Instantiate(SynapsePrefab, sourceNeuron.transform.position, Quaternion.identity);
        synapse.transform.SetParent(SynapsesPanel.transform);

        //Set tag
        synapse.transform.tag = "Synapse";

        //Set name
        synapse.name = sourceNeuronName + "-" + destNeuronName;

        //Set scale
        synapse.transform.localScale = new Vector3(1, 1, 1);

        //Set length
        float distance = Vector3.Distance(sourceNeuron.transform.localPosition, destNeuron.transform.localPosition);
        synapse.GetComponent<RectTransform>().sizeDelta = new Vector2(distance, 30);

        //Set rotation
        ConstraintSource source = new ConstraintSource();
        source.sourceTransform = destNeuron.transform;
        source.weight = 1;
        synapse.GetComponent<AimConstraint>().AddSource(source);

        //Attach Objects
        // synapse.AddComponent<SynapseController>();
        synapse.GetComponent<SynapseController>().SetSourceNeuronName(sourceNeuronName);
        synapse.GetComponent<SynapseController>().SetDestNeuronName(destNeuronName);
        synapse.GetComponent<SynapseController>().SetSourceNeuron(sourceNeuron);
        synapse.GetComponent<SynapseController>().SetDestNeuron(destNeuron);

        // if(deleteSynapseMode){
        //     GameObject delbut = Instantiate(deleteSynapseInstanceButton, (sourceNeuron.transform.position + destNeuron.transform.position) * 0.5f, Quaternion.identity);
        //     delbut.transform.localScale = new Vector3(.015f, .015f, 0);
        //     delbut.transform.SetParent(synapse.transform);
        //     delbut.transform.tag = "Delete Button";
        // }

        // synapse.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, distance); 
    }

    public void UpdateSynapseLength(){
        
    }

    public void DrawLine(string sourceNeuronName, string destNeuronName, float duration = 0.03f)
    {
        //Deprecated
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
            GameObject delbut = Instantiate(deleteSynapseInstanceButton, (start + end) * 0.5f, Quaternion.identity);
            delbut.transform.localScale = new Vector3(.015f, .015f, 0);
            delbut.transform.SetParent(newSynapse.transform);
            delbut.transform.tag = "Delete Button";
        }
        if(!deleteSynapseMode){
            // GameObject.Destroy(newSynapse, duration);
        }
        lineCount += 1;
    }

    public void PlayButton()
    {
        Debug.Log((playButton.GetComponent<Image>().sprite == pauseImage));
        if (playButton.GetComponent<Image>().sprite == pauseImage)
            StopContinuous();
        else if (playButton.GetComponent<Image>().sprite == playImage)
            StartContinuous();
    }

    public void StopContinuous()
    {
        backButton.GetComponent<Button>().interactable = true;
        nextButton.GetComponent<Button>().interactable = true;
        playButton.GetComponent<Image>().sprite = playImage;
        fireState = 0;
        SetStatusText("Stopped at t = " + globalTime);
    }

    public void StartContinuous()
    {
        backButton.GetComponent<Button>().interactable = false;
        nextButton.GetComponent<Button>().interactable = false;
        playButton.GetComponent<Image>().sprite = pauseImage;
        fireState = 1;
        IEnumerator continuousIEnum = ContinuousFire();
        StartCoroutine(continuousIEnum);
    }

    IEnumerator ContinuousFire()
    {
        while(fireState > 0)
        {
            Debug.Log("before:"+fireState);
            StartFire();
            Debug.Log("after:" + fireState);
            while (fireState != 2)
                yield return null;
            print(fireState);
            yield return new WaitForSeconds(waitTime);
            print("Waited 2 secs");
        }
    }

    public bool CheckHalt()
    {
        List<int> currentTimers = GetAllDelay();
        bool noActionsLeft = true;
        foreach(int timer in currentTimers)
        {
            if (timer >= 0)
                noActionsLeft = false;
        }
        foreach((List<string>,string,int)appliedRule in appliedRulesStorage)
        {
            if (appliedRule.Item1.Count > 0)
                noActionsLeft = false;
        }

        fireState = 2;
        return noActionsLeft;
    }

    public void StartFire()
    {
        IEnumerator oneStep = FireOneStep();
        StartCoroutine(oneStep);
    }

    IEnumerator FireOneStep()
    {
        configHistory.Add(GetAllSpikes());
        delayHistory.Add(GetAllDelay());
        fireState = 1;
        //create Root (ie. the first configuration)
        if (root == null)
        {
            root = new ChoiceNode(root, GetAllSpikes());
            last = root;
            choiceTimes.Add(globalTime);
        }

        //Check applicable rules of all neurons
        appliedRulesStorage.Clear();
        (List<string>, string) rule = (new List<string>(), "");
        foreach (int i in neurons)
        {
            //rules is a tuple of (List of applicable rules, chosenrule)
            rule = Neurons.GetComponent<NeuronsController>().Fire(GameObject.Find("Neurons/" + i.ToString()));
            appliedRulesStorage.Add((rule.Item1, rule.Item2, i));
        }
        bool halting = CheckHalt();
        yield return halting;
        if (halting)
        {
            StopContinuous();
            configHistory.RemoveAt(configHistory.Count - 1);
            delayHistory.RemoveAt(delayHistory.Count - 1);
        }
        else
        {
            if (guidedMode && appliedRulesStorage.Count > 0 && guidedCreated == false)
                CreateGuidedMenus();
            IEnumerator waitGuided = WaitForGuided();
            StartCoroutine(waitGuided);
        }     

        /*
        List<(List<string>, string ,int)> nondeterministicList = new List<(List<string>, string, int)>();
        (List<string>, string) determinismCheck = (new List<string>(), "");

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
        }*/
    }

    private void CreateGuidedMenus()
    {
        SetFreeMode(false);
        guidedCreated = true;
        foreach((List<string>, string, int) rule in appliedRulesStorage)
        {
            if(rule.Item1.Count > 1)
            {
                GameObject neuron = GameObject.Find("Neurons/" + rule.Item3.ToString());
                GameObject newGuidedMenu = Instantiate(guidedMenu, neuron.transform.position, Quaternion.identity, guidedMenusContainer.transform);
                newGuidedMenu.GetComponent<GuidedMenuController>().SetUpMenu(rule.Item1, rule.Item3);
            }
        }
    }

    public void SetGuidedChoice(List<string> rules, string choice, int neuronNo)
    {
        for (int i=0; i<appliedRulesStorage.Count; i++)
        {
            if(neuronNo == appliedRulesStorage[i].Item3)
            {
                appliedRulesStorage[i] = (rules, choice, neuronNo);
                Neurons.GetComponent<NeuronsController>().SetChosenRule(GameObject.Find("Neurons/" + neuronNo.ToString()), choice);
            }
        }
        LogAppliedRules();
    }

    public void CancelGuided()
    {

    }

    IEnumerator WaitForGuided()
    {
        bool allNeuronsGuided = false;
        while (!allNeuronsGuided)
        {
            allNeuronsGuided = true;
            foreach ((List<string>, string, int) rule in appliedRulesStorage)
            {
                if (rule.Item1.Count > 0 && rule.Item2 == "")
                    allNeuronsGuided = false;
            }
            yield return null;
        }
                
        EndFire();
    }

    public void EndFire()
    {
        guidedCreated = false;
        SetFreeMode(true);
        foreach(int i in neurons)
        {
            Neurons.GetComponent<NeuronsController>().EndFireNeurons(GameObject.Find("Neurons/" + i.ToString()));
        }
        
        outputBitstrings.Clear();
        if (outputneurons.Count > 0)
        {
            foreach (int i in outputneurons)
            {
                string outputBitsring = i.ToString() + ":" + 
                    Neurons.GetComponent<NeuronsController>().UpdateOutputNeurons(GameObject.Find("Neurons/" + i.ToString()));
                outputBitstrings.Add(outputBitsring);
            }
        }
        if(outputBitstrings.Count > 0)
            SaveOutput(outputBitstrings);

        
        globalTime++;
        SetStatusText("Fired at t = " + globalTime);

        List<(List<string>, string, int)> nondeterministicList = new List<(List<string>, string, int)>();
        foreach ((List<string>, string, int)rule in appliedRulesStorage)
        {
            if (rule.Item1.Count > 1)
            {
                nondeterministicList.Add(rule);
            }
        }
        if(nondeterministicList.Count > 0)
            AddChoiceElement(nondeterministicList);
        fireState = 2;
    }

    public void GoBackOne()
    {
        if(configHistory.Count > 0 && globalTime > 0)
        {
            SetAllSpikes(configHistory[configHistory.Count - 1]);
            configHistory.RemoveAt(configHistory.Count - 1);
            SetAllDelays(delayHistory[delayHistory.Count - 1]);
            delayHistory.RemoveAt(delayHistory.Count - 1);

            outputBitstrings.Clear();
            if (outputneurons.Count > 0)
            {
                foreach (int i in outputneurons)
                {
                    string outputBitsring = i.ToString() + ":" +
                        Neurons.GetComponent<NeuronsController>().RetractOutput(GameObject.Find("Neurons/" + i.ToString()));
                    outputBitstrings.Add(outputBitsring);
                }
            }
            if (outputBitstrings.Count > 0)
                SaveOutput(outputBitstrings);

            globalTime--;
            SetStatusText("Went back to t = " + globalTime);
        }    
    }

    public void LogConfig()
    {
        string log = "";
        foreach(List<int> config in configHistory)
        {
            foreach(int i in config)
            {
                log+=i.ToString() + ", ";
            }
            log+="\n";
        }
        Debug.Log(log);
    }

    public void LogDelay()
    {
        string log = "";
        foreach(List<int> delay in delayHistory)
        {
            foreach(int i in delay)
            {
                log += i.ToString() + ", ";
            }
            log += "\n";
        }
        Debug.Log(log);
    }

    public void GoToChoice()
    {

            if (configHistory.Count > 0)
            {
                SetAllSpikes(configHistory[0]);
                if (outputneurons.Count > 0)
                {
                    foreach (int i in outputneurons)
                    {
                        Neurons.GetComponent<NeuronsController>().ClearOutput(GameObject.Find("Neurons/" + i.ToString()));
                    }
                }
                globalTime = 0;
                SetStatusText("Went back to t = " + globalTime);
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
            ignoredRules = ignoredRules.TrimEnd((new char[] { '\n' }));
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
        foreach ((List<string> matchedRules, string chosen, int name) in appliedRulesStorage)
        {
            foreach(string rule in matchedRules)
            {
                appliedRules += rule + ", ";
            }
            appliedRules += "|| The Chosen: " + chosen;
            appliedRules += "|| NeuronNo: " + name.ToString();
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
                    newPath = Application.dataPath + "/..";
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
        // var path = StandaloneFileBrowser.SaveFilePanel(
        // "Save as",
        // "",
        // "Untitled.snapse",
        // "snapse");
        string path = autoSavePath;

        if (path.Length != 0)
        {
            var configData = System.Text.Encoding.UTF8.GetBytes(EncodeToFormat());
            if (configData != null)
                File.WriteAllBytes(path, configData);
            
            SetStatusText("Saved!");
        }

    }

    public string GetAutoSavePath(){
        return autoSavePath;
    }

    public void ChangeAutoSavePath(string newPath){
        autoSavePath = newPath;
        AutoSave();
    }

    public void AutoSave(){
        print(Time.time);
        string path = autoSavePath;

        if (path.Length != 0)
        {
            var configData = System.Text.Encoding.UTF8.GetBytes(EncodeToFormat());
            if (configData != null)
                File.WriteAllBytes(path, configData);
            
            SetStatusText("Autosaved!");
        }
        autoSaveNotif.GetComponent<Text>().text = "System autosaved at: \n" + autoSavePath;
        IEnumerator notify = AutoSaveNotify();
        StartCoroutine(notify);

    }

    private IEnumerator AutoSaveNotify(){
        autoSaveNotif.SetActive(true);
        yield return new WaitForSeconds(10f);
        autoSaveNotif.SetActive(false);

    }

    public void LoadFromPath(string path){
        // foreach (string p in path)
        //     print(p);
        
        if (path.Length != 0)
        {
            var fileContent = File.ReadAllBytes(path);
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
        configHistory.Clear();
        globalTime = 0;
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
        configHistory.Clear();
        globalTime = 0;
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
        foreach (int i in neurons)
        {
            if (i < delayList.Count)
            {
                GameObject neuronObject = GameObject.Find(i.ToString());
                neuronObject.GetComponent<NeuronController>().SetDelay(delayList[i]);
            }
        }
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
            string[] parts = rule.Split(separators, 4, StringSplitOptions.None);

            try
            {
                print("2:" + parts[2]+"end");
                print(parts[2] != "");
                print(!Regex.Match(parts[2], "^ *a* *$").Success);
                if(!Regex.Match(parts[0],"^[^A-Zb-z0-9]*a[^A-Zb-z0-9]*$").Success)
                    return false;
                if(!Regex.Match(parts[1], "^ *a+ *$").Success)
                    return false;
                if(!Regex.Match(parts[2], "^ *a* *$").Success)
                    return false;
                if(!Regex.Match(parts[3], "^ *[0-9]+ *$").Success)
                {
                    if(parts[3].Length != 0)
                    {
                        if(!Regex.Match(parts[3], "^ *$").Success)
                            return false;
                    }         
                }  
                Regex.Match("", parts[0]);
                string part1 = parts[1].Replace(" ", "");
                string part2 = parts[2].Replace(" ", "");
                //consumed should be greater than or equal to produced
                if (part1.Length < part2.Length)
                    return false;
            }
            catch(ArgumentException e)
            {
                print(e);
                return false;
            }
            catch (IndexOutOfRangeException e)
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
            // print(i.ToString() + ": " + str);
            i+=1;
        }

        // print("PARSING START");

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
            // print("Parsing Spikes");
            spikesIndex = Array.IndexOf(strValues, "spikes", spikesSearchIndex, Mathf.Min(searchArea, strValues.Length-spikesSearchIndex));
            neuron.GetComponent<NeuronController>().SetSpikes(int.Parse(strValues[spikesIndex + 1]));
            spikesSearchIndex = spikesIndex + 1;
            rulesSearchIndex = spikesIndex;

            //parse rules
            // print("Parsing Rules");
            rulesIndex = Array.IndexOf(strValues, "rules", rulesSearchIndex, Mathf.Min(searchArea, strValues.Length-rulesSearchIndex));
            rulesSearchIndex = rulesIndex + 1;
            outSynapsesSearchIndex = rulesIndex;

            rulesSearchIndex = rulesIndex + 1;

            string rules = strValues[rulesIndex + 1];
            rules = string.Join("\n", rules.Split(new char[] {'[', ']', ','}, StringSplitOptions.RemoveEmptyEntries));
            // print(rules);
            if(rules != ""){
                neuron.GetComponent<NeuronController>().SetRules(rules);
            }
            // print(rules);

            //parse outsynapses
            // print("Parsing OutSynapses");
            outSynapsesIndex = Array.IndexOf(strValues, "outsynapses", outSynapsesSearchIndex, Mathf.Min(searchArea, strValues.Length-outSynapsesSearchIndex));
            outSynapsesSearchIndex = outSynapsesIndex + 1;
            delaySearchIndex = outSynapsesIndex;
            positionSearchIndex = outSynapsesIndex;
            spikesSearchIndex = outSynapsesIndex;

            string outSynapses = strValues[outSynapsesIndex + 1];
            string[] outSynapsesArray = outSynapses.Split(new char[] {'[', ']', ',', 'N', 'O'}, StringSplitOptions.RemoveEmptyEntries);
            List<int> outSynapsesList = new List<int>();
            foreach(string outSynapse in outSynapsesArray){
                NewSynapse(neuron.name, outSynapse, true);
                outSynapsesList.Add(int.Parse(outSynapse));
            }
            neuron.GetComponent<NeuronController>().SetOutSynapses(outSynapsesList);

            //parse delay
            delayIndex = Array.IndexOf(strValues, "delay", delaySearchIndex, Mathf.Min(searchArea, strValues.Length-delaySearchIndex));
            if(delayIndex >= 0){
                // print("Parsing Delay");
                positionSearchIndex = delayIndex;
                storedGiveSearchIndex = delayIndex;
                spikesSearchIndex = delayIndex;
                neuron.GetComponent<NeuronController>().SetDelay(int.Parse(strValues[delayIndex + 1]));
            }

            //parse storedGive & storedConsume
            storedGiveIndex = Array.IndexOf(strValues, "storedGive", storedGiveSearchIndex, Mathf.Min(searchArea, strValues.Length-storedGiveSearchIndex));
            if(storedGiveIndex >= 0){
                // print("Parsing StoredGive");
                positionSearchIndex = storedGiveIndex;
                spikesSearchIndex = storedGiveIndex;
                storedConsumeSearchIndex = storedGiveIndex;
                neuron.GetComponent<NeuronController>().SetStoredGive(int.Parse(strValues[storedGiveIndex + 1]));
            }

            //storedConsume
            storedConsumeIndex = Array.IndexOf(strValues, "storedConsume", storedConsumeSearchIndex, Mathf.Min(searchArea, strValues.Length-storedConsumeSearchIndex));
            if(storedConsumeIndex >= 0){
                // print("Parsing StoredConsume");
                positionSearchIndex = storedConsumeIndex;
                spikesSearchIndex = storedConsumeIndex;
                outputNeuronSearchIndex = storedConsumeIndex;
                neuron.GetComponent<NeuronController>().SetStoredConsume(int.Parse(strValues[storedConsumeIndex + 1]));
            }

            //outputNeuron
            outputNeuronIndex = Array.IndexOf(strValues, "outputNeuron", outputNeuronSearchIndex, Mathf.Min(searchArea, strValues.Length-outputNeuronSearchIndex));
            if(outputNeuronIndex >= 0){
                // print("Parsing OutputNeuron");
                positionSearchIndex = outputNeuronIndex;
                spikesSearchIndex = outputNeuronIndex;
                storedReceivedSearchIndex = outputNeuronIndex;
                if(bool.Parse(strValues[outputNeuronIndex+1])){
                    neuron.GetComponent<NeuronController>().SetToOutputNeuron();
                }
            }

            storedReceivedIndex = Array.IndexOf(strValues, "storedReceived", storedReceivedSearchIndex, Mathf.Min(searchArea, strValues.Length-storedReceivedSearchIndex));
            if(storedReceivedIndex >= 0){
                // print("Parsing StoredReceived");
                positionSearchIndex = storedReceivedIndex;
                spikesSearchIndex = storedReceivedIndex;
                bitStringSearchIndex = storedReceivedIndex;
                neuron.GetComponent<NeuronController>().SetStoredReceived(int.Parse(strValues[storedReceivedIndex + 1]));
            }

            bitStringIndex = Array.IndexOf(strValues, "bitString", bitStringSearchIndex, Mathf.Min(searchArea, strValues.Length-bitStringSearchIndex));
            if(bitStringIndex >= 0){
                // print("Parsing BitString");
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
                // print("Parsing Positions");
                hasPositionData = true;
                positionSearchIndex = positionIndex + 1;
                string[] values = strValues[positionIndex + 1].Split(new char[] {',', '(', ')'}, StringSplitOptions.RemoveEmptyEntries);
                neuron.transform.position = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                // print(strValues[positionIndex + 1]);
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
