using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuidedMenuController : MonoBehaviour
{
    public GameObject rulePrefab;
    public EditorController ec;
    private List<string> rules;
    private int neuronNo;

    // Start is called before the first frame update
    void Start()
    {
        ec = GameObject.Find("EditorController").GetComponent<EditorController>();
    }

    private void SetNeuronNo(int neuronNo)
    {
        this.neuronNo = neuronNo;
        gameObject.transform.Find("Neuron No").Find("No Text").gameObject.GetComponent<Text>().text = "N" + neuronNo.ToString();
    }

    //Add rules and instaniate a prefab for each
    private void AddRules(List<string> rules)
    {
        this.rules = rules;
        foreach(string rule in rules)
        {
            Transform ruleContainer = gameObject.transform.Find("Viewport").Find("Content");
            GameObject newRule = Instantiate(rulePrefab, ruleContainer.transform.position, 
                Quaternion.identity, ruleContainer.transform);
            newRule.GetComponent<RulePrefabController>().SetRule(rule);
        }
    }

    //Called when first creating the guidedmenu
    public void SetUpMenu(List<string> rules, int neuronNo)
    {
        SetNeuronNo(neuronNo);
        AddRules(rules);
    }

    //Called when a choice is clicked
    public void ChoiceMade(string rule)
    {
        ec.SetGuidedChoice(this.rules,rule,this.neuronNo);
        Destroy(gameObject);
    }
    
    public void CloseGuided()
    {
        ec.CancelGuided();
    }
}
