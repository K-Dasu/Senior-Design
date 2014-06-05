using UnityEngine;
using System.Collections;
using RTS;

public class UserInput : MonoBehaviour {
	private Player player;
	public AudioClip barracks,castle,goldmine,orc,bluegoblin,skullwarrior,demon;
	private float timer;
	private float counter;
	private static int counts = 0;
	private bool pause = false;
	// Use this for initialization
	void Start () {
		counter = 0f;
		timer = 5f;
		player = transform.root.GetComponent<Player> ();
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.P)) {
			Time.timeScale = 0;
			counts = 1;
			pause = true;
		}
		counter += Time.deltaTime;
		if (counter >= timer && Application.loadedLevel > 3) {
			//Time.timeScale = 0;
			counter = 0f;
		}
		if (player.human && Application.loadedLevel > 3 ) {
						MoveCamera ();
						RotateCamera ();
						MouseActivity ();
		} else if(Application.loadedLevel < 4 ) {
						MouseActivity ();
				}
	}

	void OnGUI(){
		if (counts == 1) {
			if (pause) {
				GUI.Box (new Rect (580, 100, 100, 180), "Pause");
				if (GUI.Button (new Rect (590, 120, 80, 20), "Quit")) {
						Application.Quit ();
				}
				if (GUI.Button (new Rect (590, 140, 80, 20), "Main Menu")) {
						pause = false;
						counts = 0;
						Time.timeScale = 1;
						AutoFade.LoadLevel (0, 1f, 1f, Color.black);
				}
				if (GUI.Button (new Rect (590, 160, 80, 20), "Resume")) {
						pause = false;
						counts = 0;
						Time.timeScale = 1;
				}

			}
		}
	}

	private void MoveCamera() {
		float xpos = Input.mousePosition.x;
		float ypos = Input.mousePosition.y;
		Vector3 movement = new Vector3(0,0,0);
		bool mouseScroll = false;

		//horizontal camera movement
		if((xpos >= 0 && xpos < ResourceManager.ScrollWidth) || Input.GetKey(KeyCode.A)) {
			movement.x -= ResourceManager.ScrollSpeed;
			if(xpos >= 0 && xpos < ResourceManager.ScrollWidth) player.hud.SetCursorState(CursorState.PanLeft);
			mouseScroll = true;
		} else if((xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) || Input.GetKey(KeyCode.D)) {
			movement.x += ResourceManager.ScrollSpeed;
			if(xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) player.hud.SetCursorState(CursorState.PanRight);
			mouseScroll = true;
		}
		
		//vertical camera movement
		if((ypos >= 0 && ypos < ResourceManager.ScrollWidth) || Input.GetKey(KeyCode.S)) {
			movement.z -= ResourceManager.ScrollSpeed;
			if(ypos >= 0 && ypos < ResourceManager.ScrollWidth) player.hud.SetCursorState(CursorState.PanDown);
			mouseScroll = true;
		} else if((ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth)  || Input.GetKey(KeyCode.W)){
			movement.z += ResourceManager.ScrollSpeed;
			if (ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth) player.hud.SetCursorState(CursorState.PanUp);
			mouseScroll = true;
		}
		
		//make sure movement is in the direction the camera is pointing
		//but ignore the vertical tilt of the camera to get sensible scrolling
		movement = Camera.main.transform.TransformDirection(movement);
		movement.y = 0;
		
		//away from ground movement
		movement.y -= ResourceManager.ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");
		
		//calculate desired camera position based on received input
		Vector3 origin = Camera.main.transform.position;
		Vector3 destination = origin;
		destination.x += movement.x;
		destination.y += movement.y;
		destination.z += movement.z;
		
		//limit away from ground movement to be between a minimum and maximum distance
		if(destination.y > ResourceManager.MaxCameraHeight) {
			destination.y = ResourceManager.MaxCameraHeight;
		} else if(destination.y < ResourceManager.MinCameraHeight) {
			destination.y = ResourceManager.MinCameraHeight;
		}
		
		//if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
		}

		if(!mouseScroll) {
			player.hud.SetCursorState(CursorState.Select);
		}

	}
	
	private void RotateCamera() {
		Vector3 origin = Camera.main.transform.eulerAngles;
		Vector3 destination = origin;
		
		//detect rotation amount if ALT is being held and the Right mouse button is down
		if((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetMouseButton(0)) {
			destination.x += Input.GetAxis("Mouse Y") * ResourceManager.RotateAmount;
			destination.y -= Input.GetAxis("Mouse X") * ResourceManager.RotateAmount;
		}
		
		//if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
		}
	}

	private void MouseActivity() {
		if(Input.GetMouseButtonDown(0)) LeftMouseClick();
		else if(Input.GetMouseButtonDown(1)) RightMouseClick();
		MouseHover();
	}
	
	private void LeftMouseClick() {
		if(player.hud.MouseInBounds()) {
			if(player.IsFindingBuildingLocation()) {
				if(player.CanPlaceBuilding()) player.StartConstruction();
			} else {
				GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
				Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
				if(hitObject && hitPoint != ResourceManager.InvalidPosition) {
					if(player.SelectedObject) player.SelectedObject.MouseClick(hitObject, hitPoint, player);
					else if(hitObject.name == "LowWall"){
						if(Application.loadedLevel == 0){  
							AutoFade.LoadLevel(4,1f,1f,Color.black);
						}else if(Application.loadedLevel == 1){
							AutoFade.LoadLevel(0,1f,1f,Color.black);
						}else if(Application.loadedLevel == 2){
							//Load The Next level..
						}
					}else if(hitObject.name == "Back"){
						AutoFade.LoadLevel(0,1f,1f,Color.black);
					}else if(hitObject.name == "LowWall2"){
						AutoFade.LoadLevel(3,1f,1f,Color.black);
					}else if(hitObject.name == "LowWall3"){
						Application.Quit();
					}
					else if(hitObject.name != "Ground" && !hitObject.name.Contains("Tree")&& !hitObject.name.Contains("Water")  && Application.loadedLevel > 3  ) {
						Debug.Log("UserInput child : " + hitObject.name);
						WorldObject worldObject = hitObject.transform.GetComponent<WorldObject>();
						//Allows models to recognize mouse input
						if(worldObject == null)
								worldObject = hitObject.transform.parent.GetComponent<WorldObject>();

						if(worldObject) {
							//Debug.Log("Name Of PARENT: " + hitObject.transform.parent.name);
							//we already know the player has no selected object
							Debug.Log("UserInput LC is WorldObject: " + hitObject.name);
							player.SelectedObject = worldObject;
							worldObject.SetSelection(true, player.hud.GetPlayingArea());
							if(worldObject.objectName == "Barracks")
								audio.PlayOneShot(barracks);
							if(worldObject.objectName == "Castle")
								audio.PlayOneShot(castle);
							if(worldObject.objectName == "Gold Mine"){
								Debug.Log ("GOLDMINE HERE");
								audio.PlayOneShot(goldmine);
							}
							if(worldObject.objectName == "Orc")
								audio.PlayOneShot(orc);
							if(worldObject.objectName == "Blue Goblin")
								audio.PlayOneShot(bluegoblin);
							if(worldObject.objectName == "SkullWarrior")
								audio.PlayOneShot(skullwarrior);
							if(worldObject.objectName == "Demon")
								audio.PlayOneShot(demon);
						}
					}
				}
			}
		}
	}

	private void RightMouseClick() {
		if(player.hud.MouseInBounds() && !Input.GetKey(KeyCode.LeftAlt) && player.SelectedObject) {
			if(player.IsFindingBuildingLocation()) {
				player.CancelBuildingPlacement();
			} else {
				player.SelectedObject.SetSelection(false, player.hud.GetPlayingArea());
				player.SelectedObject = null;
			}
		}
	}

	private void MouseHover() {
		if(player.hud.MouseInBounds()) {
			if(player.IsFindingBuildingLocation()) {
				player.FindBuildingLocation();
			}else{
				GameObject hoverObject = WorkManager.FindHitObject(Input.mousePosition);
			if(hoverObject) {
				if(player.SelectedObject) player.SelectedObject.SetHoverState(hoverObject);
				else if(hoverObject.name != "Ground") {
					Player owner = hoverObject.transform.root.GetComponent< Player >();
					if(owner) {
						Unit unit = hoverObject.transform.parent.GetComponent< Unit >();
						Building building = hoverObject.transform.parent.GetComponent< Building >();
						if(owner.username == player.username && (unit || building)) player.hud.SetCursorState(CursorState.Select);
					}
				}
			 }
			}
		}
	}



}
