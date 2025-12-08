using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    [Header("Battle State")]
    private string currentBattleId;
    private bool isMyTurn;
    private bool isPlayer1;
    private PlayerInfo myInfo;
    private PlayerInfo opponentInfo;
    private string opponentName;

    [Header("UI References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject matchingPanel;
    [SerializeField] private GameObject battlePanel;
    [SerializeField] private GameObject resultPanel;

    [Header("Menu UI")]
    [SerializeField] private Button findMatchButton;
    [SerializeField] private Button cancelMatchButton;
    [SerializeField] private Text statusText;

    [Header("Battle UI")]
    [SerializeField] private Text battleInfoText;
    [SerializeField] private Text turnText;
    [SerializeField] private Text myNameText;
    [SerializeField] private Text opponentNameText;
    [SerializeField] private Slider myHpBar;
    [SerializeField] private Slider opponentHpBar;
    [SerializeField] private Text myHpText;
    [SerializeField] private Text opponentHpText;
    [SerializeField] private Text actionLogText;

    [Header("Action Buttons")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button defendButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private GameObject actionButtonsPanel;

    [Header("Result UI")]
    [SerializeField] private Text resultText;
    [SerializeField] private Button backToMenuButton;

    void Start()
    {
        // 버튼 이벤트 등록
        findMatchButton.onClick.AddListener(OnFindMatchClicked);
        cancelMatchButton.onClick.AddListener(OnCancelMatchClicked);
        attackButton.onClick.AddListener(() => OnActionClicked("attack"));
        defendButton.onClick.AddListener(() => OnActionClicked("defend"));
        skillButton.onClick.AddListener(() => OnActionClicked("skill"));
        backToMenuButton.onClick.AddListener(OnBackToMenuClicked);

        // 네트워크 이벤트 등록
        BattleNetworkManager.Instance.OnConnected += OnConnected;
        BattleNetworkManager.Instance.OnMatchSearching += OnMatchSearching;
        BattleNetworkManager.Instance.OnMatchCanceled += OnMatchCanceled;
        BattleNetworkManager.Instance.OnBattleStart += OnBattleStart;
        BattleNetworkManager.Instance.OnBattleAction += OnBattleAction;
        BattleNetworkManager.Instance.OnNextTurn += OnNextTurn;
        BattleNetworkManager.Instance.OnBattleEnd += OnBattleEnd;
        BattleNetworkManager.Instance.OnOpponentDisconnected += OnOpponentDisconnected;
        BattleNetworkManager.Instance.OnError += OnError;

        // 초기 패널 설정
        ShowMenuPanel();
    }

    void OnDestroy()
    {
        // 이벤트 해제
        if (BattleNetworkManager.Instance != null)
        {
            BattleNetworkManager.Instance.OnConnected -= OnConnected;
            BattleNetworkManager.Instance.OnMatchSearching -= OnMatchSearching;
            BattleNetworkManager.Instance.OnMatchCanceled -= OnMatchCanceled;
            BattleNetworkManager.Instance.OnBattleStart -= OnBattleStart;
            BattleNetworkManager.Instance.OnBattleAction -= OnBattleAction;
            BattleNetworkManager.Instance.OnNextTurn -= OnNextTurn;
            BattleNetworkManager.Instance.OnBattleEnd -= OnBattleEnd;
            BattleNetworkManager.Instance.OnOpponentDisconnected -= OnOpponentDisconnected;
            BattleNetworkManager.Instance.OnError -= OnError;
        }
    }

    // === 버튼 클릭 이벤트 ===
    void OnFindMatchClicked()
    {
        BattleNetworkManager.Instance.FindMatch();
    }

    void OnCancelMatchClicked()
    {
        BattleNetworkManager.Instance.CancelMatch();
    }

    void OnActionClicked(string action)
    {
        if (!isMyTurn)
        {
            AddActionLog("아직 내 턴이 아닙니다!");
            return;
        }

        // 버튼 비활성화 (중복 클릭 방지)
        SetActionButtonsEnabled(false);

        BattleNetworkManager.Instance.SendBattleAction(action);

        string actionName = action == "attack" ? "공격" : action == "defend" ? "방어" : "필살기";
        AddActionLog($"<color=blue>나: {actionName} 사용!</color>");
    }

    void OnBackToMenuClicked()
    {
        ShowMenuPanel();
    }

    // === 네트워크 이벤트 핸들러 ===
    void OnConnected(string playerId)
    {
        statusText.text = $"연결됨 (ID: {playerId})";
        findMatchButton.interactable = true;
    }

    void OnMatchSearching()
    {
        ShowMatchingPanel();
    }

    void OnMatchCanceled()
    {
        ShowMenuPanel();
        statusText.text = "매칭이 취소되었습니다.";
    }

    void OnBattleStart(BattleMessage msg)
    {
        currentBattleId = msg.battleId;
        isMyTurn = msg.yourTurn;
        isPlayer1 = msg.isPlayer1;
        opponentName = msg.opponent;

        // 플레이어 정보 저장
        if (isPlayer1)
        {
            myInfo = msg.player1;
            opponentInfo = msg.player2;
        }
        else
        {
            myInfo = msg.player2;
            opponentInfo = msg.player1;
        }

        ShowBattlePanel();
        UpdateBattleUI();
        AddActionLog($"<color=green>배틀 시작! vs {opponentName}</color>");
        AddActionLog(isMyTurn ? "내 턴입니다!" : "상대 턴입니다...");
    }

    void OnBattleAction(BattleMessage msg)
    {
        // HP 업데이트
        if (isPlayer1)
        {
            myInfo.hp = msg.player1Hp;
            opponentInfo.hp = msg.player2Hp;
        }
        else
        {
            myInfo.hp = msg.player2Hp;
            opponentInfo.hp = msg.player1Hp;
        }

        UpdateBattleUI();

        // 액션 로그 추가
        string damageText = msg.damage > 0 ? $" ({msg.damage} 데미지)" : "";
        AddActionLog($"<color=red>{msg.actionText}{damageText}</color>");
    }

    void OnNextTurn(BattleMessage msg)
    {
        isMyTurn = msg.yourTurn;
        UpdateBattleUI();

        if (isMyTurn)
        {
            AddActionLog("<color=yellow>내 턴입니다!</color>");
            SetActionButtonsEnabled(true);
        }
        else
        {
            AddActionLog("상대 턴입니다...");
            SetActionButtonsEnabled(false);
        }
    }

    void OnBattleEnd(BattleMessage msg)
    {
        bool didWin = msg.result == "win";

        ShowResultPanel(didWin, msg.message);

        if (didWin)
        {
            AddActionLog($"<color=green>승리! {msg.message}</color>");
        }
        else
        {
            AddActionLog($"<color=gray>패배... {msg.message}</color>");
        }
    }

    void OnOpponentDisconnected(string message)
    {
        ShowResultPanel(true, message);
        AddActionLog($"<color=orange>{message}</color>");
    }

    void OnError(string errorMsg)
    {
        AddActionLog($"<color=red>에러: {errorMsg}</color>");
    }

    // === UI 업데이트 ===
    void UpdateBattleUI()
    {
        // 이름
        myNameText.text = myInfo.name;
        opponentNameText.text = opponentInfo.name;

        // HP 바
        myHpBar.maxValue = myInfo.maxHp;
        myHpBar.value = myInfo.hp;
        opponentHpBar.maxValue = opponentInfo.maxHp;
        opponentHpBar.value = opponentInfo.hp;

        // HP 텍스트
        myHpText.text = $"HP: {myInfo.hp}/{myInfo.maxHp}";
        opponentHpText.text = $"HP: {opponentInfo.hp}/{opponentInfo.maxHp}";

        // 턴 표시
        turnText.text = isMyTurn ? "내 턴" : "상대 턴";
        turnText.color = isMyTurn ? Color.green : Color.gray;

        // 액션 버튼 활성화/비활성화
        SetActionButtonsEnabled(isMyTurn);
    }

    void SetActionButtonsEnabled(bool enabled)
    {
        attackButton.interactable = enabled;
        defendButton.interactable = enabled;
        skillButton.interactable = enabled;
    }

    void AddActionLog(string message)
    {
        if (actionLogText != null)
        {
            actionLogText.text += $"\n{message}";

            // 스크롤을 최신 로그로 (옵션)
            // Canvas.ForceUpdateCanvases();
        }
    }

    // === 패널 전환 ===
    void ShowMenuPanel()
    {
        menuPanel.SetActive(true);
        matchingPanel.SetActive(false);
        battlePanel.SetActive(false);
        resultPanel.SetActive(false);

        statusText.text = "대기 중...";
    }

    void ShowMatchingPanel()
    {
        menuPanel.SetActive(false);
        matchingPanel.SetActive(true);
        battlePanel.SetActive(false);
        resultPanel.SetActive(false);
    }

    void ShowBattlePanel()
    {
        menuPanel.SetActive(false);
        matchingPanel.SetActive(false);
        battlePanel.SetActive(true);
        resultPanel.SetActive(false);

        // 액션 로그 초기화
        actionLogText.text = "=== 배틀 로그 ===";
    }

    void ShowResultPanel(bool didWin, string message)
    {
        resultPanel.SetActive(true);
        battlePanel.SetActive(false);

        resultText.text = didWin ? $"승리!\n{message}" : $"패배...\n{message}";
        resultText.color = didWin ? Color.green : Color.red;
    }
}