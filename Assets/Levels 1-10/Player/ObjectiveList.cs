using UnityEngine;
using System.Collections;
using RTS;

public class ObjectiveList : MonoBehaviour {
	public Texture2D grayStar, goldStar;
	public GUISkin objectivesSkin;
	private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
	public bool start;
	private string obj1, obj2;
	private bool comp1, comp2;

	private int currentLevel;

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;
		start = false;
		currentLevel = Application.loadedLevel-4;
		if (currentLevel >= 0) {
			obj1 = ResourceManager.GetObjective (currentLevel, 0);
			obj2 = ResourceManager.GetObjective (currentLevel, 1);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI() {
		if (!start) {
			GUI.skin = objectivesSkin;
			GUI.BeginGroup (new Rect (150, RESOURCE_BAR_HEIGHT + 150, Screen.width - ORDERS_BAR_WIDTH - 300, Screen.height - RESOURCE_BAR_HEIGHT - 300));
			GUI.Box (new Rect (0, 0, Screen.width - ORDERS_BAR_WIDTH - 300, Screen.height - RESOURCE_BAR_HEIGHT - 300), "");
			float x = (Screen.width - ORDERS_BAR_WIDTH - 450) / 2;
			GUI.Label (new Rect (x, 10, 400, 100), "Objectives");


			if(currentLevel >= 0 && ResourceManager.GetCompletions(currentLevel, 0))
				GUI.DrawTexture (new Rect (10, 70, 40, 40), goldStar);
			else
				GUI.DrawTexture (new Rect (10, 70, 40, 40), grayStar);
			GUI.Label (new Rect (60, 70, Screen.width - ORDERS_BAR_WIDTH - 360, 100), obj1);

			//ResourceManager.SetCompletions(1, 1);

			if(currentLevel >= 0 && ResourceManager.GetCompletions(currentLevel, 1))
				GUI.DrawTexture (new Rect (10, 140, 40, 40), goldStar);
			else
				GUI.DrawTexture (new Rect (10, 140, 40, 40), grayStar);
			GUI.Label (new Rect (60, 140, Screen.width - ORDERS_BAR_WIDTH - 360, 100), obj2);


			if (GUI.Button (new Rect (x, 200, 200, 40), "Challenge Accepted!")) {
				start = true;
				Time.timeScale = 1;	
			}
			GUI.EndGroup ();
		}
	}
}
