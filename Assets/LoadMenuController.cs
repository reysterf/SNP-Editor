using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadMenuController : MonoBehaviour
{
    public EditorController editorController;

    public GameObject loadMenu;
    public Text loadPathText;
    public Dropdown loadMenuDropdown;
    public Text errorNotif;

    string loadPath;
    FileInfo[] fileInfo;
    List<string> fileNames = new List<string>();
    List<Dropdown.OptionData> optionDataList = new List<Dropdown.OptionData>();
    DirectoryInfo di;

    void Awake(){
        // loadMenu.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        errorNotif.gameObject.SetActive(false);

        UpdateOptions();
    }

    // Update is called once per frame
    void Update()
    {
        loadPathText.text = loadPath;
    }

    public void UpdateOptions(){
        loadPath = Application.dataPath + "/saves/";
        di = new DirectoryInfo(loadPath);
        loadMenuDropdown.ClearOptions();
        fileNames.Clear();
        fileInfo = di.GetFiles("*.snapse", SearchOption.TopDirectoryOnly);
        
		foreach (var item in fileInfo)
		{
            fileNames.Add(item.Name);
            // print(item.Name);
		}

        loadMenuDropdown.AddOptions(fileNames);

    }

    void ErrorNotification(string error){
        errorNotif.text = error;
        errorNotif.gameObject.SetActive(true);
        Invoke("DisableErrorNotif", 3.0f);
    }

    void DisableErrorNotif(){
        errorNotif.gameObject.SetActive(false);
    }


    public void OpenLoadMenu(){
        loadMenu.SetActive(true);
        editorController.DisableButtonsAll();
        editorController.DisableNonInteractable();
        editorController.SetDragMode(false);
    }

    public void CloseLoadMenu(){
        loadMenu.SetActive(false);
        editorController.EnableButtonsAll();
        editorController.EnableNonInteractable();
        editorController.SetDragMode(true);
    }

    public void Load(){
        optionDataList = loadMenuDropdown.options;
        Dropdown.OptionData chosenOption = optionDataList[loadMenuDropdown.value];
        if(File.Exists(loadPath + chosenOption.text)){
            editorController.LoadFromPath(loadPath + chosenOption.text);
            CloseLoadMenu();            
        }
        else{
            ErrorNotification("File does not exist");
        }
    }
}
