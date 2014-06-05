using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class HUD : MonoBehaviour {
	public GUISkin resourceSkin, ordersSkin, selectBoxSkin, mouseCursorSkin;
	public Texture2D[] resources;
	public Texture2D activeCursor;
	public Texture2D selectCursor, upCursor, downCursor, leftCursor, rightCursor;
	public Texture2D[] moveCursors, attackCursors, harvestCursors;
	public Texture2D buttonHover, buttonClick;
	public Texture2D buildFrame, buildMask;
	public Texture2D smallButtonHover, smallButtonClick;
	public Texture2D rallyPointCursor;
	public Texture2D healthy, damaged, critical;
	public Texture2D[] resourceHealthBars;
	public Texture2D checkbox, checkedbox;
	private Player player;
	private CursorState activeCursorState;
	private CursorState previousCursorState;
	private int currentFrame = 0;
	private int buildAreaHeight = 0;
	private Dictionary< ResourceType, int > resourceValues, resourceLimits;
	private Dictionary< ResourceType, Texture2D > resourceImages;
	private WorldObject lastSelection;
	private float sliderValue;
	private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
	private const int SELECTION_NAME_HEIGHT = 15;
	private const int BUTTON_SPACING = 7;
	private const int SCROLL_BAR_WIDTH = 22;
	private const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
	private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64;
	private const int BUILD_IMAGE_PADDING = 8;
	private int buttonPlacement;
	
	
	// Use this for initialization
	void Start () {
		player = transform.root.GetComponent<Player> ();
		resourceValues = new Dictionary< ResourceType, int >();
		resourceLimits = new Dictionary< ResourceType, int >();
		ResourceManager.StoreSelectBoxItems(selectBoxSkin, healthy, damaged, critical);
		SetCursorState(CursorState.Select);
		
		buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;
		resourceImages = new Dictionary< ResourceType, Texture2D >();
		for(int i = 0; i < resources.Length; i++) {
			switch(resources[i].name) {
			case "Money":
				resourceImages.Add(ResourceType.Money, resources[i]);
				resourceValues.Add(ResourceType.Money, 0);
				resourceLimits.Add(ResourceType.Money, 0);
				break;
			case "Power":
				resourceImages.Add(ResourceType.Power, resources[i]);
				resourceValues.Add(ResourceType.Power, 0);
				resourceLimits.Add(ResourceType.Power, 0);
				break;
			default: break;
			}
		}
		Dictionary< ResourceType, Texture2D > resourceHealthBarTextures = new Dictionary< ResourceType, Texture2D >();
		for(int i = 0; i < resourceHealthBars.Length; i++) {
			switch(resourceHealthBars[i].name) {
			case "Ore":
				resourceHealthBarTextures.Add(ResourceType.Ore, resourceHealthBars[i]);
				break;
			default: break;
			}
		}
		ResourceManager.SetResourceHealthBarTextures(resourceHealthBarTextures);
	}
	
	void OnGUI () {
		if (player && player.human ) {
			if(Application.loadedLevel > 3  ){
						DrawOrdersBar ();
						DrawResourceBar ();
			}
						DrawMouseCursor ();
		} 
	}
	
	private void DrawActions(string[] actions) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = buttonHover;
		buttons.active.background = buttonClick;
		GUI.skin.button = buttons;
		int numActions = actions.Length;
		
		//define the area to draw the actions inside
		GUI.BeginGroup(new Rect(BUILD_IMAGE_WIDTH, buttonPlacement, ORDERS_BAR_WIDTH, buildAreaHeight));		
		//draw scroll bar for the list of actions if need be
		//if(numActions <= MaxNumRows(buildAreaHeight)) DrawSlider(buildAreaHeight, numActions / 2.0f);
		//display possible actions as buttons and handle the button click for each
		for(int i = 0; i < numActions; i++) {
			Rect pos = GetButtonPos(i, 0);
			pos.y += i * 15;
			Texture2D action = ResourceManager.GetBuildImage(actions[i]);
			if(action) {
				//create the button and handle the click of that button 
				GUI.Label(new Rect(pos.x, pos.y + BUILD_IMAGE_HEIGHT, ORDERS_BAR_WIDTH-20, SELECTION_NAME_HEIGHT), actions[i] + " $(" + ResourceManager.GetUnit(actions[i]).GetComponent<Unit>().cost + ")");
				if(GUI.Button(pos, action)) {
					if(player.SelectedObject) player.SelectedObject.PerformAction(actions[i]);
				}
			}
		}
		GUI.EndGroup();
	}
	
	private void DrawOrdersBar(){
		GUI.skin = ordersSkin;
		GUI.BeginGroup(new Rect(Screen.width - ORDERS_BAR_WIDTH - BUILD_IMAGE_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH + BUILD_IMAGE_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");
		string selectionName = "";
		
		if(player.SelectedObject) {
			selectionName = player.SelectedObject.objectName;
			
			if(player.SelectedObject.IsOwnedBy(player)){
				//reset slider value if the selected object has changed
				if(lastSelection && lastSelection != player.SelectedObject) 
					sliderValue = 0.0f;
				DrawActions(player.SelectedObject.GetActions());
				//store the current selection
				lastSelection = player.SelectedObject;
				Building selectedBuilding = lastSelection.GetComponent<Building>();
				if(selectedBuilding) {
					DrawBuildQueue(selectedBuilding.getBuildQueueValues(), selectedBuilding.getBuildPercentage());
					DrawStandardBuildingOptions(selectedBuilding);
				}

				Unit selectedUnit = lastSelection.GetComponent<Unit>();
				if(selectedUnit)
					DrawStandardUnitOptions(selectedUnit);
			}
			
			if(!selectionName.Equals("")) {
				int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH / 2;
				int topPos = 70;
				GUI.Label(new Rect(leftPos, 10, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), selectionName);
				GUI.Label(new Rect(leftPos, 30, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Level: " + player.SelectedObject.level);
				GUI.Label(new Rect(leftPos, 50, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Life: " + player.SelectedObject.hitPoints + "/" + player.SelectedObject.maxHitPoints);
				
				if(player.SelectedObject.objectName == "Gold Mine"){
					GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Generated: " + player.SelectedObject.GetComponent<GoldMine>().totalAmount);
					topPos += 20;
					GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Rate: " + player.SelectedObject.GetComponent<GoldMine>().productionRate);
					
				}
				if(player.SelectedObject.objectName == "Castle") {
					GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Generated: " + player.SelectedObject.GetComponent<Castle>().totalAmount);
					topPos += 20;
					GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Rate: " + player.SelectedObject.GetComponent<Castle>().productionRate);
					
				}
				if(player.SelectedObject.objectName == "Barracks") {
					if(player.SelectedObject.level >= 1) {
						GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Orc: " + ResourceManager.O);
						topPos += 20;
					}
					if(player.SelectedObject.level >= 2) {
						GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Blue Goblin: " + ResourceManager.B);
						topPos += 20;
					}
					if(player.SelectedObject.level >= 3) {
						GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Skull Warrior: " + ResourceManager.S);
						topPos += 20;
					}
					if(player.SelectedObject.level == 4) {
						GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Demon: " + ResourceManager.D);
						topPos += 20;
					}
				}
				if(player.SelectedObject.objectName == "Orc" || player.SelectedObject.objectName == "Blue Goblin" ||
				   player.SelectedObject.objectName == "Skull Warrior" || player.SelectedObject.objectName == "Demon" ) {
					GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Damage: " + player.SelectedObject.GetComponent<Unit>().damage);
					topPos += 20;
					GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Speed: " + player.SelectedObject.GetComponent<Unit>().moveSpeed);
					topPos += 20;
					GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), "Kills: " + player.SelectedObject.GetComponent<Unit>().kills);
				}
				buttonPlacement = topPos + 20;
			}
			
		}
		
		GUI.EndGroup();		
	}
	
	private void DrawResourceBar() {
		GUI.skin = resourceSkin;
		GUI.BeginGroup (new Rect (0, 0, Screen.width, RESOURCE_BAR_HEIGHT));
		GUI.Box (new Rect (0, 0, Screen.width, RESOURCE_BAR_HEIGHT), "");
		int topPos = 4, iconLeft = 4, textLeft = 20;
		GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), resourceImages[ResourceType.Money]);
		//Debug.Log (player.Money + "-" + resourceValues[ResourceType.Money]);
		GUI.Label (new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), "$" + player.Money);
		iconLeft = Screen.width / 3;
		int currentLevel = Application.loadedLevel - 4;
		if(ResourceManager.GetCompletions(currentLevel, 0))
			GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), checkedbox);
		else
			GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), checkbox);
		GUI.Label (new Rect(iconLeft + 10, topPos, TEXT_WIDTH + 100, TEXT_HEIGHT), ResourceManager.GetObjective(currentLevel, 0));
		//objective 2
		iconLeft = (Screen.width * 2) / 3;
		if(ResourceManager.GetCompletions(currentLevel, 1))
			GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), checkedbox);
		else
			GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), checkbox);
		GUI.Label (new Rect(iconLeft + 10, topPos, TEXT_WIDTH + 100, TEXT_HEIGHT), ResourceManager.GetObjective(currentLevel, 1));
		GUI.EndGroup ();
	}
	
	private void DrawBuildQueue(string[] buildQueue, float buildPercentage) {
		for(int i = 0; i < buildQueue.Length; i++) {
			float topPos = i * BUILD_IMAGE_HEIGHT + (i+1) * BUILD_IMAGE_PADDING;
			Rect buildPos = new Rect(BUILD_IMAGE_PADDING, topPos, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
			GUI.DrawTexture(buildPos, ResourceManager.GetBuildImage(buildQueue[i]));
			GUI.DrawTexture(buildPos, buildFrame);
			topPos += BUILD_IMAGE_PADDING;
			float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
			float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
			if(i==0) {
				//shrink the build mask on the item currently being built to give an idea of progress
				Debug.Log (buildPercentage);
				topPos += height * buildPercentage;
				height *= (1 - buildPercentage);
			}
			GUI.DrawTexture(new Rect(2 * BUILD_IMAGE_PADDING, topPos, width, height), buildMask);
		}
	}
	
	private void DrawStandardBuildingOptions(Building building) {
		GUIStyle buttons = new GUIStyle ();
		buttons.hover.background = smallButtonHover;
		buttons.active.background = smallButtonClick;
		GUI.skin.button = buttons;
		int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + BUTTON_SPACING;
		int topPos = buildAreaHeight - BUILD_IMAGE_HEIGHT / 2;
		int width = BUILD_IMAGE_WIDTH / 2;
		int height = BUILD_IMAGE_HEIGHT / 2;
		if (!building.name.Equals ("Castle") && GUI.Button (new Rect (leftPos, topPos, width, height), building.sellImage))
			building.Sell ();
		if (building.hasSpawnPoint ()) {
			leftPos += width + BUTTON_SPACING;
		}
	}

	private void DrawStandardUnitOptions(Unit unit) {
		GUIStyle buttons = new GUIStyle ();
		buttons.hover.background = smallButtonHover;
		buttons.active.background = smallButtonClick;
		GUI.skin.button = buttons;

		int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + BUTTON_SPACING;
		int topPos = buildAreaHeight - BUILD_IMAGE_HEIGHT / 2;
		int width = BUILD_IMAGE_WIDTH / 2;
		int height = BUILD_IMAGE_HEIGHT / 2;

		if (GUI.Button (new Rect (leftPos, topPos, width, height), unit.sellImage))
						unit.Sell ();
	}

	public bool MouseInBounds() {
		Vector3 mousePos = Input.mousePosition;
		bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
		bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
		if (Application.loadedLevel < 4 )
						return true;
		return insideWidth && insideHeight;
	}
	
	public Rect GetPlayingArea() {
		return new Rect (0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
	}
	
	private void DrawMouseCursor() {
		bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;
		if (mouseOverHud ) {
			Screen.showCursor = true;
		} 
		else 
		{
			Screen.showCursor = false;
			if(!player.IsFindingBuildingLocation()) 
			{
				GUI.skin = mouseCursorSkin;
				GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
				UpdateCursorAnimation();
				Rect cursorPosition = GetCursorDrawPosition();
				GUI.Label(cursorPosition, activeCursor);
				GUI.EndGroup();
			}
		}
	}
	
	private void UpdateCursorAnimation() {
		if (activeCursorState == CursorState.Move) {
			currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors [currentFrame];
		} 
		else if (activeCursorState == CursorState.Attack) {
			currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors [currentFrame];
		} 
		else if (activeCursorState == CursorState.Harvest) {
			currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors [currentFrame];
		}
	}
	
	private Rect GetCursorDrawPosition() {
		//set base position for custom cursor image
		float leftPos = Input.mousePosition.x;
		float topPos = Screen.height - Input.mousePosition.y; //screen draw coordinates are inverted
		//adjust position base on the type of cursor being shown
		if(activeCursorState == CursorState.PanRight) 
			leftPos = Screen.width - activeCursor.width;
		else if(activeCursorState == CursorState.PanDown) 
			topPos = Screen.height - activeCursor.height;
		else if(activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
			topPos -= activeCursor.height / 2;
			leftPos -= activeCursor.width / 2;
		}
		else if(activeCursorState == CursorState.RallyPoint) 
			topPos -= activeCursor.height;
		return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
	}
	
	public void SetCursorState(CursorState newState) {
		if(activeCursorState != newState) 
			previousCursorState = activeCursorState;
		activeCursorState = newState;
		switch(newState) {
		case CursorState.Select:
			activeCursor = selectCursor;
			break;
		case CursorState.Attack:
			currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors[currentFrame];
			break;
		case CursorState.Harvest:
			currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors[currentFrame];
			break;
		case CursorState.Move:
			currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors[currentFrame];
			break;
		case CursorState.PanLeft:
			activeCursor = leftCursor;
			break;
		case CursorState.PanRight:
			activeCursor = rightCursor;
			break;
		case CursorState.PanUp:
			activeCursor = upCursor;
			break;
		case CursorState.PanDown:
			activeCursor = downCursor;
			break;
		case CursorState.RallyPoint:
			activeCursor = rallyPointCursor;
			break;
		default: break;
		}
	}
	
	public void SetResourceValues(Dictionary< ResourceType, int > resourceValues, Dictionary< ResourceType, int > resourceLimits) {
		this.resourceValues = resourceValues;
		this.resourceLimits = resourceLimits;
	}
	
	private int MaxNumRows(int areaHeight) {
		return areaHeight / BUILD_IMAGE_HEIGHT;
	}
	
	private Rect GetButtonPos(int row, int column) {
		int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
		float top = row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
		return new Rect(left, top, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
	}
	
	public CursorState GetPreviousCursorState() {
		return previousCursorState;
	}
	
	public CursorState GetCursorState() {
		return activeCursorState;
	}
	
	/*private void DrawStandardUnitOptions(Unit unit) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = smallButtonHover;
		buttons.active.background = smallButtonClick;
		GUI.skin.button = buttons;
		int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + BUTTON_SPACING;
		int topPos = buildAreaHeight - BUILD_IMAGE_HEIGHT / 2;
		int width = BUILD_IMAGE_WIDTH / 2;
		int height = BUILD_IMAGE_HEIGHT / 2;
		if(player.human) {
			if(!unit.name.Equals("Worker") && GUI.Button(new Rect(leftPos, topPos, width, height), unit.sellImage))
				unit.Sell();
			leftPos += width + BUTTON_SPACING;
		}
	}*/
}
