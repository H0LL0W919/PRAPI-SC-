using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class PlayerID : NetworkBehaviour
{
    private NameTag nameTag;
    public PlayerListText playerListText;

    [SerializeField] private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>("Player", NetworkVariableReadPermission.Everyone);
    [SerializeField] private NetworkVariable<FixedString32Bytes> playerTeam = new NetworkVariable<FixedString32Bytes>("", NetworkVariableReadPermission.Everyone);
    [SerializeField] private TMP_Text nameHeader;


    public override void OnNetworkSpawn()
    {
        nameTag = GameObject.Find("Canvas").GetComponent<NameTag>();
        playerListText = GameObject.Find("/Canvas/Hud/PlayerList/Viewport/PlayerListTextBox").GetComponent<PlayerListText>();

        if (!IsOwner) return;

        if (IsServer)
        {
            SetName(nameTag.Name);
            
            // Randomly assign the player to a team
            AssignRandomTeam();
            
            SetHeaderName(playerName.Value);
            ClearListHost();
            Debug.Log("Host");
        }
        else
        {
            SetNameServerRpc(nameTag.Name);
            Debug.Log("Client");

            // Request the server to assign a random team for the client
            AssignRandomTeamServerRpc();
            Debug.Log("Client");
        }
    }

    // Assigns a random team to the player (server-side)
    private void AssignRandomTeam()
    {
        string team = Random.Range(0, 2) == 0 ? "Red" : "Blue";
        playerTeam.Value = new FixedString32Bytes(team);
        Debug.Log($"Assigned {playerName.Value} to team {team}");
    }

    // ServerRpc to assign a random team to the player (called by clients)
    [ServerRpc]
    private void AssignRandomTeamServerRpc()
    {
        AssignRandomTeam();
    }


    //Sets list for host
    private void ClearListHost()
    {
        playerListText.ClearOnScreenList();
        playerListText.UpdateOnScreenList(playerName.Value + " - Team: " + playerTeam.Value);
    }


    //Sets name header
    public void SetHeaderName(FixedString32Bytes name)
    {
        nameHeader.text = name.ToString();
    }


    //Sets name header for all clients
    [ClientRpc]
    public void SetHeaderNameClientRpc(FixedString32Bytes name)
    {
        SetHeaderName(name);
    }



    //Sets name
    private void SetName(FixedString32Bytes name)
    {
        playerName.Value = name;
    }


    //Changes name
    public void ChangeName(int num)
    {
        playerName.Value = playerName.Value + num.ToString();
    }



    //This function sets the player name and calls for the player list and player headers to update for new joining clients
    [ServerRpc]
    private void SetNameServerRpc(FixedString32Bytes name)
    {
        SetName(name);
        playerListText.CheckActivePlayers();
    }


    public FixedString32Bytes PlayerName
    {
        get { return playerName.Value; }
    }

    public FixedString32Bytes PlayerTeam
    {
        get { return playerTeam.Value; }
    }
}