using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AdjustmentTool {
  public class EditorSystem : MonoBehaviour {
    protected Part SelectedPart => EditorLogic.SelectedPart;
    protected AdjustmentTool Tool => EditorHook.AdjustmentTool;
    protected EditorLogic Editor { get; private set; }
    protected EditorHook EditorHook { get; private set; }

    protected virtual void Awake() {
      (Editor, EditorHook) = (EditorLogic.fetch, GetComponent<EditorHook>());
    }

    protected static bool IsTextLocked(ControlTypes type = ControlTypes.None) {
      if (InputLockManager.IsLocked(type))
        return true;

      var item = EventSystem.current.currentSelectedGameObject;
      if (item == null)
        return false;
      return item.GetComponent<TMP_InputField>() != null;
    }
  }
}
