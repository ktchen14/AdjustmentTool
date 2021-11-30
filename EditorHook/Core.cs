using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;

namespace AdjustmentTool {
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  [KSPAddon(KSPAddon.Startup.EditorVAB, false)]
  public partial class EditorHook : MonoBehaviour {
    private EditorLogic editor;
    private AdjustmentTool adjustmentTool;

    [RemoteMember("fsm")] private KerbalFSM efsm;
    [RemoteMember] private KFSMState st_idle;
    [RemoteMember] private KFSMState st_place;
    [RemoteMember] private KFSMState st_offset_select;
    [RemoteMember] private KFSMState st_offset_tweak;
    [RemoteMember] private KFSMState st_rotate_select;
    [RemoteMember] private KFSMState st_rotate_tweak;
    [RemoteMember] private KFSMState st_root_unselected;
    [RemoteMember] private KFSMState st_root_select;
    [RemoteMember] private KFSMEvent on_podDeleted;
    [RemoteMember] private KFSMEvent on_partCreated;
    [RemoteMember] private KFSMEvent on_partPicked;
    [RemoteMember] private KFSMEvent on_partDeleted;
    [RemoteMember] private KFSMEvent on_partOverInventoryPAW;
    [RemoteMember] private KFSMEvent on_goToModeOffset;
    [RemoteMember] private KFSMEvent on_goToModeRotate;
    [RemoteMember] private KFSMEvent on_goToModeRoot;
    [RemoteMember] private KFSMEvent on_goToModePlace;
    [RemoteMember] private KFSMEvent on_undoRedo;
    [RemoteMember] private KFSMEvent on_newShip;
    [RemoteMember] private KFSMEvent on_shipLoaded;

    [RemoteMember] private KFSMCallback snapInputUpdate;
    [RemoteMember] private KFSMCallback deleteInputUpdate;
    [RemoteMember] private KFSMCallback partSearchUpdate;

    private delegate Part PickPart(
      int layerMask, bool pickRoot, bool pickRootIfFrozen);
    [RemoteMember] private PickPart pickPart;

    private Action<Part> selectedPartSetter;
    private Part SelectedPart {
      get => EditorLogic.SelectedPart;
      set => selectedPartSetter(value);
    }

    private KFSMState st_adjust_select;
    private KFSMState st_adjust_active;
    private KFSMEvent on_goToModeAdjust;
    private KFSMEvent on_adjustSelect;
    private KFSMEvent on_adjustDeselect;

    private AudioSource audioSource;

    private void Awake() => editor = EditorLogic.fetch;

    private void Start() => Initialize();

    /// Hook the AdjustmentTool code into the EditorLogic instance
    private void HookEditor(EditorLogic editor) {
      RemoteMemberAttribute.Load(this, editor);

      const BindingFlags search = RemoteMemberAttribute.Search;
      var type = typeof(EditorLogic);
      (FieldInfo f, PropertyInfo p) info;
      if ((info.p = type.GetProperty("selectedPart", search)) != null) {
        if (!typeof(Part).IsAssignableFrom(info.p.PropertyType))
          throw new Exception("Can't hook into EditorLogic.selectedPart");
        if (!info.p.CanRead || !info.p.CanWrite)
          throw new Exception("Can't hook into EditorLogic.selectedPart");
        selectedPartSetter = part => info.p.SetValue(editor, part);
      } else if ((info.f = type.GetField("selectedPart", search)) != null) {
        if (!typeof(Part).IsAssignableFrom(info.f.FieldType))
          throw new Exception("Can't hook into EditorLogic.selectedPart");
        selectedPartSetter = part => info.f.SetValue(editor, part);
      } else throw new Exception("Can't hook into EditorLogic.selectedPart");

      st_adjust_select = new KFSMState("st_adjust_select");
      InitializeAdjustSelect(st_adjust_select);
      st_adjust_select.OnUpdate += editor.UndoRedoInputUpdate;
      st_adjust_select.OnUpdate += snapInputUpdate;
      st_adjust_select.OnUpdate += partSearchUpdate;
      efsm.AddState(st_adjust_select);

      st_adjust_active = new KFSMState("st_adjust_active");
      InitializeAdjustActive(st_adjust_active);
      st_adjust_active.OnUpdate += editor.UndoRedoInputUpdate;
      st_adjust_active.OnUpdate += snapInputUpdate;
      st_adjust_active.OnUpdate += deleteInputUpdate;
      st_adjust_active.OnUpdate += partSearchUpdate;
      efsm.AddState(st_adjust_active);

      on_goToModeAdjust = new KFSMEvent("on_goToModeAdjust");
      InitializeOn_goToModeAdjust(on_goToModeAdjust);
      efsm.AddEvent(on_goToModeAdjust, st_idle,
        st_offset_select, st_offset_tweak,
        st_rotate_select, st_rotate_tweak,
        st_root_unselected, st_root_select);

      on_adjustSelect = new KFSMEvent("on_adjustSelect");
      InitializeOn_AdjustSelect(on_adjustSelect);
      efsm.AddEvent(on_adjustSelect, st_adjust_select);

      on_adjustDeselect = new KFSMEvent("on_adjustDeselect");
      InitializeOn_AdjustDeselect(on_adjustDeselect);
      efsm.AddEvent(on_adjustDeselect, st_adjust_active);

      efsm.AddEvent(on_podDeleted, st_adjust_select, st_adjust_active);
      efsm.AddEvent(on_partCreated, st_adjust_select, st_adjust_active);
      efsm.AddEvent(on_partDeleted, st_adjust_active);
      efsm.AddEvent(
        on_partOverInventoryPAW, st_adjust_select, st_adjust_active);
      efsm.AddEvent(on_goToModeOffset, st_adjust_select, st_adjust_active);
      efsm.AddEvent(on_goToModeRotate, st_adjust_select, st_adjust_active);
      efsm.AddEvent(on_goToModeRoot, st_adjust_select, st_adjust_active);
      efsm.AddEvent(on_goToModePlace, st_adjust_select, st_adjust_active);
      efsm.AddEvent(on_undoRedo, st_adjust_active);
      efsm.AddEvent(on_newShip, st_adjust_select, st_adjust_active);
      efsm.AddEvent(on_shipLoaded, st_adjust_select, st_adjust_active);
    }

    // Whether the part is adjustable (surface attached)
    private static bool isPartAdjustable(Part part)
      => part.srfAttachNode != null && part.srfAttachNode.attachedPart != null;

    // Call EditorLogic.pickPart. The layer mask is inscrutable so we'll just
    // duplicate what's used by the Offset tool. Also don't pick the root part
    // on Left Shift.
    private Part choosePart()
      => pickPart(EditorLogic.LayerMask | (1 << 2) | (1 << 21), false, false);
  }
}
