using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Building : WorldObject {
	public float maxBuildProgress;
	protected Queue< string > buildQueue;
	private float currentBuildProgress = 0.0f;
	private Vector3 spawnPoint;
	public AudioClip goldSound;

	private bool needsBuilding = false;

	//Intializing the spawn point to be in the middle of the front wall of the building set outside a bit
	protected override void Awake() {
		base.Awake();
		buildQueue = new Queue< string >();
		float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
		float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * 10;
		spawnPoint = new Vector3(spawnX , 0.0f, spawnZ);
	}
	
	protected override void Start () {
		base.Start();
	}
	
	protected override void Update () {
		base.Update();
		ProcessBuildQueue();
	}

	public override bool isBuilding() {
		//default behaviour needs to be overidden by children
		return true;
	}

	protected override void OnGUI() {
		base.OnGUI();
		if(needsBuilding) DrawBuildProgress();
	}

	protected void CreateUnit(string unitName) {
		if (buildQueue.Count < 4) {
			player.AddResource(ResourceType.Money, -1 * ResourceManager.GetUnit (unitName).GetComponent<Unit> ().cost);
			if (unitName == "Orc")
				ResourceManager.O++;
			else if (unitName == "Blue Goblin")
				ResourceManager.B++;
			else if (unitName == "Skull Warrior")
				ResourceManager.S++;
			else if (unitName == "Demon")
				ResourceManager.D++;
			buildQueue.Enqueue (unitName);
		}
	}

	private int buildCounterX = 0;
	private int buildCounterZ = 0;
	protected void ProcessBuildQueue() {
		if(buildQueue.Count > 0) {
			maxBuildProgress = ResourceManager.GetUnit(buildQueue.Peek()).GetComponent<Unit>().buildTime;
			currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
			if(currentBuildProgress > maxBuildProgress ){
				if(player){
					spawnPoint.x += 4;
					buildCounterX += 1;
					if(buildCounterX > 5) {spawnPoint.z += 10; spawnPoint.x -= 20; buildCounterX = 0; buildCounterZ += 1;}
					if(buildCounterZ > 3) {spawnPoint.z -= 30; buildCounterZ = 0;}
					player.AddUnit(buildQueue.Dequeue(), spawnPoint, transform.rotation, this);
				}
				currentBuildProgress = 0.0f;
			}
		}
	}

	public string[] getBuildQueueValues() {
		string[] values = new string[buildQueue.Count];
		int pos=0;
		foreach(string unit in buildQueue) values[pos++] = unit;
		return values;
	}
	
	public float getBuildPercentage() {
		return currentBuildProgress / maxBuildProgress;
	}

	public override void SetSelection(bool selected, Rect playingArea) {
		base.SetSelection(selected, playingArea);
	}

	public override void SetHoverState(GameObject hoverObject) {
		base.SetHoverState(hoverObject);
		//only handle input if owned by a human player and currently selected
		if(player && player.human && currentlySelected) {
			if(hoverObject.name == "Ground") {
				if(player.hud.GetPreviousCursorState() == CursorState.RallyPoint) player.hud.SetCursorState(CursorState.RallyPoint);
			}
		}
	}

	public void Sell() {
		if(player) player.AddResource(ResourceType.Money, sellValue);
		if(currentlySelected) SetSelection(false, playingArea);
		Destroy(this.gameObject);
	}

	public bool hasSpawnPoint() {
		return spawnPoint != ResourceManager.InvalidPosition;
	}

	public void StartConstruction() {
		CalculateBounds();
		needsBuilding = true;
		hitPoints = 0;
	}

	public bool UnderConstruction() {
		return needsBuilding;
	}
	
	public void Construct(int amount) {
		hitPoints += amount;
		if(hitPoints >= maxHitPoints) {
			hitPoints = maxHitPoints;
			needsBuilding = false;
			RestoreMaterials();
			SetTeamColor();
		}
	}


	private void DrawBuildProgress() {
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		//Draw the selection box around the currently selected object, within the bounds of the main draw area
		GUI.BeginGroup(playingArea);
		CalculateCurrentHealth(0.5f, 0.99f);
		DrawHealthBar(selectBox, "Building ...");
		GUI.EndGroup();
	}

}
