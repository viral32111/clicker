using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

// Script for handling everything for the gameplay scene
public class GamePlay : MonoBehaviour {

	// Public properties for the UI
	public TextMeshProUGUI progressText;
	public GameObject upgradeStore;
	public GameObject upgradePrefab;
	public int colorResetSeconds = 2;
	public float upgradeEntryPadding = 5.0f;

	// Public properties for object manipulations that can be set in the inspector
	public Material shaderMaterial;
	public float rotationSpeed = 0.05f;
	public float clickScaleSmoothing = 15.0f;
	public float clickScaleResetDistance = 1.0f;
	public float clickScaleMultiplier = 1.1f;

	// Public properties for default upgrade starting amounts
	public ulong defaultClickAmount = 1;
	public ulong defaultIdleAmount = 1;

	// Private properties for object scaling on click
	private Vector3 startScale = Vector3.zero;
	private Vector3 targetScale = Vector3.zero;
	private bool doScaleAnimation = false;

	// Private properties for UI
	private float upgradeStoreTotalHeight = 0.0f;
	private float lastUpgradeStoreEntryPositionY = 0.0f;
	private RectTransform upgradeStoreTransform;
	private RectTransform upgradePrefabTransform;
	private int clicksPerMinute = 0;
	private int totalClicks = 0;
	private int totalMinutes = 1; // Start at 1 to prevent divide by zero error
	private int pointsPerMinute = 0;
	private ulong totalPoints = 0;
	private bool canClick = true;
	private bool firstRun = true;

	// Public properties for achievements
	public GameObject achievementList;
	public GameObject achievementPrefab;
	public float achievementListPadding = 5.0f;

	// Private properties for achievements
	private float achievementListTotalHeight = 0.0f;
	private float achievementListLastEntryPositionY = 0.0f;
	private RectTransform achievementListTransform;
	private RectTransform achievementPrefabTransform;

	// Private properties for the sound effects
	private AudioSource audioSource;

	// Create a coroutine for resetting an button's color after a delay
	private IEnumerator<WaitForSeconds> resetButtonColor( Image buttonImage, Color originalColor ) {

		// Delay for the configured amount of seconds
		yield return new WaitForSeconds( colorResetSeconds );

		// Set the provided button image color to the provided color
		// NOTE: Only runs if image is not null, in case the image was removed while waiting above
		if ( buttonImage ) buttonImage.color = originalColor;

	}

	// Create a coroutine for resetting an image's color, and repopulating the upgrade store
	private IEnumerator<IEnumerator<WaitForSeconds>> repopulateUpgradeStore( Image buttonImage, Color originalColor ) {

		// Call the reset button color coroutine
		yield return resetButtonColor( buttonImage, originalColor );

		// Repopulate the upgrade store
		populateUpgradeStore();

	}

	// Create a coroutine for setting the clicks per minute, average points per minute, and idle points, every minute
	private IEnumerator<WaitForSeconds> doPerMinutes() {

		// Run forever...
		while ( true ) {

			// Delay for the configured amount of seconds
			yield return new WaitForSeconds( 60 );

			// Add any idle points, if applicable
			ulong idlePoints = defaultIdleAmount * new Upgrade( UpgradeType.IDLE_INCREASE, Progress.GetIdleTier() ).GetMultiplier();
			if ( idlePoints > 0 ) {
				Progress.IncrementPoints( idlePoints );
				totalPoints += idlePoints;
			}

			// Set the clicks per minute to an average
			clicksPerMinute = totalClicks / totalMinutes;

			// Set the points per minute to an average
			pointsPerMinute = ( int ) totalPoints / totalMinutes;

			// Update the progress text
			updateProgressText();

			// Increment the total minutes elapsed
			totalMinutes++;

		}

	}

	// Create a coroutine for waiting until the click interval is up
	private IEnumerator<WaitForSeconds> waitForClickInterval() {

		// Delay for the configured amount of seconds
		yield return new WaitForSeconds( new Upgrade( UpgradeType.CLICK_SPEED, Progress.GetSpeedTier() ).GetMultiplier() );

		// Allow the user to click again
		canClick = true;

	}

