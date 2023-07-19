using System;

// Custom enumerations for the type of upgrades
public enum UpgradeType {
	CLICK_INCREASE = 1,
	CLICK_SPEED = 2,
	IDLE_INCREASE = 3,
	STYLE_COLOR = 4
}

// Custom class for each dynamic upgrade
public class Upgrade {

	// Public properties for click speed upgrade
	public static readonly ulong maximumClickSpeed = 4;

	// Private properties for the type and tier of this upgrade
	private readonly UpgradeType upgradeType;
	private readonly ulong upgradeTier;

	// Constructor for creating a new upgrade with a provided type and tier
	public Upgrade( UpgradeType type, ulong tier ) {
		upgradeType = type;
		upgradeTier = tier;
	}

	// Can be called to get this upgrade's name for the upgrade shop UI
	public string GetName() {

		// Return a different value depending on the upgrade type
		switch ( upgradeType ) {
			case UpgradeType.CLICK_INCREASE: return $"BETTER CLICKS { upgradeTier }";
			case UpgradeType.CLICK_SPEED: return $"FASTER CLICKS { upgradeTier }";
			case UpgradeType.IDLE_INCREASE: return $"IDLE POINTS { upgradeTier }";
			case UpgradeType.STYLE_COLOR: return $"CUSTOM STYLE"; // Style has no tier
			default: return string.Empty; // Default to nothing
		}

	}

	// Can be called to get this upgrade's description for the upgrade shop UI
	public string GetDescription() {

		// Return a different value depending on the upgrade type
		switch ( upgradeType ) {
			case UpgradeType.CLICK_INCREASE: return $"INCREASES CLICK INCOME TO { GetMultiplier() }";
			case UpgradeType.CLICK_SPEED: return $"DECREASES CLICK INTERVAL TO { GetMultiplier() }";
			case UpgradeType.IDLE_INCREASE: return $"INCREASES IDLE INCOME RATE TO { GetMultiplier() }";
			case UpgradeType.STYLE_COLOR: return $"CHANGES OBJECT APPERANCE TO RANDOM COLOR"; // Style has no tier
			default: return string.Empty; // Default to nothing
		}

	}

	// Can be called to get this upgrade's price for the upgrade shop UI
	public ulong GetPrice() {

		// Return a different value depending on the upgrade type
		switch ( upgradeType ) {
			case UpgradeType.CLICK_INCREASE: return upgradeTier * 15;
			case UpgradeType.CLICK_SPEED: return upgradeTier * 8;
			case UpgradeType.IDLE_INCREASE: return upgradeTier * 25;
			case UpgradeType.STYLE_COLOR: return 1000; // Style is always the same price
			default: return 0; // Default to zero
		}

	}

	// Can be called to get this upgrade's multiplier
	public ulong GetMultiplier() {

		// Return a different value depending on the upgrade type
		switch ( upgradeType ) {
			case UpgradeType.CLICK_INCREASE: return upgradeTier * 2; // Double every time
			case UpgradeType.CLICK_SPEED: return maximumClickSpeed - upgradeTier; // Subtract one every time
			case UpgradeType.IDLE_INCREASE: return upgradeTier * 4; // Double every time
			case UpgradeType.STYLE_COLOR: return 0; // Style has no multiplier
			default: return 0; // Default to no multiplier
		}

	}

}