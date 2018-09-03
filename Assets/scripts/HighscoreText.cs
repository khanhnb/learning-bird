using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighscoreText : MonoBehaviour {

    Text highscore;
	private void OnEnable()
	{
        highscore = GetComponent<Text>();
        highscore.text = PlayerPrefs.GetInt("Highscore").ToString();
	}
}
