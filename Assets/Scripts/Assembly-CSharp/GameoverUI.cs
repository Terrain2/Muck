﻿using System;
using TMPro;
using UnityEngine;


public class GameoverUI : MonoBehaviour
{

	private void Awake()
	{
		int winnerId = GameManager.instance.winnerId;
		if (winnerId == -2)
		{
			this.daysText.text = "Survived for " + GameManager.instance.currentDay + " days.";
			return;
		}
		if (winnerId == -1)
		{
			this.daysText.text = "Draw...";
			return;
		}
		string username = GameManager.players[winnerId].username;
		this.daysText.text = username + " won the game!";
	}


	public TextMeshProUGUI daysText;
}
