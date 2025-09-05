using UnityEngine;
using Mirror;
using System.Collections.Generic;
using com.burningthumb.examples;

public class BTSAITankManager : NetworkBehaviour
{
    private static bool m_shouldSpawn = false;

    [Header("AI Tank Settings")]
    [SerializeField]
    private GameObject aiTankPrefab;
    public int maxAITanks = 3;
    public float respawnDelay = 5f;
    
    [Header("Spawn Settings")]
    [SerializeField]
    private Transform[] spawnPoints;
    [Tooltip("Minimum distance from players to consider a spawn point safe")]
    public float minSpawnDistanceFromPlayer = 5f;

    private List<BTSAITank> activeAITanks = new List<BTSAITank>();
    private List<Transform> availableSpawnPoints = new List<Transform>();
    private float respawnTimer = 0f;

    private bool shouldRespawn = false;

    public static bool ShouldSpawn
    {
        get
        {
           return m_shouldSpawn;
        }

        set
        {
            m_shouldSpawn = value;
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (!ShouldSpawn)
        {
            return;
        }
        
        Debug.Log("BTSAITankManager: Starting server initialization.");

        if (aiTankPrefab == null)
        {
            FindAITankPrefab();
        }
        
        if (aiTankPrefab == null)
        {
            Debug.LogError("BTSAITankManager: No BTSAITank prefab found!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            FindSpawnPoints();
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("BTSAITankManager: No spawn points found!");
            return;
        }

        // Spawn an AI Tank at every spawnPoint
        maxAITanks = spawnPoints.Length;

        Debug.Log($"BTSAITankManager: Found {spawnPoints.Length} spawn points, maxAITanks set to {maxAITanks}.");

        if (maxAITanks > spawnPoints.Length)
        {
            Debug.LogError($"BTSAITankManager: maxAITanks ({maxAITanks}) exceeds spawn points ({spawnPoints.Length}). No tanks will spawn.");
            return;
        }

        availableSpawnPoints.AddRange(spawnPoints);
        InitialSpawn();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        activeAITanks.Clear(); // Clear the list to remove stale references
        availableSpawnPoints.Clear(); // Reset spawn points for next session
        Debug.Log("BTSAITankManager: Server stopped, cleared active AI tanks and spawn points.");
    }

    void Update()
    {
        if (!isServer) return;

        if (!ShouldSpawn)
        {
            return;
        }

        if (shouldRespawn)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0)
            {
                RespawnAllTanks();
                shouldRespawn = false;
            }
        }
    }

    [Server]
    void FindAITankPrefab()
    {
        NetworkManager networkManager = NetworkManager.singleton;

        if (networkManager == null)
        {
            Debug.LogError("BTSAITankManager: NetworkManager not found!");
            return;
        }

        foreach (GameObject prefab in networkManager.spawnPrefabs)
        {
            if (prefab.GetComponent<BTSAITank>() != null)
            {
                aiTankPrefab = prefab;
                Debug.Log("BTSAITankManager: Found BTSAITank prefab: " + prefab.name);
                break;
            }
        }
    }

    [Server]
    void FindSpawnPoints()
    {
        NetworkStartPosition[] networkStartPositions = FindObjectsOfType<NetworkStartPosition>();
        if (networkStartPositions.Length > 0)
        {
            spawnPoints = new Transform[networkStartPositions.Length];
            for (int i = 0; i < networkStartPositions.Length; i++)
            {
                spawnPoints[i] = networkStartPositions[i].transform;
            }
            Debug.Log($"BTSAITankManager: Found {networkStartPositions.Length} NetworkStartPosition objects.");
        }
    }

    [Server]
    void InitialSpawn()
    {
        Debug.Log($"BTSAITankManager: Attempting to spawn {maxAITanks} AI tanks.");
        for (int i = 0; i < maxAITanks; i++)
        {
            SpawnAITank();
        }
        Debug.Log($"BTSAITankManager: Active AI tanks after initial spawn: {activeAITanks.Count}");
    }

