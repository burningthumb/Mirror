using UnityEngine;
using Mirror;
using com.burningthumb.examples;

public class NetworkManagerTanksGame : NetworkManager
{
    [Header("Tanks")]
    public int m_selectedTank = -1;
    public GameObject[] playerPrefabs;

    [Header("Game Settings")]
    public bool autoStartServer = false;

    private BTSAITankManager aiTankManager;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Server started.");

        aiTankManager = FindFirstObjectByType<BTSAITankManager>();

        if (aiTankManager == null)
        {
            Debug.LogWarning("NetworkManagerTanksGame: Could not find BTSAITankManager in OnStartServer. Will retry in OnServerAddPlayer.");
        }
    }

    public override void OnValidate()
    {
        Random.InitState(System.DateTime.Now.Millisecond);

        if (playerPrefabs.Length > 0)
        {
            if (-1 == m_selectedTank)
            { 
                playerPrefab = playerPrefabs[Random.Range(0, playerPrefabs.Length)];
            }
        }

        // always >= 0
        maxConnections = Mathf.Max(maxConnections, 0);

        if (playerPrefab != null && !playerPrefab.TryGetComponent<NetworkIdentity>(out _))
        {
            Debug.LogError("NetworkManager - Player Prefab must have a NetworkIdentity.");
            playerPrefab = null;
        }

        // This avoids the mysterious "Replacing existing prefab with assetId ... Old prefab 'Player', New prefab 'Player'" warning.
        if (playerPrefab != null && spawnPrefabs.Contains(playerPrefab))
        {
            Debug.LogWarning("NetworkManager - Player Prefab should not be added to Registered Spawnable Prefabs list...removed it.");
            spawnPrefabs.Remove(playerPrefab);
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Server stopped.");
        BTSTank.ActivePlayers.Clear();
        BTSTank.PlayerTanks.Clear();
        BTSTank.m_playerID.Clear();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("Client started.");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("Client stopped.");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Vector3 spawnPosition;
        Quaternion spawnRotation;

        // Ensure aiTankManager is set
        if (aiTankManager == null)
        {
            aiTankManager = FindFirstObjectByType<BTSAITankManager>();

            if (aiTankManager == null)
            {
                Debug.LogError("NetworkManagerTanksGame: BTSAITankManager not found in scene!");
            }
        }

        // Try to replace an AI tank if one exists
        if (aiTankManager != null && aiTankManager.TryReplaceAITank(out spawnPosition, out spawnRotation))
        {
            Debug.Log("NetworkManagerTanksGame: Successfully replaced an AI tank.");
            // Use the AI tank's position and rotation
        }
        else
        {
            Debug.Log("NetworkManagerTanksGame: No AI tank replaced, using random spawn point.");
            // Fallback to a random NetworkStartPosition
            NetworkStartPosition[] startPositions = FindObjectsByType<NetworkStartPosition>(FindObjectsSortMode.None);
            Transform startPos = startPositions.Length > 0 ?
                startPositions[Random.Range(0, startPositions.Length)].transform :
                transform;
            spawnPosition = startPos.position;
            spawnRotation = startPos.rotation;
        }

        GameObject player = Instantiate(playerPrefab != null ? playerPrefab : spawnPrefabs[0],
            spawnPosition, spawnRotation);

        BTSTank tank = player.GetComponent<BTSTank>();
        if (tank == null)
        {
            Debug.LogError("NetworkManagerTanksGame: Player prefab does not have BTSTank component!");
            Destroy(player);
            return;
        }

        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log($"Player added for connection {conn.connectionId} at position {spawnPosition}");
    }

    public override void Update()
    {
        base.Update();

        if (autoStartServer && !NetworkServer.active && !NetworkClient.active)
        {
            StartServer();
        }
    }
}