using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Globalization;

public class TeamChat : NetworkBehaviour
{

    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TMP_Text chatDisplay;


    private PlayerID playerID;
    
    void Start()
    {
        playerID = GetComponent<PlayerID>();


        if (playerID == null)
        {
            
            return;
        }

        // Retrieve the chat input field and display from ChatUIManager.
        chatInputField = ChatUIManager.Instance.ChatInputField;
        chatDisplay = ChatUIManager.Instance.ChatDisplay;

        if (playerID != null && !playerID.IsOwner)
        {
            chatInputField.interactable = false;
        }
    }

    
    void Update()
    {
       // If the player presses Enter and the input field has text
        if (IsOwner && Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(chatInputField.text))
        {
            string message = chatInputField.text;
            chatInputField.text = "";

            // Send the message to the server
            SendMessageServerRpc(message, playerID.PlayerTeam.ToString());
        } 
    }

    // Sends a message to the server with the player's team
    [ServerRpc(RequireOwnership = false)]
    private void SendMessageServerRpc(string message, string team, ServerRpcParams rpcParams = default)
    {
        // Broadcast the message to all players on the same team
        BroadcastMessageToTeamClientRpc($"{playerID.PlayerName}: {message}", team);
    }

    // Sends the message to all clients in the specified team
    [ClientRpc]
    private void BroadcastMessageToTeamClientRpc(string message, string team, ClientRpcParams clientRpcParams = default)
    {

        // Check if the current player is on the same team
        if (playerID.PlayerTeam.ToString() == team)
        {

            chatDisplay.text += message + "\n";
        }

    } 
}
