﻿using AYellowpaper.SerializedCollections;
using Fusion;
using System.Collections;
using UnityEngine;

namespace Samson
{
    public class PlayerModelManager : NetworkBehaviour
    {
        private Animator animator;

        public enum Model
        {
            YBOT,
            PETER,
            MIKU,
        }

        [SerializeField, SerializedDictionary("Model Type", "Prefab")]
        private SerializedDictionary<Model, PlayerModel> models = new();

        [Networked, OnChangedRender(nameof(OnModelChanged))] public Model CurrentModel { get; set; } = Model.YBOT;
        [field: SerializeField] public PlayerModel CurrentModelObject { get; private set; }

        public Transform HeadTransform { get; private set; }

        private void Awake()
        {
            animator = GetComponent<Animator>();
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
            if (CurrentModelObject != null)
            {
                Destroy(CurrentModelObject.gameObject);
            }

            CurrentModelObject = Instantiate(models[newModel], transform.position, transform.rotation, transform);
            if(Object.InputAuthority == Runner.LocalPlayer) CurrentModelObject.HideFromLocal();

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
        }

        private void OnModelChanged()
        {
            ChangeModelLocal(CurrentModel);
        }
    }
}
