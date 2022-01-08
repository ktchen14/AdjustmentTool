using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AdjustmentTool.UI;
using EditorGizmos;
using UnityEngine;

namespace AdjustmentTool {
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public partial class EditorHook {
    private GizmoOffset offsetTool;

    private delegate void UpdateSymmetryCall(Part selPart, int symMode,
      Part partParent, AttachNode selPartNode);
    [RemoteMember] private UpdateSymmetryCall UpdateSymmetry;

    private int symUpdateMode;
    private Part symUpdateParent;
    private AttachNode symUpdateAttachNode;

    private void InitializeAdjustActive() {
      st_adjust_active.OnEnter += st_offset_tweak.OnEnter;
      st_adjust_active.OnEnter += onAdjustEntrance;
      st_adjust_active.OnLeave += st_offset_tweak.OnLeave;
      st_adjust_active.OnLeave += onAdjustExit;
      st_adjust_active.OnUpdate += st_offset_tweak.OnUpdate;
    }

    private void onAdjustEntrance(KFSMState from) {
      offsetTool = FindObjectOfType<GizmoOffset>();
      offsetTool.gameObject.SetActive(false);

      symUpdateMode = SelectedPart.symmetryCounterparts.Count;
      symUpdateParent = SelectedPart.parent;
      symUpdateAttachNode = SelectedPart.FindAttachNodeByPart(symUpdateParent);

      AdjustmentTool = AdjustmentTool.Attach(
        SelectedPart.GetReferenceTransform(),
        SelectedPart.initRotation,
        onMove,
        onMoveStop);

      partCollection.enabled = true;
      partCollection.Change.AddListener(OnPartCollectionChange);
      partCollection.IsSelectable = IsPartCollectionSelectable;

      OnPartCollectionChange(partCollection);

      GameEvents.onEditorPartEvent.Add(OnPartOffset);
    }

    private void onAdjustExit(KFSMState to) {
      AdjustmentTool.Detach();

      partCollection.enabled = false;
      partCollection.Change.RemoveListener(OnPartCollectionChange);
      partCollection.IsSelectable = null;

      GameEvents.onEditorPartEvent.Remove(OnPartOffset);
    }


    private void onMove(Vector3 position) {
      var host = SelectedPart.GetReferenceTransform();

      offsetTool.transform.position = position;

      host.position = position;
      SelectedPart.attPos = host.localPosition - SelectedPart.attPos0;

      if (symUpdateMode != 0) {
        UpdateSymmetry(SelectedPart, symUpdateMode, symUpdateParent, symUpdateAttachNode);
        foreach (var part in SelectedPart.symmetryCounterparts)
          part.attPos = part.transform.localPosition - part.attPos0;
      }

      GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartOffsetting,
        SelectedPart);
    }

    private void onMoveStop() {
      if (SelectedPart != null && editor.ship.Contains(SelectedPart))
        editor.SetBackup();

      GameEvents.onEditorPartEvent.Remove(OnPartOffset);
      GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartOffset,
        SelectedPart);
      GameEvents.onEditorPartEvent.Add(OnPartOffset);
    }

    private bool IsPartCollectionSelectable() => !AdjustmentTool.Held;

    private void OnPartCollectionChange(ICollection<Part> collection) {
      var list = collection.DefaultIfEmpty(SelectedPart.GetReferenceParent());
      AdjustmentTool.Encase(list.ToArray());
    }

    // Revert the adjustment tool if the part is offset from an external source
    // (e.g. if the user hits Editor_resetRotation).
    private void OnPartOffset(ConstructionEventType type, Part part) {
      if (type != ConstructionEventType.PartOffset || part != SelectedPart)
        return;
      efsm.RunEvent(on_adjustRevert);
    }
  }
}
