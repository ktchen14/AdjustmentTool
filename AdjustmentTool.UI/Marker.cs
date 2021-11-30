using UnityEngine;

namespace AdjustmentTool.UI {
  public class Marker : MonoBehaviour {
    public const float Radius = 0.10f;

    [SerializeField] private Renderer renderer;
    [SerializeField] private Axis axis;

    public void Show(float offset, float size) {
      var transform = this.transform;
      gameObject.SetActive(true);
      transform.localPosition = new Vector3(offset, 0, 0);
      transform.localScale = size * Vector3.one;
    }

    public void Hide() => gameObject.SetActive(false);

    private void Start() {
      axis.HandleMove.AddListener(OnHandleMove);
      renderer.sharedMaterial = axis.markerMaterial;
    }

    private void OnDestroy() {
      axis.HandleMove.RemoveListener(OnHandleMove);
    }

    private void OnHandleMove(Handle _, float offset)
      => transform.Translate(-offset, 0, 0);
  }
}
