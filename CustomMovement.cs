using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CustomMovement : UdonSharpBehaviour
{
    public StatManager statManager;

    [Header("Base Movement Settings")]
    public float walkSpeed = 3.5f;
    public float baseSprintSpeed = 6f;
    public float sprintSpeedPerAgility = 0.3f;

    [Header("Stamina Drain")]
    public float baseSprintStaminaDrain = 10f;

    [Header("Jump Settings")]
    public float jumpForce = 6f;
    public float jumpStaminaCost = 15f;

    [Header("Jump Scaling")]
    public float jumpBoostPerAgility = 0.3f;
    public float maxJumpForce = 9f;

    private VRCPlayerApi localPlayer;
    private bool isSprinting = false;
    private bool canSprint = true;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    void Update()
    {
        if (localPlayer == null || statManager == null) return;

        // Reset movement states to use VRC's built-in locomotion system
        localPlayer.Immobilize(false);

        // Input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;
        Vector3 move = localPlayer.GetRotation() * inputDir;

        // Sprint Input: Desktop = LeftShift, PCVR = Left Stick Full Forward
        bool desktopSprint = Input.GetKey(KeyCode.LeftShift);
        bool vrSprint = Input.GetAxis("Vertical") > 0.95f;
        bool isTryingToSprint = (desktopSprint || vrSprint) && move.sqrMagnitude > 0.01f;

        float currentSpeed = walkSpeed;

        if (isTryingToSprint && canSprint)
        {
            float sprintSpeed = baseSprintSpeed + (statManager.agility * sprintSpeedPerAgility);
            float drain = baseSprintStaminaDrain * Time.deltaTime;

            if (statManager.TryUseStamina(drain))
            {
                isSprinting = true;
                currentSpeed = sprintSpeed;
            }
            else
            {
                isSprinting = false;
                canSprint = false;
            }
        }
        else
        {
            isSprinting = false;
        }

        // Reactivate sprint after stamina recovery
        if (!canSprint && statManager.currentStamina >= statManager.GetMaxStamina() * 0.25f)
        {
            canSprint = true;
        }

        // Apply movement via built-in locomotion
        localPlayer.SetWalkSpeed(currentSpeed);
        localPlayer.SetRunSpeed(currentSpeed);
        localPlayer.SetStrafeSpeed(currentSpeed);

        // Handle jumping
        if (Input.GetButtonDown("Jump") && localPlayer.IsPlayerGrounded())
        {
            if (statManager.TryUseStamina(jumpStaminaCost))
            {
                float scaledJump = jumpForce + (statManager.agility * jumpBoostPerAgility);
                scaledJump = Mathf.Min(scaledJump, maxJumpForce);
                localPlayer.SetVelocity(new Vector3(localPlayer.GetVelocity().x, scaledJump, localPlayer.GetVelocity().z));
            }
        }
    }
}
