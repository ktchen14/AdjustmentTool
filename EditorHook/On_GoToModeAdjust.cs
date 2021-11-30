namespace AdjustmentTool {
  public partial class EditorHook {
    private const string HelpText = "Select a surface attached part to Adjust";

    // We need a reference to the actual ScreenMessage instance here so that
    // ScreenMessages.PostScreenMessage(...) will displace a redundant message.
    // The on_goToModeAdjust transition is (almost?) always subsequent to a
    // on_goToModeOffset transition. If we just duplicate the format of
    // EditorLogic.modeMsg here, then both "Select an attached part to Offset"
    // and "Select a surface attached part to Adjust" will appear as help text
    // when the Adjust Tool is used.
    [RemoteMember] private ScreenMessage modeMsg;

    private void InitializeOn_goToModeAdjust(KFSMEvent on) {
      on.updateMode = KFSMUpdateMode.MANUAL_TRIGGER;
      on.OnEvent = onGoToModeAdjust;
    }

    private void onGoToModeAdjust() {
      if (SelectedPart == null) {
        ScreenMessages.PostScreenMessage(HelpText, modeMsg);
        on_goToModeAdjust.GoToStateOnEvent = st_adjust_select;
      } else if (!editor.ship.Contains(SelectedPart)) {
        on_goToModeAdjust.GoToStateOnEvent = st_place;
        on_partPicked.OnEvent();
      } else if (!isPartAdjustable(SelectedPart)) {
        SelectedPart = null;
        ScreenMessages.PostScreenMessage(HelpText, modeMsg);
        on_goToModeAdjust.GoToStateOnEvent = st_adjust_select;
      } else
        on_goToModeAdjust.GoToStateOnEvent = st_adjust_active;
    }

    private void GoToModeAdjust() => efsm.RunEvent(on_goToModeAdjust);
  }
}
