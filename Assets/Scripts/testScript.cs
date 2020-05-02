using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testScript : MonoBehaviour
{
    private int neuronCount = 0;
    public GameObject NeuronPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NewNeuron()
    {
        Instantiate(NeuronPrefab, new Vector3(1, 0, 0), Quaternion.identity);
    }
}
