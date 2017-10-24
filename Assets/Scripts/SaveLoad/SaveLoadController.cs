using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;

public class SaveLoadController : MonoBehaviour
{
    public List<Button> recordButtonList;
    public bool isSaveMode;
    public Text saveTip;
    public Text loadTip;
    private List<GameSaveRecord> recordList;
    private string infoFormat = "Stage {0}…『{1}』";
    private string dateFormat = "yyyy/MM/dd HH:mm";

    public void SetUI(bool isSaveMode)
    {
        this.isSaveMode = isSaveMode;
        try
        {
            recordList = JsonConvert.DeserializeObject<List<GameSaveRecord>>(PlayerPrefs.GetString("recordList"));
        }
        catch 
        {
            recordList = new List<GameSaveRecord>();
        }
        for (int i = 0; i < recordButtonList.Count; i++)
        {
            //disable empty when load mode
            recordButtonList[i].interactable = isSaveMode ? recordList.Exists(x => x.id == i) : true;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
