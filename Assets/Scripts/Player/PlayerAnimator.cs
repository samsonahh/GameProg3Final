using Fusion;
using System;
using System.Collections;
using UnityEngine;

namespace Samson
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerAnimator : NetworkBehaviour
    {
        private PlayerMovement playerMovement;
        private Animator animator;

        private readonly int MOVEMENT_STATE = Animator.StringToHash("Movement");
        private readonly int JUMP_STATE = Animator.StringToHash("Jump");
        private readonly int FALL_STATE = Animator.StringToHash("Fall");
        private readonly int DANCE_STATE = Animator.StringToHash("Dance");

        private const string MOVESPEED_PARAMETER = "MoveSpeed";
        private const string MOVEX_PARAMETER = "MoveX";
        private const string MOVEZ_PARAMETER = "MoveZ";

        [Header("Config")]
        [SerializeField] private float transitionDuration = 0.1f;
        [SerializeField] private float animatorMoveSpeedLerpMultiplier = 10f;
        [SerializeField] private float jumpToFallDuration = 0.1f;
        private float animatorMoveSpeed;
        private Coroutine jumpToFallCoroutine;

        private int currentState;
        [Networked, OnChangedRender(nameof(OnAnimationStateChanged))]
        private int networkedAnimationState { get; set; }

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            animator = GetComponent<Animator>();

            playerMovement.OnGrounded += PlayerMovement_OnGrounded;
            playerMovement.OnJump += PlayerMovement_OnJump;
            playerMovement.OnFalling += PlayerMovement_OnFalling;
            playerMovement.OnDance += PlayerMovement_OnDance;
        }

        private void OnDestroy()
        {
            playerMovement.OnGrounded -= PlayerMovement_OnGrounded;
            playerMovement.OnJump -= PlayerMovement_OnJump;
            playerMovement.OnFalling -= PlayerMovement_OnFalling;
            playerMovement.OnDance -= PlayerMovement_OnDance;
        }

        private void Update()
        {
            animatorMoveSpeed = Mathf.Lerp(animatorMoveSpeed, playerMovement.NetworkedSpeedModifier, animatorMoveSpeedLerpMultiplier * Time.deltaTime);
            animator.SetFloat(MOVESPEED_PARAMETER, animatorMoveSpeed);

            animator.SetFloat(MOVEX_PARAMETER, playerMovement.NetworkedMoveInput.x);
            animator.SetFloat(MOVEZ_PARAMETER, playerMovement.NetworkedMoveInput.y);
        }

        private void OnAnimationStateChanged()
        {
            PlayAnimationState(networkedAnimationState);
            currentState = networkedAnimationState;
        }

        private void PlayerMovement_OnGrounded()
        {
            if (playerMovement.IsDancing) return;

            if(HasStateAuthority) networkedAnimationState = MOVEMENT_STATE;
        }

        private void PlayerMovement_OnJump()
        {
            if (HasStateAuthority) networkedAnimationState = JUMP_STATE;

            if(jumpToFallCoroutine != null) StopCoroutine(jumpToFallCoroutine);
            jumpToFallCoroutine = StartCoroutine(JumpToFallCoroutine());
        }

        private IEnumerator JumpToFallCoroutine()
        {
            yield return new WaitForSeconds(jumpToFallDuration);

            if(currentState == JUMP_STATE)
            {
                PlayAnimationState(FALL_STATE);
            }

            jumpToFallCoroutine = null;
        }

        private void PlayerMovement_OnFalling()
        {
            if (networkedAnimationState == DANCE_STATE) return;

            if (HasStateAuthority) networkedAnimationState = FALL_STATE;
        }

        private void PlayerMovement_OnDance(bool isDancing)
        {
            if(HasStateAuthority) networkedAnimationState = isDancing ? DANCE_STATE : MOVEMENT_STATE;
        }

        private void PlayAnimationState(int stateHash)
        {
            animator.CrossFadeInFixedTime(stateHash, transitionDuration);
        }
    }
}
