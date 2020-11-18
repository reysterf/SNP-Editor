using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine.UI;

public class NeuronController : MonoBehaviour
{
    public GameObject neuron;
    public GameObject EditorController;
    public EditorController ec;

    public GameObject simulationCanvas;

    public BoxCollider2D collider;
    public GameObject neuronContainer;

    public GameObject rulesUI;
    public GameObject spikesUI;
    public GameObject neuronLabel;
    public GameObject timerUI;

    public Text rulesText;
    public Text spikesText;
    public Text neuronLabelText;
    public Text outputText;
    public Text timerText;

    public GameObject spikeSprite;

    private bool newSynapseMode = false;
    private bool synapseV1Mode = false;
    private bool synapseV2Mode = false;
    private bool editNeuronMode = false;
    private bool deleteNeuronMode = false;

    private bool dragMode = false;

    private bool showRules = true;
    private bool showLabel = true;

    private bool UIChanged = false;

    private List<int> outSynapses = new List<int>();

    private Vector3 screenPoint;
    private Vector3 offset;

    private int storedGive; //int containing the current number of spikes to be given
    private int storedConsume; //int containing the current number of spikes to be consumed
    private int storedReceived; //int containing the current number of spikes to be received
    private int timer = -1; //int for the internal neuron timer
    public string spikes = ""; //spikes are stored as strings in the alphabet {a}
    private List<string> rules = new List<string>(); //list of rules in the neuron

    private bool isClosed = false; //boolean for checking if the neuron is closed

    // Start is called before the first frame update
    void Start()
    {
        if(IsOutputNeuron()){
            timerUI.SetActive(false);

        }
        EditorController = GameObject.Find("EditorController");
        ec = EditorController.GetComponent<EditorController>();
        float scale = (float)spikes.Length / ((float)30);
        transform.localScale = new Vector3(1, 1, 1);

        neuronLabelText.text = gameObject.name;

        showRules = ec.isShowRulesMode();
        showLabel = ec.isShowLabelsMode();

        if (gameObject.tag == "OutputNeuron")
            showRules = false;

        if(showRules){
            ShowRules();
        }
        else if(!showRules){
            HideRules();
        }

        if(showLabel){
            ShowLabel();
        }
        else if(!showLabel){
            HideLabel();
        }

        rulesUI.transform.Find("Rules Container").GetComponent<Button>().interactable = false;
        spikesUI.transform.Find("Spikes Container").GetComponent<Button>().interactable = false;            
        
    }

    // Update is called once per frame
    void Update()
    {
        if(UIChanged){
            //Rules Text
            rulesText.text = string.Join("\n", rules.ToArray());
            //Spikes Text
            spikesText.text = GetSpikesNum().ToString();
            collider.size = gameObject.GetComponent<RectTransform>().sizeDelta;
            if(isClosed || IsOutputNeuron()){
                neuronContainer.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
            }
            if(!isClosed){
                neuronContainer.GetComponent<Image>().color = new Color(1f, 1f, 1f);
            }
            if(timer >= 0){
                timerText.text = timer.ToString();
            }
            if(timer < 0){
                timerText.text = "0";
            }

            UIChanged = false;
        }
    }

    // Handles broadcasted messages for edit neuron mode
    void EditNeuronModeReceiver(bool mode){
        editNeuronMode = mode;
        if(editNeuronMode){
            rulesUI.transform.Find("Rules Container").GetComponent<Button>().interactable = true;
            spikesUI.transform.Find("Spikes Container").GetComponent<Button>().interactable = true;
        }
        else if(!editNeuronMode){
            rulesUI.transform.Find("Rules Container").GetComponent<Button>().interactable = false;
            spikesUI.transform.Find("Spikes Container").GetComponent<Button>().interactable = false;            
        }
    }

    public void EditRules(){        //Called by clicking the rules box of a neuron
        if(editNeuronMode){
            ec.EditNeuron(gameObject, "rules");
        }
    }

    public void EditSpikes(){       //Called by clicking the spikes box of a neuron
        print("EDIT SPIKES");
        if(editNeuronMode){
            ec.EditNeuron(gameObject, "spikes");
        }
    }

    public void ShowRules(){
        if(gameObject.tag != "OutputNeuron")
        {
            showRules = true;
            rulesUI.SetActive(true);
            UIChanged = true;
        }       
    }

    public void HideRules(){
        showRules = false;
        rulesUI.SetActive(false);
        UIChanged = true;
    }

    public void ShowLabel(){
        showLabel = true;
        neuronLabel.SetActive(true); //GetChild(1) gets the transform of the label gameobject
        UIChanged = true;
    }

    public void HideLabel(){
        showLabel = false;
        neuronLabel.SetActive(false); //GetChild(1) gets the transform of the label gameobject
        UIChanged = true;
    }

