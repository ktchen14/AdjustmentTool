using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AdjustmentTool {
  public partial class EditorHook {
    private static AssetBundle bundle;

    private void Initialize(Action<EditorLogic> hookEditor) {
      GameObject selector = null;

      try {
        if (bundle == null) {
          var root = Assembly.GetExecutingAssembly().Location;
          const string name = "main.bundle";
          var bundleLocation = Path.Combine(root, "..", name);

          if ((bundle = AssetBundle.LoadFromFile(bundleLocation)) == null)
            throw new Exception($"Can't load bundle at {bundleLocation}");
        }

        AdjustmentTool.Load(bundle);
        selector = ToolSelector.Initialize(bundle, GoToModeAdjust);
        hookEditor(editor);

        partCollection = gameObject.AddComponent<PartCollection>();
        partCollection.enabled = false;
      } catch (Exception e) {
        if (selector != null)
          Destroy(selector);
        Destroy(this);
        Debug.LogException(e, this);
      }
    }
  }
}
