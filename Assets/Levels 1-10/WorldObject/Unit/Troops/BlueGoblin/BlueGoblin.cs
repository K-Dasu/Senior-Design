using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;


public class BlueGoblin : Unit {
	private Quaternion aimRotation;
	
	//A* pathfinding
	public Seeker seeker;
	//public Path path;
	private float nextWayPointDistance = 3.0f;
	private Vector3 currentdest;
	private bool Attack_Anim, Run_Anim, Idle_Anim, Hit_Anim;
	
	
	protected override void Start () {
		base.Start ();
		//this is fine
		Attack_Anim = false;
		Idle_Anim = true;
		Run_Anim = false;
		Hit_Anim = false;
		//
		//
		//
		//A* pathfinding
		seeker = GetComponent<Seeker>();
		//
		//
		//
		
	}
	
	protected override void Update () {
		base.Update ();
		//animation.Play ();
		if (aiming) {
			Debug.Log ("AIMING");
			transform.rotation = Quaternion.RotateTowards (transform.rotation, aimRotation, weaponAimSpeed);
			CalculateBounds ();
			//sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
			Quaternion inverseAimRotation = new Quaternion (-aimRotation.x, -aimRotation.y, -aimRotation.z, -aimRotation.w);
			if (transform.rotation == aimRotation || transform.rotation == inverseAimRotation) {
				aiming = false;
			}
		}
		
		//
		//A* pathfinding
		if (isnewdest) {
			seeker.StartPath (transform.position, newdestination, OnPathComplete);
			//this is fine
			moveSpeed = 5.25f;
			isnewdest = false;
			Attack_Anim = false;
			Idle_Anim = false;
			Hit_Anim = false;
			Run_Anim = true;
		} else if (Move_To_Attack) {
			seeker.StartPath (transform.position, attackDestination, OnPathComplete);
			if(attackUnit){
				moveSpeed = 50f;
			}else if(attackBuilding){
				moveSpeed = 5f;
			}
			Move_To_Attack = false; 
			Attack_Anim = false; Idle_Anim = false; Hit_Anim = false; Run_Anim = true;
		} 
		//
		if (Run_Anim) {
			animation.CrossFade ("anim_run");
		} else if (Attack_Anim) {
			StartCoroutine ("Attack1");
		} else if (Idle_Anim) {
			animation.CrossFade ("anim_idle");
		} else if (Hit_Anim) {
			animation.CrossFade ("anim_hit");
		}
		
		if ((kills >= level * 10) && level < maxlevel) {
			level++;
			hitPoints = (int)(hitPoints * 1.5);
			maxHitPoints = (int)(maxHitPoints * 1.5);
			damage = (int)(damage * 1.5);
		}
	}
	
	IEnumerator Run() {
		animation.CrossFade ("anim_run");
		yield return new WaitForSeconds(animation.clip.length);
	}
	
	IEnumerator Attack1() {
		animation.CrossFade ("anim_attack_01");
		yield return new WaitForSeconds(animation.clip.length);
		
		
	}
	
	IEnumerator Idle() {
		animation.CrossFade ("anim_idle");
		yield return new WaitForSeconds(animation.clip.length);
	}
	
	//
	//
	//
	//A* pathfinding
	protected void OnPathComplete(Path p)
	{
		if (!p.error) {
			Debug.Log (tag + " is at destination");
			path = p;
			currentWaypoint = 0;
			//movingIntoPosition = false;
		}
	}
	//
	//
	//
	
	//
	//
	//
	//A* pathfinding
	protected override void FixedUpdate ()
	{
		
		base.FixedUpdate ();
		if (path == null) {
			return;
		}
		if (currentWaypoint >= path.vectorPath.Count) {
			movingIntoPosition = false;
			if(!attacking){
				Attack_Anim = false;
				Idle_Anim = true;
				Run_Anim = false;
				Hit_Anim = false;
			}else{
				Attack_Anim = true;
				Idle_Anim = false;
				Run_Anim = false;
				Hit_Anim = false;
			}
			return;
		}
		
		if (!aiming) 
		{
			Vector3 dir = (path.vectorPath [currentWaypoint] - transform.position).normalized;
			dir *= moveSpeed * Time.fixedDeltaTime;
			GetComponent<Rigidbody> ().MovePosition (rigidbody.position + dir);
			Attack_Anim = false;
			Idle_Anim = false;
			Run_Anim = true;
			Hit_Anim = false;
			CalculateBounds();
			//GetComponent<Rigidbody> ().transform.LookAt (path.vectorPath[currentWaypoint]);
			if (Vector3.Distance (rigidbody.position, path.vectorPath [currentWaypoint]) < nextWayPointDistance) {
				currentWaypoint++;
				return;
			}
		}
	}
	//
	//
	//
	
	protected override void UseWeapon () {
		base.UseWeapon();
		//this is fine
		Attack_Anim = true;
		Idle_Anim = false;
		Run_Anim = false;
		Hit_Anim = false;
		Vector3 spawnPoint = transform.position;
		spawnPoint.x += (2.1f * transform.forward.x);
		spawnPoint.y += 1.4f;
		spawnPoint.z += (2.1f * transform.forward.z);
		//Create the projectile
		if(Random.Range(0,100) < MissRate){
			GameObject gameObject = (GameObject)Instantiate(ResourceManager.GetWorldObject("TankProjectile"), spawnPoint, transform.rotation);
			Projectile projectile = gameObject.GetComponentInChildren< Projectile >();
			projectile.SetRange(0.9f * weaponRange);
			projectile.attacker = this;
			projectile.SetTarget(target);
			projectile.SetDamage (damage);
		}
	}
	
	public override bool CanAttack() {
		return true;
	}
	
	protected override void AimAtTarget () {
		base.AimAtTarget();
		aimRotation = Quaternion.LookRotation (target.transform.position - transform.position);
	}
	
	public override void TakeDamage(int damage) {
		hitPoints -= damage;
		Attack_Anim = false;
		Hit_Anim = true;
		Idle_Anim = false;
		Run_Anim = false;
		if(hitPoints<=0) Destroy(gameObject);
	}
	
	public override void receiveNewLocation(Vector3 dest)
	{
		if (currentdest != dest) {
			seeker.StartPath (transform.position, dest, OnPathComplete);
			animation.CrossFade ("run");
			currentdest = dest;
			is_attacking = true;
		}
	}
}
