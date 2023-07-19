using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Script for handling the Start Menu scene UI
public class StartMenu : MonoBehaviour {

	// Runs when the scene loads...
	public void Start() {

		// Load the game's latest progress
		Progress.Load();

	}

	// Runs whenever the start button is clicked...
	public void OnStartClick() {

		// Quick message for debugging
		Debug.Log( "The start button has been clicked!" );

		// Change to the main gameplay scene
		SceneManager.LoadScene( "GamePlay", LoadSceneMode.Single );

	}

	// Runs whenever the exit button is clicked...
	public void OnExitClick() {

		// Quick message for debugging
		Debug.Log( "The exit button has been clicked!" );

		// Save the game's current progress
		Progress.Save();

		// Exit the game
		// NOTE: This is ignored in the editor
		Application.Quit();

	}

}
