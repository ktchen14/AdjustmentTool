using UnityEngine;

namespace AdjustmentTool {
  public partial class EditorHook {
    private void InitializeOn_AdjustSelect() {
      on_adjustSelect.updateMode = KFSMUpdateMode.UPDATE;
      on_adjustSelect.OnCheckCondition = isAdjustSelect;
      on_adjustSelect.GoToStateOnEvent = st_adjust_active;
    }

    private bool isAdjustSelect(KFSMState node) {
      if (Input.GetKey(KeyCode.LeftShift))
        return false;

      var result = on_offsetSelect.OnCheckCondition(node);

      if (!result || isPartAdjustable(SelectedPart))
        return result;

      return SelectedPart = null;
    }
  }
}
