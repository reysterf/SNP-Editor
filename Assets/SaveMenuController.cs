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
        UpdateAutoSavePath();
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

    void UpdateAutoSavePath(){
        editorController.ChangeAutoSavePath(savePath);
    }


}
