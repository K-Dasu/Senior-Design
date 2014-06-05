using UnityEngine;
using RTS;

public class Castle : Building {
	
	// Use this for initialization
	private float time = 0.0f;
	public int productionRate = 2;
	public int totalAmount = 0;

	
	protected override void Start () {
		base.Start();
	}
	
	protected override void Update () {
		base.Update();
		time += Time.deltaTime;
		
		if (time >= 5.0f && player.human) {
			time -= 5.0f;
			player.AddResource(ResourceType.Money, 5 * productionRate);
			ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
			audio.PlayOneShot(goldSound);
			ps.Play ();
			totalAmount += 5 * productionRate;
		}

	}
}