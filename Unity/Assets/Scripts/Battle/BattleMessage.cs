using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleMessage
{
    public string type;
    public string playerId;
    public string battleId;
    public string message;
    public string action;
    public PlayerData playerData;
    public PlayerInfo player1;
    public PlayerInfo player2;
    public string opponent;
    public bool yourTurn;
    public bool isPlayer1;
    public string attacker;
    public int damage;
    public string actionText;
    public int player1Hp;
    public int player2Hp;
    public string currentTurn;
    public int turnCount;
    public string winner;
    public string winnerId;
    public string loser;
    public string loserId;
    public string result;
}

[Serializable]
public class PlayerData
{
    public string id;
    public string name;
    public int hp;
    public int maxHp;
    public bool inBattle;
    public string battleId;
}

[Serializable]
public class PlayerInfo
{
    public string id;
    public string name;
    public int hp;
    public int maxHp;
}

[Serializable]
public class ActionMessage
{
    public string type = "battleAction";
    public string action; // "attack", "defend", "skill"
}

[Serializable]
public class FindMatchMessage
{
    public string type = "findMatch";
}

[Serializable]
public class CancelMatchMessage
{
    public string type = "cancelMatch";
}
