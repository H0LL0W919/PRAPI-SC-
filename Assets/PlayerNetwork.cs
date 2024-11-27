using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{

    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1);

    private void Update() //just a quick player movement test script - will create a better movement script later!
    {
        Debug.Log(OwnerClientId + "; Random Number: " + randomNumber.Value);

        if (!IsOwner) return; 

        if (Input.GetKeyDown(KeyCode.T))
        {
            randomNumber.Value = Random.Range(0, 11);
        }

        Vector3 moveDirection = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDirection.z = + 1f;
        if (Input.GetKey(KeyCode.S)) moveDirection.z = - 1f;
        if (Input.GetKey(KeyCode.A)) moveDirection.x = - 1f;
        if (Input.GetKey(KeyCode.D)) moveDirection.x = + 1f;

        float moveSpeed = 5f;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
