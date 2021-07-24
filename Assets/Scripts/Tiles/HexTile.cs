using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Events;

public class HexTile : IGameItem, IPointerClickHandler, IPointerEnterHandler
{

    public HexCoord m_Hex { get; private set; }

    GameObject m_Prefab;
    [SerializeField]
    private Renderer m_Visual;
    public Transform tileLine;
    public bool m_IsHighLight { get; private set; }

    public TileType m_TileType { get; private set; }
    public TileType2D m_TileType2D { get; private set; }

    public float m_MovementCost { get; private set; }
    public bool m_Impassible { get; private set; }
    private int m_SpriteIndex;
    private int m_SpritChestIndex;

    public List<HexTile> m_Neighbors { get; private set; }

    public float m_DefenseRate { get; private set; }

    //[Header("Chest Setting")]
    private bool m_IsHaveChest = false;
    private bool m_IsChestOpened = false;
    private bool m_IsShop;
    private int m_Gold;
    private int m_ItemId;
    private int m_WeaponId;

    private UnityAction<PointerEventData> m_ClickAction;

    public Vector3 HexTilePos()
    {
        return m_Hex.PositionSqr();
    }

    public TileXml CreateTileXml()
    {
        return new TileXml()
        {
            id = (int)m_TileType2D,
            spritIndex = m_SpriteIndex,
            locX = m_Hex.m_Q,
            locY = m_Hex.m_R,
            gold = m_Gold,
            itemId = m_ItemId,
            weaponId = m_WeaponId,
            spritChestIndex = m_SpritChestIndex,
            isShop = m_IsShop
        };
    }

    private void GenerateNeighbors()
    {
        m_Neighbors = new List<HexTile>();
        for (int i = 0; i < 6; i++)
        {
            HexTile tile = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(m_Hex.HexNeighbor(i));
            if (tile != null)
            {
                m_Neighbors.Add(tile);
            }
        }

        //[+1,0][-1,0][0,+1][0,-1][+1,-1][-1,+1]
        //for (int i = 0; i < 6; i++)
        //{
        //    Vector2 n = MapHexIndex(CubeNeighbor(new Vector2(m_Hex.m_Q, m_Hex.m_R), i));

        //    if (n.x < 0 || n.x > m_MapSizeX - 1 - (n.y % 2) || n.y < 0 || n.y > m_MapSizeY - 1)
        //    {
        //        continue;
        //    }
        //    if (SceneManager.GetActiveScene().name == "GameScene")
        //    {
        //        //m_Neighbors.Add(GameMidiator.m_Instance.m_StageMapManager.m_MapHex[(int)n.y][(int)n.x]);
        //    }
        //    else if (SceneManager.GetActiveScene().name == "MapCreatorScene")
        //    {
        //        m_Neighbors.Add(MapCreatorManager.instance.mapHex[(int)n.y][(int)n.x]);
        //    }

        //}
    }

    public void SetHightLight(bool isHighLight, bool isAtk)
    {
        m_IsHighLight = isHighLight;
        m_Visual.material.color = m_IsHighLight ? (isAtk ? GameManager.m_Instance.attackTileColor : GameManager.m_Instance.moveTileColor) : Color.white;
    }


    //public static Vector2 CubeDirection(int direction)
    //{
    //    return m_CubeDirections[direction];
    //}

    //public static Vector2 CubeNeighbor(Vector2 cube, int direction)
    //{
    //    return cube + CubeDirection(direction);
    //}

    //public static Vector2 Scale(Vector2 pos, int k)
    //{
    //    return pos * k;
    //}

    //public static List<Vector2> CubeRing(Vector2 center, int radius)
    //{
    //    List<Vector2> results = new List<Vector2>();
    //    if (radius <= 0)
    //    {
    //        results.Add(center);
    //        return results;
    //    }

    //    var cube = center + Scale(CubeDirection(4), radius);
    //    for (int i = 0; i < 6; i++)
    //    {
    //        for (int j = 0; j < radius; j++)
    //        {
    //            results.Add(cube);
    //            cube = CubeNeighbor(cube, i);
    //        }
    //    }
    //    return results;
    //}

    //public static List<Vector2> CubeSpiral(Vector2 center, int min, int max)
    //{

    //    List<Vector2> results = new List<Vector2>();
    //    if (min > max)
    //    {
    //        return new List<Vector2>();
    //    }
    //    for (int i = min; i <= max; i++)
    //    {
    //        results.AddRange(CubeRing(center, i));
    //    }
    //    return results;
    //}

