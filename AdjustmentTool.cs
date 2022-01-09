using System;
using System.Linq;
using AdjustmentTool.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdjustmentTool {
  public static class AdjustmentTool {
    public static Axes Template { get; private set; }

    public static Axes Attach(Transform host) {
      return Object.Instantiate(Template, host.position, Quaternion.identity);
    }

    public static void Detach(this Axes axes) =>
      Object.Destroy(axes.gameObject);

    public static void Encase(this Axes axes, params Part[] list)
      => axes.Encase(list.SelectMany(part => part.GetPartRenderers()));

    public static void Load(AssetBundle bundle) {
      if (!(AssetBase.GetPrefab("OffsetGizmo") is GameObject offsetTool))
        throw new Exception("Can't locate loaded prefab OffsetGizmo");

      Material handleRendererMaterial(string name) {
        if (!(offsetTool.GetChild(name) is GameObject handle))
          throw new Exception($"No such object {name} in OffsetGizmo");
        var renderer = handle.GetComponent<Renderer>();
        if (renderer == null)
          throw new Exception($"No renderer in {name} in OffsetGizmo");
        return renderer.sharedMaterial;
      }

      Template = bundle.LoadAsset<GameObject>("Axes").GetComponent<Axes>();
      Template.AxisX.material = handleRendererMaterial("Handle X+");
      Template.AxisY.material = handleRendererMaterial("Handle Y+");
      Template.AxisZ.material = handleRendererMaterial("Handle Z+");
    }
  }
}
