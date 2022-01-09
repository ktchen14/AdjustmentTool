namespace AdjustmentTool {
  public class QuantizationSystem : EditorSystem {
    private void OnEnable() {
      EditorHook.st_adjust_active.OnEnter += OnAdjustEntrance;
      EditorHook.st_adjust_active.OnLeave += OnAdjustExit;
      if (EditorHook.CurrentState == EditorHook.st_adjust_active)
        OnAdjustEntrance(null);
    }

    private void OnDisable() {
      EditorHook.st_adjust_active.OnEnter -= OnAdjustEntrance;
      EditorHook.st_adjust_active.OnLeave -= OnAdjustExit;
      if (EditorHook.CurrentState == EditorHook.st_adjust_active)
        OnAdjustExit(null);
    }

    private void OnAdjustEntrance(KFSMState _) {
      GameEvents.onEditorSnapModeChange.Add(OnChange);
      Tool.Axes.Quantize = GameSettings.VAB_USE_ANGLE_SNAP;
    }

    private void OnAdjustExit(KFSMState _) {
      GameEvents.onEditorSnapModeChange.Remove(OnChange);
    }

    private void OnChange(bool mode) => Tool.Axes.Quantize = mode;
  }
}
