using AdjustmentTool.UI;
using UnityEngine;

namespace AdjustmentTool {
  public partial class EditorHook {
    private delegate void UpdateSymmetryCall(Part selPart, int symMode,
      Part partParent, AttachNode selPartNode);
    [RemoteMember] private UpdateSymmetryCall UpdateSymmetry;

    private int symUpdateMode;
    private Part symUpdateParent;
    private AttachNode symUpdateAttachNode;

    private void InitializeAdjustActive(KFSMState node) {
      node.OnEnter = onAdjustEntrance;
      node.OnLeave = onAdjustExit;
      node.OnUpdate += divisionUpdate;
      node.OnUpdate += revertUpdate;
    }

    private void onAdjustEntrance(KFSMState _) {
      SelectedPart.onEditorStartTweak();
      symUpdateMode = SelectedPart.symmetryCounterparts.Count;
      symUpdateParent = SelectedPart.parent;
      symUpdateAttachNode = SelectedPart.FindAttachNodeByPart(symUpdateParent);

      adjustmentTool = AdjustmentTool.Attach(
        SelectedPart.GetReferenceParent(),
        SelectedPart.GetReferenceTransform(),
        SelectedPart.initRotation,
        onMove);

      audioSource.PlayOneShot(editor.tweakGrabClip);
    }

    private void onAdjustExit(KFSMState to) {
      adjustmentTool.Detach();
      audioSource.PlayOneShot(editor.tweakReleaseClip);

      if (SelectedPart == null)
        return;
      if (to != st_offset_tweak && to != st_rotate_tweak)
        SelectedPart.onEditorEndTweak();
      if (to == st_idle)
        SelectedPart = null;
    }

    private void divisionUpdate() {
      if (InputLockManager.IsLocked(ControlTypes.EDITOR_SYM_SNAP_UI) ||
          editor.AnyTextFieldHasFocus() ||
          DeltaVApp.AnyTextFieldHasFocus())
        return;

      // if (RoboticControllerManager.AnyWindowTextFieldHasFocus())
      //   return;

      if (!GameSettings.Editor_toggleSymMode.GetKeyDown())
        return;

      if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        Division.Last();
      else
        Division.Next();
    }

    private void revertUpdate() {
      if (InputLockManager.IsLocked(ControlTypes.EDITOR_GIZMO_TOOLS))
        return;
      if (!GameSettings.Editor_resetRotation.GetKeyDown())
        return;
      if (SelectedPart.transform != SelectedPart.GetReferenceTransform())
        return;

      SelectedPart.attPos = Vector3.zero;
      SelectedPart.transform.localPosition = SelectedPart.attPos0;
      editor.srfAttachCursorOffset = Vector3.zero;
      if (symUpdateMode != 0) {
        UpdateSymmetry(SelectedPart, symUpdateMode, symUpdateParent, symUpdateAttachNode);
        foreach (var part in SelectedPart.symmetryCounterparts)
          part.attPos = part.transform.localPosition - part.attPos0;
      }

      adjustmentTool.Detach();
      SelectedPart.onEditorEndTweak();
      SelectedPart.onEditorStartTweak();
      symUpdateMode = SelectedPart.symmetryCounterparts.Count;
      symUpdateParent = SelectedPart.parent;
      symUpdateAttachNode = SelectedPart.FindAttachNodeByPart(symUpdateParent);

      adjustmentTool = AdjustmentTool.Attach(
        SelectedPart.GetReferenceParent(),
        SelectedPart.GetReferenceTransform(),
        SelectedPart.initRotation,
        onMove);

      GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartOffset,
        SelectedPart);
    }

    private void onMove(Vector3 _) {
      var host = SelectedPart.GetReferenceTransform();
      host.position = adjustmentTool.transform.position;

      SelectedPart.attPos = host.localPosition - SelectedPart.attPos0;

      if (symUpdateMode != 0) {
        UpdateSymmetry(SelectedPart, symUpdateMode, symUpdateParent, symUpdateAttachNode);
        foreach (var part in SelectedPart.symmetryCounterparts)
          part.attPos = part.transform.localPosition - part.attPos0;
      }

      GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartOffsetting,
        SelectedPart);
    }

    private void onMoveStop(Vector3 _) {
      if (editor.ship.Contains(SelectedPart))
        editor.SetBackup();

      GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartOffset,
        SelectedPart);
    }
  }
}