	// Helper function to easily update the progress text UI
	private void updateProgressText() {

		// Do not continue if the progress text has not been set in the inspector
		if ( progressText == null ) throw new Exception( "Progress text not set!" );

		// Get the current upgrade multipliers for clicking, and idling over a minute
		ulong pointsPerClick = defaultClickAmount * new Upgrade( UpgradeType.CLICK_INCREASE, Progress.GetClickTier() ).GetMultiplier();
		ulong idlePointsPerMin = defaultIdleAmount * new Upgrade( UpgradeType.IDLE_INCREASE, Progress.GetIdleTier() ).GetMultiplier();

		// Update the progress text using the provided values
		progressText.text = $"TOTAL POINTS: { Progress.GetPoints() }\n\n" +
			$"POINTS/CLICK: { pointsPerClick }\n" +
			$"AVG. CLICKS/MIN: { clicksPerMinute }\n" +
			$"AVG. POINTS/MIN: { pointsPerMinute }\n\n" +
			$"IDLE POINTS/MIN: { idlePointsPerMin }";

	}

	// Can be called to create a new upgrade object and add all the required UI
	private void createUpgrade( UpgradeType type, ulong tier ) {

		// Create a new object of the custom upgrade class, with the provided type and tier
		Upgrade upgrade = new( type, tier );

		// Increase the height of the scrollable content using the height of the upgrade store prefab so it can fit the upgrade we are about to add
		upgradeStoreTotalHeight += upgradePrefabTransform.rect.height + upgradeEntryPadding;
		upgradeStoreTransform.sizeDelta = new Vector2( 0.0f, upgradeStoreTotalHeight );

		// Add a new upgrade store entry using the prefab as a child to the upgrade store scrollable content, and store its UI transform for later use
		GameObject upgradeEntry = Instantiate( upgradePrefab, upgradeStore.transform, false );
		RectTransform upgradeEntryTransform = ( RectTransform ) upgradeEntry.transform;

		// Set its name to the upgrade type for debugging in the inspector
		upgradeEntry.name = type.ToString();

		// Reset the anchor points and relative position
		// NOTE: These exist on the prefab but for some reason get reset when instansiated?
		upgradeEntryTransform.anchorMin = new Vector2( 0.5f, 1.0f );
		upgradeEntryTransform.anchorMax = new Vector2( 0.5f, 1.0f );
		upgradeEntryTransform.pivot = new Vector2( 0.5f, 0.5f );
		upgradeEntryTransform.localPosition = new Vector3( ( firstRun ? -5.0f : 117.0f ), -86.0f, 0.0f ); // Unity is dumb

		// Increment the vertical position of this entry in the store
		upgradeEntryTransform.localPosition += new Vector3( 0.0f, lastUpgradeStoreEntryPositionY, 0.0f );

		// Decrement for the next store entry vertical position, accounting for padding between each store entry
		lastUpgradeStoreEntryPositionY -= ( upgradeEntryTransform.rect.height + upgradeEntryPadding );

		// Get the image and button for the upgrade store entry
		Image upgradeEntryImage = upgradeEntry.GetComponent<Image>();
		Button upgradeEntryButton = upgradeEntry.GetComponent<Button>();

		// Add an event listener for whenever the upgrade store entry button is clicked
		upgradeEntryButton.onClick.AddListener( () => {

			// Store the original color of the button
			Color originalColor = upgradeEntryImage.color;

			// If the user does not have enough points yet...
			if ( upgrade.GetPrice() > Progress.GetPoints() ) {

				// Change the background color of the upgrade store entry to red for visual feedback
				upgradeEntryImage.color = new Color( 0xFF / 255.0f, 0x83 / 255.0f, 0x7E / 255.0f );

				// Reset the color after a short time
				StartCoroutine( resetButtonColor( upgradeEntryImage, originalColor ) );

			// Otherwise, the user does have enough points
			} else {

				// Subtract the price from the user's points
				Progress.BuyUpgrade( upgrade );

				// Change the background color of the upgrade store entry to green for visual feedback
				upgradeEntryImage.color = new Color( 0xA7 / 255.0f, 0xFF / 255.0f, 0xA5 / 255.0f );

				// Do a different action depending on the upgrade type
				switch ( type ) {

					// Set the click tier if this upgrade is for better clicks
					case UpgradeType.CLICK_INCREASE: {
						Progress.SetClickTier( tier );
						unlockRespectiveAchievements(); // Unlock any achievements for this event
						break;
					}

					// Set the speed tier if this upgrade is for faster clicks
					case UpgradeType.CLICK_SPEED: {
						Progress.SetSpeedTier( tier );
						unlockRespectiveAchievements(); // Unlock any achievements for this event
						break;
					}

					// Set the idle tier if this upgrade is for idle points
					case UpgradeType.IDLE_INCREASE: {
						Progress.SetIdleTier( tier );
						unlockRespectiveAchievements(); // Unlock any achievements for this event
						break;
					}

					// Apply a random color if this is the style upgrade
					case UpgradeType.STYLE_COLOR: {
						shaderMaterial.SetColor( "ColorProperty", UnityEngine.Random.ColorHSV() ); // Property name should match what is defined in the shader properties
						unlockRespectiveAchievements(); // Unlock any achievements for this event
						break;
					}
				}

				// Reset the color and repopulate the upgrade store after a short time
				StartCoroutine( repopulateUpgradeStore( upgradeEntryImage, originalColor ) );

				// Update the progress text to reflect new upgrade values
				updateProgressText();

			}

		} );

		// Get the title, description and price text UI elements
		TextMeshProUGUI upgradeEntryTitle = upgradeEntryTransform.Find( "TitleText" ).gameObject.GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI upgradeEntryDescription = upgradeEntryTransform.Find( "DescriptionText" ).gameObject.GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI upgradeEntryPrice = upgradeEntryTransform.Find( "PriceText" ).gameObject.GetComponent<TextMeshProUGUI>();

		// If this is a speed upgrade and we have hit its limit
		if ( type == UpgradeType.CLICK_SPEED && ( upgrade.GetMultiplier() < 0 || upgrade.GetMultiplier() > Upgrade.maximumClickSpeed ) ) {
			
			// Disable interactivity with the button
			//upgradeEntryButton.enabled = false;
			upgradeEntryButton.interactable = false;

			// Set the title, description and price to indicate the upgrade is over
			upgradeEntryTitle.text = "FASTER CLICKS";
			upgradeEntryDescription.text = "YOU HAVE MAXED\nOUT THIS UPGRADE";
			upgradeEntryPrice.text = $"PRICE: 0";

		// Otherwise its another type of upgrade, or we haven't hit the limit for click speed upgrade yet
		} else {

			// Set the title, description and price for this upgrade
			upgradeEntryTitle.text = upgrade.GetName();
			upgradeEntryDescription.text = upgrade.GetDescription();
			upgradeEntryPrice.text = $"PRICE: { upgrade.GetPrice() }";

		}

	}

