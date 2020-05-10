﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeTrigger : MonoBehaviour
{
    public Player player;
    void OnTriggerEnter()
    {
        if (GameManager.instance.state == GameState.Play) player.CompleteLevel();
    }
    
}
