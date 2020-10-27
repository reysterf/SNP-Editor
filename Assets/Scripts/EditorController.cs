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
    private int globalTime = 0; //int containing the current time for the whole SN P system

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

    private List<int> neurons = new List<int>(); //List of all the neuron names (names are numbers)
    private List<int> outputneurons = new List<int>(); //List of all the output neuron names (names are numbers)
    private List<(int, int)> synapses = new List<(int, int)>(); //List of synapse connections in the form: (source, target)

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
    public GameObject simulationButtons;
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
    private int fireState = 0; //0 = stop, 1 = start, 2 = end of a round, 3 = waiting for choices

    public Text showRulesText;
    public Text showLabelsText;

    public GameObject deleteSynapseInstanceButton;

    public GameObject pseudorandomToggleIndicator;
    public GameObject guidedToggleIndicator;
    public Text outputPathText;
    public Text savesPathText;

    public Material white;

    public int waitTime; //Changeable time to wait for each step in continuous fire
    private string lastData;
    public List<(List<string>, string, int)> appliedRulesStorage; //List containing (applicable rules, chosen rule, neuron number)
    public ChoiceNode root; //The original configuration of the system at t = 0
    public ChoiceNode last; //The last nondeterministic choice made
    public List<int> choiceTimes; //List of time t that a nondeterministic choice was made
    public List<List<int>> configHistory; //List containing the spikes for each neuron. A neuron is identified by the index
    public List<List<int>> delayHistory; //List containing the timers for each neuron. A neuron is identified by the index
    private string outputPath;
    List<string> outputBitstrings = new List<string>(); //List containing the bitstrings of all output neurons
    private bool guidedCreated = false; //Bool for checking if a guided menu has already been created

    public static bool quitConfirmation = false;
    public static GameObject quitTest;

    public SaveMenuController saveMenuController;
    public string autoSavePath;
    public string tempAutoSavePath;
    public float autoSaveInterval;
    public GameObject autoSaveNotif;

    bool dragMode = true;
    bool dragModeChanged = false;
    bool editInstanceMode = false;

    public GameObject invalidFileErrorWindow;

    void Awake(){
        CreateSavesFolder();
        autoSavePath = Application.dataPath + "/saves/autosave.snapse";
    }

    // Start is called before the first frame update
    void Start()
    {
        //Hide menus at startup
        editNeuronMenu.SetActive(false);
        editRulesMenu.SetActive(false);
        editSpikesMenu.SetActive(false);
        settingsMenu.SetActive(false);
        helpMenu.SetActive(false);

        //Hide mode indicators at startup
        newSynapseModeIndicator.SetActive(false);
        editNeuronModeIndicator.SetActive(false);
        deleteNeuronModeIndicator.SetActive(false);
        deleteSynapseModeIndicator.SetActive(false);

        //Disable pan mode at startup
        panModeIndicator.SetActive(false);

        //Initial values
        lastData = null;
        root = null;
        configHistory = new List<List<int>>();
        delayHistory = new List<List<int>>();
        choiceTimes = new List<int>();
        appliedRulesStorage = new List<(List<string>, string, int)>();
        outputPath = Application.dataPath + "/output.txt";
        outputPathText.text = outputPath;
        savesPathText.text = Application.dataPath + "/saves/";


        showLabelsText.text = "Hide Labels";
        showRulesText.text = "Hide Rules";

        SetStatusText("");

        AutoSave();
        
        autoSaveInterval = 60f;

        //Start autosaving thread
        InvokeRepeating("AutoSave", autoSaveInterval, autoSaveInterval);
    }

    // Update is called once per frame
    void Update()
    {
        if (freeModeChanged) {
            if (!freeMode) {
                DisableButtonsExceptView();
                print("Disable Buttons");
                freeModeChanged = false;
            }
            else if (freeMode) {
                EnableButtonsExceptView();
                print("Enable Buttons");
                freeModeChanged = false;
            }
        }
        if (dragModeChanged)
        {
            if (panMode)
            {
                dragMode = false;
            }
            if (!panMode)
            {
                dragMode = true;
            }
            dragModeChanged = false;
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

        //Save shortcut
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

    void ReduceOpacity(Image img){
        img.color = new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.5019608f);
    }

    void ReturnToWhite(Image img){
        img.color = Color.white;
    }

    public void DisableNonInteractable(){
        //Reduces opacity of the non-interactable objects
        ReduceOpacity(titleText.GetComponent<Image>());
        ReduceOpacity(statusBar.GetComponent<Image>());
    }

    public void EnableNonInteractable(){
        //Returns opacity of the non-interactable objects to full
        ReturnToWhite(titleText.GetComponent<Image>());
        ReturnToWhite(statusBar.GetComponent<Image>());
    }

    public bool GetDragMode(){
        return dragMode;
    }

    public void SetDragMode(bool mode){
        dragMode = mode;
        dragModeChanged = true;
    }

    public bool GetEditInstanceMode(){
        return editInstanceMode;
    }

    void CreateSavesFolder(){
        // Specify a name for your top-level folder.
        string folderName = Application.dataPath;

        // To create a string that specifies the path to a subfolder under your
        // top-level folder, add a name for the subfolder to folderName.
        string pathString = System.IO.Path.Combine(folderName, "saves");

        System.IO.Directory.CreateDirectory(pathString);
    }


    void DisableViewButtons(){
        // Get all view buttons
        Button[] buttons = viewButtons.transform.GetComponentsInChildren<Button>();

        // Make the buttons not interactable
        foreach (Button button in buttons) {
            button.interactable = false;
        }
    }

    void EnableViewButtons(){
        // Get all view buttons
        Button[] buttons = viewButtons.transform.GetComponentsInChildren<Button>();

        // Make the buttons interactable
        foreach (Button button in buttons) {
            button.interactable = true;
        }
    }

    void DisableSimulationButtons(){
        // Get all simulation buttons
        Button[] buttons = simulationButtons.transform.GetComponentsInChildren<Button>();

        // Make the buttons not interactable
        foreach (Button button in buttons) {
            button.interactable = false;
        }
    }

    void EnableSimulationButtons(){
        // Get all simulation buttons
        Button[] buttons = simulationButtons.transform.GetComponentsInChildren<Button>();

        // Make the buttons interactable
        foreach (Button button in buttons) {
            button.interactable = true;
        }
    }


    void DisableControlButtons(){
        // Get all control buttons
        Button[] buttons = controlButtons.transform.GetComponentsInChildren<Button>();

        // Make the buttons not interactable
        foreach (Button button in buttons) {
            button.interactable = false;
        }
    }

    void EnableControlButtons(){
        // Get all control buttons
        Button[] buttons = controlButtons.transform.GetComponentsInChildren<Button>();

        // Make the buttons interactable
        foreach (Button button in buttons) {
            button.interactable = true;
        }
    }

    void DisableMiscButtons(){
        // Make the settings, show, and help button not interactable
        settingsButton.GetComponent<Button>().interactable = false;
        showButtonsButton.GetComponent<Button>().interactable = false;
        helpButton.GetComponent<Button>().interactable = false;
    }

    void EnableMiscButtons(){
        // Make the settings, show, and help button interactable
        settingsButton.GetComponent<Button>().interactable = true;
        showButtonsButton.GetComponent<Button>().interactable = true;
        helpButton.GetComponent<Button>().interactable = true;

    }

    //Used by hide buttons button
    public void HideButtonsToggle(){
        if(!showButtonsMode){
            // Shows view and control button groups and the title text
            viewButtons.SetActive(true);
            controlButtons.SetActive(true);
            titleText.SetActive(true);
            showButtonsMode = true;
        }
        else if(showButtonsMode){
            // Hides view and control button groups and the title text
            viewButtons.SetActive(false);
            controlButtons.SetActive(false);
            titleText.SetActive(false);
            showButtonsMode = false;
        }
    }


    private void BlankSlate() {
        // Reset the program state into a blank slate
        DeleteAllNeurons();
        neuronCount = 0;
    }

    public void HelpMenuOpen(){
        // Changed UI Free mode to false
        SetFreeMode(false);

        // Displays the help menu
        helpMenu.SetActive(true);

        // Makes buttons not interactable and reduces opacity for noninteractable elements
        DisableButtonsAll();
        DisableNonInteractable();
    }

    public void HelpMenuClose(){
        // Changed UI Free mode to true
        SetFreeMode(true);

        // Hides help menu
        helpMenu.SetActive(false);

        // Makes buttons interactable and returns opacity to full for noninteractable elements
        EnableButtonsAll();
        EnableNonInteractable();
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
        // Changed UI Free mode to false
        SetFreeMode(false);

        // Shows settings menu
        settingsMenu.SetActive(true);

        // Makes buttons not interactable and reduces opacity for noninteractable elements
        DisableButtonsAll();
        DisableNonInteractable();
    }

    public void SettingsMenuClose(){
        // Changed UI Free mode to true
        SetFreeMode(true);

        // Hides help menu
        settingsMenu.SetActive(false);

        // Makes buttons interactable and returns opacity to full for noninteractable elements
        EnableButtonsAll();
        EnableNonInteractable();
    }

    public void SetToPseudorandomMode(){
        //Change nondeterminism mode to pseudorandom
        if(guidedMode == true){
            guidedMode = false;

            // Updates the radio button UI
            pseudorandomToggleIndicator.SetActive(true);
            guidedToggleIndicator.SetActive(false);
        }
    }

    public void SetToGuidedMode(){
        if(guidedMode == false){
            guidedMode = true;

            // Updates the radio button UI
            pseudorandomToggleIndicator.SetActive(false);
            guidedToggleIndicator.SetActive(true);
        }
    }

    public void SetStatusText(string statusText) {
        // Changes the text on the Status bar
        statusBar.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = statusText;
    }

    public bool isFreeMode() {
        return freeMode;
    }

    public void SetFreeMode(bool mode) {
        freeMode = mode;
        freeModeChanged = true;
    }

    public void DisableButtonsExceptView() {
        DisableControlButtons();
        DisableMiscButtons();
        DisableSimulationButtons();
    }

    public void EnableButtonsExceptView() {
        EnableControlButtons();
        EnableMiscButtons();
        EnableSimulationButtons();
    }

    public void DisableButtonsAll() {
        DisableControlButtons();
        DisableMiscButtons();
        DisableSimulationButtons();
        DisableViewButtons();
    }

    public void EnableButtonsAll() {
        EnableControlButtons();
        EnableMiscButtons();
        EnableSimulationButtons();
        EnableViewButtons();
    }

    public void ChangePanMode() {
        panMode = !panMode;
        panModeIndicator.SetActive(panMode);
        panController.SetPanMode(panMode);
        if (panMode) {
            // Disables dragging
            SetDragMode(false);
            SetStatusText("Pan Mode");
        }
        else if(!panMode){
            // Enables dragging
            SetDragMode(true);
        }
    }

    public void ChangeShowLabelMode() {
        showModeChanged = true;
        showLabels = !showLabels;
    }

    public void ChangeShowRulesMode() {
        showModeChanged = true;
        showRules = !showRules;
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


    public void CancelCurrentMode(){ //Used by cancel button
        if(newSynapseMode){
            Neurons.GetComponent<NeuronsController>().NewSynapseMode(false);
            NewSynapseCancel();
        }
        if(editNeuronMode){
            EditNeuronCancel();
        }
        if(deleteNeuronMode){
            DeleteNeuronCancel();
        }
        if(deleteSynapseMode){
            DeleteSynapseCancel();
        }
    }


    public void NewNeuron() //Used by New Neuron button
    {
        if (freeMode) {
            // Looks at the current view port
            Vector3[] neuronsBounds = new Vector3[4];
            cameraCenterArea.GetComponent<RectTransform>().GetWorldCorners(neuronsBounds);

            // Sets the new neuron's position within the view port
            Vector3 initialPosition = new Vector3(UnityEngine.Random.Range(neuronsBounds[0].x, neuronsBounds[3].x), UnityEngine.Random.Range(neuronsBounds[0].y, neuronsBounds[1].y), 0);
            GameObject newron = Instantiate(NeuronPrefab, initialPosition, Quaternion.identity);

            newron.name = neuronCount.ToString();
            newron.transform.SetParent(Neurons.transform);
            newron.transform.tag = "Neuron";
            neurons.Add(neuronCount); //neuronCount

            neuronCount += 1;

            SetStatusText("New neuron created");
        }
    }

    public void NewOutputNeuron() //Used by New Output button
    {
        if (freeMode)
        {
            // Looks at the current view port
            Vector3[] neuronsBounds = new Vector3[4];
            cameraCenterArea.GetComponent<RectTransform>().GetWorldCorners(neuronsBounds);

            // Sets the new neuron's position within the view port
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

    public GameObject NewOutputNeuron(int neuronInt) // Used by load function
    {
        // Looks at the current view port
        Vector3[] neuronsBounds = new Vector3[4];
        cameraCenterArea.GetComponent<RectTransform>().GetWorldCorners(neuronsBounds);

        // Sets the new neuron's position to be within the viewport
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

    public GameObject NewNeuron(int number, bool setActive) //Used by load function
    {
        // Looks at the current view port
        Vector3[] neuronsBounds = new Vector3[4];
        cameraCenterArea.GetComponent<RectTransform>().GetWorldCorners(neuronsBounds);

        // Sets the new neuron's position to be within the viewport
        Vector3 initialPosition = new Vector3(UnityEngine.Random.Range(neuronsBounds[0].x, neuronsBounds[3].x), UnityEngine.Random.Range(neuronsBounds[0].y, neuronsBounds[1].y), 0);
        GameObject newron = Instantiate(NeuronPrefab, initialPosition, Quaternion.identity);

        newron.transform.localScale = new Vector3(1, 1, 1);
        newron.SetActive(setActive);
        newron.name = number.ToString();
        newron.transform.SetParent(Neurons.transform);
        newron.transform.tag = "Neuron";
        neurons.Add(number); //neuronCount

        neuronCount += 1;


        return newron;
    }

    public void DeleteSynapseToggle(){ //Used by Delete Synapse button
        if(!deleteSynapseMode){
            DeleteSynapseOn();
        }
        else if(deleteSynapseMode){
            DeleteSynapseOff();
        }
    }

    // Starts delete synapse mode
    public void DeleteSynapseOn(){
        if(freeMode){
            SetFreeMode(false);
            deleteSynapseMode = true;
            deleteSynapseModeIndicator.SetActive(true);
            DeleteSynapseStart();
        }
    }

    // Ends delete synapse mode
    public void DeleteSynapseOff(){
        deleteSynapseModeIndicator.SetActive(false);
        SetFreeMode(true);
        deleteSynapseMode = false;
        Synapses.GetComponent<SynapsesController>().DeleteSynapseMode(false);

        GameObject[] deleteButtons = GameObject.FindGameObjectsWithTag("Delete Button");
        foreach(GameObject delBut in deleteButtons){
            Destroy(delBut);
        }
    }

    // Starts a synapse deletion cycle
    public void DeleteSynapseStart(){
        Synapses.GetComponent<SynapsesController>().DeleteSynapseMode(true);
    }

    // Deletes a synapse
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

    // Ends a synapse deletion cycle
    public void DeleteSynapseEnd(){
        SetStatusText("Synapse deleted");

        DeleteSynapseStart();
    }

    // Exits the delete synapse mode
    public void DeleteSynapseCancel(){
        SetFreeMode(true);
        deleteSynapseMode = false;
        Synapses.GetComponent<SynapsesController>().DeleteSynapseMode(false);

        SetStatusText("Synapse deletion cancelled");

        GameObject[] deleteButtons = GameObject.FindGameObjectsWithTag("Delete Button");
        foreach(GameObject delBut in deleteButtons){
            Destroy(delBut);
        }

    }

    public void DeleteNeuronToggle(){ //Used by Delete Neuron button
        if(!deleteNeuronMode){
            DeleteNeuronOn();
        }
        else if(deleteNeuronMode){
            DeleteNeuronOff();
        }
    }

    // Starts the delete neuron mode
    public void DeleteNeuronOn(){
        if(freeMode){
            SetStatusText("Delete Neuron: Select a neuron to delete");
            SetFreeMode(false);
            deleteNeuronMode = true;
            deleteNeuronModeIndicator.SetActive(true);
            DeleteNeuronStart();
        }
    }

    // Ends the delete neuron mode
    public void DeleteNeuronOff(){
        SetFreeMode(true);
        deleteNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().DeleteNeuronMode(false);   
        deleteNeuronModeIndicator.SetActive(false);
    }

    // Starts a neuron deletion cycle
    public void DeleteNeuronStart(){
        Neurons.GetComponent<NeuronsController>().DeleteNeuronMode(true);
    }

    // Deletes a neuron
    public void DeleteNeuron(GameObject neuron){
        removeConnectedSynapses(neuron);
        neurons.Remove(int.Parse(neuron.name));
        Destroy(neuron);
        DeleteNeuronEnd();
    }

    // Ends a neuron deletion cycle
    public void DeleteNeuronEnd(){
        SetStatusText("Neuron deleted");     
    }

    // Exits a neuron deletion cycle
    public void DeleteNeuronCancel(){
        SetFreeMode(true);
        deleteNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().DeleteNeuronMode(false);   
        SetStatusText("Neuron deletion cancelled");     
    }

    // Gets all neurons and deletes them
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

    // Deletes the synapses  connected to a neuron
    private void removeConnectedSynapses(GameObject neuron){
        int n = int.Parse(neuron.name);
        int k = 0;

        List<int> indexToRemove = new List<int>();
        List<(int,int)> synapsesToDelete = new List<(int,int)>();

        // Finds synapses to delete
        foreach ((int i, int j) in synapses)
        {
            if (i == n || j == n){
                indexToRemove.Add(k);
                synapsesToDelete.Add((i, j));
            }
            k += 1;
        }
        
        // Removes synapses from array
        int indexOffset = 0;
        foreach (int i in indexToRemove){
            synapses.RemoveAt(i - indexOffset);
            indexOffset += 1;
        }

        // Destroys the synapse gameobjects
        foreach ((int i, int j) in synapsesToDelete){
            GameObject synapseToDelete = GameObject.Find(i.ToString() + "-" + j.ToString());
            RemoveOutSynapseFromSource(i, j);
            Destroy(synapseToDelete);
        }
    }

    // Deletes a neuron's outsynapse
    void RemoveOutSynapseFromSource(int source, int dest){
        GameObject sourceNeuron = GameObject.Find(source.ToString());
        sourceNeuron.GetComponent<NeuronController>().DeleteOutSynapse(dest);
    }

    public void EditNeuronToggle(){ //Used by Edit Neuron button
        if(!editNeuronMode){
            EditNeuronOn();
        }
        else if(editNeuronMode){
            EditNeuronOff();
        }
    }

    // Starts the edit neuron mode
    public void EditNeuronOn(){
        if(freeMode){
            editNeuronModeIndicator.SetActive(true);
            SetFreeMode(false);
            EditNeuronStart();
        }
    }

    // Ends the edit neuron mode
    public void EditNeuronOff(){
        editNeuronModeIndicator.SetActive(false);
        SetFreeMode(true);
        EditNeuronEnd();
    }

    // Ends a neuron editing cycle
    public void EditNeuronEnd(){
        editNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);       
    }

    // Starts a neuron editing cycle
    public void EditNeuronStart(){
        editNeuronMode = true;
        Neurons.GetComponent<NeuronsController>().EditNeuronMode(true);
    }

    // Handles calls for editing a neuron's spikes or rules
    public void EditNeuron(GameObject neuron, string mode){
        activeNeuronForEditing = neuron;
        editInstanceMode = true;
        if(mode == "spikes"){
            EditSpikesStart();
        }  
        else if(mode == "rules"){
            EditRulesStart();
        }
    }

    // Cancels the editting a neuron and disables the editing menu
    public void EditNeuronCancel(){ //Used by cancel button
        editNeuronMode = false;
        Neurons.GetComponent<NeuronsController>().EditNeuronMode(false);
        editNeuronMenu.SetActive(false);
        SetStatusText("Neuron editting cancelled");
    }

    // Starts a rule editing cycle
    public void EditRulesStart(){
        editRulesMode = true;
        editRulesMenu.SetActive(true);
        editRulesMenu.transform.position = activeNeuronForEditing.transform.position;

        EditRules();
    }

    // Rule editing main function
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

    // Saves rule changes
    public void EditRulesConfirm(){ //Used by confirm button in edit rules window
        editRulesMode = false;
        editRulesMenu.SetActive(false);

        InputField rulesInputField = editRulesMenu.transform.Find("Rules InputField").GetComponent<InputField>();

        if(activeNeuronForEditing.GetComponent<NeuronController>().SetRules(rulesInputField.text))
            SetStatusText("Rules successfully edited");
        editInstanceMode = false;
    }

    // Cancel rule changes
    public void EditRulesCancel(){ //Used by cancel button in edit rules window 
        editRulesMode = false;
        editRulesMenu.SetActive(false);

        SetStatusText("Rules edit cancelled");
        editInstanceMode = false;
    }

    // Starts a spike editing cycle
    public void EditSpikesStart(){
        editSpikesMode = true;
        editSpikesMenu.SetActive(true);
        editSpikesMenu.transform.position = activeNeuronForEditing.transform.position;

        EditSpikes();
    }

    // Spike editing main function
    public void EditSpikes(){
        InputField spikesInputField = editSpikesMenu.transform.Find("Spikes InputField").GetComponent<InputField>();

        spikesInputField.text = activeNeuronForEditing.GetComponent<NeuronController>().GetSpikesNum().ToString();
    }

    // Save spikes changes
    public void EditSpikesConfirm(){ //Used by confirm button in edit spikes window

        InputField spikesInputField = editSpikesMenu.transform.Find("Spikes InputField").GetComponent<InputField>();

        editSpikesMode = false;
        editSpikesMenu.SetActive(false);

        activeNeuronForEditing.GetComponent<NeuronController>().SetSpikes(int.Parse(spikesInputField.text));

        SetStatusText("Spikes successfully edited");
        editInstanceMode = false;

    }

    // Cancel spike changes
    public void EditSpikesCancel(){ // Used by cancel button in edit spikes window

        editSpikesMode = false;
        editSpikesMenu.SetActive(false);
        editInstanceMode = false;
    }

    public void NewSynapseToggle(){ //Used by New Synapse button

        if(!newSynapseMode){
            NewSynapseOn();
        }
        else if(newSynapseMode){
            NewSynapseOff();
        }
    }

    // Starts the New Synapse mode
    void NewSynapseOn(){
        if(freeMode){
            SetStatusText("Entered New Synapse Mode");
            newSynapseModeIndicator.SetActive(true);
            newSynapseMode = true;
            SetFreeMode(false);
            NewSynapseStart();
        }
    }

    // Starts a synapse creation cycle
    public void NewSynapseStart(){
        Neurons.GetComponent<NeuronsController>().NewSynapseMode(true);
    }

    // Synapse creation main function
    public void NewSynapse(string sourceNeuronName, string destNeuronName, bool loadMode = false){
        int sourceNeuron = int.Parse(sourceNeuronName);
        int destNeuron = int.Parse(destNeuronName);

        //Checks if synapse already exists
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
            invalidSynapse = true;
            NewSynapseError();
        }

        if(!synapseExists && !invalidSynapse){
            synapses.Add((sourceNeuron, destNeuron));

            //add outSynapse to source neuron
            GameObject.Find(sourceNeuronName).GetComponent<NeuronController>().AddOutSynapse(int.Parse(destNeuronName));

            InstantiateSynapse(sourceNeuronName, destNeuronName);
            if(!loadMode){
                NewSynapseEnd();
            }
        }
    }

    // Ends a synapse creation cycle
    public void NewSynapseEnd(){
        SetStatusText("Synapse successfully created");
        NewSynapseStart();
    }

    // Ends New Synapse mode
    public void NewSynapseOff(){
        newSynapseMode = false;
        newSynapseModeIndicator.SetActive(false);
        SetStatusText("Exited New Synapse Mode");
        Neurons.GetComponent<NeuronsController>().NewSynapseMode(false);
        SetFreeMode(true);
    }

    // Restart creation cycle
    public void NewSynapseError(){
        NewSynapseStart();
    }

    // Exit new synapse mode
    public void NewSynapseCancel(){
        newSynapseMode = false;
        SetFreeMode(true);
        SetStatusText("Synapse creation cancelled");
    }

    public void testPrintSynapse(){ //Debug function
        print("[Synapses]");
        foreach ((int i, int j) in synapses)
        {
            print(i.ToString() + " " + j.ToString());            
        }
    }

    public void testPrintNeurons(){ //Debug function
        print("[Neurons]");
        foreach(int i in neurons){
            print(i.ToString());
        }
    }

    // Create synapse gameObject
    public void InstantiateSynapse(string sourceNeuronName, string destNeuronName){
        print("Instantiating synapse " + sourceNeuronName + "-" + destNeuronName);
        GameObject sourceNeuron = GameObject.Find(sourceNeuronName);
        print(sourceNeuron.transform.position);
        if(sourceNeuron == null){
            print("No sourceNeuron");
        }
        GameObject destNeuron = GameObject.Find(destNeuronName);
        if(destNeuron == null){
            print("No destNeuron");
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
        synapse.GetComponent<SynapseController>().SetSourceNeuronName(sourceNeuronName);
        synapse.GetComponent<SynapseController>().SetDestNeuronName(destNeuronName);
        synapse.GetComponent<SynapseController>().SetSourceNeuron(sourceNeuron);
        synapse.GetComponent<SynapseController>().SetDestNeuron(destNeuron);

    }


    //Called when play button is pressed. Stops or starts the firing depending on the current state
    public void PlayButton()
    {
        Debug.Log((playButton.GetComponent<Image>().sprite == pauseImage));
        if (playButton.GetComponent<Image>().sprite == pauseImage)
            StopContinuous();
        else if (playButton.GetComponent<Image>().sprite == playImage)
            StartContinuous();
    }

    //Stops continuous firing. fireState = 0
    public void StopContinuous()
    {
        backButton.GetComponent<Button>().interactable = true;
        nextButton.GetComponent<Button>().interactable = true;
        playButton.GetComponent<Image>().sprite = playImage;
        fireState = 0;
        SetStatusText("Stopped at t = " + globalTime);
    }

    //Starts continuous firing. fireState = 1
    public void StartContinuous()
    {
        backButton.GetComponent<Button>().interactable = false;
        nextButton.GetComponent<Button>().interactable = false;
        playButton.GetComponent<Image>().sprite = pauseImage;
        fireState = 1;
        IEnumerator continuousIEnum = ContinuousFire();
        StartCoroutine(continuousIEnum);
    }

    //Continuous firing loop
    IEnumerator ContinuousFire()
    {
        //while fireState is not stop (0), continue firing
        while(fireState > 0)
        {
            Debug.Log("before:"+fireState);
            StartFire();
            Debug.Log("after:" + fireState);
            //if fireState == 1, pause
            while (fireState == 1)
                yield return null;
            print(fireState);
            //arbitrary wait time (number of seconds before next firing)
            yield return new WaitForSeconds(waitTime);
            print("Waited 2 secs");
        }
    }

    //Checks if there is no available move left in the system
    //Checks if there are any applicable rules OR if there is any neuron with timer > 0
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

        return noActionsLeft;
    }

    public void StartFire()
    {
        //Check if waiting for choices
        //if not, begin next time step
        if (fireState != 3)
        {
            IEnumerator oneStep = FireOneStep();
            StartCoroutine(oneStep);
        }
    }

    //Moves forward one time step
    IEnumerator FireOneStep()
    {
        //Adds current configuration and neuron timers to their respective histories
        configHistory.Add(GetAllSpikes());
        LogConfig();
        delayHistory.Add(GetAllDelay());

        //Create Root (ie. the first configuration)
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
        
        //Check if halting
        bool halting = CheckHalt();
        Debug.Log("halt " + halting);
        yield return halting;
        if (halting) // Stop firing
        {
            StopContinuous();
            configHistory.RemoveAt(configHistory.Count - 1);
            delayHistory.RemoveAt(delayHistory.Count - 1);
        }
        else //Check if guided, create menus (if guided)
        {
            Debug.Log("Going in");
            Debug.Log(guidedMode + "store" + appliedRulesStorage.Count + " create" + guidedCreated);
            if (guidedMode && appliedRulesStorage.Count > 0 && guidedCreated == false)
                CreateGuidedMenus();
            IEnumerator waitGuided = WaitForGuided();
            StartCoroutine(waitGuided);
        }
    }

    //Create guided menu UI
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
                //Set up with the list of applicable rules and the neuron number
                newGuidedMenu.GetComponent<GuidedMenuController>().SetUpMenu(rule.Item1, rule.Item3);
            }
        }
        //Set fireState = 3  (waiting for choices)
        fireState = 3;
    }

    //Called when a choice is clicked
    public void SetGuidedChoice(List<string> rules, string choice, int neuronNo)
    {
        //Cycle through appliedRulesStorage and search for the neuron that called the function
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

    //Checks if all rules in appliedRulesStorage has a chosen rule
    IEnumerator WaitForGuided()
    {
        bool allNeuronsGuided = false;
        while (!allNeuronsGuided)
        {
            allNeuronsGuided = true;
            foreach ((List<string>, string, int) rule in appliedRulesStorage)
            {
                //Check if neuron is non-deterministic and chosenRule is empty
                if (rule.Item1.Count > 0 && rule.Item2 == "")
                    allNeuronsGuided = false;
            }
            yield return null;
        }
     
        EndFire();
    }

    //Signals the end of one time step
    public void EndFire()
    {
        Debug.Log("end of the world");
        guidedCreated = false;
        SetFreeMode(true);
        foreach(int i in neurons)
        {
            Neurons.GetComponent<NeuronsController>().EndFireNeurons(GameObject.Find("Neurons/" + i.ToString()));
        }
        
        //Update output neurons
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

        //Log non-deterministic choises into the Choice History storage
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

    //Moves back one time step:
    //Take last configuration and delay
    //Delete last bit in output bitstring
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

    //For debugging: Logs the configuration History
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

    //For debugging: Logs the delay History
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

    //Goes to choice selected in Choice History
    //Currently set to go back to the original configuration at t = 0
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

    //Create a choice element or node in the choice history
    //Choice elements are stored as a linked list
    public void AddChoiceElement(List<(List<string>, string, int)> nondeterministicList)
    {
        //Add the new element to the linked list
        ChoiceNode newChoice = new ChoiceNode(root, GetAllSpikes(), nondeterministicList, globalTime);
        newChoice.SetFather(last);
        choiceTimes.Add(globalTime);
                
        GameObject newChoiceElement = Instantiate(choiceElement, new Vector3(transform.position.x, transform.position.y, transform.position.z),
               Quaternion.identity, choiceContent.transform);

        //set Timetext to time
        newChoiceElement.transform.Find("Time").Find("TimeText").gameObject.GetComponent<Text>().text = "t=" + globalTime.ToString();
        Transform perNeuronContainer = newChoiceElement.transform.Find("PerNeuron Container");

        foreach ((List<string> matched, string chosen, int neuronNo) in nondeterministicList)
        {
            //remove the chosen rule string from the matched (applicable) rules
            matched.Remove(chosen);
            GameObject newChoicePerNeuron = Instantiate(choicePerNeuron, new Vector3(transform.position.x, transform.position.y, transform.position.z),
               Quaternion.identity, perNeuronContainer.transform);
            //set NeuronNoText to neuron no in nondeterministic list
            //set ChosenText to chosen in nondeterministic list
            //set IgnoredText to the elements in the matched rules
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

        // Update layout calculation
        newChoiceElement.GetComponent<HorizontalLayoutGroup>().enabled = false;
        newChoiceElement.GetComponent<HorizontalLayoutGroup>().CalculateLayoutInputHorizontal();
        newChoiceElement.GetComponent<HorizontalLayoutGroup>().CalculateLayoutInputVertical();
        newChoiceElement.GetComponent<HorizontalLayoutGroup>().SetLayoutHorizontal();
        newChoiceElement.GetComponent<HorizontalLayoutGroup>().SetLayoutVertical();
        newChoiceElement.GetComponent<HorizontalLayoutGroup>().enabled = true;
    }

    //For debugging: Logs the contents of applied rules storage
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

    // Changes path where output bitstring would be saved
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

    // Save output bitstring to outputpath
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

    // Update auto save path
    public void ChangeAutoSavePath(string newPath){
        autoSavePath = newPath;
        AutoSave();
    }

    // Function triggered during autosave
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

        // Start notification coroutine
        autoSaveNotif.GetComponent<Text>().text = "System autosaved at: \n" + autoSavePath;
        IEnumerator notify = AutoSaveNotify();
        StartCoroutine(notify);

    }

    private IEnumerator AutoSaveNotify(){
        autoSaveNotif.SetActive(true);
        yield return new WaitForSeconds(10f);
        autoSaveNotif.SetActive(false);

    }

    // Load an SNP system from a snapse file
    public void LoadFromPath(string path){
        
        if (path.Length != 0)
        {
            var fileContent = File.ReadAllBytes(path);
            string formatData = System.Text.Encoding.UTF8.GetString(fileContent);

            tempAutoSavePath = autoSavePath;
            autoSavePath = path;

            bool hasPositionData = false;
            hasPositionData = DecodeFromFormat(formatData);

           
            // Auto layout files in a grid if there's no given position data
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
        int counter = 0;
        List<GameObject> nonexistentneurons = new List<GameObject>();
        foreach (int i in neurons)
        {            
            GameObject neuronObject = GameObject.Find(i.ToString());
            try
            {
                neuronObject.GetComponent<NeuronController>().SetSpikes(config[counter]);
                counter++;
            }
            catch (ArgumentOutOfRangeException e)
            {
                nonexistentneurons.Add(neuronObject);
                //SetStatusText("Neuron deleted. Went back to t = " + globalTime);
            }
        }
        foreach (GameObject neuron in nonexistentneurons)
        {
            DeleteNeuron(neuron);
            SetStatusText("Neuron(s) deleted. Went back to t = " + globalTime);
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

    //Check validity of rules
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
                print(!Regex.Match(parts[2], "^ *a+ *$").Success);
                //Check if valid regex only using a as the letter
                if (!Regex.Match(parts[0], "^[^A-Zb-z0-9]*a[^A-Zb-z0-9]*$").Success)
                {
                    SetStatusText("Invalid Rule Format: Invalid regex");
                    return false;
                }
                //Check if consumed spikes are in the alphabet {a}
                if(!Regex.Match(parts[1], "^ *a+ *$").Success)
                {
                    SetStatusText("Invalid Rule Format: Consumed spikes must be a string in {a}");
                    return false;
                }                    
                //Check if given spikes are in the alphabet{a}
                if (!Regex.Match(parts[2], "^ *a+ *$").Success)
                {
                    //Check if given spikes is 0 (for forgetting rules)
                    if(!Regex.Match(parts[2], "^ *0 *$").Success)
                    {
                        SetStatusText("Invalid Rule Format: Given spikes must be a string in {a} or 0");
                        return false;
                    }     
                    else
                    {
                        //Check if delay is 0 for forgetting rules
                        if (!Regex.Match(parts[3], "^ *0 *$").Success)
                        {
                            SetStatusText("Invalid Rule Format: Forgetting rules must have a delay of 0");
                            return false;
                        }
                    }
                }
                //Check if delay is a natural number
                if (!Regex.Match(parts[3], "^ *[0-9]+ *$").Success)
                {
                    //Also allow putting nothing in the delay (defaulting to 0)
                    if (parts[3].Length != 0)
                    {
                        if (!Regex.Match(parts[3], "^ *$").Success)
                        {
                            SetStatusText("Invalid Rule Format: Delay must be an integer");
                            return false;
                        }
                    }
                }
                Regex.Match("", parts[0]);
                //Remove all spaces
                string part1 = parts[1].Replace(" ", "");
                string part2 = parts[2].Replace(" ", "");
                //Consumed should be greater than or equal to produced
                if (part1.Length < part2.Length)
                {
                    SetStatusText("Invalid Rule Format: Consumed must be greater than given");
                    return false;
                }
                    
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

    // Create a snapse file from the SNP system in the program
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

        }

        format += neuronsDefinition;
        
        return format;
    }

    public void testEncodeToFormat(){ // Debug
        print(EncodeToFormat());        
    }

    // Parse snapse file into SNP system
    public bool DecodeFromFormat(string formatData){
        BlankSlate();
        print(formatData);
        formatData = Regex.Replace(formatData, @"\s+", ""); //Remove all whitespace

        bool hasPositionData = false;


        char[] separators = {'{', '}', '=', ':' }; //Remove delimiters
        string[] strValues = formatData.Split(separators, StringSplitOptions.RemoveEmptyEntries);


        //Parse neuron declaration
        string[] neuronDeclarations = strValues[1].Split(new char[] {'[', ']', ','}, StringSplitOptions.RemoveEmptyEntries);

        if(neuronDeclarations == null)
        {
            print("Null declarations");
        }
        if(neuronDeclarations.Length == 0)
        {
            print("Empty declarations");
            autoSavePath = tempAutoSavePath;
            BlankSlate();
            ErrorInvalidFileNotify();
            return false;
        }
        List<GameObject> neurons = new List<GameObject>();

        foreach(string neuronDeclaration in neuronDeclarations){
            if(neuronDeclaration[0] == 'N'){                
                neurons.Add(NewNeuron(int.Parse(neuronDeclaration.Substring(1, neuronDeclaration.Length-1)), true));
            }
            else if(neuronDeclaration[0] == 'O'){
                neurons.Add(NewOutputNeuron(int.Parse(neuronDeclaration.Substring(1, neuronDeclaration.Length-1))));
            }
            print(neuronDeclaration);
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

        try
        {
            foreach (GameObject neuron in neurons)
            {
                //parse spikes
                spikesIndex = Array.IndexOf(strValues, "spikes", spikesSearchIndex, Mathf.Min(searchArea, strValues.Length - spikesSearchIndex));
                neuron.GetComponent<NeuronController>().SetSpikes(int.Parse(strValues[spikesIndex + 1]));
                spikesSearchIndex = spikesIndex + 1;
                rulesSearchIndex = spikesIndex;

                //parse rules
                rulesIndex = Array.IndexOf(strValues, "rules", rulesSearchIndex, Mathf.Min(searchArea, strValues.Length - rulesSearchIndex));
                rulesSearchIndex = rulesIndex + 1;
                outSynapsesSearchIndex = rulesIndex;

                rulesSearchIndex = rulesIndex + 1;

                string rules = strValues[rulesIndex + 1];
                rules = string.Join("\n", rules.Split(new char[] { '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries));
                if (rules != "")
                {
                    neuron.GetComponent<NeuronController>().SetRules(rules);
                }

                //parse outsynapses
                outSynapsesIndex = Array.IndexOf(strValues, "outsynapses", outSynapsesSearchIndex, Mathf.Min(searchArea, strValues.Length - outSynapsesSearchIndex));
                outSynapsesSearchIndex = outSynapsesIndex + 1;
                delaySearchIndex = outSynapsesIndex;
                positionSearchIndex = outSynapsesIndex;
                spikesSearchIndex = outSynapsesIndex;

                string outSynapses = strValues[outSynapsesIndex + 1];
                string[] outSynapsesArray = outSynapses.Split(new char[] { '[', ']', ',', 'N', 'O' }, StringSplitOptions.RemoveEmptyEntries);
                List<int> outSynapsesList = new List<int>();
                foreach (string outSynapse in outSynapsesArray)
                {
                    NewSynapse(neuron.name, outSynapse, true);
                    outSynapsesList.Add(int.Parse(outSynapse));
                }
                neuron.GetComponent<NeuronController>().SetOutSynapses(outSynapsesList);

                //parse delay
                delayIndex = Array.IndexOf(strValues, "delay", delaySearchIndex, Mathf.Min(searchArea, strValues.Length - delaySearchIndex));
                if (delayIndex >= 0)
                {
                    positionSearchIndex = delayIndex;
                    storedGiveSearchIndex = delayIndex;
                    spikesSearchIndex = delayIndex;
                    neuron.GetComponent<NeuronController>().SetDelay(int.Parse(strValues[delayIndex + 1]));
                }

                //parse storedGive & storedConsume
                storedGiveIndex = Array.IndexOf(strValues, "storedGive", storedGiveSearchIndex, Mathf.Min(searchArea, strValues.Length - storedGiveSearchIndex));
                if (storedGiveIndex >= 0)
                {
                    positionSearchIndex = storedGiveIndex;
                    spikesSearchIndex = storedGiveIndex;
                    storedConsumeSearchIndex = storedGiveIndex;
                    neuron.GetComponent<NeuronController>().SetStoredGive(int.Parse(strValues[storedGiveIndex + 1]));
                }

                //storedConsume
                storedConsumeIndex = Array.IndexOf(strValues, "storedConsume", storedConsumeSearchIndex, Mathf.Min(searchArea, strValues.Length - storedConsumeSearchIndex));
                if (storedConsumeIndex >= 0)
                {
                    positionSearchIndex = storedConsumeIndex;
                    spikesSearchIndex = storedConsumeIndex;
                    outputNeuronSearchIndex = storedConsumeIndex;
                    neuron.GetComponent<NeuronController>().SetStoredConsume(int.Parse(strValues[storedConsumeIndex + 1]));
                }

                //outputNeuron
                outputNeuronIndex = Array.IndexOf(strValues, "outputNeuron", outputNeuronSearchIndex, Mathf.Min(searchArea, strValues.Length - outputNeuronSearchIndex));
                if (outputNeuronIndex >= 0)
                {
                    positionSearchIndex = outputNeuronIndex;
                    spikesSearchIndex = outputNeuronIndex;
                    storedReceivedSearchIndex = outputNeuronIndex;
                    if (bool.Parse(strValues[outputNeuronIndex + 1]))
                    {
                        neuron.GetComponent<NeuronController>().SetToOutputNeuron();
                    }
                }

                //storedReceived
                storedReceivedIndex = Array.IndexOf(strValues, "storedReceived", storedReceivedSearchIndex, Mathf.Min(searchArea, strValues.Length - storedReceivedSearchIndex));
                if (storedReceivedIndex >= 0)
                {
                    positionSearchIndex = storedReceivedIndex;
                    spikesSearchIndex = storedReceivedIndex;
                    bitStringSearchIndex = storedReceivedIndex;
                    neuron.GetComponent<NeuronController>().SetStoredReceived(int.Parse(strValues[storedReceivedIndex + 1]));
                }

                bitStringIndex = Array.IndexOf(strValues, "bitString", bitStringSearchIndex, Mathf.Min(searchArea, strValues.Length - bitStringSearchIndex));
                if (bitStringIndex >= 0)
                {
                    positionSearchIndex = bitStringIndex;
                    spikesSearchIndex = bitStringIndex;
                    if (strValues[bitStringIndex + 1] == "null")
                    {
                        neuron.GetComponent<NeuronController>().SetBitString("");
                    }
                    else
                    {
                        neuron.GetComponent<NeuronController>().SetBitString(strValues[bitStringIndex + 1]);
                    }
                }

                //parse positions
                positionIndex = Array.IndexOf(strValues, "position", positionSearchIndex, Mathf.Min(searchArea, strValues.Length - positionSearchIndex));
                if (positionIndex >= 0)
                {
                    hasPositionData = true;
                    positionSearchIndex = positionIndex + 1;
                    string[] values = strValues[positionIndex + 1].Split(new char[] { ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    neuron.transform.position = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                    spikesSearchIndex = positionIndex;
                }
            }
        }
        catch(FormatException e)
        {
            print("Format Exception");
            autoSavePath = tempAutoSavePath;
            BlankSlate();
            ErrorInvalidFileNotify();
        }
        catch(NullReferenceException e)
        {
            print("Null Exception");
            autoSavePath = tempAutoSavePath;
            BlankSlate();
            ErrorInvalidFileNotify();
        }

        print("PARSING END");

        return hasPositionData;
    }

    public void ErrorInvalidFileNotify()
    {
        invalidFileErrorWindow.SetActive(true);
    }

    public void ErrorInvalidFileClose()
    {
        invalidFileErrorWindow.SetActive(false);
    }
}