	// Can be called to regenerate the upgrade store UI
	private void populateUpgradeStore() {

		// If there are currently upgrades in the store...
		if ( upgradeStoreTransform.childCount > 0 ) {

			// Remove all of them
			foreach ( RectTransform transform in upgradeStoreTransform ) Destroy( transform.gameObject );

			// Reset vertical position/height variables
			upgradeStoreTotalHeight = 0.0f;
			lastUpgradeStoreEntryPositionY = 0.0f;

		}

		// Add a click increase upgrade above what the user currently has
		createUpgrade( UpgradeType.CLICK_INCREASE, Progress.GetClickTier() + 1 );

		// Add a click interval upgrade above what the user currently has
		createUpgrade( UpgradeType.CLICK_SPEED, Progress.GetSpeedTier() + 1 );

		// Add an idle increase upgrade above what the user currently has
		createUpgrade( UpgradeType.IDLE_INCREASE, Progress.GetIdleTier() + 1 );

		// Add the style upgrade
		// NOTE: Style upgrade has no tier
		createUpgrade( UpgradeType.STYLE_COLOR, 0 );

		// We've ran at least once
		if ( firstRun ) firstRun = false;

	}

	// Can be called to add a new achievement to the achievement list
	private void addAchievementToList( Achievement achievement, bool isInitial = false ) {

		// Increase the height of the scrollable content using the height of the achievement entry prefab so it can fit the achievement we are about to add
		achievementListTotalHeight += achievementPrefabTransform.rect.height + achievementListPadding;
		achievementListTransform.sizeDelta = new Vector2( 0.0f, achievementListTotalHeight );

		// Add a new achievement entry using the prefab as a child to the achievement list scrollable content, and store its UI transform for later use
		GameObject achievementEntry = Instantiate( achievementPrefab, achievementList.transform, false );
		RectTransform achievementEntryTransform = ( RectTransform ) achievementEntry.transform;

		// Set its name to the achievement name for debugging in the inspector
		achievementEntry.name = achievement.GetName();

		// Reset the anchor points and relative position
		// NOTE: These exist on the prefab but for some reason get reset when instansiated?
		achievementEntryTransform.anchorMin = new Vector2( 0.5f, 1.0f );
		achievementEntryTransform.anchorMax = new Vector2( 0.5f, 1.0f );
		achievementEntryTransform.pivot = new Vector2( 0.5f, 0.5f );
		achievementEntryTransform.localPosition = new Vector3( ( isInitial ? -14.0f : 116.0f ), -31.0f, 0.0f ); // Unity is still dumb

		// Increment the vertical position of this entry in the store
		achievementEntryTransform.localPosition += new Vector3( 0.0f, achievementListLastEntryPositionY, 0.0f );

		// Decrement for the next achievement entry vertical position, accounting for padding between each achievement entry
		achievementListLastEntryPositionY -= ( achievementEntryTransform.rect.height + achievementListPadding );

		// Get the title and reward text UI elements
		TextMeshProUGUI achievementEntryTitle = achievementEntryTransform.Find( "TitleText" ).gameObject.GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI achievementEntryReward = achievementEntryTransform.transform.Find( "RewardText" ).gameObject.GetComponent<TextMeshProUGUI>();

		// Set the title and reward text for this achievement
		achievementEntryTitle.text = achievement.GetName();
		achievementEntryReward.text = $"+{ achievement.GetReward() } POINTS";

	}

