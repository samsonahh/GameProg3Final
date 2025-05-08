using AYellowpaper.SerializedCollections;
using Fusion;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Samson
{
    public class PlayerModelManager : NetworkBehaviour
    {
        private Animator animator;
        private RigBuilder rigBuilder;
        private PlayerMovement playerMovement;

        [Header("References")]
        [SerializeField] private Rig rigRoot;

        public enum Model
        {
            YBOT,
            PETER,
            MIKU,
            SANS,
            GURA,
            JHIN,
        }

        [SerializeField, SerializedDictionary("Model Type", "Prefab")]
        private SerializedDictionary<Model, PlayerModel> models = new();

        [Networked, OnChangedRender(nameof(OnModelChanged))] public Model CurrentModel { get; set; } = Model.YBOT;
        [field: SerializeField] public PlayerModel CurrentModelObject { get; private set; }
        public Action<Model> OnLocalModelChanged = delegate { };

        public Transform HeadTransform { get; private set; }

        public bool HideFromLocal { get; private set; }

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rigBuilder = GetComponent<RigBuilder>();
            playerMovement = GetComponent<PlayerMovement>();
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                ChangeModelLocal(Model.YBOT, true);
            }
            else
            {
                // Proxy side needs to build the correct model too
                ChangeModelLocal(CurrentModel, true);
            }
        }

        public void ChangeModel(Model newModel, bool force = false)
        {
            if (!HasStateAuthority) return;
            if (CurrentModel == newModel && !force) return;

            CurrentModel = newModel;
        }

        private void ChangeModelLocal(Model newModel, bool force = false)
        {
            rigRoot.transform.SetParent(transform);

            if (CurrentModelObject != null)
            {
                Destroy(CurrentModelObject.gameObject);
            }

            CurrentModelObject = Instantiate(models[newModel], transform.position, transform.rotation, transform);
            CurrentModelObject.HideFromLocal(HideFromLocal);

            rigRoot.transform.SetParent(CurrentModelObject.transform);

            StartCoroutine(DelayedAnimatorRebind());
        }

        private IEnumerator DelayedAnimatorRebind()
        {
            yield return null;
            // Save Animator state
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.normalizedTime;
            int currentStateHash = stateInfo.shortNameHash;

            // Do Avatar swap
            animator.avatar = CurrentModelObject.ModelAvatar;
            animator.Rebind();

            // Restore animation
            animator.Play(currentStateHash, 0, normalizedTime);

            OnLocalModelChanged.Invoke(CurrentModel);

            rigBuilder.Build();

            playerMovement.OnRagdolledChange();
        }

        private void OnModelChanged()
        {
            ChangeModelLocal(CurrentModel);
        }
        
        public void HideModelFromLocal(bool isHiding)
        {
            HideFromLocal = isHiding;
            CurrentModelObject.HideFromLocal(isHiding);
        }
    }
}
