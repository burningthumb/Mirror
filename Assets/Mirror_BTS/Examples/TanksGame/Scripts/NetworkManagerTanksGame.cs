using UnityEngine;
using Mirror;
using com.burningthumb.examples;

public class NetworkManagerTanksGame : NetworkManager
{
    [Header("Game Settings")]
    public bool autoStartServer = false; // For testing in Editor

    private BTSAITankManager aiTankManager; // Reference to the AI tank manager

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Server started.");

        // Find the BTSAITankManager in the scene
        aiTankManager = FindObjectOfType<BTSAITankManager>();
        if (aiTankManager == null)
        {
            Debug.LogError("NetworkManagerTanksGame: Could not find BTSAITankManager in the scene!");
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Server stopped.");
        BTSTank.ActivePlayers.Clear(); // Reset static player tracking
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

        // Try to replace an AI tank if one exists
        if (aiTankManager != null && aiTankManager.TryReplaceAITank(out spawnPosition, out spawnRotation))
        {
            // Use the AI tank's position and rotation
        }
        else
        {
            // Fallback to a random NetworkStartPosition if no AI tank is available
            NetworkStartPosition[] startPositions = FindObjectsByType<NetworkStartPosition>(FindObjectsSortMode.None);
            Transform startPos = startPositions.Length > 0 ?
                startPositions[Random.Range(0, startPositions.Length)].transform :
                transform; // Fallback to NetworkManager position
            spawnPosition = startPos.position;
            spawnRotation = startPos.rotation;
        }

        // Use the inherited playerPrefab from NetworkManager
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
        base.Update(); // Call base Update to maintain NetworkManager functionality

        if (autoStartServer && !NetworkServer.active && !NetworkClient.active)
        {
            StartServer();
        }
    }
}