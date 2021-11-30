using System;
using AdjustmentTool.UI;
using KSP.UI.TooltipTypes;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AdjustmentTool {
  public static class ToolSelector {
    private const string HelpText = "Tool: Adjust";

    private static Bottle bottle;

    public static GameObject Initialize(AssetBundle bundle, Action action) {
      if (bottle == null)
        bottle = bundle.LoadAsset<GameObject>("Bottle").GetComponent<Bottle>();

      var moveTool = EditorLogic.fetch.toolsUI.gameObject.GetChild("ToolMove");

      var adjustTool = Object.Instantiate(moveTool, moveTool.transform.parent);
      adjustTool.name = "ToolAdjust";
      var toggle = adjustTool.GetComponent<Toggle>();

      // Each toggle is positioned (30, 0, 0) from the previous one. We can't
      // use toggle.transform.Translate() here as that movement is affected by
      // the UI scale (on MainCanvas).
      toggle.transform.localPosition += new Vector3(3 * 30, 0, 0);
      ((Image) toggle.targetGraphic).sprite = bottle.NormalSprite;
      ((Image) toggle.graphic).sprite = bottle.ActiveSprite;
      toggle.GetComponent<TooltipController_Text>().textString = HelpText;

      toggle.onValueChanged.RemoveAllListeners();
      toggle.onValueChanged.AddListener(on => onToggle(toggle, on, action));

      return toggle.gameObject;
    }

    private static void onToggle(Toggle toggle, bool on, Action action) {
      // We can't add a ConstructionMode so we'll piggyback off of Move mode
      // when we want to be in "Adjust" mode to ensure that the game's behavior
      // is as similar to what we expect as possible. However, if the user
      // switches directly from "Adjust" mode to the actual Move mode, then the
      // onEditorConstructionModeChange event isn't invoked; from the game's
      // perspective, it's already in Move mode. To work around this, we always
      // transition from "Adjust" mode to Rotate mode. Then:
      // - If the target mode is Rotate, we're already there.
      // - If the target mode is another mode, the transition to Rotate mode
      //   *shouldn't* make a difference.

      if (!on)
        EditorLogic.fetch.toolsUI.SetMode(ConstructionMode.Rotate, false);

      if (!on || !toggle.interactable)
        return;

      EditorLogic.fetch.toolsUI.SetMode(ConstructionMode.Move, false);
      action();
    }
  }
}
