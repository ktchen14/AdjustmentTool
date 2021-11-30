using System;
using System.Linq;
using AdjustmentTool.UI;
using UnityEngine;

namespace AdjustmentTool {
  public class AdjustmentTool : MonoBehaviour {
    private static GameObject Object { get; set; }
    private Axes axes;

    public bool Held => axes.Held;

    public static AdjustmentTool Attach(Part parent, Transform host, Quaternion rotation, OnMove onMove) {
      var tool = Instantiate(Object).GetComponent<AdjustmentTool>();
      tool.transform.position = host.position;
      tool.transform.rotation = host.rotation * Quaternion.Inverse(rotation);

      tool.Encase(parent);
      tool.axes.OnMove = onMove;

      return tool;
    }

    public void Encase(params Part[] list)
      => axes.Encase(list.SelectMany(part => part.GetPartRenderers()));

    public void Detach() => Destroy(gameObject);

    private void Awake() => axes = GetComponent<Axes>();

    private void OnEnable() {
      GameEvents.onEditorSnapModeChange.Add(onEditorSnapModeChange);
      axes.Quantize = GameSettings.VAB_USE_ANGLE_SNAP;
    }

    private void OnDisable() {
      GameEvents.onEditorSnapModeChange.Remove(onEditorSnapModeChange);
    }

    private void onEditorSnapModeChange(bool mode) => axes.Quantize = mode;

    public static void Load(AssetBundle bundle) {
      if (Object && Object.GetComponent<AdjustmentTool>() != null)
        return;

      var offsetTool = AssetBase.GetPrefab("OffsetGizmo");
      if (offsetTool == null)
        throw new Exception("Can't locate loaded prefab OffsetGizmo");

      Object = bundle.LoadAsset<GameObject>("Axes");

      var axes = Object.GetComponent<Axes>();
      axes.AxisX.material = handleMaterial(offsetTool, "X");
      axes.AxisY.material = handleMaterial(offsetTool, "Y");
      axes.AxisZ.material = handleMaterial(offsetTool, "Z");

      Object.AddComponent<AdjustmentTool>();
    }

    private static Material handleMaterial(GameObject tool, string name) {
      var handle = tool.GetChild($"Handle {name}+");
      if (handle == null)
        throw new Exception("No such object Handle {name}+ in OffsetGizmo");

      var renderer = handle.GetComponent<Renderer>();
      if (renderer == null)
        throw new Exception("No renderer in Handle {name}+ in OffsetGizmo");

      return renderer.sharedMaterial;
    }
  }
}