	// Quick helper function for checking if an achievement has already been unlocked
	private bool alreadyUnlockedAchievement( Achievement achievement ) {

		// Loop through all currently unlocked achievements...
		foreach ( Achievement currentAchievement in Progress.GetAchievements() ) {

			// Compare the currently unlocked achievement to the one provided
			// NOTE: Must compare data, cannot directly compare objects because they are different instansiations
			if ( currentAchievement.GetName() == achievement.GetName() && currentAchievement.GetReward() == achievement.GetReward() ) {

				// Return yes for this achievement is already unlocked
				return true;
			}

		}

		// Otherwise return no for this achievement not unlocked yet
		return false;

	}

	// Can be called to unlock any/all respective achievements given the current progress
	private void unlockRespectiveAchievements( bool isInitial = false ) {

		// Clear all existing achievements for repopulating the list
		if ( achievementListTransform.childCount > 0 ) foreach ( RectTransform transform in achievementListTransform ) Destroy( transform.gameObject );

		// Reset vertical position/height variables
		achievementListTotalHeight = 0.0f;
		achievementListLastEntryPositionY = 0.0f;

		// Store the total for all upgrade tiers
		// NOTE: -2 as player starts at click tier 1, idle tier 0 and speed tier 1
		ulong totalUpgradeTier = ( Progress.GetClickTier() + Progress.GetIdleTier() + Progress.GetSpeedTier() ) - 2;

		// Achievement for the first point
		if ( Progress.GetPoints() >= 1 ) {

			// Create a new achievement with an appropriate name and reward
			Achievement achievement = new( "FIRST POINT", 5 );

			// If the user has not already unlocked this achievement
			if ( !alreadyUnlockedAchievement( achievement ) ) {

				// Unlock the achievement and give them the reward
				Progress.UnlockAchievement( achievement );
				Progress.IncrementPoints( achievement.GetReward() );

			}

		}

		// Achievement for every power of 10 points (10, 100, 1,000, 10,000, 100,000, etc.)
		double pointsLogarithm = Math.Log10( Progress.GetPoints() );
		if ( Progress.GetPoints() >= 10 && Math.Floor( pointsLogarithm ) == pointsLogarithm ) {

			// Create a new achievement with an appropriate name and reward
			Achievement achievement = new( "MASS POINTS", Progress.GetPoints() * 2 );

			// If the user has not already unlocked this achievement
			if ( !alreadyUnlockedAchievement( achievement ) ) {

				// Unlock the achievement and give them the reward
				Progress.UnlockAchievement( achievement );
				Progress.IncrementPoints( achievement.GetReward() );

			};

		}

		// Achievement for first upgrade
		if ( totalUpgradeTier >= 1 ) {

			// Create a new achievement with an appropriate name and reward
			Achievement achievement = new( "FIRST UPGRADE", 25 );

			// If the user has not already unlocked this achievement
			if ( !alreadyUnlockedAchievement( achievement ) ) {

				// Unlock the achievement and give them the reward
				Progress.UnlockAchievement( achievement );
				Progress.IncrementPoints( achievement.GetReward() );

			};

		}

		// Achievement for every power of 10 upgrades
		double tierLogarithm = Math.Log10( totalUpgradeTier );
		if ( totalUpgradeTier >= 10 && Math.Floor( tierLogarithm ) == tierLogarithm ) {

			// Create a new achievement with an appropriate name and reward
			Achievement achievement = new( "MASS UPGRADE", totalUpgradeTier * 2 );

			// If the user has not already unlocked this achievement
			if ( !alreadyUnlockedAchievement( achievement ) ) {

				// Unlock the achievement and give them the reward
				Progress.UnlockAchievement( achievement );
				Progress.IncrementPoints( achievement.GetReward() );

			};

		}

		// Achievement for maxing out click interval upgrade
		if ( Progress.GetSpeedTier() == Upgrade.maximumClickSpeed ) {

			// Create a new achievement with an appropriate name and reward
			Achievement achievement = new( "MAX SPEED", 500 );

			// If the user has not already unlocked this achievement
			if ( !alreadyUnlockedAchievement( achievement ) ) {

				// Unlock the achievement and give them the reward
				Progress.UnlockAchievement( achievement );
				Progress.IncrementPoints( achievement.GetReward() );

			};

		}

		// Add all unlocked achievements to the achievements list
		foreach ( Achievement achievement in Progress.GetAchievements() ) addAchievementToList( achievement, isInitial );

		// Update the progression text with any new reward points
		updateProgressText();

	}

