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

    private void InitializeAdjustSelect() {
      st_adjust_select.OnEnter = OnAdjustSelectEntrance;
      st_adjust_select.OnUpdate = st_offset_select.OnUpdate;
      st_adjust_select.OnLeave = OnAdjustSelectExit;
    }

    private void OnAdjustSelectEntrance(KFSMState from) {
      partCollection.enabled = true;
      if (from != st_adjust_active)
        ScreenMessages.PostScreenMessage(HelpText, modeMsg);
    }

    private void OnAdjustSelectExit(KFSMState to) {
      partCollection.enabled = false;
    }
  }
}
