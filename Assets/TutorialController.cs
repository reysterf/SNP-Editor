using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    public GameObject tutorialMenu;
    public GameObject title;
    public GameObject content;
    public GameObject pageNav;

    int currentPage = 0;

    List<string> tutorialText;
    List<string> tutorialTitle;

    void Awake(){
        if(PlayerPrefs.GetInt("ShowTutorial", 1) == 0){
            tutorialMenu.SetActive(false);
        }
    }


    void Start(){
        tutorialText = new List<string>();
        tutorialTitle = new List<string>();

        tutorialTitle.Add("Creating Neurons");
        tutorialText.Add("Creating a neuron (via \"New Neuron\") will create an empty neuron with no rules and zero spikes. You can add rules and spikes using the \"Edit Neuron\" button.\n\nYou can also create an output neuron via \"New Output\" to receive the output of your system. This can serve as a substitute for the environment in SNP systems.");

        tutorialTitle.Add("Editing Neurons");
        tutorialText.Add("Clicking \"Edit Neuron\" toggles neuron editing mode where you can edit the spikes and rules of the neurons in the system. To add rules and spikes to an empty neuron, click the box below \"Spikes\" or \"Rules\", whichever you want to edit.\n\nClicking \"Edit Neuron\" again will exit neuron editing mode.");

        tutorialTitle.Add("Deleting Neurons");
        tutorialText.Add("Clicking \"Delete Neuron\" toggles neuron deletion mode. During neuron deletion mode, you can delete neurons by clicking them.\n\nTo exit neuron deletion mode, simply click \"Delete Neuron\" again.");

        tutorialTitle.Add("Creating Synapses");
        tutorialText.Add("To create synapses between two neurons, click \"New Synapse\" to toggle synapse creation mode. During synapse creation mode, the first neuron you click will be considered the source neuron and the next neuron you click will be the destination neuron, a synapse will then be created between the two. You can repeat the process with other neurons.\n\nTo exit the synapse creation mode simply click \"New Synapse\" again.");

        tutorialTitle.Add("Deleting Synapses");
        tutorialText.Add("Clicking \"Delete Synapse\" starts the synapse deletion mode. After clicking \"Delete Synapse\", round buttons with an X inside them will appear on all synapses and clicking it will the delete the synapse it's attached to.\n\nIf you\'re finished deleting synapses, simply click \"Delete Synapse\" again to exit the mode.");

        tutorialTitle.Add("Step-by-step Simulation");
        tutorialText.Add("You can simulate a system one step at a time by using the next step(>|) and the previous step(|<) buttons. \n\nThe next step button simulates the system by one timestep while the previous step button reverts the system to its configuration in the previous timestep");

        tutorialTitle.Add("Continuous Simulation");
        tutorialText.Add("You can simulate a system continuously. \n\nYou can start continuous simulation using the play (>) button. During continuous simulation, the program simulates the system until the user clicks the pause (||) button or until the system halts");

        title.GetComponent<Text>().text = tutorialTitle[0];
        content.GetComponent<Text>().text = tutorialText[0];
        pageNav.GetComponent<Text>().text = (1).ToString() + "/" + tutorialTitle.Count.ToString();        
    }

    void Update(){
        title.GetComponent<Text>().text = tutorialTitle[currentPage];
        content.GetComponent<Text>().text = tutorialText[currentPage];
        pageNav.GetComponent<Text>().text = (currentPage+1).ToString() + "/" + tutorialTitle.Count.ToString();

    }

    public void CloseTutorial(){
        tutorialMenu.SetActive(false);
    }

    public void NextPage(){
        if(currentPage < tutorialTitle.Count-1){

            currentPage += 1;
        }
    }

    public void PrevPage(){
        if(currentPage+1 > 1){
            // title.GetComponent<Text>().text = tutorialTitle[currentPage];
            // content.GetComponent<Text>().text = tutorialText[currentPage];
            // pageNav.GetComponent<Text>().text = (currentPage+1).ToString() + "/" + tutorialTitle.Count.ToString();

            currentPage -= 1;
        }
    }

    public void ToggleShowAgain(){
        if(PlayerPrefs.GetInt("ShowTutorial", 1) == 0){
            PlayerPrefs.SetInt("ShowTutorial", 1);
        }
        else{
            PlayerPrefs.SetInt("ShowTutorial", 0);
        }
    }

        //SKELETON
        //Change Page Tutorial Function
        //Close Tutorial
        //Disable Tutorial

        //Add enableTutorial preference
    //SKELETON

}
