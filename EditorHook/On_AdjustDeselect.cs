using UnityEngine;

namespace AdjustmentTool {
  public partial class EditorHook {
    private void InitializeOn_AdjustDeselect() {
      on_adjustDeselect.updateMode = KFSMUpdateMode.UPDATE;
      on_adjustDeselect.OnCheckCondition = isAdjustDeselect;
      on_adjustDeselect.GoToStateOnEvent = st_adjust_select;
    }

    private bool isAdjustDeselect(KFSMState node) {
      if (Input.GetKey(KeyCode.LeftShift))
        return false;
      if (AdjustmentTool.Held)
        return false;
      return on_offsetDeselect.OnCheckCondition(node);
    }
  }
}
