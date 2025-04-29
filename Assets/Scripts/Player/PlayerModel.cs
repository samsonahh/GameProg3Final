using UnityEngine;

namespace Samson
{
    public class PlayerModel : MonoBehaviour
    {
        [field: SerializeField] public Transform HeadTransform { get; private set; }
        [field: SerializeField] public Avatar ModelAvatar { get; private set; }

        [field: Header("Bones")]
        [field: SerializeField] public Transform HeadBone { get; private set; }
        [field: SerializeField] public Transform HipBone { get; private set; }
        [field: SerializeField] public Transform RightArmBone { get; private set; }
        [field: SerializeField] public Transform RightForearmBone { get; private set; }
        [field: SerializeField] public Transform RightHandBone { get; private set; }
        [field: SerializeField] public Transform RightElbowHint { get; private set; }

        [field: Header("Ragdoll")]
        private Rigidbody[] Rigidbodies;
        private CharacterJoint[] Joints;
        private Collider[] Colliders;

        private void Awake()
        {
            Rigidbodies = HipBone.GetComponentsInChildren<Rigidbody>();
            Joints = HipBone.GetComponentsInChildren<CharacterJoint>();
            Colliders = HipBone.GetComponentsInChildren<Collider>();
        }

        public void HideFromLocal()
        {
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("HideFromLocal"));
        }

        public void ShowToAll()
        {
            SetLayerRecursively(gameObject, 0);
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public void EnableRagdollComponents(bool isEnabled)
        {
            foreach(CharacterJoint joint in Joints)
            {
                joint.enableCollision = isEnabled;
            }

            foreach(Collider collider in Colliders)
            {
                collider.enabled = isEnabled;
            }

            foreach(Rigidbody rigidbody in Rigidbodies)
            {
                rigidbody.detectCollisions = isEnabled;
                rigidbody.useGravity = isEnabled;
                rigidbody.velocity = Vector3.zero;
            }
        }

        public Rigidbody[] GetRigidBodies()
        {
            return Rigidbodies;
        }
    }
}
