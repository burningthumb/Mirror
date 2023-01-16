using Mirror;
using UnityEngine;

namespace com.burningthumb.examples
{
    public class BTSProjectile : NetworkBehaviour
    {
        public float destroyAfter = 2;
        public Rigidbody rigidBody;
        public float force = 1000;
        public Transform m_explosion;

        public override void OnStartServer()
        {
            Invoke(nameof(DestroySelf), destroyAfter);
        }

        // set velocity for server and client. this way we don't have to sync the
        // position, because both the server and the client simulate it.
        void Start()
        {
            rigidBody.AddForce(transform.forward * force);
        }

        [ClientRpc]
        void DestroyOnClient()
        {
            m_explosion.parent = null;
            m_explosion.gameObject.SetActive(true);
            Destroy(gameObject);
        }

        // destroy for everyone on the server
        [Server]
        void DestroySelf()
        {
            NetworkServer.Destroy(gameObject);
        }

        // ServerCallback because we don't want a warning
        // if OnTriggerEnter is called on the client
        [ServerCallback]
        void OnTriggerEnter(Collider co)
        {
            DestroyOnClient();
            //DestroySelf();

        }
    }
}
