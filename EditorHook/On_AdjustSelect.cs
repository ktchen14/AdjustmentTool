using UnityEngine;
using UnityEngine.EventSystems;

namespace AdjustmentTool {
  public partial class EditorHook {
    private void InitializeOn_AdjustSelect(KFSMEvent on) {
      on.updateMode = KFSMUpdateMode.UPDATE;
      on.OnCheckCondition = isAdjustSelect;
      on.GoToStateOnEvent = st_adjust_active;
    }

    private bool isAdjustSelect(KFSMState _) {
      if (!Input.GetMouseButtonUp(0))
        return false;
      if (EventSystem.current.IsPointerOverGameObject())
        return false;

      if ((SelectedPart = choosePart()) == null)
        return false;

      if (!editor.ship.Contains(SelectedPart)) {
        on_adjustSelect.GoToStateOnEvent = st_place;
        on_partPicked.OnEvent();
        return false;
      }

      if (!isPartAdjustable(SelectedPart)) {
        SelectedPart = null;
        return false;
      }

      return SelectedPart;
    }
  }
}
