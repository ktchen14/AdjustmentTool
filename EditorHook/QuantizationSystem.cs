namespace AdjustmentTool {
  public class QuantizationSystem : EditorSystem {
    private void OnEnable() {
      EditorHook.st_adjust_active.OnEnter += OnActiveEntrance;
      EditorHook.st_adjust_active.OnLeave += OnActiveExit;
      if (EditorHook.CurrentState == EditorHook.st_adjust_active)
        OnActiveEntrance(null);
    }

    private void OnDisable() {
      EditorHook.st_adjust_active.OnEnter -= OnActiveEntrance;
      EditorHook.st_adjust_active.OnLeave -= OnActiveExit;
      if (EditorHook.CurrentState == EditorHook.st_adjust_active)
        OnActiveExit(null);
    }

    private void OnActiveEntrance(KFSMState _) {
      GameEvents.onEditorSnapModeChange.Add(OnChange);
      Tool.Quantize = GameSettings.VAB_USE_ANGLE_SNAP;
    }

    private void OnActiveExit(KFSMState _) {
      GameEvents.onEditorSnapModeChange.Remove(OnChange);
    }

    private void OnChange(bool mode) => Tool.Quantize = mode;
  }
}
