using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class SaveMenuController : MonoBehaviour
{
    public EditorController editorController;

    public GameObject saveMenu;
    public Text savePathText;
    public InputField savePathInputField;
    public Text saveNotif;

    string savePath;

    void Awake(){
        savePath = Application.dataPath + "/saves/autosave.snapse";
    }

    // Start is called before the first frame update
    void Start()
    {
        CreateSavesFolder();
        saveMenu.SetActive(false);
        saveNotif.gameObject.SetActive(false);
        savePathInputField.text = "autosave";
    }

    // Update is called once per frame
    void Update()
    {
        savePath = Application.dataPath + "/saves/" + savePathInputField.text + ".snapse";
        savePathText.text = savePath;        
    }

    public void OpenSaveMenu(){
        saveMenu.SetActive(true);
    }

    public void CloseSaveMenu(){
        saveMenu.SetActive(false);
    }
    
    public void Save(){
        SaveNotification();
        ChangeAutoSavePath();
    }

    public string GetSavePath(){
        return savePath;
    }

    void SaveNotification(){
        saveNotif.gameObject.SetActive(true);
        Invoke("DisableNotif", 3.0f);
    }

    void DisableNotif(){
        saveNotif.gameObject.SetActive(false);
    }

    void ChangeAutoSavePath(){
        editorController.ChangeAutoSavePath(savePath);
    }

    void CreateSavesFolder(){
        // Specify a name for your top-level folder.
        string folderName = Application.dataPath;

        // To create a string that specifies the path to a subfolder under your
        // top-level folder, add a name for the subfolder to folderName.
        string pathString = System.IO.Path.Combine(folderName, "saves");

        System.IO.Directory.CreateDirectory(pathString);
    }
}
