using UnityEngine;

namespace Samson
{
    public class PlayerModel : MonoBehaviour
    {
        [field: SerializeField] public Transform HeadTransform { get; private set; }
        [field: SerializeField] public Avatar ModelAvatar { get; private set; }

        public void HideFromLocal()
        {
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("HideFromLocal"));
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}
