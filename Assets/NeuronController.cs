using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;

public class NeuronController : MonoBehaviour
{
    public GameObject neuron;
    public GameObject EditorController;
    public EditorController ec;

    private bool newSynapseMode = false;
    private bool synapseV1Mode = false;
    private bool synapseV2Mode = false;
    private bool editNeuronMode = false;
    private bool deleteNeuronMode = false;

    private Vector3 screenPoint;
    private Vector3 offset;

    private int storedGive;
    private int storedConsume;
    private int timer = -1;
    public string spikes = "";
    private List<string> rules = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        spikes = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        EditorController = GameObject.Find("EditorController");
        ec = EditorController.GetComponent<EditorController>();
        rules.Add("a+/a -> a;0");
        rules.Add("(aaa)+/aaa -> aaa;0");
        float scale = (float)spikes.Length / ((float)30);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void EditNeuronModeReceiver(bool mode){
        editNeuronMode = mode;
    }

    public string GetSpikes(){
        return spikes;
    }

    public List<string> GetRules(){
        return rules;
    }

    public void SetRules(string rulesNew){
        rules = rulesNew.Split('\n').ToList();;
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
        Vector3 cursorScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint (cursorScreenPoint) + offset;
        transform.position = cursorPosition;
        // ec.Draw();
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
            Debug.Log("UNO");
            var x = new {position = gameObject.transform.position, name = gameObject.name};
            SendMessageUpwards("SynapseCoordinate1", gameObject);

        }
        else if(newSynapseMode && synapseV2Mode){
            Debug.Log("DOS");
            var x = new {position = gameObject.transform.position, name = gameObject.name};
            SendMessageUpwards("SynapseCoordinate2", gameObject);
        }
        else if(editNeuronMode){
            SendMessageUpwards("EditNeuronTarget", gameObject);
        }
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

    public void FireOneStep(GameObject target)
    {
        Debug.Log(timer);
        if (timer == -1)
            CheckRules(target);
        else if (timer == 0)
            Fire(storedConsume, storedGive, target);
        else if (timer > 0)
            timer = timer - 1;
    }

    private void CheckRules(GameObject target)
    {
        int i = 0;
        List<string> matches = new List<string>();
        foreach (string rule in rules)
        {
            int slashInd = rule.IndexOf("/");
            string reg = rule.Substring(0, slashInd);
            if (Regex.IsMatch(spikes, reg))
            {
                matches.Add(rule);
            }
        }
        
        if (matches.Count > 0)
        {
            string chosenRule = matches[Random.Range(0, matches.Count)];
            Debug.Log(chosenRule);
            int slashInd = chosenRule.IndexOf("/");
            string reg = chosenRule.Substring(0, slashInd);
            int arrowInd = chosenRule.IndexOf(">") + 1;
            int semicolInd = chosenRule.IndexOf(";");
            int consume = (chosenRule.Substring(slashInd + 1, arrowInd - slashInd - 4)).Length;
            int give = (chosenRule.Substring(arrowInd + 1, semicolInd - arrowInd - 1)).Length;
            int delay = int.Parse(chosenRule.Substring(semicolInd + 1, chosenRule.Length - semicolInd - 1));
            Debug.Log("c" + consume + "g" + give + "d" + delay);
            timer = delay;
            if (delay == 0)
                Fire(consume, give, target);
            else
                CloseNeuron(consume, give);
        }
    }

    private void CloseNeuron(int consumed, int give)
    {
        storedGive = give;
        storedConsume = consumed;
    }

    private void Fire(int consumed, int give, GameObject target)
    {
        Debug.Log("Fire!");
        int i = 0;
        spikes = spikes.Substring(consumed);
        //for (i = 0; i < connexion.Length; i++)
        //{
        //    Neuron target = connexion[i];
        //    DrawMovingSpike(transform.position, target.transform.position);
        target.GetComponent<NeuronController>().Receive(give);
        //}
        float scale = (float)spikes.Length / ((float)30);
        transform.localScale = new Vector3(scale, scale, scale);
        timer = timer - 1;
    }

    public void Receive(int received)
    {
        string recStr = new string('a', received);
        spikes = string.Concat(spikes, recStr);
        float scale = (float)spikes.Length / ((float)30);
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