    //public static List<HexTile> GetCubeRingTile(Vector2 center, int radius, int mapSizeX, int mapSizeY)
    //{
    //    List<HexTile> cubeRingTile = new List<HexTile>();
    //    List<Vector2> cubeRing = CubeRing(center, radius);

    //    for (int i = 0; i < cubeRing.Count; i++)
    //    {
    //        Vector2 n = MapHexIndex(cubeRing[i]);

    //        if (n.x < 0 || n.x > mapSizeX - 1 - (n.y % 2) || n.y < 0 || n.y > mapSizeY - 1)
    //        {
    //            continue;
    //        }
    //        if (SceneManager.GetActiveScene().name == "GameScene")
    //        {
    //            cubeRingTile.Add(GameMidiator.m_Instance.m_StageMapManager.m_MapHex[(int)n.y][(int)n.x]);
    //        }
    //    }
    //    return cubeRingTile;
    //}

    public static List<HexTile> GetCubeRingTile(HexCoord center, int radius)
    {
        List<HexTile> cubeRingTile = new List<HexTile>();
        List<HexCoord> cubeRing = HexCoord.HexRing(center, radius);
        for (int i = 0; i < cubeRing.Count; i++)
        {
            HexTile tile = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(cubeRing[i]);
            if (tile != null)
            {
                cubeRingTile.Add(tile);
            }
        }
        return cubeRingTile;
    }

    //public static List<HexTile> GetCubeSpiralTile(Vector2 center, int min, int max, int mapSizeX, int mapSizeY)
    //{
    //    List<HexTile> cubeRingTile = new List<HexTile>();
    //    List<Vector2> cubeRing = CubeSpiral(center, min, max);

    //    for (int i = 0; i < cubeRing.Count; i++)
    //    {
    //        Vector2 n = MapHexIndex(cubeRing[i]);

    //        if (n.x < 0 || n.x > mapSizeX - 1 - (n.y % 2) || n.y < 0 || n.y > mapSizeY - 1)
    //        {
    //            continue;
    //        }
    //        if (SceneManager.GetActiveScene().name == "GameScene")
    //        {
    //            cubeRingTile.Add(GameMidiator.m_Instance.m_StageMapManager.m_MapHex[(int)n.y][(int)n.x]);
    //        }
    //    }
    //    return cubeRingTile;
    //}


    //public static HexCoord Subtract(HexCoord a, HexCoord b)
    //{
    //    return new HexCoord(a.m_Q - b.m_Q, a.m_R - b.m_R);
    //}

    //public static int Length(HexCoord hex)
    //{
    //    return (int)((Math.Abs(hex.m_Q) + Math.Abs(hex.m_R) + Math.Abs(hex.m_Z)) / 2);
    //}

    //public static int Distance(HexCoord a, HexCoord b)
    //{
    //    return Length(Subtract(a, b));
    //}

    //public static Vector2 MapHexIndex(Vector2 pos)
    //{
    //    return new Vector2(pos.x + (((int)pos.y) >> 1), pos.y);
    //}

    //TODO remove
    public void TileInitializer(TileType tileType, TileType2D tileType2D, int spriteIndex, int spritChestIndex, int q, int r, int gold, int itemId, int weaponId, bool isShop)
    {
        //SetType(tileType);
        SetType2D(tileType2D, spriteIndex);
        m_SpritChestIndex = spritChestIndex;
        m_Hex = new HexCoord(q, r);
        m_Gold = gold;
        m_ItemId = itemId;
        m_WeaponId = weaponId;
        m_IsShop = isShop;
        if (gold > 0 || itemId > 0 || weaponId > 0)
        {
            m_IsHaveChest = true;
            m_IsChestOpened = false;
        }
        transform.localPosition = HexTilePos();
    }

    public void TileInitialize(TileXml tileData, int spritChestIndex, int q, int r)
    {
        //SetType(tileType);
        SetType2D((TileType2D)tileData.id, tileData.spritIndex);
        m_SpritChestIndex = spritChestIndex;
        m_Hex = new HexCoord(q, r);
        m_Gold = tileData.gold;
        m_ItemId = tileData.itemId;
        m_WeaponId = tileData.weaponId;
        m_IsShop = tileData.isShop;
        if (m_Gold > 0 || m_ItemId > 0 || m_WeaponId > 0)
        {
            m_IsHaveChest = true;
            m_IsChestOpened = false;
        }
        transform.localPosition = HexTilePos();
    }

