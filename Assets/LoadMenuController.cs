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

    string loadPath;
    FileInfo[] fileInfo;
    List<string> fileNames = new List<string>();
    List<Dropdown.OptionData> optionDataList = new List<Dropdown.OptionData>();


    // Start is called before the first frame update
    void Start()
    {
        CloseLoadMenu();
        loadPath = Application.dataPath + "/saves/";

        DirectoryInfo di = new DirectoryInfo(loadPath);
        fileInfo = di.GetFiles("*.snapse", SearchOption.TopDirectoryOnly);
        
		foreach (var item in fileInfo)
		{
            fileNames.Add(item.Name);
            print(item.Name);
		}

        loadMenuDropdown.ClearOptions();
        loadMenuDropdown.AddOptions(fileNames);
    }

    // Update is called once per frame
    void Update()
    {
        loadPathText.text = loadPath;
    }

    public void OpenLoadMenu(){
        loadMenu.SetActive(true);
    }

    public void CloseLoadMenu(){
        loadMenu.SetActive(false);
    }

    public void Load(){
        optionDataList = loadMenuDropdown.options;
        Dropdown.OptionData chosenOption = optionDataList[loadMenuDropdown.value];
        print(loadPath + chosenOption.text);
        editorController.LoadFromPath(loadPath + chosenOption.text);
        CloseLoadMenu();
    }
}
