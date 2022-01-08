using AdjustmentTool.UI;
using UnityEngine;

namespace AdjustmentTool {
  public class DivisionSystem : EditorSystem {
    private void Start() {
      EditorHook.st_adjust_active.OnUpdate += DivisionUpdate;
    }

    private void DivisionUpdate() {
      if (!GameSettings.Editor_toggleSymMode.GetKeyDown())
        return;

      if (IsTextLocked(ControlTypes.EDITOR_SYM_SNAP_UI))
        return;

      if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        Division.Last();
      else
        Division.Next();
    }
  }
}
