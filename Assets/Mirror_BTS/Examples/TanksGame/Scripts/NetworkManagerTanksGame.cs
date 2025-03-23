using Mirror;
using UnityEngine;
using com.burningthumb.examples;

public class NetworkManagerTanksGame : NetworkManagerDelegates
{
    [Header("AI Settings")]
    [SerializeField]
    private int numberOfAITanks = 2;

    private NetworkStartPosition[] m_networkStartPositions;

    public override void OnStartServer()
    {
        base.OnStartServer();
        //SpawnAITanks(); // Keep this active for testing
    }

    private void SpawnAITanks()
    {
        GameObject aiTankPrefab = null;
        foreach (GameObject prefab in spawnPrefabs)
        {
            if (prefab != null && prefab.GetComponent<BTSAITank>() != null)
            {
                aiTankPrefab = prefab;
                break;
            }
        }

        if (aiTankPrefab == null)
        {
            Debug.LogError("No BTSAITank prefab found in spawnPrefabs list!");
            return;
        }

        m_networkStartPositions = FindObjectsOfType<NetworkStartPosition>();
        if (m_networkStartPositions.Length == 0)
        {
            Debug.LogError("No NetworkStartPositions found in the scene!");
            return;
        }

        int spawnCount = Mathf.Min(numberOfAITanks, m_networkStartPositions.Length);
        Debug.Log($"Spawning {spawnCount} AI tanks. ActivePlayers before: {BTSTank.ActivePlayers.Count}");

        for (int i = 0; i < spawnCount; i++)
        {
            Transform startPosition = GetStartPosition();
            Vector3 spawnPosition = startPosition != null ? startPosition.position : Vector3.zero;
            Quaternion spawnRotation = startPosition != null ? startPosition.rotation : Quaternion.identity;

            GameObject aiTank = Instantiate(aiTankPrefab, spawnPosition, spawnRotation);
            NetworkServer.Spawn(aiTank);
            Debug.Log($"Spawned AI Tank {i + 1} at {spawnPosition}. hasAuthority: {aiTank.GetComponent<NetworkIdentity>().hasAuthority}");
        }

        Debug.Log($"ActivePlayers after spawning: {BTSTank.ActivePlayers.Count}");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log($"Player added. Connection: {conn.connectionId}, ActivePlayers: {BTSTank.ActivePlayers.Count}");
    }
}