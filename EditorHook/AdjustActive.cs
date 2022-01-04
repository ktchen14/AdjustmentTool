using System.Diagnostics.CodeAnalysis;
using AdjustmentTool.UI;
using EditorGizmos;
using UnityEngine;

namespace AdjustmentTool {
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public partial class EditorHook {
    private GizmoOffset offsetTool;

    private delegate void UpdateSymmetryCall(Part selPart, int symMode,
      Part partParent, AttachNode selPartNode);
    [RemoteMember] private UpdateSymmetryCall UpdateSymmetry;

    private int symUpdateMode;
    private Part symUpdateParent;
    private AttachNode symUpdateAttachNode;

    private void InitializeAdjustActive() {
      st_adjust_active.OnEnter += st_offset_tweak.OnEnter;
      st_adjust_active.OnEnter += onAdjustEntrance;
      st_adjust_active.OnLeave += st_offset_tweak.OnLeave;
      st_adjust_active.OnLeave += onAdjustExit;
      st_adjust_active.OnUpdate += st_offset_tweak.OnUpdate;
      st_adjust_active.OnUpdate += divisionUpdate;
    }

    private void onAdjustEntrance(KFSMState from) {
      offsetTool = FindObjectOfType<GizmoOffset>();
      offsetTool.gameObject.SetActive(false);

      symUpdateMode = SelectedPart.symmetryCounterparts.Count;
      symUpdateParent = SelectedPart.parent;
      symUpdateAttachNode = SelectedPart.FindAttachNodeByPart(symUpdateParent);

      adjustmentTool = AdjustmentTool.Attach(
        SelectedPart.GetReferenceParent(),
        SelectedPart.GetReferenceTransform(),
        SelectedPart.initRotation,
        onMove,
        onMoveStop);
      GameEvents.onEditorPartEvent.Add(OnPartOffset);
    }

    private void onAdjustExit(KFSMState to) {
      GameEvents.onEditorPartEvent.Remove(OnPartOffset);
      adjustmentTool.Detach();
    }

    private void divisionUpdate() {
      if (!GameSettings.Editor_toggleSymMode.GetKeyDown())
        return;

      if (IsTextLocked(ControlTypes.EDITOR_SYM_SNAP_UI))
        return;

      if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        Division.Last();
      else
        Division.Next();
    }

    private void onMove(Vector3 position) {
      var host = SelectedPart.GetReferenceTransform();

      offsetTool.transform.position = position;

      host.position = position;
      SelectedPart.attPos = host.localPosition - SelectedPart.attPos0;

      if (symUpdateMode != 0) {
        UpdateSymmetry(SelectedPart, symUpdateMode, symUpdateParent, symUpdateAttachNode);
        foreach (var part in SelectedPart.symmetryCounterparts)
          part.attPos = part.transform.localPosition - part.attPos0;
      }

      GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartOffsetting,
        SelectedPart);
    }

    private void onMoveStop() {
      if (SelectedPart != null && editor.ship.Contains(SelectedPart))
        editor.SetBackup();

      GameEvents.onEditorPartEvent.Remove(OnPartOffset);
      GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartOffset,
        SelectedPart);
      GameEvents.onEditorPartEvent.Add(OnPartOffset);
    }

    // Revert the adjustment tool if the part is offset from an external source
    // (e.g. if the user hits Editor_resetRotation).
    private void OnPartOffset(ConstructionEventType type, Part part) {
      if (type != ConstructionEventType.PartOffset || part != SelectedPart)
        return;
      efsm.RunEvent(on_adjustRevert);
    }
  }
}