	// Runs whenever the object is loaded...
	public void Start() {

		// Set the original and target scale
		startScale = transform.localScale;
		targetScale = transform.localScale * clickScaleMultiplier;

		// Set the upgrade store transforms for use when creating an upgrade
		// NOTE: Cast is required to convert from generic transform to UI transform
		upgradeStoreTransform = ( RectTransform ) upgradeStore.transform;
		upgradePrefabTransform = ( RectTransform ) upgradePrefab.transform;

		// Set the achievement list transforms for use when adding an achievement
		// NOTE: Cast is required to convert from generic transform to UI transform
		achievementListTransform = ( RectTransform ) achievementList.transform;
		achievementPrefabTransform = ( RectTransform ) achievementPrefab.transform;

		// Set a random starting rotation
		gameObject.transform.rotation = UnityEngine.Random.rotation;

		// Reset the clicker color since it retains it when in the editor
		shaderMaterial.SetColor( "ColorProperty", Color.white ); // Property name should match what is defined in the shader properties

		// Get the audio source
		audioSource = gameObject.GetComponent<AudioSource>();

		// Load the game's latest progress only if we're debugging in the editor 
		// NOTE: Needed because the editor does not load the start menu scene when debugging the gameplay scene
		if ( Application.isEditor ) Progress.Load();

		// Update progress text UI with values from loaded progression
		updateProgressText();

		// Add all initial upgrades to the store
		populateUpgradeStore();

		// Start the per minute coroutine
		StartCoroutine( doPerMinutes() );

		// Unlock all previous achievements
		unlockRespectiveAchievements( true );

	}

	// Runs every frame
	public void Update() {

		// If we should be scaling the object...
		if ( doScaleAnimation ) {

			// Update the object scale with a lerp from the current scale to the target scale using current time and the smoothing value
			gameObject.transform.localScale = Vector3.Lerp( gameObject.transform.localScale, targetScale, Time.deltaTime * clickScaleSmoothing );

			// If we are near the specified end of the lerp...
			if ( gameObject.transform.localScale.x >= ( targetScale.x - clickScaleResetDistance ) ) {

				// Reset the object's scale to its original value
				gameObject.transform.localScale = startScale;

				// Stop the object scaling
				doScaleAnimation = false;

			}

		}

	}

	// Runs whenever the mouse is held down and moved on the object...
	[ Obsolete ] // NOTE: Requried as .RotateAround() can be done with .Rotate() but the parameters need to be different?
	public void OnMouseDrag() {

		// Rotate the object according to the current mouse X and Y position and specified rotation speed
		gameObject.transform.RotateAround( Vector3.down, Input.GetAxis( "Mouse X" ) * rotationSpeed );
		gameObject.transform.RotateAround( Vector3.right, Input.GetAxis( "Mouse Y" ) * rotationSpeed );

	}

	// Runs whenever the mouse is released on the object...
	public void OnMouseUp() {

		// Do not continue if the click interval has not expired yet
		if ( !canClick ) return;

		// Run the scale animation for user feedback
		if ( !doScaleAnimation ) doScaleAnimation = true;

		// Increment the current points by whatever click upgrade there is
		ulong clickPoints = defaultClickAmount * new Upgrade( UpgradeType.CLICK_INCREASE, Progress.GetClickTier() ).GetMultiplier();
		Progress.IncrementPoints( clickPoints );
		totalPoints += clickPoints;

		// Increment the total clicks
		totalClicks++;

		// Update progress text UI with new points
		updateProgressText();

		// Unlock any achievements for this event
		unlockRespectiveAchievements();

		// Start the click interval timer
		canClick = false;
		StartCoroutine( waitForClickInterval() );

		// Play the sound effect
		audioSource.Play();

	}

	// Runs when the exit game button is clicked...
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
