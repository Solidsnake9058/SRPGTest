using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreaterActorSelector : MonoBehaviour {
    public Text ActorName;
    public bool isAction;

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

    public void RemoveItem()
    {
        if (!isAction)
        {
            MapCreatorManager.instance.RemoveCreateActorList(name);
        }
        else
        {
            MapCreatorManager.instance.RemoveActionList(name);
        }
        
    }
}
