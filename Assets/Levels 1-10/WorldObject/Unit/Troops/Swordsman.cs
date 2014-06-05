using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;


public class Swordsman : Unit {
	private Quaternion aimRotation;

	//A* pathfinding
	public Seeker seeker;
	//public Path path;
	private float nextWayPointDistance = 3.0f;
	private Vector3 currentdest;
	private bool Attack_Anim, Run_Anim, Idle_Anim;
	private float timer;
	private float counter;
	
	
	protected override void Start () {
		base.Start ();
		Attack_Anim = false;
		Idle_Anim = true;
		Run_Anim = false;
		counter = 0f;
		timer = Random.Range (0, 2);
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
		base.Update();
		//animation.Play ();
		if(aiming) {
//			Debug.Log ("AIMING");
			transform.rotation = Quaternion.RotateTowards(transform.rotation, aimRotation, weaponAimSpeed);
			CalculateBounds();
			//sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
			Quaternion inverseAimRotation = new Quaternion(-aimRotation.x, -aimRotation.y, -aimRotation.z, -aimRotation.w);
			if(transform.rotation == aimRotation || transform.rotation == inverseAimRotation) {
				aiming = false;
			}
		}
		

		//A* pathfinding
		if (isnewdest) {
						animation.CrossFade ("run");
						seeker.StartPath (transform.position, newdestination, OnPathComplete);
						isnewdest = false;
						Attack_Anim = false;
						Idle_Anim = false;
						Run_Anim = true;
				}else if (Move_To_Attack) {
						animation.CrossFade ("run");
						seeker.StartPath (transform.position, attackDestination, OnPathComplete);
						Move_To_Attack = false;	
						Attack_Anim = false;
						Idle_Anim = false;
						Run_Anim = true;
				} 
			
			
			counter += Time.deltaTime;
			if (Run_Anim) {
				animation.CrossFade ("run");
			} else if (Attack_Anim) {
				StartCoroutine("Attack1");
			} else if (Idle_Anim) {
				animation.CrossFade ("idle");
//				Debug.Log ("JUMP ANIM: " + timer);
			if(counter >= timer && Application.loadedLevel != 0){
				//Debug.Log ("JUMP ANIM: " + timer);
				   animation.CrossFade("jump");
				   counter = 0f;
				}

			}

		if ((kills >= level * 10) && level < maxlevel) {
						level++;
						hitPoints = (int)(hitPoints * 1.5);
						maxHitPoints = (int)(maxHitPoints * 1.5);
						damage = (int)(damage * 1.5);
				}
	}

	IEnumerator Jump(){
		animation.CrossFade ("jump");
		yield return new WaitForSeconds(animation.clip.length);
		}

	IEnumerator Run() {
		animation.CrossFade ("run");
		yield return new WaitForSeconds(animation.clip.length);
	}
	
	IEnumerator Attack1() {
		animation.CrossFade ("attack");
		yield return new WaitForSeconds(animation.clip.length);
		
		
	}
	
	IEnumerator Idle() {
		animation.CrossFade ("idle");
		yield return new WaitForSeconds(animation.clip.length);
	}

	//A* pathfinding
	protected void OnPathComplete(Path p)
	{
		if (!p.error) {
//			Debug.Log (tag + " is at destination");
			path = p;
			currentWaypoint = 0;
		}
	}

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
			}else{
				Attack_Anim = true;
				Idle_Anim = false;
				Run_Anim = false;
			}
			return;
		}
		
		if (!aiming) 
		{
			Vector3 dir = (path.vectorPath [currentWaypoint] - transform.position).normalized;
			dir *= moveSpeed * Time.fixedDeltaTime;
			GetComponent<Rigidbody> ().MovePosition (rigidbody.position + dir);
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
		Attack_Anim = true;
		Idle_Anim = false;
		Run_Anim = false;
		Vector3 spawnPoint = transform.position;
		spawnPoint.x += (2.1f * transform.forward.x);
		spawnPoint.y += 1.4f;
		spawnPoint.z += (2.1f * transform.forward.z);
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
	
	//AI
	public override void receiveNewLocation(Vector3 dest)
	{
		if (currentdest != dest) {
			seeker.StartPath (transform.position, dest, OnPathComplete);
			//animation.CrossFade ("run");
			Attack_Anim = false;
			Idle_Anim = false;
			Run_Anim = true;
			currentdest = dest;
			is_attacking = true;
		}
	}
	
	public override Vector3 getCurrentDestination()
	{
		return currentdest;
	}
}
