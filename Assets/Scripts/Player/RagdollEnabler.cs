using UnityEngine;

namespace Samson
{
    public class RagdollEnabler : MonoBehaviour
    {
        private Animator animator;
        private CharacterController controller;
        private PlayerModelManager modelManager;
        private PlayerMovement playerMovement;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            controller = GetComponent<CharacterController>();
            modelManager = GetComponent<PlayerModelManager>();
            playerMovement = GetComponent<PlayerMovement>();
        }

        private void OnDestroy()
        {
            EnableRagdoll(false);
        }

        private void FixedUpdate()
        {
            if (playerMovement.IsRagdolled)
            {
                transform.position = modelManager.CurrentModelObject.HipBone.position;
            }
        }

        public void EnableRagdoll(bool isEnabled)
        {
            if (isEnabled)
            {
                animator.enabled = false;
                controller.enabled = false;

                modelManager.CurrentModelObject.transform.SetParent(null);

                modelManager.CurrentModelObject.EnableRagdollComponents(true);
            }
            else
            {
                modelManager.CurrentModelObject.transform.SetParent(transform, true);
                modelManager.CurrentModelObject.transform.localPosition = Vector3.zero;
                modelManager.CurrentModelObject.transform.localRotation = Quaternion.identity;

                controller.enabled = true;
                animator.enabled = true;

                modelManager.CurrentModelObject.EnableRagdollComponents(false);
            }
        }

        public void AddForce(Vector3 force, ForceMode forceMode = default)
        {
            Rigidbody[] rigidbodies = modelManager.CurrentModelObject.GetRigidBodies();
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.AddForce(force, forceMode);
            }
        }
    }
}
