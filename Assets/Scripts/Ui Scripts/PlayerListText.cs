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
    [SerializeField] public List<FixedString32Bytes> playerList = new List<FixedString32Bytes>();

 
    public override void OnNetworkSpawn()
    {
        //Calls for disconnect client function when a player leaves, doesnt work when host leaves tho :(
        NetworkManager.Singleton.OnClientDisconnectCallback += DisconnectClient;
    }

    //Creates a text prefab for the player's name and adds it to the list 
    public void UpdateOnScreenList(FixedString32Bytes name)
    {
        textPrefab.text = name.ToString();
        TMP_Text nameTag = Instantiate(textPrefab, playerListBox.transform);
        textList.Add(nameTag);
    }


    //Clears the list
    public void ClearOnScreenList()
    {
        for (int i = 0; i < textList.Count; ++i)
        {
            Debug.Log("cleared list");
            Destroy(textList[i].gameObject);
        }
        textList.Clear();
    }



    //Adds player name to each client's screen
    [ClientRpc]
    public void UpdateOnScreenListClientRpc(FixedString32Bytes name)
    {
        UpdateOnScreenList(name);
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
            UpdatePlayerList(playerList);

            for (int i = 0; i < playerList.Count; i++)
            {
                UpdateOnScreenListClientRpc(playerList[i]);

                //This code checks if the host owns a Player Object, if not the server screen will update
                NetworkManager.Singleton.ConnectedClients.TryGetValue(0, out var hostClient);

                if (hostClient == null)
                {
                    UpdateOnScreenList(playerList[i]);
                }

            }
        }
    }





    //Updates player list and player header for each client when called
    private void UpdatePlayerList(List<FixedString32Bytes> list)
    {
        if (!IsServer) return;

        playerList.Clear();
        ClearOnScreenListClientRpc();

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
                    if (playerList.Count > 0)
                    {
                        //This changes the player's name if it already exists in the server
                        for (int i = 0; i < playerList.Count; i++)
                        {
                            if (playerID.PlayerName == playerList[i])
                            {
                                playerID.ChangeName(i);
                            }
                        }
                    }

                    Debug.Log(playerID.PlayerName);

                    list.Add(playerID.PlayerName);

                    playerID.playerListText.playerList = list;
                    playerID.SetHeaderNameClientRpc(playerID.PlayerName);

                    if (hostClient == null)
                    {
                        playerID.SetHeaderName(playerID.PlayerName);
                    }

                }
            }
        }
    }


    //Gets disconnecting client's name
    private void DisconnectClient(ulong clientID)
    {
        if (!IsServer) return;

        var client = NetworkManager.Singleton.ConnectedClients[clientID];
        PlayerID playerID = client.PlayerObject.GetComponent<PlayerID>();

        if (playerID != null)
        {
            FixedString32Bytes playerToRemove = playerID.PlayerName;

            Debug.Log(playerToRemove.ToString());
            RemoveFromList(playerToRemove);
        }
    }



    //Removes disconnecting client's name from list
    public void RemoveFromList(FixedString32Bytes playerToRemove)
    {
        if (!IsServer) return;
        
        NetworkManager.Singleton.ConnectedClients.TryGetValue(0, out var hostClient);

        ClearOnScreenListClientRpc();

        if (hostClient == null)
        {
            ClearOnScreenList();
        }

        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i] != playerToRemove)
            {
                UpdateOnScreenListClientRpc(playerList[i]);

                if (hostClient == null)
                {
                    UpdateOnScreenList(playerList[i]);
                }
            }
            else
            {
                playerList.Remove(playerList[i]);

                i-= 1;
            }
        }
    }
}