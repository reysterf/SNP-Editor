using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryNode : MonoBehaviour
{
    private HistoryNode father;
    private HistoryNode root;
    private List<HistoryNode> children;
    private List<int> config;

    public HistoryNode GetRoot()
    {
        return root;
    }

    public HistoryNode GetFather()
    {
        return father;
    }

    public List<int> GetConfig()
    {
        return config;
    }

    public HistoryNode(HistoryNode root, List<int> config)
    {
        this.root = root;
        this.config = config;
    }

    public HistoryNode(HistoryNode root, HistoryNode father, List<int> config)
    {
        this.root = root;
        father.children.Add(this);
        this.config = config;
    }
}
