using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class PlayerListText : NetworkBehaviour
{

    [SerializeField] private GameObject playerListBox;
    [SerializeField] private TMP_Text textPrefab;

    [SerializeField] private List<TMP_Text> textList = new List<TMP_Text>();
    [SerializeField] public List<string> playerList = new List<string>();

 
    public override void OnNetworkSpawn()
    {
        //Calls for disconnect client function when a player leaves, doesnt work when host leaves tho :(
        NetworkManager.Singleton.OnClientDisconnectCallback += DisconnectClient;
    }

    //Creates a text prefab for the player's name and adds it to the list 
    // Creates a text prefab for the player's name and team, adds it to the list
    public void UpdateOnScreenList(string playerEntry)
    {
        textPrefab.text = playerEntry;
        TMP_Text nameTag = Instantiate(textPrefab, playerListBox.transform);
        textList.Add(nameTag);
    }

    // Clears the list
    public void ClearOnScreenList()
    {
        foreach (var text in textList)
        {
            Destroy(text.gameObject);
        }
        textList.Clear();
    }


    // Adds player name and team to each client's screen
    [ClientRpc]
    public void UpdateOnScreenListClientRpc(string playerEntry)
    {
        UpdateOnScreenList(playerEntry);
    }

    //Clears each client's list
    [ClientRpc]
    public void ClearOnScreenListClientRpc()
    {
        ClearOnScreenList();
    }



    //Checks for all active clients currently in the server
    public void CheckActivePlayers()
    {
        if (IsServer)
        {
            UpdatePlayerList();

            // Declare hostClient only once
            NetworkManager.Singleton.ConnectedClients.TryGetValue(0, out var hostClient);

            foreach (string playerEntry in playerList)
            {
                UpdateOnScreenListClientRpc(playerEntry);

                // Use the already declared hostClient variable
                if (hostClient == null)
                {
                    UpdateOnScreenList(playerEntry);
                }
            }
        }
    }



    //Updates player list and player header for each client when called
    private void UpdatePlayerList()
    {
        if (!IsServer) return;

        playerList.Clear();
        ClearOnScreenListClientRpc();

        // If the host is not connected, clear the list manually
        NetworkManager.Singleton.ConnectedClients.TryGetValue(0, out var hostClient);

        if (hostClient == null)
        {
            ClearOnScreenList();
        }

        //Loops for each active client to update their list and reset their name header for new joining clients
        foreach (var client in NetworkManager.ConnectedClientsList)
        {
            
            if (client.PlayerObject != null)
            {

                
                PlayerID playerID = client.PlayerObject.GetComponent<PlayerID>();

                if (playerID != null)
                {
                    string playerEntry = $"{playerID.PlayerName.ToString()} - Team: {playerID.PlayerTeam.ToString()}";

                    playerList.Add(playerEntry);

                    // Update the player's header
                    playerID.SetHeaderNameClientRpc(playerEntry);

                    // Use NetworkManager.Singleton.ConnectedClients.TryGetValue(0, out var hostClient) only once
                    if (hostClient == null)
                    {
                        playerID.SetHeaderName(playerEntry);
                    }
                }
            }
        }
    }


    //Gets disconnecting client's name
    private void DisconnectClient(ulong clientID)
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientID, out var client))
        {
            PlayerID playerID = client.PlayerObject.GetComponent<PlayerID>();

            if (playerID != null)
            {
                string playerToRemove = $"{playerID.PlayerName} - Team: {playerID.PlayerTeam}";
                RemoveFromList(playerToRemove);
            }
        }
    }



    //Removes disconnecting client's name from list
    public void RemoveFromList(string playerToRemove)
    {
        if (!IsServer) return;
        

        ClearOnScreenListClientRpc();

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(0, out var hostClient) == false)
        {
            ClearOnScreenList();
        }

        foreach (var playerEntry in new List<string>(playerList))
        {
            if (playerEntry != playerToRemove)
            {
                UpdateOnScreenListClientRpc(playerEntry);

                if (hostClient == null)
                {
                    UpdateOnScreenList(playerEntry);
                }
            }
            else
            {
                playerList.Remove(playerEntry);
            }
        }
    }
}
