using UnityEngine;

namespace AdjustmentTool.UI {
  public class Axle : MonoBehaviour {
    [SerializeField] private Renderer renderer;
    [SerializeField] private Axis axis;
    [SerializeField] private Color color;

    private Material material;

    public void Show(float minimum, float maximum) {
      var transform = this.transform;
      transform.localPosition = new Vector3((minimum + maximum) / 2, 0, 0);
      transform.localScale = new Vector3(maximum - minimum, 1, 1);
    }

    private void Awake() {
      material = new Material(axis.material) { color = color };
      material.renderQueue += 2;
    }

    private void Start() {
      axis.HandleMove.AddListener(OnHandleMove);
      renderer.sharedMaterial = material;
    }

    private void OnDestroy() {
      axis.HandleMove.RemoveListener(OnHandleMove);
      Destroy(material);
    }

    private void OnHandleMove(Handle _, float offset)
      => transform.Translate(-offset, 0, 0);
  }
}
