using UnityEngine;
using UnityEngine.EventSystems;

namespace AdjustmentTool {
  public partial class EditorHook {
    private void InitializeOn_AdjustDeselect(KFSMEvent on) {
      on.updateMode = KFSMUpdateMode.UPDATE;
      on.OnCheckCondition = isAdjustDeselect;
      on.GoToStateOnEvent = st_adjust_select;
    }

    private bool isAdjustDeselect(KFSMState _) {
      if (!Mouse.Left.GetButtonDown() || Mouse.Left.WasDragging())
        return false;
      if (adjustmentTool.Held)
        return false;
      if (EventSystem.current.IsPointerOverGameObject())
        return false;

      var part = choosePart();

      if (part == null || EditorGeometryUtil.GetPixelDistance(
            adjustmentTool.transform.position, Input.mousePosition,
            editor.editorCamera) > 75.0 && isPartAdjustable(part)) {
        SelectedPart.onEditorEndTweak();
        SelectedPart.gameObject.SetLayerRecursive(0, true, 1 << 21);
        SelectedPart = part;
        return true;
      }

      return false;
    }
  }
}
