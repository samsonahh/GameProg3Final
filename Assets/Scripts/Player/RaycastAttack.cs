using Fusion;
using UnityEngine;

namespace Samson
{
    public class RaycastAttack : NetworkBehaviour
    {
        public float Damage = 10f;

        public PlayerMovement Player;

        private void Update()
        {
            if (!HasStateAuthority) return;

            Ray ray = Player.Camera.ScreenPointToRay(Input.mousePosition);
            ray.origin += Player.Camera.transform.forward;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Debug.DrawRay(ray.origin, ray.direction, Color.red, 1f);

                if(Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit))
                {
                    if(hit.transform.TryGetComponent(out Health health))
                    {
                        health.DealDamageRpc(Damage);
                    }
                }
            }
        }
    }
}
