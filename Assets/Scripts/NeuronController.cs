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

    private int storedGive;
    private int storedConsume;
    private int storedReceived;
    private int timer = -1;
    public string spikes = "";
    private List<string> rules = new List<string>();

    private bool isClosed = false;

    // Start is called before the first frame update
    void Start()
    {
        if(IsOutputNeuron()){
            timerUI.SetActive(false);

        }
        // spikes = "";
        EditorController = GameObject.Find("EditorController");
        ec = EditorController.GetComponent<EditorController>();
        // rules.Add("a+/a -> a;0");
        // rules.Add("(aaa)+/aaa -> aaa;0");
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
                // timerUI.SetActive(true);
                timerText.text = timer.ToString();
            }
            if(timer < 0){
                timerText.text = "0";
                // timerUI.SetActive(false);
            }

            UIChanged = false;
        }

        // if(showRules){
        //     ShowRules();
        // }
        // else if(!showRules){
        //     HideRules();
        // }

        // if(showLabel){
        //     ShowLabel();
        // }
        // else if(!showLabel){
        //     HideLabel();
        // }

    }

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
        // if(ec.isFreeMode() && editNeuronMode){
        if(editNeuronMode){
            ec.EditNeuron(gameObject, "rules");
        }
    }

    public void EditSpikes(){       //Called by clicking the spikes box of a neuron
        print("EDIT SPIKES");
        // if(ec.isFreeMode() && editNeuronMode){
        if(editNeuronMode){
            ec.EditNeuron(gameObject, "spikes");
        }
    }

    public void ShowRules(){
        if(gameObject.tag != "OutputNeuron")
        {
            showRules = true;
            // gameObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
            rulesUI.SetActive(true);
            UIChanged = true;
        }       
    }

    public void HideRules(){
        showRules = false;
        // gameObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
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
        outSynapses.Remove(neuron);
    }

    public List<int> GetOutSynapses(){
        return outSynapses;
    }

    void NewSynapseModeReceiver(bool mode)
    {
        newSynapseMode = mode;
    }

    void SynapseV1ModeReceiver(bool mode)
    {
        synapseV1Mode = mode;
    }

    void SynapseV2ModeReceiver(bool mode)
    {
        synapseV2Mode = mode;
    }

    void DeleteNeuronModeReceiver(bool mode)
    {
        deleteNeuronMode = mode;
    }

    void OnMouseDrag()
    {
        if(!ec.isDeleteSynapseMode()){
            Vector3 cursorScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 cursorPosition = Camera.main.ScreenToWorldPoint (cursorScreenPoint) + offset;
            transform.position = cursorPosition;
        }
        // ec.Draw();
    }

    void OnMouseUp(){
    }


    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

        // EditorController ec = EditorController.GetComponent<EditorController>();
        // Debug.Log("OOF");
        // ec.nemu();

        // Handles clicks received depending on editing mode
        if(newSynapseMode && synapseV1Mode){
            Debug.Log("Synapse source selected");
            // var x = new {position = gameObject.transform.position, name = gameObject.name};
            SendMessageUpwards("SynapseCoordinate1", gameObject);

        }
        else if(newSynapseMode && synapseV2Mode){
            Debug.Log("Synapse destination selected");
            // var x = new {position = gameObject.transform.position, name = gameObject.name};
            SendMessageUpwards("SynapseCoordinate2", gameObject);
        }
        // else if(editNeuronMode){
        //     SendMessageUpwards("EditNeuronTarget", gameObject);
        // }
        else if(deleteNeuronMode){
            SendMessageUpwards("DeleteNeuronTarget", gameObject);
        }
    }

    void OnMouseOver()
    {
        //If your mouse hovers over the GameObject with the script attached, output this message
        // Debug.Log("Mouse is over GameObject." + neuron.name);
        // Debug.Log();
    }

    void OnMouseExit()
    {
        //The mouse is no longer hovering over the GameObject so output this message each frame
        // Debug.Log("Mouse is no longer on GameObject.");
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

    public (List<string>, string) StartFire()
    {
        timer = timer - 1;
        (List<string>, string) rules = (new List<string>(),"");
        if (timer <= -1){
            rules = CheckRules();
        }
        UIChanged = true;
        return rules;
    }

    public void EndFire()
    {
        if(!IsOutputNeuron() && timer == 0)
        {
            Fire(storedConsume, storedGive);
            isClosed = false;
        }
    }

    private (List<string>, string) CheckRules()
    {
        int i = 0;
        List<string> matches = new List<string>();
        string chosenRule = "";
        foreach (string rule in rules)
        {
            int slashInd = rule.IndexOf("/");
            string reg = rule.Substring(0, slashInd);
            reg = "^" + reg + "$";
            if (Regex.IsMatch(spikes, reg))
            {
                matches.Add(rule);
            }
        }

        Debug.Log(ec.guidedMode);
        if (!ec.guidedMode)
        {
            if (matches.Count > 0)
            {
                chosenRule = matches[Random.Range(0, matches.Count)];
                ProcessRule(chosenRule);
            }

            return (matches, chosenRule);
        }
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

    public void ProcessRule(string chosenRule)
    {
        chosenRule = chosenRule.Replace(" ", "");
        int slashInd = chosenRule.IndexOf("/");
        string reg = chosenRule.Substring(0, slashInd);
        int arrowInd = chosenRule.IndexOf(">");
        int semicolInd = chosenRule.IndexOf(";");
        int consume = (chosenRule.Substring(slashInd + 1, arrowInd - slashInd - 2)).Length;
        int give = (chosenRule.Substring(arrowInd + 1, semicolInd - arrowInd - 1)).Length;
        int delay = int.Parse(chosenRule.Substring(semicolInd + 1, chosenRule.Length - semicolInd - 1));
        timer = delay;
        if (delay == 0)
            StoreRule(consume, give);
        else
            CloseNeuron(consume, give);
    }

    //private string GuidedNondetStart(List<string> matches)
    //{
    //    GameObject newGuideMenu = Instantiate(ec.guidedMenu, new Vector3(transform.position.x, transform.position.y, transform.position.z),
    //          Quaternion.identity, GameObject.Find("Overlay").transform);
    //    foreach(string rule in matches)
    //    {
    //        GameObject newRulePrefab = Instantiate(ec.guidedMenu, new Vector3(transform.position.x, transform.position.y, transform.position.z),
    //          Quaternion.identity, GameObject.Find("Overlay").transform);
    //    }
    //}

    //private (int, int, List<GameObject>) parseRule()
    //{

    //}

    private void StoreRule(int consume, int give)
    {
        storedConsume = consume;
        storedGive = give;
    }

    private void CloseNeuron(int consumed, int give)
    {
        isClosed = true;
        storedGive = give;
        storedConsume = consumed;

        UIChanged = true;
    }

    private void Fire(int consumed, int give)
    {
        spikes = spikes.Substring(consumed);
        //for (i = 0; i < connexion.Length; i++)
        //{
        //    Neuron target = connexion[i];
        //    DrawMovingSpike(transform.position, target.transform.position);
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

        //}
        //float scale = (float)spikes.Length / ((float)30);
        // transform.localScale = new Vector3(scale, scale, scale);
        UIChanged = true;
    }

    public void Receive(int received)
    {
        if(!isClosed)
        {
            string recStr = new string('a', received);
            spikes = string.Concat(spikes, recStr);
            storedReceived += received;
            //float scale = (float)spikes.Length / ((float)30);
            // transform.localScale = new Vector3(scale, scale, scale);
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
        // print("Animate Firing");
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
