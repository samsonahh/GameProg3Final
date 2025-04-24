using Fusion;
using UnityEngine;

namespace Samson
{
    public class PlayerColor : NetworkBehaviour
    {
        public MeshRenderer MeshRenderer;

        [Networked, OnChangedRender(nameof(OnNetworkedColorChanged))]
        public Color NetworkedColor { get; set; }

        private void Update()
        {
            if(HasStateAuthority && Input.GetKeyDown(KeyCode.E))
            {
                NetworkedColor = new Color(Random.value, Random.value, Random.value);
            }
        }

        private void OnNetworkedColorChanged()
        {
            MeshRenderer.material.color = NetworkedColor;
        }
    }
}