    public override void GameSetting()
    {
        GenerateNeighbors();
        transform.name = "Tile [" + m_Hex.m_Q + "," + m_Hex.m_R + "," + m_Hex.m_Z + "]";
        SetCliclAction(SceneManager.GetActiveScene().name == "GameScene");
    }

    //private void Start()
    //{
    //    GenerateNeighbors();
    //    //m_GridPosition = new Vector2(m_Hex.m_Q, m_Hex.m_R);
    //    transform.name = "Tile [" + m_Hex.m_Q + "," + m_Hex.m_R + "," + m_Hex.m_Z + "]";
    //    //if (SceneManager.GetActiveScene().name == "GameScene")
    //    //{
    //    //    //menuImage = GameObject.Find("MapMenu").GetComponent<Image>();

    //    //}
    //    //TODO temp calling
    //    SetCliclAction(SceneManager.GetActiveScene().name == "GameScene");
    //}

    public bool OpenChest()
    {
        if (m_IsHaveChest && !m_IsChestOpened)
        {
            GameObject container = transform.Find("Visuals").gameObject;
            SetType2D(m_TileType2D, m_SpritChestIndex + container.GetComponentInChildren<SpriteMetarial>().GetSpritesCount());
            GameManager.m_Instance.GetChest(m_Gold, m_ItemId, m_WeaponId);
            m_IsChestOpened = true;
            return true;
        }
        return false;
    }

