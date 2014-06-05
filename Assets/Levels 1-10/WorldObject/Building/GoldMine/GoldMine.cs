using UnityEngine;
using RTS;

public class GoldMine : Building {
	
	// Use this for initialization
	private float time = 0.0f;
	public int productionRate = 2;
	public int totalAmount = 0;
	
	protected override void Start () {
		base.Start();
		//statsToPrint = new string[] {"resource", "rate"};
	}
	
	protected override void Update () {
		base.Update();
		time += Time.deltaTime;
		
		if (time >= 5.0f && player.human) {
			time -= 5.0f;
			player.AddResource(ResourceType.Money, 5 * productionRate);
			ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
			ps.Play ();
			audio.PlayOneShot(goldSound);
			totalAmount += 5 * productionRate;
		}
		
		if (totalAmount > 2000 && level == 4) {
			productionRate = (int)(productionRate * 1.5);
			level++;
		}
		else if (totalAmount > 1000 && level == 3) {
			productionRate = (int)(productionRate * 1.5);
			level++;
		}
		else if (totalAmount > 500 && level == 2) {
			productionRate = (int)(productionRate * 1.5);
			level++;
		}
		else if (totalAmount > 200 && level == 1) {
			productionRate = (int)(productionRate * 1.5);
			level++;
		}
	}
}