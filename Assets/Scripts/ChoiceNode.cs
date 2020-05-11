using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceNode
{
    private ChoiceNode father;
    private List<ChoiceNode> siblings;
    private ChoiceNode root;
    private List<ChoiceNode> children;
    private List<int> config;
    private List<(List<string>, string, int)> nondetRules;
    public int time;

    public ChoiceNode GetRoot()
    {
        return root;
    }

    public ChoiceNode GetFather()
    {
        return father;
    }

    public List<int> GetConfig()
    {
        return config;
    }

    public void PrintNondetRules()
    {
        string contents = "";
        foreach ((List<string> matched, string chosen, int neuronNo) in nondetRules)
        {
            foreach (string rule in matched)
            {
                contents = contents + rule;
            }
            contents = contents + " || ";
            contents = contents + chosen;
        }
        Debug.Log(contents);
    }

    public string GetChosen()
    {
        string chosenFormatted = "t = ";
        chosenFormatted += time.ToString() + "\n";
        foreach((List<string> matched, string chosen, int neuronNo) in nondetRules)
        {
            chosenFormatted += "N";
            chosenFormatted += neuronNo.ToString() + ": ";
            chosenFormatted += chosen + "\n";
        }
        return chosenFormatted;
    }
    
    public void SetFather(ChoiceNode father)
    {
        this.father = father;
    }

    public ChoiceNode(ChoiceNode root, List<int> config)
    {
        this.root = root;
        this.father = null;
        this.config = config;
        this.time = 0;
    }

    public ChoiceNode(ChoiceNode root, List<int> config, List<(List<string>, string, int)> rules, int globalTime)
    {
        this.root = root;
        this.config = config;
        this.nondetRules = rules;
        this.time = globalTime;
    }
}
