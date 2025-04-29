using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace Samson
{
    public class PlayerArmConstraint : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerModelManager playerModelManager;
        [SerializeField] private PlayerObjectDragger playerObjectDragger;
        [SerializeField] private TwoBoneIKConstraint twoBoneIKConstraint;
        [SerializeField] private Transform dragTargetTransform;

        private Transform dragTargetCopy;

        private void Awake()
        {
            playerModelManager.OnLocalModelChanged += PlayerModelManager_OnLocalModelChanged;
        }

        private void OnDestroy()
        {
            playerModelManager.OnLocalModelChanged -= PlayerModelManager_OnLocalModelChanged;
        }

        private void Update()
        {
            if (playerObjectDragger.IsDragging)
            {
                twoBoneIKConstraint.weight = 1;

                if(dragTargetCopy != null) dragTargetCopy.position = dragTargetTransform.position;
            }
            else
            {
                twoBoneIKConstraint.weight = 0;
            }
        }

        private void PlayerModelManager_OnLocalModelChanged(PlayerModelManager.Model newModel)
        {
            PlayerModel currentModelObject = playerModelManager.CurrentModelObject;

            if (dragTargetCopy != null) Destroy(dragTargetCopy.gameObject);
            dragTargetCopy = new GameObject("Drag Target Copy").transform;
            dragTargetCopy.SetParent(transform);
            dragTargetCopy.position = dragTargetTransform.position;

            var data = twoBoneIKConstraint.data;

            data.root = currentModelObject.RightArmBone;
            data.mid = currentModelObject.RightForearmBone;
            data.tip = currentModelObject.RightHandBone;

            data.target = dragTargetCopy;
            data.hint = currentModelObject.RightElbowHint;

            twoBoneIKConstraint.data = data;
        }
    }
}