    public void SetCliclAction(bool isGaming)
    {
        if (isGaming)
        {
            m_ClickAction = ClickEventGame;

        }
        else
        {
            m_ClickAction = ClickEventCreator;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //ClickEvent(eventData);

        m_ClickAction?.Invoke(eventData);
    }

    public void ClickEventGame(PointerEventData eventData)
    {
        if (GameMidiator.m_Instance.m_ScenarionManager.m_IsScenarionRunning)
        {
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!GameMidiator.m_Instance.m_GameUIManager.m_IsUIMenuShowing)
            {
                if (GameManager.m_Instance.moving && !GameManager.m_Instance.attacking)
                {
                    GameManager.m_Instance.MoveCurrentPlayer(this);
                }
                else if (GameManager.m_Instance.attacking)
                {
                    GameManager.m_Instance.AttackWithCurrentPlayer(this);
                }
                else
                {
                    GameManager.m_Instance.ShowPlayerTileMenu(m_Hex, m_IsShop);
                }
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            GameManager.m_Instance.CancelAction();
        }
    }

    public void ClickEventCreator(PointerEventData eventData)
    {
        if (!MapCreatorManager.instance.isScenarioMode)
        {
            if (MapCreatorManager.instance.isGetPos)
            {
                MapCreatorManager.instance.conditionGetPosX.text = m_Hex.m_Q.ToString();
                MapCreatorManager.instance.conditionGetPosY.text = m_Hex.m_R.ToString();
                MapCreatorManager.instance.pointer.position = transform.position;
            }
            else
            {
                switch (MapCreatorManager.instance.settingSelection)
                {
                    case MapSettingType.Tile:
                        //SetType(MapCreatorManager.instance.pallerSelection);
                        SetType2D(MapCreatorManager.instance.pallerSelection2D, MapCreatorManager.instance.spriteIndex);
                        if (MapCreatorManager.instance.pallerSelection2D == TileType2D.Plain && MapCreatorManager.instance.isHaveChest.isOn)
                        {
                            m_IsHaveChest = true;
                            m_Gold = Convert.ToInt32(MapCreatorManager.instance.chestGoldInput.text);
                            m_ItemId = MapCreatorManager.instance.chestItem;
                            m_WeaponId = MapCreatorManager.instance.chestWeapon;
                        }
                        else if (MapCreatorManager.instance.pallerSelection2D == TileType2D.Villa && MapCreatorManager.instance.isShop.isOn)
                        {
                            m_IsShop = true;
                        }
                        else
                        {
                            m_IsHaveChest = false;
                            m_IsShop = false;
                            m_Gold = 0;
                            m_ItemId = -1;
                            m_WeaponId = -1;
                        }
                        break;
                    case MapSettingType.Player:
                        if (eventData.button == PointerEventData.InputButton.Left)
                        {
                            MapCreatorManager.instance.SetPlayer(m_Hex, transform.position);
                        }
                        else if (eventData.button == PointerEventData.InputButton.Right)
                        {
                            MapCreatorManager.instance.SetPlayer(m_Hex, transform.position, true);
                        }
                        break;
                    case MapSettingType.Enemy:
                        if (eventData.button == PointerEventData.InputButton.Left)
                        {
                            MapCreatorManager.instance.SetEnemyPlayer(m_Hex, transform.position);
                        }
                        else if (eventData.button == PointerEventData.InputButton.Right)
                        {
                            MapCreatorManager.instance.SetEnemyPlayer(m_Hex, transform.position, true);
                        }
                        break;
                }
            }
        }
        else
        {

            if (MapCreatorManager.instance.isGetPos)
            {
                MapCreatorManager.instance.getPosX.text = m_Hex.m_Q.ToString();
                MapCreatorManager.instance.getPosY.text = m_Hex.m_R.ToString();
                MapCreatorManager.instance.pointer.position = transform.position;
            }
        }
    }

    public void ClickEvent(PointerEventData eventData)
    {
        m_ClickAction?.Invoke(eventData);

        //if (SceneManager.GetActiveScene().name == "GameScene")
        //{
        //    //if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving)
        //    //{
        //    //    GameManager.instance.moveCurrentPlayer(this);
        //    //}
        //    //else if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking)
        //    //{
        //    //    GameManager.instance.attackWithCurrentPlayer(this);
        //    //}
        //    //else
        //    //{
        //    //    //impassible = impassible ? false : true;
        //    //    //if (impassible)
        //    //    //{
        //    //    //	transform.GetComponentInChildren<Renderer>().material.color = new Color(0.5f, 0.5f, 0.0f);
        //    //    //}
        //    //    //else
        //    //    //{
        //    //    //	transform.GetComponentInChildren<Renderer>().material.color = Color.white;
        //    //    //}
        //    //}
        //    if (eventData.button == PointerEventData.InputButton.Left)
        //    {
        //        if (!GameMidiator.m_Instance.m_GameUIManager.m_IsUIMenuShowing)
        //        {
        //            if (GameManager.m_Instance.moving && !GameManager.m_Instance.attacking)
        //            {
        //                GameManager.m_Instance.MoveCurrentPlayer(this);
        //            }
        //            else if (GameManager.m_Instance.attacking)
        //            {
        //                GameManager.m_Instance.AttackWithCurrentPlayer(this);
        //            }
        //            else
        //            {
        //                MenuType setType = MenuType.TileMenu;
        //                Player player = null;
        //                GameManager.m_Instance.SetPlayerIndex(-1);
        //                if (GameManager.m_Instance.enemyPlayers.Values.Where(x => x.m_Hex == m_Hex).Count() > 0)
        //                {
        //                    player = GameManager.m_Instance.enemyPlayers.Values.Where(x => x.m_Hex == m_Hex).FirstOrDefault();
        //                    setType = MenuType.PlayerDeadMenu;
        //                }
        //                else
        //                {
        //                    if (GameManager.m_Instance.userPlayers.Values.Where(x => x.m_Hex == m_Hex).Count() > 0)
        //                    {
        //                        player = GameManager.m_Instance.userPlayers.Values.Where(x => x.m_Hex == m_Hex).FirstOrDefault();
        //                        bool isShowAction = false;
        //                        if (player.GetIsCanHeal())
        //                        {
        //                            List<HexCoord> hexTiles = player.GetHealRange();
        //                            for (int i = 0; !isShowAction && i < hexTiles.Count; i++)
        //                            {
        //                                Player healPlayer = GameMidiator.m_Instance.m_PlayerManager.GetPlayer(hexTiles[i]);
        //                                if (healPlayer && !healPlayer.m_IsEnemy && healPlayer.m_Hp < healPlayer.m_MaxHP)
        //                                {
        //                                    isShowAction = true;
        //                                }
        //                            }
        //                            //isShowAction = player.GetHealRange().Where(x => GameManager.m_Instance.userPlayers.Values.Where(y => y.m_Hp < y.m_MaxHP && y.m_Hex == x.m_Hex).Count() > 0).Count() > 0;
        //                        }
        //                        isShowAction = isShowAction || player.GetAttackRange().Where(x => GameManager.m_Instance.enemyPlayers.Values.Where(y => y.m_Hp > 0 && y.m_Hex == x.m_Hex).Count() > 0).Count() > 0;
        //                        setType = (player.m_Hp > 0) ? (player.m_IsActable ? (isShowAction ? (m_IsShop ? MenuType.PlayerShopMenu : MenuType.PlayerMenu) : (m_IsShop ? MenuType.PlayerMoveShopMenu : MenuType.PlayerMoveMenu)) : (m_IsShop ? MenuType.PlayerStandShopMenu : MenuType.PlayerStandMenu)) : MenuType.PlayerDeadMenu;
        //                        GameManager.m_Instance.SetPlayerIndex(player.playerIndex);
        //                    }
        //                }

        //                if (player != null)
        //                {
        //                    GameManager.m_Instance.SetPlayerStatusUI(player);
        //                }
        //                GameMidiator.m_Instance.m_GameUIManager.SetMenu(setType, HexTilePos());


        //                //GameManager.m_Instance.ShowMenu();
        //                //Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
        //                ////Debug.Log("(" + pos.x + "," + pos.y + "," + pos.z + ")");
        //                //Vector2 newSize = TileMenu.instance.SetMenu(setType);
        //                //float newX = (pos.x + newSize.x) + 10f >= Screen.width ? (pos.x - newSize.x) - 10f : pos.x;
        //                //float newY = (pos.y - newSize.y) - 10f <= 0 ? (pos.y + newSize.y) + 10f : pos.y;

        //                //menuImage.rectTransform.position = new Vector3(newX, newY, 0);

        //            }
        //        }
        //        //else
        //        //{
        //        //    if (!GameManager.m_Instance.moving && !GameManager.m_Instance.attacking)
        //        //    {
        //        //        GameManager.m_Instance.DisableGroup(GameManager.m_Instance.menu);
        //        //        GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
        //        //    }
        //        //}
        //    }
        //    else if (eventData.button == PointerEventData.InputButton.Right)
        //    {
        //        GameManager.m_Instance.CancelAction();
        //    }
        //}
        //else if (SceneManager.GetActiveScene().name == "MapCreatorScene")
        //{
        //    if (!MapCreatorManager.instance.isScenarioMode)
        //    {
        //        if (MapCreatorManager.instance.isGetPos)
        //        {
        //            MapCreatorManager.instance.conditionGetPosX.text = m_Hex.m_Q.ToString();
        //            MapCreatorManager.instance.conditionGetPosY.text = m_Hex.m_R.ToString();
        //            MapCreatorManager.instance.pointer.position = transform.position;
        //        }
        //        else
        //        {
        //            switch (MapCreatorManager.instance.settingSelection)
        //            {
        //                case MapSettingType.Tile:
        //                    //SetType(MapCreatorManager.instance.pallerSelection);
        //                    SetType2D(MapCreatorManager.instance.pallerSelection2D, MapCreatorManager.instance.spriteIndex);
        //                    if (MapCreatorManager.instance.pallerSelection2D == TileType2D.Plain && MapCreatorManager.instance.isHaveChest.isOn)
        //                    {
        //                        m_IsHaveChest = true;
        //                        m_Gold = Convert.ToInt32(MapCreatorManager.instance.chestGoldInput.text);
        //                        m_ItemId = MapCreatorManager.instance.chestItem;
        //                        m_WeaponId = MapCreatorManager.instance.chestWeapon;
        //                    }
        //                    else if (MapCreatorManager.instance.pallerSelection2D == TileType2D.Villa && MapCreatorManager.instance.isShop.isOn)
        //                    {
        //                        m_IsShop = true;
        //                    }
        //                    else
        //                    {
        //                        m_IsHaveChest = false;
        //                        m_IsShop = false;
        //                        m_Gold = 0;
        //                        m_ItemId = -1;
        //                        m_WeaponId = -1;
        //                    }
        //                    break;
        //                case MapSettingType.Player:
        //                    if (eventData.button == PointerEventData.InputButton.Left)
        //                    {
        //                        MapCreatorManager.instance.SetPlayer(m_Hex, transform.position);
        //                    }
        //                    else if (eventData.button == PointerEventData.InputButton.Right)
        //                    {
        //                        MapCreatorManager.instance.SetPlayer(m_Hex, transform.position, true);
        //                    }
        //                    break;
        //                case MapSettingType.Enemy:
        //                    if (eventData.button == PointerEventData.InputButton.Left)
        //                    {
        //                        MapCreatorManager.instance.SetEnemyPlayer(m_Hex, transform.position);
        //                    }
        //                    else if (eventData.button == PointerEventData.InputButton.Right)
        //                    {
        //                        MapCreatorManager.instance.SetEnemyPlayer(m_Hex, transform.position, true);
        //                    }
        //                    break;
        //            }
        //        }
        //    }
        //    else
        //    {

        //        if (MapCreatorManager.instance.isGetPos)
        //        {
        //            MapCreatorManager.instance.getPosX.text = m_Hex.m_Q.ToString();
        //            MapCreatorManager.instance.getPosY.text = m_Hex.m_R.ToString();
        //            MapCreatorManager.instance.pointer.position = transform.position;
        //        }
        //    }
        //}
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SceneManager.GetActiveScene().name == "MapCreatorScene" && Input.GetMouseButton(0))
        {
            if (!MapCreatorManager.instance.isScenarioMode)
            {
                switch (MapCreatorManager.instance.settingSelection)
                {
                    case MapSettingType.Tile:
                        //SetType(MapCreatorManager.instance.pallerSelection);
                        SetType2D(MapCreatorManager.instance.pallerSelection2D, MapCreatorManager.instance.spriteIndex);
                        if (MapCreatorManager.instance.pallerSelection2D == TileType2D.Plain && MapCreatorManager.instance.isHaveChest.isOn)
                        {
                            m_Gold = Convert.ToInt32(MapCreatorManager.instance.chestGoldInput.text);
                            m_ItemId = MapCreatorManager.instance.chestItem;
                            m_WeaponId = MapCreatorManager.instance.chestWeapon;
                        }
                        break;
                    case MapSettingType.Player:
                        break;
                    case MapSettingType.Enemy:
                        break;
                }

            }
        }
    }

