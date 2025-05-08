using System.Collections.Generic;
using UnityEngine;

namespace Samson
{
    public class PlayerModel : MonoBehaviour
    {
        [SerializeField] private List<Renderer> renderers = new();

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

        public void HideFromLocal(bool isHiding)
        {
            foreach (var renderer in renderers)
            {
                renderer.shadowCastingMode = isHiding ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly : UnityEngine.Rendering.ShadowCastingMode.On;
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
