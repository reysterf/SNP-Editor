using System;
using System.IO;
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
    public Text errorNotif;

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
        editorController.DisableButtonsAll();
        editorController.DisableNonInteractable();
        editorController.SetDragMode(false);
    }

    public void CloseSaveMenu(){
        saveMenu.SetActive(false);
        editorController.EnableButtonsAll();
        editorController.EnableNonInteractable();
        editorController.SetDragMode(true);
    }

    public void Save(){
        if(ValidPath()){
            SaveNotification();
            UpdateAutoSavePath();
        }
        else if(!ValidPath()){
            ErrorNotification("Invalid Path");
        }
    }

    // Checks if path is valid to save to
    bool ValidPath(){
        System.IO.FileInfo fi = null;
        try {
        fi = new System.IO.FileInfo(savePathText.text);
        }
        catch (ArgumentException) { }
        catch (System.IO.PathTooLongException) { }
        catch (NotSupportedException) { }
        if (ReferenceEquals(fi, null)) {
            return false;
        } 
        else {
            char[] invalidFileChars = Path.GetInvalidFileNameChars();
            char[] fileNameChars = savePathInputField.text.ToCharArray();
            foreach (char c in fileNameChars){
                foreach (char ic in invalidFileChars){
                    if(c == ic){
                        return false;
                    }
                }
                if(c == '\\'){
                    return false;
                }
            }
        }

        return true;
    }

    public string GetSavePath(){
        return savePath;
    }

    // Starts save notification coroutine
    void SaveNotification(){
        saveNotif.gameObject.SetActive(true);
        Invoke("DisableNotif", 3.0f);
    }

    void DisableNotif(){
        saveNotif.gameObject.SetActive(false);
    }

    // Starts error notification coroutine
    void ErrorNotification(string error){
        errorNotif.text = error;
        errorNotif.gameObject.SetActive(true);
        Invoke("DisableErrorNotif", 3.0f);
    }

    void DisableErrorNotif(){
        errorNotif.gameObject.SetActive(false);
    }

    void UpdateAutoSavePath(){
        editorController.ChangeAutoSavePath(savePath);
    }


}
