using Mirror;
using UnityEngine;

public class BTSTankDestroyed : NetworkBehaviour
{
    public void OnDestroy()
    {
        if (isLocalPlayer)
        {
            EscKey l_escKey = FindAnyObjectByType<EscKey>();

            if (null != l_escKey)
            {
                l_escKey.ShowMenu();
            }
        }
    }
}