    public void SetType(TileType t)
    {
        m_TileType = t;
        //definition of TileType properties
        switch (t)
        {
            case TileType.Normal:
                m_MovementCost = 1;
                m_Impassible = false;
                m_Prefab = PrefabHolder.instance.tile_Normal_prefab;
                break;
            case TileType.Difficult:
                m_MovementCost = 2;
                m_Impassible = false;
                m_Prefab = PrefabHolder.instance.tile_Difficult_prefab;
                break;
            case TileType.VeryDifficult:
                m_MovementCost = 4;
                m_Impassible = false;
                m_Prefab = PrefabHolder.instance.tile_VeryDifficult_prefab;
                break;
            case TileType.Impassible:
                m_MovementCost = 9999;
                m_Impassible = true;
                m_Prefab = PrefabHolder.instance.tile_Impassible_prefab;
                break;
        }

        GenerateVisuals();
    }

    public void SetType2D(TileType2D t, int index)
    {
        m_TileType2D = t;
        m_SpriteIndex = index;
        m_Impassible = false;
        switch (t)
        {
            case TileType2D.Impassible:
                m_MovementCost = 9999;
                m_Impassible = true;
                m_Prefab = TilePrefabHolder.instance.tile_Impassible_prefab;
                break;
            case TileType2D.Road:
                m_MovementCost = 1;
                m_DefenseRate = 0;
                m_Prefab = TilePrefabHolder.instance.tile_Road_prefab;
                break;
            case TileType2D.Plain:
                m_MovementCost = 1.5f;
                m_DefenseRate = 10;
                m_Prefab = TilePrefabHolder.instance.tile_Plain_prefab;
                break;
            case TileType2D.Wasteland:
                m_MovementCost = 3;
                m_DefenseRate = 30;
                m_Prefab = TilePrefabHolder.instance.tile_Wasteland_prefab;
                break;
            case TileType2D.Villa:
                m_MovementCost = 1;
                m_DefenseRate = 50;
                m_Prefab = TilePrefabHolder.instance.tile_Villa_prefab;
                break;
            case TileType2D.Forest:
                m_MovementCost = 2;
                m_DefenseRate = 40;
                m_Prefab = TilePrefabHolder.instance.tile_Forest_prefab;
                break;
        }
        GenerateVisuals();

    }

    public void GenerateVisuals()
    {
        GameObject container = transform.Find("Visuals").gameObject;
        for (int i = 0; i < container.transform.childCount; i++)
        {
            Destroy(container.transform.GetChild(i).gameObject);
        }
        GameObject newVisual = Instantiate(m_Prefab, transform.position, m_Prefab.transform.rotation, container.transform);
        newVisual.GetComponent<SpriteMetarial>().SetSprite(m_SpriteIndex);
    }

    public void SetShowUI()
    {
        tileLine.localPosition = Vector3.zero;
    }

    public void SetHideUI()
    {
        tileLine.localPosition = new Vector3(0, -100, 0);
    }
}
