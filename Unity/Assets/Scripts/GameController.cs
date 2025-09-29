using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameView gameView;
    public PlayerModel playerModel;
    private GameAPI gameAPI;
    // Start is called before the first frame update
    void Start()
    {
        gameAPI = gameObject.AddComponent<GameAPI>();
        gameView.SetRegisterButtonListener(OnRegisterButtonClicked);
        gameView.SetRegisterButtonListener(OnLoginButtonClicked);
    }

    public void OnRegisterButtonClicked()
    {
        string playerName = gameView.playerNameInput.text;
        StartCoroutine(gameAPI.ResisterPlayer(playerName, "1234"));
    }

    public void OnLoginButtonClicked()
    {
        string playerName = gameView.playerNameInput.text;
        StartCoroutine(LoginPlayerCouroutine(playerName, "1234"));
    }

    private IEnumerator LoginPlayerCouroutine(string playerName, string password)
    {
        yield return gameAPI.LoginPlayer(playerName, password, player =>
        {
            playerModel = player;
            UpdateResourcesDisplay();
        });
    }

    private void UpdateResourcesDisplay()
    {
        if (playerModel != null)
        {
            gameView.SetPlayerName(playerModel.playerName);
            gameView.UpdateResources(playerModel.metal, playerModel.crystal, playerModel.deuterium);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
