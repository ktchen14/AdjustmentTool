using System;
using System.Linq;
using AdjustmentTool.UI;
using UnityEngine;

namespace AdjustmentTool {
  public class AdjustmentTool : MonoBehaviour {
    public static AdjustmentTool Template { get; private set; }
    public Axes Axes { get; private set; }

    public bool Held => Axes.Held;

    public static AdjustmentTool Attach(Transform host, Quaternion rotation, OnMove onMove, OnMoveStop onMoveStop) {
      var tool = Instantiate(Template, host.position, host.rotation * Quaternion.Inverse(rotation));

      tool.Axes.OnMove = onMove;
      tool.Axes.OnMoveStop = onMoveStop;

      return tool;
    }

    public void Encase(params Part[] list)
      => Axes.Encase(list.SelectMany(part => part.GetPartRenderers()));

    public void Detach() => Destroy(gameObject);

    private void Awake() => Axes = GetComponent<Axes>();

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

      var axes = bundle.LoadAsset<GameObject>("Axes").GetComponent<Axes>();
      axes.AxisX.material = handleRendererMaterial("Handle X+");
      axes.AxisY.material = handleRendererMaterial("Handle Y+");
      axes.AxisZ.material = handleRendererMaterial("Handle Z+");
      Template = axes.gameObject.AddComponent<AdjustmentTool>();
    }
  }
}
