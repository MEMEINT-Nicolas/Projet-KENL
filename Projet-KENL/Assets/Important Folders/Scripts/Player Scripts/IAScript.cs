﻿using UnityEngine;
using UnityEngine.Networking;

public class IAScript : NetworkBehaviour
{
    private float xInput;
    private bool jumpButtonPressed;
    private int attackSelected;

    private PlayerScript player;
    private CharacterController charaControl;
    private GameObject otherPlayer;
    private CharacterController otherPlayerCharaControl;

    // Use this for initialization
    void Start()
    {
        player = GetComponent<PlayerScript>();
        charaControl = GetComponent<CharacterController>();

        foreach (var player in GameObject.FindGameObjectsWithTag("Player")) {
            if (!player.GetComponent<PlayerScript>().isKO) {
                otherPlayer = player;
                break;
            }
        }

        otherPlayerCharaControl = otherPlayer.GetComponent<CharacterController>();
    }

    private void Update()
    {
        // We move the IA
        GetInputsIA();

        // Function for moving the player with input (!= IA)
        MovementPlayer();
    }

    private void GetInputsIA()
    {
        // If -1 => no attacks
        attackSelected = -1;

        if (otherPlayer.transform.position.x < transform.position.x - 2)
            xInput = -1.0f;
        else if (otherPlayer.transform.position.x > transform.position.x + 2)
            xInput = 1.0f;
        else
            xInput = 0f;
        /*else
        {
            System.Random rand = new System.Random();
            if (rand.Next(0, 30) == 1)
                inputs[0] = true;
            else
                xInput = 1.0f;

        }*/

        // Lorsque IA touchée
        if (player.IsHit()) {
            if (otherPlayer.transform.position.x < transform.position.x)
                xInput = 1.0f;
            else
                xInput = -1.0f;
        }

        // Le time évite le bug au début ou le perso saute sans raison
        if ((!otherPlayerCharaControl.isGrounded && Time.deltaTime > 1.0f)
            || (otherPlayer.transform.position.y - transform.position.y > 0.2f))
            jumpButtonPressed = true;
        else
            jumpButtonPressed = false;
    }

    private void MovementPlayer()
    {
        /* To move the player with input */

        player.Movements(xInput, jumpButtonPressed, attackSelected);

        // We make sure there is no movement through Z-Axis
        charaControl.transform.position.Set(
            charaControl.transform.position.x,
            charaControl.transform.position.y,
            0);
    }
}
