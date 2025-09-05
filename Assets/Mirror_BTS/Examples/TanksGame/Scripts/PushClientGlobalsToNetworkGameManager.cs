using UnityEngine;

public class PushClientGlobalsToNetworkGameManager : MonoBehaviour
{
    [SerializeField] NetworkManagerTanksGame m_networkGameManager;

    void Awake()
    {

        if (null == m_networkGameManager)
        {
            m_networkGameManager = FindAnyObjectByType<NetworkManagerTanksGame>();
        }

        if (null != m_networkGameManager)
        {
            m_networkGameManager.m_selectedTank = ClientGlobals.SelectedTank;
        }
        
    }

 
}