    public void SetSpikes(int num){
        spikes = RepeatString("a", num);
        UIChanged = true;
    }

    public string GetSpikes(){
        return spikes;
    }

    public int GetSpikesNum(){
        return spikes.Length;
    }

    public List<string> GetRules(){
            return rules;
    }

    public bool SetRules(string rulesNew){
        if (ec.ValidateRules(rulesNew))
            rules = rulesNew.Split('\n').ToList();

        UIChanged = true;
        return (ec.ValidateRules(rulesNew));
    }

    public void SetDelay(int delay){
        timer = delay;
        if(timer > 0){
            isClosed = true;
        }
        else
        {
            isClosed = false;
        }
        UIChanged = true;
    }

    public int GetDelay(){
        return timer;
    }

    public void AddOutSynapse(int neuron){
        outSynapses.Add(neuron);
    }

    public void SetOutSynapses(List<int> outSynapsesList){
        outSynapses = outSynapsesList;
    }

    public void DeleteOutSynapse(int neuron){
        print("DELETED");
        outSynapses.Remove(neuron);
    }

    public List<int> GetOutSynapses(){
        return outSynapses;
    }

    // Handles new synapse mode messages
    void NewSynapseModeReceiver(bool mode)
    {
        newSynapseMode = mode;
    }

    // Handles source neuron messages
    void SynapseV1ModeReceiver(bool mode)
    {
        synapseV1Mode = mode;
    }

    // Handles destination neuron messages
    void SynapseV2ModeReceiver(bool mode)
    {
        synapseV2Mode = mode;
    }

    // Handles delete neuron messages
    void DeleteNeuronModeReceiver(bool mode)
    {
        deleteNeuronMode = mode;
    }

    // Handles neuron dragging
    void OnMouseDrag()
    {
        if(!ec.isDeleteSynapseMode() && ec.GetDragMode() && !ec.GetEditInstanceMode()){
            Vector3 cursorScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 cursorPosition = Camera.main.ScreenToWorldPoint (cursorScreenPoint) + offset;
            transform.position = cursorPosition;
        }
    }

    // Handles clicks on the neuron
    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

