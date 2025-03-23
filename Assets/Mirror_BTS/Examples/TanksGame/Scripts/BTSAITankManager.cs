using UnityEngine;
using Mirror;
using System.Collections.Generic;
using com.burningthumb.examples;

public class BTSAITankManager : NetworkBehaviour
{
    [Header("AI Tank Settings")]
    [SerializeField] // Optional manual override
    private GameObject aiTankPrefab; // Can be left unassigned if using auto-detection
    public int maxAITanks = 3;      // Maximum number of AI tanks to spawn
    public float respawnDelay = 5f; // Delay before respawning all tanks
    
    [Header("Spawn Points")]
    [SerializeField] // Optional manual override
    private Transform[] spawnPoints; // Can be left unassigned if using NetworkStartPositions

    private List<BTSAITank> activeAITanks = new List<BTSAITank>();
    private float respawnTimer = 0f;
    private bool shouldRespawn = false;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        // Automatically find the BTSAITank prefab if not assigned
        if (aiTankPrefab == null)
        {
            FindAITankPrefab();
        }
        
        if (aiTankPrefab == null)
        {
            Debug.LogError("BTSAITankManager: No BTSAITank prefab found in Registered Spawnable Prefabs or assigned manually!");
            return;
        }

        // Automatically find spawn points if not assigned
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            FindSpawnPoints();
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("BTSAITankManager: No spawn points found (neither NetworkStartPositions nor manual assignment)!");
            return;
        }

        InitialSpawn();
    }

    void Update()
    {
        if (!isServer) return;

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
                Debug.Log("BTSAITankManager: Found BTSAITank prefab in Registered Spawnable Prefabs: " + prefab.name);
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
            Debug.Log($"BTSAITankManager: Found {networkStartPositions.Length} NetworkStartPosition objects for spawn points.");
        }
    }

    [Server]
    void InitialSpawn()
    {
        for (int i = 0; i < maxAITanks; i++)
        {
            SpawnAITank(i);
        }
    }

    [Server]
    void SpawnAITank(int spawnIndex)
    {
        if (spawnPoints.Length == 0 || aiTankPrefab == null) return;

        Vector3 spawnPos = spawnPoints[spawnIndex % spawnPoints.Length].position;
        Quaternion spawnRot = spawnPoints[spawnIndex % spawnPoints.Length].rotation;

        GameObject tankObj = Instantiate(aiTankPrefab, spawnPos, spawnRot);
        BTSAITank tank = tankObj.GetComponent<BTSAITank>();
        
        NetworkServer.Spawn(tankObj);
        activeAITanks.Add(tank);
        
        tank.OnTankDestroyed += HandleTankDestroyed;
    }

    [Server]
    void HandleTankDestroyed(BTSAITank tank)
    {
        activeAITanks.Remove(tank);
        
        if (activeAITanks.Count == 0 && ArePlayersAlive())
        {
            shouldRespawn = true;
            respawnTimer = respawnDelay;
        }
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
        for (int i = 0; i < maxAITanks; i++)
        {
            SpawnAITank(i);
        }
    }
}