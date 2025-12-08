using System.Collections;
using System.Collections.Generic;


using System;
using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;

public class BattleNetworkManager : MonoBehaviour
{
    private static BattleNetworkManager instance;
    public static BattleNetworkManager Instance => instance;

    private WebSocket webSocket;
    [SerializeField] private string serverUrl = "ws://localhost:3001";

    public string MyPlayerId { get; private set; }
    public PlayerData MyPlayerData { get; private set; }
    public bool IsConnected => webSocket != null && webSocket.State == WebSocketState.Open;

    // 이벤트
    public event Action<string> OnConnected;
    public event Action OnDisconnected;
    public event Action OnMatchSearching;
    public event Action OnMatchCanceled;
    public event Action<BattleMessage> OnBattleStart;
    public event Action<BattleMessage> OnBattleAction;
    public event Action<BattleMessage> OnNextTurn;
    public event Action<BattleMessage> OnBattleEnd;
    public event Action<string> OnOpponentDisconnected;
    public event Action<string> OnError;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ConnectToServer();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (webSocket != null)
        {
            webSocket.DispatchMessageQueue();
        }
#endif
    }

    private async void ConnectToServer()
    {
        if (IsConnected)
        {
            Debug.Log("이미 서버에 연결되어 있습니다.");
            return;
        }

        Debug.Log($"서버 연결 시도: {serverUrl}");
        webSocket = new WebSocket(serverUrl);

        webSocket.OnOpen += () =>
        {
            Debug.Log("서버 연결 성공!");
        };

        webSocket.OnError += (e) =>
        {
            Debug.LogError($"WebSocket 에러: {e}");
            OnError?.Invoke(e);
        };

        webSocket.OnClose += (e) =>
        {
            Debug.Log("서버 연결 종료");
            OnDisconnected?.Invoke();
        };

        webSocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            HandleMessage(message);
        };

        await webSocket.Connect();
    }

    private void HandleMessage(string json)
    {
        try
        {
            BattleMessage message = JsonConvert.DeserializeObject<BattleMessage>(json);
            Debug.Log($"수신: {message.type}");

            switch (message.type)
            {
                case "connected":
                    MyPlayerId = message.playerId;
                    MyPlayerData = message.playerData;
                    Debug.Log($"내 플레이어 ID: {MyPlayerId}");
                    OnConnected?.Invoke(MyPlayerId);
                    break;

                case "matchSearching":
                    Debug.Log("매칭 검색 중...");
                    OnMatchSearching?.Invoke();
                    break;

                case "matchCanceled":
                    Debug.Log("매칭 취소됨");
                    OnMatchCanceled?.Invoke();
                    break;

                case "battleStart":
                    Debug.Log($"배틀 시작! vs {message.opponent}");
                    OnBattleStart?.Invoke(message);
                    break;

                case "battleAction":
                    Debug.Log($"배틀 액션: {message.actionText} ({message.damage} 데미지)");
                    OnBattleAction?.Invoke(message);
                    break;

                case "nextTurn":
                    Debug.Log($"턴 {message.turnCount}: {(message.yourTurn ? "내 턴" : "상대 턴")}");
                    OnNextTurn?.Invoke(message);
                    break;

                case "battleEnd":
                    Debug.Log($"배틀 종료: {message.winner} 승리!");
                    OnBattleEnd?.Invoke(message);
                    break;

                case "opponentDisconnected":
                    Debug.Log("상대방 연결 종료");
                    OnOpponentDisconnected?.Invoke(message.message);
                    break;

                case "error":
                    Debug.LogWarning($"에러: {message.message}");
                    OnError?.Invoke(message.message);
                    break;

                default:
                    Debug.LogWarning($"알 수 없는 메시지 타입: {message.type}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"메시지 파싱 에러: {e.Message}\n{json}");
        }
    }

    // 매칭 찾기
    public async void FindMatch()
    {
        if (!IsConnected)
        {
            Debug.LogWarning("서버에 연결되지 않았습니다.");
            return;
        }

        FindMatchMessage msg = new FindMatchMessage();
        await webSocket.SendText(JsonConvert.SerializeObject(msg));
        Debug.Log("매칭 요청 전송");
    }

    // 매칭 취소
    public async void CancelMatch()
    {
        if (!IsConnected)
        {
            Debug.LogWarning("서버에 연결되지 않았습니다.");
            return;
        }

        CancelMatchMessage msg = new CancelMatchMessage();
        await webSocket.SendText(JsonConvert.SerializeObject(msg));
        Debug.Log("매칭 취소 요청 전송");
    }

    // 배틀 액션 전송
    public async void SendBattleAction(string action)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("서버에 연결되지 않았습니다.");
            return;
        }

        ActionMessage msg = new ActionMessage { action = action };
        await webSocket.SendText(JsonConvert.SerializeObject(msg));
        Debug.Log($"배틀 액션 전송: {action}");
    }

    private async void OnApplicationQuit()
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            await webSocket.Close();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}