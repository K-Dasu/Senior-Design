using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Player : MonoBehaviour {
	public string username;
	public int startMoney, startMoneyLimit, Money;
	public bool human;
	public HUD hud;
	public WorldObject SelectedObject { get; set; }
	public Material notAllowedMaterial, allowedMaterial;
	public Color teamColor;
	
	private Building tempBuilding;
	private Unit tempCreator;
	private bool findingPlacement = false;
	private Dictionary< ResourceType, int > resources, resourceLimits;
	public List<Unit> Units;
	public List<Building> buildings;

	public Swordsman swordsman;
	
	Castle playercastle;
	Castle enemycastle;

	//AI
	List<Unit> p1units = new List<Unit> ();
	List<Unit> p2units = new List<Unit> ();

	
	// Use this for initialization
	void Start () {
		hud = GetComponentInChildren< HUD > ();
		AddStartResourceLimits();
		AddStartResources();
		Money = startMoney;
	}
	
	void Awake() {
		resources = InitResourceList();
		resourceLimits = InitResourceList();
		Castle [] c = FindObjectsOfType<Castle>();
		for (int i = 0; i < c.Length; i++) {
						if (c [i].tag == "P1Castle")
								playercastle = c [i];
						else if (c [i].tag == "P2Castle") {
								enemycastle = c [i];
						}
				}

	}
	
	float timer_getnewunits = 0.0f;
	float timer_findallunits = Mathf.Infinity;
	// Update is called once per frame
	void Update () 
	{
//		Debug.Log (Application.loadedLevel);
		if (Application.loadedLevel > 4 ) {
//						Debug.Log ("CURRENT LEVEL IS : " + Application.loadedLevel);
						if (playercastle.hitPoints <= 0)
								AutoFade.LoadLevel(1,1f,1f,Color.black);
						else if (playercastle.hitPoints <= 0)
								Debug.Log ("Schmeh i guess its a win");
						if (human) {
								hud.SetResourceValues (resources, resourceLimits);
								if (findingPlacement) {
										tempBuilding.CalculateBounds ();
										if (CanPlaceBuilding ())
												tempBuilding.SetTransparentMaterial (allowedMaterial, false);
										else
												tempBuilding.SetTransparentMaterial (notAllowedMaterial, false);
								}
						} else {
								timer_getnewunits += Time.deltaTime;
								if (timer_getnewunits > 30.0f) {
										timer_getnewunits = 0.0f;
										for (int i = 0; i < 1; ++i) {
												Barracks[] b = FindObjectsOfType<Barracks> ();
												Barracks bar;
												if (b [0].IsOwnedBy (this)) 
														bar = b [0];
												else
														bar = b [1];
												Vector3 pos = new Vector3 (bar.transform.position.x, 0, bar.transform.position.z);
												AddUnit ("Swordsman", pos, Quaternion.identity);
										}
								}
			
								timer_findallunits += Time.deltaTime;
								if (timer_findallunits > 1.0f) {
										timer_findallunits = 0.0f;
										findAllUnits ();
				
										//
										//
										//AI  //remove this
										//if(p1units.Count == 0)
										//return;
										//
										//
				
										for (int i = 0; i < p2units.Count; ++i) {
												if (!p2units [i].getHasAssignment ()) {
														NewChoice:
														int choice = Random.Range (0, 3);
														if (choice == 0) {
																if (!p2units [i].getIsAttacking ()) {
																		findCastles ();
																		targetCastle (i);
																		p2units [i].setHasAssignment (true);
																		p2units [i].setAssignment ("castle");
																		break;
																}
														} else if (choice == 1) {
																if (p1units.Count == 0)
																		goto NewChoice;
																targetAUnit (i);
																p2units [i].setHasAssignment (true);
																p2units [i].setAssignment ("unit");
														} else if (choice == 2) {
																findCastles ();
																p2units [i].setHasAssignment (true);
																p2units [i].setAssignment ("moving to own castle");
																moveToOwnCastle (i);
														}
												} else {
														if (p2units [i].getAssignment () == "unit") {
																if (p1units.Count > 0) {
																		targetAUnit (i);
																} else if (p1units.Count == 0) {
																		p2units [i].setAssignment (null);
																		p2units [i].setHasAssignment (false);
																}
														} else if (p2units [i].getAssignment () == "moving to own castle") {
																if ((p2units [i].getCurrentDestination () - p2units [i].transform.position).magnitude < 3.0f) {
																		p2units [i].setAssignment ("defending castle");
																}	
														} else if (p2units [i].getAssignment () == "defending castle") {
																findNearbyEnemy (i);
														}	
												}
										}
								} 
						}
				}
	}

	void findAllUnits()
	{
		p1units.Clear ();
		p2units.Clear ();
		Unit[] allunits = FindObjectsOfType<Unit> ();
		for (int i = 0; i < allunits.Length; ++i) 
		{
			if(allunits[i].tag == "Swordsman")
				p2units.Add (allunits[i]);
			else if( allunits[i].tag == "Orc" || allunits[i].tag == "Demon" || allunits[i].tag == "BlueGoblin" || allunits[i].tag == "SkullWarrior")
				p1units.Add (allunits[i]);
		}
	}
	
	void targetCastle(int index)
	{
		p2units[index].BeginAttack (enemycastle);
	}
	
	void moveToOwnCastle(int index)
	{
		Vector3 tmp = playercastle.transform.position;
		tmp.x -= 20;
		p2units[index].receiveNewLocation(tmp);
	}

	void findCastles()
	{
		Castle[] tmp = FindObjectsOfType<Castle>();
		for(int i = 0; i < tmp.Length; ++i)
		{
			if (tag == "Player1") 
			{	
				if( tmp[i].tag == "P1Castle")
					playercastle = tmp[i];
				else if(tmp[i].tag == "P2Castle")
					enemycastle = tmp[i];
			}
			else if(tag == "Player2")
			{
				if( tmp[i].tag == "P2Castle")
					playercastle = tmp[i];
				else if(tmp[i].tag == "P1Castle")
					enemycastle = tmp[i];
			}
		}
	}
	

	void findNearbyEnemy(int index)
	{
		double closest = Mathf.Infinity;
		Vector3 closest_dest = p2units[index].transform.position;
		for(int i = 0; i < p1units.Count; ++i)
		{
			if( p1units[i].path != null)
			{
				int c = p1units[i].path.vectorPath.Count;
				float dist = (playercastle.transform.position - p1units[i].path.vectorPath[c-1]).magnitude;
				Debug.Log ("dist " + dist);
				if(dist < 50.0f && dist < closest)
				{
					closest = dist;
					closest_dest = p1units[i].path.vectorPath[c-1];
				}
			}
			else 
			{
				float dist = (playercastle.transform.position -p1units[i].transform.position).magnitude;
				Debug.Log ("dist " + dist);
				if(dist < 50.0f)
				{
					closest = dist;
					Debug.Log ("closets " + closest);
					closest_dest = p1units[i].transform.position;
				}
			}	
		}
		p2units[index].receiveNewLocation (closest_dest);
	}

	void targetAUnit(int index)
	{
		double closest = Mathf.Infinity;
		Vector3 closest_dest = new Vector3 (0, 0, 0);
		int closest_index = 0;
		for(int i = 0; i < p1units.Count; ++i)
		{
			if( p1units[i].path != null)
			{
				int c = p1units[i].path.vectorPath.Count;
				double dist = (p2units[index].transform.position - p1units[i].path.vectorPath[c-1]).magnitude;
				if(dist < closest)
				{
					closest = dist;
					closest_dest = p1units[i].path.vectorPath[c-1];
					closest_index = i;
				}
			}
			else 
			{
				double dist = (p2units[index].transform.position -p1units[i].transform.position).magnitude;
				if(dist < closest)
				{
					closest = dist;
					closest_dest = p1units[i].transform.position;
					closest_index = 0;
				}
			}	
		}
		p2units[index].BeginAttack(p1units[closest_index]);	
	}
	

	
	private Dictionary< ResourceType, int > InitResourceList() {
		Dictionary< ResourceType, int > list = new Dictionary< ResourceType, int >();
		list.Add(ResourceType.Money, 0);
		list.Add(ResourceType.Power, 0);
		return list;
	}
	
	private void AddStartResourceLimits() {
		IncrementResourceLimit(ResourceType.Money, startMoneyLimit);
	}
	
	private void AddStartResources() {
		AddResource(ResourceType.Money, startMoney);
	}
	
	public void AddResource(ResourceType type, int amount) {
		resources[type] += amount;
		Money += amount;
	}
	
	public void IncrementResourceLimit(ResourceType type, int amount) {
		resourceLimits[type] += amount;
	}
	
	public void AddUnit(string unitName, Vector3 spawnPoint, Quaternion rotation) {
		Units units = GetComponentInChildren<Units>();
		GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName),spawnPoint, rotation);
		newUnit.transform.parent = units.transform;
	}

	public void AddUnit(string unitName, Vector3 spawnPoint, Quaternion rotation, Building creator) {
		Units units = GetComponentInChildren<Units>();
		//Debug.Log ("Spawn Point: " + spawnPoint.x + " , " + spawnPoint.y + " , " + spawnPoint.z);
		GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName),spawnPoint, rotation);
		newUnit.transform.parent = units.transform;
		Unit unitObject = newUnit.GetComponent<Unit>();
		if (unitObject) {
			unitObject.SetBuilding(creator);
		}
	}
	
	public void CreateBuilding(string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea) {
		GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(buildingName), buildPoint, new Quaternion());
		tempBuilding = newBuilding.GetComponent< Building >();
		if (tempBuilding) {
			tempCreator = creator;
			findingPlacement = true;
			tempBuilding.SetTransparentMaterial(notAllowedMaterial, true);
			tempBuilding.SetColliders(false);
			tempBuilding.SetPlayingArea(playingArea);
		} else Destroy(newBuilding);
	}
	
	public bool IsFindingBuildingLocation() {
		return findingPlacement;
	}
	
	public void FindBuildingLocation() {
		Vector3 newLocation = WorkManager.FindHitPoint(Input.mousePosition);
		newLocation.y = 0;
		tempBuilding.transform.position = newLocation;
	}
	
	public bool CanPlaceBuilding() {
		bool canPlace = true;
		
		Bounds placeBounds = tempBuilding.GetSelectionBounds();
		//shorthand for the coordinates of the center of the selection bounds
		float cx = placeBounds.center.x;
		float cy = placeBounds.center.y;
		float cz = placeBounds.center.z;
		//shorthand for the coordinates of the extents of the selection box
		float ex = placeBounds.extents.x;
		float ey = placeBounds.extents.y;
		float ez = placeBounds.extents.z;
		
		//Determine the screen coordinates for the corners of the selection bounds
		List<Vector3> corners = new List<Vector3>();
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy+ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy+ey,cz-ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy-ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy+ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy-ey,cz-ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy-ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy+ey,cz-ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy-ey,cz-ez)));
		
		foreach(Vector3 corner in corners) {
			GameObject hitObject = WorkManager.FindHitObject(corner);
			if(hitObject && hitObject.name != "Ground") {
				WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
				if(worldObject && placeBounds.Intersects(worldObject.GetSelectionBounds())) canPlace = false;
			}
		}
		return canPlace;
	}
	
	public void StartConstruction() {
		findingPlacement = false;
		Buildings buildings = GetComponentInChildren< Buildings >();
		if(buildings) tempBuilding.transform.parent = buildings.transform;
		tempBuilding.SetPlayer();
		tempBuilding.SetColliders(true);
		tempCreator.SetBuilding(tempBuilding);
		tempBuilding.StartConstruction();
	}
	
	public void CancelBuildingPlacement() {
		findingPlacement = false;
		Destroy(tempBuilding.gameObject);
		tempBuilding = null;
		tempCreator = null;
	}
}
