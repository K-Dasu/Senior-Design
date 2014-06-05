using UnityEngine;
using System.Collections;
using RTS;

public class Barracks : Building {

	// the string needs to match the perfab name perfectly

	protected override void Start () {
		base.Start();
		actions = new string[] {"Orc"};
	}

	public override void PerformAction(string actionToPerform) {
		base.PerformAction(actionToPerform);
		if (player.Money >= ResourceManager.GetUnit (actionToPerform).GetComponent<Unit> ().cost) {
			CreateUnit (actionToPerform);
			if(actionToPerform == "Orc"){
				if(ResourceManager.O >= 5 && level == 1){
					level++;
					actions = new string[] {"Orc", "Blue Goblin"};
				}
			}
			if(actionToPerform == "Blue Goblin"){
				if(ResourceManager.B >= 5 && level == 2){
					level++;
					actions = new string[] {"Orc", "Blue Goblin", "Skull Warrior"};
				}
			}
			if(actionToPerform == "Skull Warrior"){
				if (ResourceManager.S >= 5 && level == 3) {
					level++;
					actions = new string[] {"Orc", "Blue Goblin", "Skull Warrior", "Demon"};
				}
			}
		}
	}


}
