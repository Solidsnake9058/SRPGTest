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
    public Text tipText;
    private List<GameSaveRecord> recordList;
    private string infoFormat = "Stage {0}…『{1}』";
    private string dateFormat = "yyyy/MM/dd HH:mm";
    private string saveTip = "セーブする場所を指定して下さい";
    private string loadTip = "ロードするデータを選択して下さい";


    public void SetUI(bool isSaveMode)
    {
        this.isSaveMode = isSaveMode;
        tipText.text = isSaveMode ? saveTip : loadTip;
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
