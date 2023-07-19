using System;

// Custom class for dynamic achievements
public class Achievement {

	// Private properties for achievement information
	private readonly string displayName = string.Empty;
	private readonly ulong pointsReward = 0;

	// Constructor for creating a new achievement
	public Achievement( string name, ulong reward ) {
		displayName = name;
		pointsReward = reward;
	}

	// Can be called to get this achievement's name
	public string GetName() {
		return displayName;
	}

	// Can be called to get this achievement's reward
	public ulong GetReward() {
		return pointsReward;
	}
}