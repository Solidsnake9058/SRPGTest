using System.Collections;
using System.Collections.Generic;

public class GameSaveRecord {

    public int id;
    public int gold;
    public int trunCount;
    public int stageId;
    public string stageName;
    public Dictionary<int, int> playerItems;
    public Dictionary<int, int> playerWeapons;
    public List<PlayerRecord> saveUserPlayerRecords;
    public List<PlayerRecord> saveEnemyPlayerRecords;
    public List<int> defeatedEnemyList;
    public List<int> removeScenaroList;

}
