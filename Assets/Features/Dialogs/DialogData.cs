using UnityEngine;

[System.Serializable]
public class DialogData
{
    [Header("Dialog Configuration")]
    public string dialogText;
    public float duration = 2f;
    
    public DialogData()
    {
        dialogText = "";
        duration = 2f;
    }
    
    public DialogData(string text, float time)
    {
        dialogText = text;
        duration = time;
    }
}
