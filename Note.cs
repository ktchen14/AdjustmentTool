// Layer 11 = UIDialog

// Axis X
// NormalColor = 253, 47, 0, 200
// DisabledColor = 125, 75, 63, 200
// HoverColor = 255, 87, 49, 255
// DownColor = 254, 87, 49, 255

// Axis Y
// NormalColor = 0, 180, 0, 200
// DisabledColor = 90, 109, 90, 200
// HoverColor = 0, 219, 0, 255
// DownColor = 0, 218, 0, 255

// Axis Z
// NormalColor = 0, 163, 253, 200
// DisabledColor = 73, 123, 150, 200
// HoverColor = 60, 185, 255, 255
// DownColor = 60, 184, 254, 255

// ToolMove is Layer 5 = UI
// LocalPosition = (330, -60, 0)
// Tooltip Prefab is Tooltip_Text

this.st_podSelect = new KFSMState("st_podSelect");
this.fsm.AddState(this.st_podSelect);

this.st_idle = new KFSMState("st_idle");
this.fsm.AddState(this.st_idle);

this.st_place = new KFSMState("st_place");
this.fsm.AddState(this.st_place);

this.st_offset_select = new KFSMState("st_offset_select");
this.fsm.AddState(this.st_offset_select);

this.st_offset_tweak = new KFSMState("st_offset_tweak");
this.fsm.AddState(this.st_offset_tweak);

this.st_rotate_select = new KFSMState("st_rotate_select");
this.fsm.AddState(this.st_rotate_select);

this.st_rotate_tweak = new KFSMState("st_rotate_tweak");
// this.st_rotate_tweak.OnLeave = if (to != this.st_offset_tweak)
this.fsm.AddState(this.st_rotate_tweak);

this.st_root_unselected = new KFSMState("st_root_unselected");
this.fsm.AddState(this.st_root_unselected);

this.st_root_select = new KFSMState("st_root_select");
// this.st_root_select.OnLeave = else if (to == this.st_offset_tweak)
this.fsm.AddState(this.st_root_select);

this.on_podSelect = new KFSMEvent("on_podSelect");
this.fsm.AddEvent(this.on_podSelect, this.st_podSelect);

this.on_podDeleted = new KFSMEvent("on_podDeleted");
this.fsm.AddEventExcluding(this.on_podDeleted);

this.on_partCreated = new KFSMEvent("on_partCreated");
this.fsm.AddEventExcluding(this.on_partCreated, this.st_place);

this.on_partPicked = new KFSMEvent("on_partPicked");
this.fsm.AddEvent(this.on_partPicked, this.st_idle);

this.on_partCopied = new KFSMEvent("on_partCopied");
this.fsm.AddEvent(this.on_partCopied, this.st_idle);

this.on_partReveal = new KFSMEvent("on_partReveal");
this.fsm.AddEvent(this.on_partReveal, this.st_idle);

this.on_partDropped = new KFSMEvent("on_partDropped");
this.fsm.AddEvent(this.on_partDropped, this.st_place);

this.on_partAttached = new KFSMEvent("on_partAttached");
this.fsm.AddEvent(this.on_partAttached, this.st_place);

this.on_partDeleted = new KFSMEvent("on_partDeleted");
this.fsm.AddEvent(this.on_partDeleted, this.st_place, this.st_offset_tweak, this.st_rotate_tweak);

this.on_partLost = new KFSMEvent("on_partLost");
this.fsm.AddEvent(this.on_partLost, this.st_place);

this.on_partOverInventoryPAW = new KFSMEvent("on_partOverInventoryPAW");
this.fsm.AddEventExcluding(this.on_partOverInventoryPAW);

this.on_goToModeOffset = new KFSMEvent("on_goToModeOffset");
this.fsm.AddEvent(this.on_goToModeOffset, this.st_idle, this.st_rotate_select, this.st_rotate_tweak, this.st_root_unselected, this.st_root_select);

this.on_offsetSelect = new KFSMEvent("on_offsetSelect");
this.fsm.AddEvent(this.on_offsetSelect, this.st_offset_select);

this.on_offsetDeselect = new KFSMEvent("on_offsetDeselect");
this.fsm.AddEvent(this.on_offsetDeselect, this.st_offset_tweak);

this.on_offsetReset = new KFSMEvent("on_offsetReset");
this.fsm.AddEvent(this.on_offsetReset, this.st_offset_tweak);

this.on_goToModeRotate = new KFSMEvent("on_goToModeRotate");
this.fsm.AddEvent(this.on_goToModeRotate, this.st_idle, this.st_offset_select, this.st_offset_tweak, this.st_root_unselected, this.st_root_select);

this.on_rotateSelect = new KFSMEvent("on_rotateSelect");
this.fsm.AddEvent(this.on_rotateSelect, this.st_rotate_select);

this.on_rotateDeselect = new KFSMEvent("on_rotateDeselect");
this.fsm.AddEvent(this.on_rotateDeselect, this.st_rotate_tweak);

this.on_rotateReset = new KFSMEvent("on_rotateReset");
this.fsm.AddEvent(this.on_rotateReset, this.st_rotate_tweak);

this.on_goToModeRoot = new KFSMEvent("on_goToModeRoot");
// this.on_goToModeRoot.OnEvent = if (this.fsm.CurrentState != this.st_offset_tweak)
this.fsm.AddEventExcluding(this.on_goToModeRoot, this.st_root_select, this.st_root_unselected, this.st_podSelect);

this.on_rootPickSet = new KFSMEvent("on_rootPickSet");
this.fsm.AddEvent(this.on_rootPickSet, this.st_root_unselected);

this.on_rootDeselect = new KFSMEvent("on_rootDeselect");
this.fsm.AddEvent(this.on_rootDeselect, this.st_root_select);

this.on_rootSelect = new KFSMEvent("on_rootSelect");
this.fsm.AddEvent(this.on_rootSelect, this.st_root_select);

this.on_rootSelectFail = new KFSMEvent("on_rootSelectFail");
this.fsm.AddEvent(this.on_rootSelectFail, this.st_root_select);

this.on_goToModePlace = new KFSMEvent("on_goToModePlace");
this.fsm.AddEvent(this.on_goToModePlace, this.st_offset_select, this.st_offset_tweak, this.st_rotate_select, this.st_rotate_tweak, this.st_root_unselected, this.st_root_select);

this.on_undoRedo = new KFSMEvent("on_undoRedo");
// this.on_undoRedo.OnEvent = if (this.fsm.CurrentState != this.st_offset_tweak)
this.fsm.AddEvent(this.on_undoRedo, this.st_offset_tweak, this.st_rotate_tweak);

this.on_newShip = new KFSMEvent("on_newShip");
this.fsm.AddEventExcluding(this.on_newShip);

this.on_shipLoaded = new KFSMEvent("on_shipLoaded");
this.fsm.AddEventExcluding(this.on_shipLoaded);