    [Server]
    void SpawnAITank()
    {
        if (!ShouldSpawn)
        {
            return;
        }

        if (availableSpawnPoints.Count == 0 || aiTankPrefab == null)
        {
            Debug.LogWarning("BTSAITankManager: No available spawn points or prefab missing!");
            return;
        }

        Transform spawnPoint = GetSafeSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("BTSAITankManager: No safe spawn points available!");
            return;
        }

        availableSpawnPoints.Remove(spawnPoint);

        Vector3 spawnPos = spawnPoint.position;
        Quaternion spawnRot = spawnPoint.rotation;

        GameObject tankObj = Instantiate(aiTankPrefab, spawnPos, spawnRot);
        BTSAITank tank = tankObj.GetComponent<BTSAITank>();
        
        NetworkServer.Spawn(tankObj);
        activeAITanks.Add(tank);
        
        tank.OnTankDestroyed += HandleTankDestroyed;
        Debug.Log($"BTSAITankManager: Spawned AI tank at {spawnPos}");
    }

    [Server]
    Transform GetSafeSpawnPoint()
    {
        List<Transform> safePoints = new List<Transform>();

        foreach (Transform spawnPoint in availableSpawnPoints)
        {
            bool isSafe = true;
            foreach (BTSTank player in BTSTank.ActivePlayers)
            {
                if (player.isLocalPlayer && Vector3.Distance(spawnPoint.position, player.transform.position) < minSpawnDistanceFromPlayer)
                {
                    isSafe = false;
                    break;
                }
            }
            if (isSafe)
            {
                safePoints.Add(spawnPoint);
            }
        }

        if (safePoints.Count > 0)
        {
            return safePoints[Random.Range(0, safePoints.Count)];
        }
        return null;
    }

    [Server]
    void HandleTankDestroyed(BTSAITank tank)
    {
        activeAITanks.Remove(tank);

        Transform spawnPoint = FindSpawnPointForTank(tank);
        if (spawnPoint != null && !availableSpawnPoints.Contains(spawnPoint))
        {
            availableSpawnPoints.Add(spawnPoint);
        }
        
        Debug.Log($"BTSAITankManager: AI tank destroyed. Remaining: {activeAITanks.Count}");

        if (activeAITanks.Count == 0 && ArePlayersAlive())
        {
            shouldRespawn = true;
            respawnTimer = respawnDelay;
        }
    }

    [Server]
    Transform FindSpawnPointForTank(BTSAITank tank)
    {
        Transform closestPoint = null;
        float minDistance = float.MaxValue;

        foreach (Transform point in spawnPoints)
        {
            float distance = Vector3.Distance(point.position, tank.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    [Server]
    bool ArePlayersAlive()
    {
        foreach (BTSTank tank in BTSTank.ActivePlayers)
        {
            if (tank.isLocalPlayer && tank.health > 0)
            {
                return true;
            }
        }
        return false;
    }

    [Server]
    void RespawnAllTanks()
    {
        activeAITanks.Clear();
        availableSpawnPoints.Clear();
        availableSpawnPoints.AddRange(spawnPoints);
        Debug.Log($"BTSAITankManager: Respawning {maxAITanks} AI tanks.");
        for (int i = 0; i < maxAITanks; i++)
        {
            SpawnAITank();
        }
    }

    [Server]
    public bool TryReplaceAITank(out Vector3 spawnPosition, out Quaternion spawnRotation)
    {
        spawnPosition = Vector3.zero;
        spawnRotation = Quaternion.identity;

        if (activeAITanks.Count > 0)
        {
            BTSAITank tankToReplace = activeAITanks[0];
            spawnPosition = tankToReplace.transform.position;
            spawnRotation = tankToReplace.transform.rotation;

            activeAITanks.Remove(tankToReplace);
            Transform spawnPoint = FindSpawnPointForTank(tankToReplace);
            if (spawnPoint != null && !availableSpawnPoints.Contains(spawnPoint))
            {
                availableSpawnPoints.Add(spawnPoint);
            }
            NetworkServer.Destroy(tankToReplace.gameObject);

            Debug.Log("BTSAITankManager: Replaced an AI tank with a player. Remaining AI tanks: " + activeAITanks.Count);
            return true;
        }

        Debug.Log("BTSAITankManager: No AI tanks available to replace.");
        return false;
    }
}