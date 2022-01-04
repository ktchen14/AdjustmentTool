using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AdjustmentTool {
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  [KSPAddon(KSPAddon.Startup.EditorVAB, false)]
  public partial class EditorHook : MonoBehaviour {
    private EditorLogic editor;
    private AdjustmentTool adjustmentTool;

    [RemoteMember("fsm")] private KerbalFSM efsm;
    private KFSMState st_offset_select;
    private KFSMState st_offset_tweak;
    private KFSMState st_rotate_select;
    private KFSMState st_rotate_tweak;
    private KFSMEvent on_goToModeOffset;
    private KFSMEvent on_goToModeRotate;
    private KFSMEvent on_offsetSelect;
    private KFSMEvent on_offsetDeselect;

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
    private KFSMEvent on_adjustRevert;

    private void Awake() => editor = EditorLogic.fetch;

    private void Start() => Initialize(HookEditor);

    /// Hook the AdjustmentTool code into the EditorLogic instance
    private void HookEditor(EditorLogic editor) {
      RemoteMemberAttribute.Load(this, editor);

      const BindingFlags Search = RemoteMemberAttribute.Search;

      (FieldInfo f, PropertyInfo p) info;
      var Editor = typeof(EditorLogic);
      if ((info.p = Editor.GetProperty("selectedPart", Search)) != null) {
        if (!typeof(Part).IsAssignableFrom(info.p.PropertyType))
          throw new Exception("Can't hook into EditorLogic.selectedPart");
        if (!info.p.CanRead || !info.p.CanWrite)
          throw new Exception("Can't hook into EditorLogic.selectedPart");
        selectedPartSetter = part => info.p.SetValue(editor, part);
      } else if ((info.f = Editor.GetField("selectedPart", Search)) != null) {
        if (!typeof(Part).IsAssignableFrom(info.f.FieldType))
          throw new Exception("Can't hook into EditorLogic.selectedPart");
        selectedPartSetter = part => info.f.SetValue(editor, part);
      } else throw new Exception("Can't hook into EditorLogic.selectedPart");

      (FieldInfo f, PropertyInfo p) nodeInfo;
      List<KFSMState> nodeList;
      if ((nodeInfo.p = efsm.GetType().GetProperty("States", Search)) != null)
        nodeList = (List<KFSMState>) nodeInfo.p.GetValue(efsm);
      else if ((nodeInfo.f = efsm.GetType().GetField("States", Search)) != null)
        nodeList = (List<KFSMState>) nodeInfo.f.GetValue(efsm);
      else throw new Exception("Can't hook into KerbalFSM.States");

      var nodeLookup = nodeList.ToDictionary(node => node.name);
      st_offset_select = nodeLookup["st_offset_select"];
      st_offset_tweak = nodeLookup["st_offset_tweak"];
      st_rotate_select = nodeLookup["st_rotate_select"];
      st_rotate_tweak = nodeLookup["st_rotate_tweak"];

      var edgeLookup = new Dictionary<string, KFSMEvent>();
      foreach (var edge in nodeList.SelectMany(node => node.StateEvents))
        edgeLookup[edge.name] = edge;
      on_goToModeOffset = edgeLookup["on_goToModeOffset"];
      on_goToModeRotate = edgeLookup["on_goToModeRotate"];
      on_offsetSelect = edgeLookup["on_offsetSelect"];
      on_offsetDeselect = edgeLookup["on_offsetDeselect"];

      st_adjust_select = new KFSMState("st_adjust_select");
      InitializeAdjustSelect();
      efsm.AddState(st_adjust_select);

      st_adjust_active = new KFSMState("st_adjust_active");
      InitializeAdjustActive();
      efsm.AddState(st_adjust_active);

      // Add on_goToModeAdjust to each node with either on_goToModeOffset or
      // on_goToModeRotate
      on_goToModeAdjust = new KFSMEvent("on_goToModeAdjust");
      InitializeOn_goToModeAdjust();
      var goToEdgeList = new [] { on_goToModeOffset, on_goToModeRotate };
      efsm.AddEvent(on_goToModeAdjust, nodeLookup.Values.Where(
        node => node.StateEvents.Any(edge => goToEdgeList.Contains(edge))
      ).ToArray());

      on_adjustSelect = new KFSMEvent("on_adjustSelect");
      InitializeOn_AdjustSelect();
      efsm.AddEvent(on_adjustSelect, st_adjust_select);

      on_adjustDeselect = new KFSMEvent("on_adjustDeselect");
      InitializeOn_AdjustDeselect();
      efsm.AddEvent(on_adjustDeselect, st_adjust_active);

      on_adjustRevert = new KFSMEvent("on_adjustRevert") {
        GoToStateOnEvent = st_adjust_active,
        updateMode = KFSMUpdateMode.MANUAL_TRIGGER
      };
      efsm.AddEvent(on_adjustRevert, st_adjust_active);

      // Add each edge in both st_offset_select and st_rotate_select as well as
      // both on_goToModeOffset and on_goToModeRotate to st_adjust_select.
      st_offset_select.StateEvents.Intersect(
        st_rotate_select.StateEvents
      ).ToList().ForEach(edge => efsm.AddEvent(edge, st_adjust_select));
      efsm.AddEvent(on_goToModeOffset, st_adjust_select);
      efsm.AddEvent(on_goToModeRotate, st_adjust_select);

      // Add each edge in both st_offset_tweak and st_rotate_tweak as well as
      // both on_goToModeOffset and on_goToModeRotate to st_adjust_active.
      st_offset_tweak.StateEvents.Intersect(
        st_rotate_tweak.StateEvents
      ).ToList().ForEach(edge => efsm.AddEvent(edge, st_adjust_active));
      efsm.AddEvent(on_goToModeOffset, st_adjust_active);
      efsm.AddEvent(on_goToModeRotate, st_adjust_active);

      // Ensure that the SelectedPart's onEditorEndTweak() method isn't called
      // on a transition to st_adjust_active
      var offsetExit = st_offset_tweak.OnLeave;
      st_offset_tweak.OnLeave = to =>
        offsetExit(to != st_adjust_active ? to : st_offset_tweak);

      var rotateExit = st_rotate_tweak.OnLeave;
      st_rotate_tweak.OnLeave = to =>
        rotateExit(to != st_adjust_active ? to : st_offset_tweak);

      // Handle st_adjust_active in on_undoRedo so that an undo or redo doesn't
      // result in a NullReferenceException
      var on_undoRedo = edgeLookup["on_undoRedo"];
      on_undoRedo.OnEvent += () => {
        if (efsm.CurrentState == st_adjust_active)
          on_undoRedo.GoToStateOnEvent = st_adjust_select;
      };
    }

    // Whether the part is adjustable (surface attached)
    private static bool isPartAdjustable(Part part)
      => part.srfAttachNode != null && part.srfAttachNode.attachedPart != null;

    private static bool IsTextLocked(ControlTypes type = ControlTypes.None) {
      if (InputLockManager.IsLocked(type))
        return true;

      var item = EventSystem.current.currentSelectedGameObject;
      if (item == null)
        return false;
      return item.GetComponent<TMP_InputField>() != null;
    }
  }
}