        // Handles clicks received depending on editing mode
        if(newSynapseMode && synapseV1Mode){
            Debug.Log("Synapse source selected");
            SendMessageUpwards("SynapseCoordinate1", gameObject);

        }
        else if(newSynapseMode && synapseV2Mode){
            Debug.Log("Synapse destination selected");
            SendMessageUpwards("SynapseCoordinate2", gameObject);
        }
        else if(deleteNeuronMode){
            SendMessageUpwards("DeleteNeuronTarget", gameObject);
        }
    }


    public void SetStoredGive(int newStoredGive){
        storedGive = newStoredGive;
    }

    public int GetStoredGive(){
        return storedGive;
    }

    public void SetStoredConsume(int newStoredConsume){
        storedConsume = newStoredConsume;
    }

    public int GetStoredConsume(){
        return storedConsume;
    }

    public void SetStoredReceived(int newStoredReceived){
        storedReceived = newStoredReceived;
    }

    public int GetStoredReceived(){
        return storedReceived;
    }

    public void SetToOutputNeuron(){
        gameObject.tag = "OutputNeuron";
    }

    public bool IsOutputNeuron(){
        if(gameObject.tag == "OutputNeuron"){
            return true;
        }
        return false;
    }

    public void SetBitString(string newBitString){
        outputText.text = newBitString;
    }

    public string GetBitString(){
        return outputText.text;
    }

    //The start of the firing phase for one neuron
    //Checks for the applicable rules in CheckRules()
    //Returns the applicable rules to NeuronsController
    //which returns it to the EditorController
    public (List<string>, string) StartFire()
    {
        timer = timer - 1;
        (List<string>, string) rules = (new List<string>(),"");
        if (timer <= -1 && outSynapses.Count > 0){
            rules = CheckRules();
        }
        UIChanged = true;
        return rules;
    }

    //Signals the end of the firing phase
    //Fires and opens a neuron if timer == 0
    public void EndFire()
    {
        if(!IsOutputNeuron() && timer == 0)
        {
            Fire(storedConsume, storedGive);
            isClosed = false;
        }
    }

    //Function for checking rules
    private (List<string>, string) CheckRules()
    {
        int i = 0;
        List<string> matches = new List<string>();
        string chosenRule = "";
        //Parses the rule and 
        //checks if the spikes in neuron matches the regex
        foreach (string rule in rules)
        {
            string checkRule = rule.Replace(" ", "");
            int slashInd = checkRule.IndexOf("/");
            string reg = checkRule.Substring(0, slashInd);
            int arrowInd = checkRule.IndexOf(">");
            int consume = (checkRule.Substring(slashInd + 1, arrowInd - slashInd - 2)).Length;
            reg = "^" + reg + "$";
            if (Regex.IsMatch(spikes, reg) && consume <= spikes.Length)
            {
                Debug.Log(checkRule);
                matches.Add(checkRule);
            }
            Debug.Log("matches done");
        }

        //If in Pseudorandom, select random from the applicable rules and
        //processes it
        if (!ec.guidedMode)
        {
            if (matches.Count > 0)
            {
                chosenRule = matches[Random.Range(0, matches.Count)];
                ProcessRule(chosenRule);
            }

            return (matches, chosenRule);
        }
        //If in guided AND more than one, return all applicable rules
        //if there is only one rule, select that rule
        else
        {
            if(matches.Count == 1)
            {
                chosenRule = matches[Random.Range(0, matches.Count)];
                ProcessRule(chosenRule);
            }
            return (matches, chosenRule);
        }           
    }

    //Parse the string and store the data in the string
    //Gets consumed, given, and delay
    public void ProcessRule(string chosenRule)
    {
        int slashInd = chosenRule.IndexOf("/");
        string reg = chosenRule.Substring(0, slashInd);
        int arrowInd = chosenRule.IndexOf(">");
        int semicolInd = chosenRule.IndexOf(";");
        int consume = (chosenRule.Substring(slashInd + 1, arrowInd - slashInd - 2)).Length;
        int give = 0;
        if(chosenRule.Substring(arrowInd + 1, semicolInd - arrowInd - 1) != "0")
            give = (chosenRule.Substring(arrowInd + 1, semicolInd - arrowInd - 1)).Length;
        int delay = 0;
        try
        {
            delay = int.Parse(chosenRule.Substring(semicolInd + 1, chosenRule.Length - semicolInd - 1));
        }
        catch(System.FormatException e) 
        {
            delay = 0;
        }
        timer = delay;
        //If there is no delay, store the rule for firing,
        //else close the neuron and wait for timer == 0
        if (delay == 0)
            StoreRule(consume, give);
        else
            CloseNeuron(consume, give);
    }

    //Stores consume and give
    private void StoreRule(int consume, int give)
    {
        storedConsume = consume;
        storedGive = give;
    }

    //Stores consume and give and closes the neuron,
    //triggering a UI change
    private void CloseNeuron(int consumed, int give)
    {
        isClosed = true;
        storedGive = give;
        storedConsume = consumed;

        UIChanged = true;
    }

    //Applies the rule to the neuron
    private void Fire(int consumed, int give)
    {
        //Spikes are removed via substring
        spikes = spikes.Substring(consumed);
        foreach(int i in outSynapses){
            GameObject target = GameObject.Find("Neurons/" + i.ToString());
            target.GetComponent<NeuronController>().Receive(give);
            if (give > 0)
            {
                if(ec.isEnableAnimationMode()){
                    IEnumerator animate = AnimateFire(transform.position, target.transform.position);
                    StartCoroutine(animate);
                }
            }
                
        }
        UIChanged = true;
    }

    //Called when a neuron is a target of a spike
    public void Receive(int received)
    {
        if(!isClosed)
        {
            string recStr = new string('a', received);
            spikes = string.Concat(spikes, recStr);
            storedReceived += received;
            UIChanged = true;
        }        
    }

    public string UpdateOutput()
    {
        try
        {
            int.Parse(outputText.text);
            outputText.text += storedReceived.ToString();
        }
        catch (System.FormatException e)
        {
            outputText.text = storedReceived.ToString();
        }
        catch (System.OverflowException e)
        {
            outputText.text += storedReceived.ToString();
        }
        storedReceived = 0;
        return outputText.text;
    }

    public string Retract()
    {
        print(outputText.text);
        if(outputText.text.Length > 0)
        {
            outputText.text = outputText.text.Remove(outputText.text.Length-1);
            print(outputText.text);
        }

        return outputText.text;
    }

    public void ClearOutput()
    {
        outputText.text = "";
    }

    public static string RepeatString(string s, int n)
    {
        return string.Concat(Enumerable.Repeat(s, n));
    }

    private IEnumerator AnimateFire(Vector3 startPosition, Vector3 endPosition)
    {
        GameObject sprite = Instantiate(spikeSprite, startPosition, Quaternion.identity);
        sprite.transform.SetParent(GameObject.Find("Neurons and Synapses").transform);
        sprite.transform.SetAsFirstSibling();
        sprite.transform.localScale = Vector3.one;

        float frames = 32f;
        for(int i = 0; i < frames; i++){
            sprite.transform.position = (i/frames * endPosition + (1-i/frames) * startPosition);
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(sprite);
    }
}
