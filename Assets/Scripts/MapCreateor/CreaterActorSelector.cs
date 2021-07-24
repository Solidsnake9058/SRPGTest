using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreaterActorSelector : MonoBehaviour {
    public Text ActorName;
    public ScenarionSelectType scenarioSelectType;

    public void SetActorName(int id,string actorName,int posX,int posY,ScenarionActorPivotType scenarioActorPivotType)
    {
        name = id.ToString();
        string pivot = "";
        switch (scenarioActorPivotType)
        {
            case ScenarionActorPivotType.Right:
                pivot = "R";
                break;
            case ScenarionActorPivotType.UpRight:
                pivot = "UR";
                break;
            case ScenarionActorPivotType.UpLeft:
                pivot = "UL";
                break;
            case ScenarionActorPivotType.Left:
                pivot = "L";
                break;
            case ScenarionActorPivotType.DownLeft:
                pivot = "DL";
                break;
            case ScenarionActorPivotType.DownRight:
                pivot = "DR";
                break;
        }
        ActorName.text = string.Format("{0} ({1},{2}) {3}", actorName, posX, posY, pivot);
    }

    public void SetActionName(int id, ScenarionActionType action)
    {
        name = id.ToString();
        ActorName.text = string.Format("Step:{0},{1}", id, Enum.GetName(typeof(ScenarionActionType), action));
    }

    public void SetActionName(int id, ScenarionType action)
    {
        name = id.ToString();
        ActorName.text = string.Format("{0},{1}", id, Enum.GetName(typeof(ScenarionType), action));
        switch (action)
        {
            case ScenarionType.Openning:
                GetComponent<Image>().color = Color.blue;
                break;
            case ScenarionType.Event:
                GetComponent<Image>().color = Color.green;
                break;
            case ScenarionType.StageClear:
                GetComponent<Image>().color = Color.red;
                break;
        }
    }

    public void RemoveItem()
    {
        switch (scenarioSelectType)
        {
            case ScenarionSelectType.Scenario:
                MapCreatorManager.instance.RemoveScenarioList(name);
                break;
            case ScenarionSelectType.Action:
                MapCreatorManager.instance.RemoveActionList(name);
                break;
            case ScenarionSelectType.Actor:
                MapCreatorManager.instance.RemoveCreateActorList(name);
                break;
        }        
    }

    public void SelectAction()
    {
        switch (scenarioSelectType)
        {
            case ScenarionSelectType.Scenario:
                MapCreatorManager.instance.LoadScenario(name.Split(',')[0]);
                break;
            case ScenarionSelectType.Action:
                MapCreatorManager.instance.LoadAction(name.Split(',')[0].Replace("Step:", ""));
                break;
            case ScenarionSelectType.Actor:
                break;
        }
    }
}
