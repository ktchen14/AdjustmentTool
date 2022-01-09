using System;
using System.IO;
using System.Linq;
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

        if (AdjustmentTool.Template == null)
          AdjustmentTool.Load(bundle);
        selector = ToolSelector.Initialize(bundle, GoToModeAdjust);
        hookEditor(editor);

        // Add each concrete subclass of EditorSystem as a component
        Assembly.GetExecutingAssembly().GetTypes().Where(
          type => type.IsSubclassOf(typeof(EditorSystem))
        ).Where(
          type => !type.IsAbstract && !type.IsGenericType
        ).Where(
          type => !gameObject.TryGetComponent(type, out _)
        ).ToList().ForEach(type => gameObject.AddComponent(type));
      } catch (Exception e) {
        if (selector != null)
          Destroy(selector);
        Destroy(this);
        Debug.LogException(e, this);
      }
    }
  }
}
