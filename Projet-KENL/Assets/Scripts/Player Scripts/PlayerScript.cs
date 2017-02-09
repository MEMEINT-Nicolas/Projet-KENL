﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerScript : MonoBehaviour
{
    // Jump var (public)
    [Header("Jumping")]
    [Space(5)]
    public float gravity = 14.0f;
    public float jumpForce = 10.0f; // Y-velocity added when jumping
    public float horizontalVelocity = 20.0f;
    public int jumpMax = 2; // How many jumps the player can do
                            // before being grounded

    [Space]

    // Attack var (public)
    [Header("Attacking")]
    [Space(5)]
    public AttackTemplate[] listAttacks;
    public int maxCombo = 4;
    public float period = 1f; // Set the period between each attackCollider check


    // Jump var (private)
    private float verticalVelocity;
    private int jumpCount = 0; // How many jumps done before grounded

    // Attack var (private)
    private float percentHealth = 0; // powerReceived = 
                                     // power * (1 + percentHealth / 100)
    private float attackTimer = 0f; // If 0f, the player can attack
                                    // again (no combos)
    private int comboActual = 0;
    private bool isAttacking = false; // See if the player is attacking
    private float currentPeriod = 0; // If <= 0, can check attackCollider
    private bool attackTimerActivated = false; // If true, activate he attack timer
    private AttackTemplate currentAttack;
    
    public float InvulnerableTimer { get; private set; }
        // If <= 0f, player can get hit, resets in function of the attack

    // Other movements var (public)
    private float waySign; // If 1: player looks to the right

    // Other movements var (private)
    private Vector3 moveVector;
    private CharacterController charaControl;

    private void Start()
    {
        charaControl = GetComponent<CharacterController>();
        InvulnerableTimer = 0f;
    }

    public Vector3 GetMoveVector() { return moveVector; }

    public void Movements(float xInput, bool jumpButtonPressed, bool[] inputs)
    {
        /* Change the moveVector based on differents forces and inputs
         * See the functions called Movement_x to see the detail */

        // We update the timers
        UpdateTimers();

        // Reset moveVector before making a change
        moveVector = Vector3.zero;
        Movement_Run(xInput);
        Movement_Jump(jumpButtonPressed);
        isAttacking = Movement_Attack(inputs);

        CheckRotation(xInput);
    }


    private void UpdateTimers()
    {
        // Attack Timer
        if (attackTimer > 0f) { attackTimer -= Time.deltaTime; }

        // Invulnerable Timer
        if (InvulnerableTimer > 0f) { InvulnerableTimer -= Time.deltaTime; }

        // Attack-Collision Timer
        if (attackTimerActivated) {
            currentPeriod = Mathf.Max(0f, currentPeriod - Time.deltaTime);

            if (currentPeriod <= 0f) {
                currentAttack.CollidersAttack();

                // We reset the attack timer
                currentPeriod = period;
            }
        }
    }

    private void AddMovement(Vector3 movement)
    {
        /* Call this function to add a movement to this player */

        moveVector += movement;
    }

    private bool Movement_Run(float xInput)
    {
        /* Make the player run based on xInput */
        if (Mathf.Abs(xInput) >= 0.25) {
            AddMovement(new Vector3(xInput * horizontalVelocity, 0, 0));
            return true;
        } else { return false; }
    }

    private bool Movement_Jump(bool jumpButtonPressed)
    {
        /* Verify if a jump can be made, if so, makes the player jump
         * Returns a bool indicating if a jump has been made */

        bool jumped = CheckJump(jumpButtonPressed);

        AddMovement(new Vector3(0, verticalVelocity, 0));

        return jumped;
    }

    private bool Movement_Attack(bool[] inputs)
    {
        /* Check if an attack can be made, if so, make the player attack
         * There is also combos (& combos limit)
         * Returns true if an attack has been made (else false) */

        if (attackTimer > 0) {
            // Here should be called an attack animation
            // Give a color to the collider (DEBUG)
            transform.GetChild(1).GetComponent<Renderer>()
                .material.color = Color.black;

            // Just for seeing when we can combo (DEBUG)
            if (attackTimer < 0.5f) {
                transform.GetChild(1).GetComponent<Renderer>()
                .material.color = Color.yellow;
            }
        } else {
            // Here we should finish an attack animation
            // Remove color of the collider (DEBUG)
            transform.GetChild(1).GetComponent<Renderer>()
                .material.color = Color.white;

            comboActual = 0; // We don't forget to reset the combo var
            attackTimerActivated = false; // And also the attacks timer
        }

        // Attacks & Combos
        for (int i = 0; i < listAttacks.Length; i++) {
            if (comboActual < maxCombo
                && attackTimer < 0.5f
                && inputs[i]) {
                // We activate the attack timer
                attackTimerActivated = true;
                currentPeriod = period;
                currentAttack = listAttacks[i];

                currentAttack.CollidersAttack();
                comboActual += currentAttack.ComboIncrease;
                attackTimer = currentAttack.AttackCooldown;

                return true;
            }
        }

        return attackTimer > 0f;
    }
    
    private void CheckRotation(float xInput)
    {
        /* Check if the player is correctly rotated
         * If not, rotate it correctly */

        waySign = IsCorrectWay() ? 1 : -1;

        if (waySign * xInput < 0) {
            transform.Rotate(new Vector3(0, waySign * 180));
        }
    }

    private bool CheckJump(bool jumpButtonPressed)
    {
        /* Verify if a jump can be made & actualize jump vars
         * It is also here we make gravity work */

        if (charaControl.isGrounded) {
            jumpCount = 0;

            // If we set verticalVelocity to -1f << 1f isGrounded doesn't work
            // => Go home Unity, you're drunk
            verticalVelocity = -1f;
        } else {
            // Gravity here
            verticalVelocity -= gravity * Time.deltaTime;

            // If we fall without making a jump, we lose 1 jump
            if (jumpCount == 0) { jumpCount = 1; }
        }

        if (jumpCount < jumpMax
                && verticalVelocity < 0.5f * jumpForce
                && jumpButtonPressed) {
            verticalVelocity = jumpForce;
            jumpCount++;
            return true;
        }

        return false;
    }

    private bool IsCorrectWay()
    {
        /* Returns true if the player is facing to the right */
        return transform.eulerAngles.y == 0;
    }
}