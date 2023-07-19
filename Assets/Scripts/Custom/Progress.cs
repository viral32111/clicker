using System;
using System.Collections.Generic;
using UnityEngine;

// Custom static class for encapsulating progression
public static class Progress {

	// Define private properties for current progress
	private static ulong currentPoints = 0;
	private static ulong clickUpgradeTier = 1;
	private static ulong idleUpgradeTier = 0;
	private static ulong speedUpgradeTier = 1;
	private static List<Achievement> achievementsUnlocked = new();

	// Can be called to add to the current number of points
	public static ulong IncrementPoints( ulong points ) {

		// Do not continue if the provided number of points is invalid
		if ( points <= 0 ) throw new ArgumentOutOfRangeException( "Points must be greater than zero." );

		// Add the provided points to the current points
		currentPoints += points;

		// Return the current number of points
		return currentPoints;

	}

	public static ulong BuyUpgrade( Upgrade upgrade ) {

		// Do not continue if we do not have enough points
		if ( upgrade.GetPrice() > currentPoints ) throw new Exception( "Not enough points to afford upgrade." );

		// Subtract the upgrade price from our points
		currentPoints -= upgrade.GetPrice();

		// Return the new number of points
		return currentPoints;

	}

	// Can be called to get the current number of points
	public static ulong GetPoints() {
		return currentPoints;
	}

	// Can be called to set the current click tier
	public static ulong SetClickTier( ulong tier ) {

		// Do not continue if the provided tier is invalid
		if ( tier <= 0 ) throw new ArgumentOutOfRangeException( "Tier must be greater than zero." );

		// Update the current click tier with the provided one
		clickUpgradeTier = tier;
		
		// Return the new click tier
		return clickUpgradeTier;

	}

	// Can be called to get the current click tier
	public static ulong GetClickTier() {
		return clickUpgradeTier;
	}

	// Can be called to set the current idle tier
	public static ulong SetIdleTier( ulong tier ) {

		// Do not continue if the provided tier is invalid
		if ( tier <= 0 ) throw new ArgumentOutOfRangeException( "Tier must be greater than zero." );

		// Update the current idle tier with the provided one
		idleUpgradeTier = tier;

		// Return the new idle tier
		return idleUpgradeTier;

	}

	// Can be called to get the current idle tier
	public static ulong GetIdleTier() {
		return idleUpgradeTier;
	}

	// Can be called to set the current speed tier
	public static ulong SetSpeedTier( ulong tier ) {

		// Do not continue if the provided tier is invalid
		if ( tier <= 0 ) throw new ArgumentOutOfRangeException( "Tier must be greater than zero." );

		// Update the current speed tier with the provided one
		speedUpgradeTier = tier;

		// Return the new speed tier
		return speedUpgradeTier;

	}

	// Can be called to get the current speed tier
	public static ulong GetSpeedTier() {
		return speedUpgradeTier;
	}

	// Can be called to add an unlocked achievement
	public static List<Achievement> UnlockAchievement( Achievement achievement ) {

		// Add the provided achievement to the unlocked achievements list
		achievementsUnlocked.Add( achievement );

		// Return the unlocked achievements list
		return achievementsUnlocked;

	}

	// Can be called to get a list of unlocked achievements
	public static List<Achievement> GetAchievements() {
		return achievementsUnlocked;
	}

	// Can be called to save the current game progress
	public static bool Save() {

		// Save all of the progression values into the player preferences file
		// NOTE: Possible data loss due to type casting from long to int
		PlayerPrefs.SetInt( "currentPoints", ( int ) currentPoints );
		PlayerPrefs.SetInt( "clickUpgradeTier", ( int ) clickUpgradeTier );
		PlayerPrefs.SetInt( "idleUpgradeTier", ( int ) idleUpgradeTier );
		PlayerPrefs.SetInt( "speedUpgradeTier", ( int ) speedUpgradeTier );
		PlayerPrefs.Save();

		// Return success/failure by checking if all the above keys exist in the preferences file
		return PlayerPrefs.HasKey( "currentPoints" )
			&& PlayerPrefs.HasKey( "clickUpgradeTier" )
			&& PlayerPrefs.HasKey( "idleUpgradeTier" )
			&& PlayerPrefs.HasKey( "speedUpgradeTier" );

	}

	// Can be called to load the current game progress
	public static bool Load() {

		// NOTE: Only used to reset for debugging purposes
		PlayerPrefs.DeleteAll();

		// Return failure if one of the required keys does not exist
		if ( !PlayerPrefs.HasKey( "currentPoints" )
			|| !PlayerPrefs.HasKey( "clickUpgradeTier" )
			|| !PlayerPrefs.HasKey( "idleUpgradeTier" )
			|| !PlayerPrefs.HasKey( "speedUpgradeTier" ) ) return false;

		// Update the static class properties with the values from the player preferences file
		currentPoints = ( ulong ) PlayerPrefs.GetInt( "currentPoints" );
		clickUpgradeTier = ( ulong ) PlayerPrefs.GetInt( "clickUpgradeTier" );
		idleUpgradeTier = ( ulong ) PlayerPrefs.GetInt( "idleUpgradeTier" );
		speedUpgradeTier = ( ulong ) PlayerPrefs.GetInt( "speedUpgradeTier" );

		// Return success
		return true;

	}
}
