namespace AdjustmentTool {
  public partial class EditorHook {
    private const string HelpText = "Select a surface attached part to Adjust";

    private void InitializeAdjustSelect() {
      st_adjust_select.OnEnter = st_offset_select.OnEnter;
      st_adjust_select.OnEnter += OnAdjustSelectEntrance;
      st_adjust_select.OnLeave = st_offset_select.OnLeave;
      st_adjust_select.OnUpdate = st_offset_select.OnUpdate;
    }

    private void OnAdjustSelectEntrance(KFSMState from) {
      if (from != st_adjust_active)
        ScreenMessages.PostScreenMessage(HelpText, modeMsg);
    }
  }
}
