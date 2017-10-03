using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreaterActorSelector : MonoBehaviour {
    public Text ActorName;
    public ScenarioSelectType scenarioSelectType;

    public void SetActorName(int id,string actorName,int posX,int posY,ScenarioActorPivotType scenarioActorPivotType)
    {
        name = id.ToString();
        string pivot = "";
        switch (scenarioActorPivotType)
        {
            case ScenarioActorPivotType.Right:
                pivot = "R";
                break;
            case ScenarioActorPivotType.UpRight:
                pivot = "UR";
                break;
            case ScenarioActorPivotType.UpLeft:
                pivot = "UL";
                break;
            case ScenarioActorPivotType.Left:
                pivot = "L";
                break;
            case ScenarioActorPivotType.DownLeft:
                pivot = "DL";
                break;
            case ScenarioActorPivotType.DownRight:
                pivot = "DR";
                break;
        }
        ActorName.text = string.Format("{0} ({1},{2}) {3}", actorName, posX, posY, pivot);
    }

    public void SetActionName(int id, ScenarioActionType action)
    {
        name = id.ToString();
        ActorName.text = string.Format("Step:{0},{1}", id, Enum.GetName(typeof(ScenarioActionType), action));
    }

    public void SetActionName(int id, ScenarioType action)
    {
        name = id.ToString();
        ActorName.text = string.Format("{0},{1}", id, Enum.GetName(typeof(ScenarioType), action));
        switch (action)
        {
            case ScenarioType.Openning:
                GetComponent<Image>().color = Color.blue;
                break;
            case ScenarioType.Event:
                GetComponent<Image>().color = Color.green;
                break;
            case ScenarioType.StageClear:
                GetComponent<Image>().color = Color.red;
                break;
        }
    }

    public void RemoveItem()
    {
        switch (scenarioSelectType)
        {
            case ScenarioSelectType.Scenario:
                MapCreatorManager.instance.RemoveScenarioList(name);
                break;
            case ScenarioSelectType.Action:
                MapCreatorManager.instance.RemoveActionList(name);
                break;
            case ScenarioSelectType.Actor:
                MapCreatorManager.instance.RemoveCreateActorList(name);
                break;
        }        
    }

    public void SelectAction()
    {
        switch (scenarioSelectType)
        {
            case ScenarioSelectType.Scenario:
                MapCreatorManager.instance.LoadScenario(name.Split(',')[0]);
                break;
            case ScenarioSelectType.Action:
                MapCreatorManager.instance.LoadAction(name.Split(',')[0].Replace("Step:", ""));
                break;
            case ScenarioSelectType.Actor:
                break;
        }
    }
}
