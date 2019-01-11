using MLAgents;
using UnityEngine;

public class BlueAcademy : Academy
{
    /// <summary>
    /// The "walking speed" of the agents in the scene. 
    /// </summary>
    public float agentRunSpeed;

    /// <summary>
    /// The agent rotation speed.
    /// Every agent will use this setting.
    /// </summary>
	public float agentRotationSpeed;

    /// <summary>
    /// When a goal is scored the ground will switch to this 
    /// material for a few seconds.
    /// </summary>
    public Material goalScoredMaterial;

    /// <summary>
    /// When an agent fails, the ground will turn this material for a few seconds.
    /// </summary>
    public Material failMaterial;
}
