using System.Collections.Generic;
using System.Linq;

namespace AdjustmentTool {
  public class PartCollectionSystem : EditorSystem {
    private PartCollection collection;

    protected override void Awake() {
      base.Awake();
      collection = gameObject.AddComponent<PartCollection>();
      collection.enabled = false;
    }

    private void Start() => EditorHook.ReloaderList.Add(ReloadTool);

    private void OnDestroy() => EditorHook.ReloaderList.Remove(ReloadTool);

    private void OnEnable() {
      EditorHook.st_adjust_select.OnEnter += OnSelectEntrance;
      EditorHook.st_adjust_select.OnLeave += OnSelectExit;
      if (EditorHook.CurrentState == EditorHook.st_adjust_select)
        OnSelectEntrance(null);

      EditorHook.st_adjust_active.OnEnter += OnActiveEntrance;
      EditorHook.st_adjust_active.OnLeave += OnActiveExit;
      if (EditorHook.CurrentState == EditorHook.st_adjust_active)
        OnActiveEntrance(null);

      EditorHook.on_goToModeAdjust.OnEvent += OnGoToModeAdjust;
    }

    private void OnDisable() {
      EditorHook.st_adjust_select.OnEnter -= OnSelectEntrance;
      EditorHook.st_adjust_select.OnLeave -= OnSelectExit;
      if (EditorHook.CurrentState == EditorHook.st_adjust_select)
        OnSelectExit(null);

      EditorHook.st_adjust_active.OnEnter -= OnActiveEntrance;
      EditorHook.st_adjust_active.OnLeave -= OnActiveExit;
      if (EditorHook.CurrentState == EditorHook.st_adjust_active)
        OnActiveExit(null);

      EditorHook.on_goToModeAdjust.OnEvent -= OnGoToModeAdjust;
    }

    private void OnSelectEntrance(KFSMState _) => collection.enabled = true;

    private void OnSelectExit(KFSMState _) => collection.enabled = false;

    private void OnActiveEntrance(KFSMState _) {
      collection.enabled = true;
      collection.Change.AddListener(OnChange);
      collection.IsSelectable = IsSelectable;
    }

    private void OnActiveExit(KFSMState _) {
      collection.enabled = false;
      collection.Change.RemoveListener(OnChange);
      collection.IsSelectable = null;
    }

    private void OnGoToModeAdjust() => collection.Clear();

    private bool IsSelectable() => !Tool.Held;

    private void OnChange(ICollection<Part> _) => EditorHook.ReloadTool();

    private bool ReloadTool(AdjustmentTool tool) {
      if (collection.Count == 0)
        return false;

      if (!isActiveAndEnabled)
        return false;

      tool.Encase(collection.ToArray());
      return true;
    }
  }
}
