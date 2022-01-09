namespace AdjustmentTool {
  public partial class EditorHook {
    private void InitializeOn_goToModeAdjust() {
      on_goToModeAdjust.updateMode = KFSMUpdateMode.MANUAL_TRIGGER;
      on_goToModeAdjust.OnEvent = onGoToModeAdjust;
      on_goToModeAdjust.GoToStateOnEvent = st_adjust_active;
    }

    private void onGoToModeAdjust() {
      on_goToModeOffset.OnEvent();

      if (on_goToModeOffset.GoToStateOnEvent == st_offset_select)
        on_goToModeAdjust.GoToStateOnEvent = st_adjust_select;
      else if (on_goToModeOffset.GoToStateOnEvent != st_offset_tweak)
        on_goToModeAdjust.GoToStateOnEvent = on_goToModeOffset.GoToStateOnEvent;
      else if (isPartAdjustable(SelectedPart))
        on_goToModeAdjust.GoToStateOnEvent = st_adjust_active;
      else {
        SelectedPart = null;
        on_goToModeAdjust.GoToStateOnEvent = st_adjust_select;
      }
    }

    private void GoToModeAdjust() => efsm.RunEvent(on_goToModeAdjust);
  }
}
