using UnityEngine;
using UnityEngine.Events;

namespace AdjustmentTool.UI {
  public class HandleHold : UnityEvent<Handle, bool> { }
  public class HandleMove : UnityEvent<Handle, float> { }

  public class Handle : MonoBehaviour {
    public const float Radius = 0.11f;

    [SerializeField] private Renderer renderer;
    [SerializeField] private Axes axes;
    [SerializeField] private Axis axis;

    [SerializeField] private Color uninteractableColor;
    [SerializeField] private Color overColor;
    [SerializeField] private Color heldColor;
    [SerializeField] private Color normalColor;

    private Camera camera;

    private bool interactable = true;
    private bool Interactable {
      get => interactable;
      set { interactable = value; UpdateMaterial(); }
    }

    private bool over;
    private bool Over {
      get => over;
      set { over = value; UpdateMaterial(); }
    }

    private bool held;
    private bool Held {
      get => held;
      set { held = value; UpdateMaterial(); }
    }

    private void Awake() {
      renderer.sharedMaterial = axis.material;
    }

    private void Start() {
      axes.HandleHold.AddListener(OnHandleHold);
    }

    private void OnDestroy() {
      axes.HandleHold.RemoveListener(OnHandleHold);
      Destroy(renderer.material);
    }

    private void OnMouseEnter() => Over = true;
    private void OnMouseExit() => Over = false;

    private void OnMouseDown() {
      camera = Camera.main;
      axes.HandleHold.Invoke(this, Held = true);
    }

    private void OnMouseUp() {
      axes.HandleHold.Invoke(this, Held = false);
    }

    private void OnMouseDrag() {
      var transform = this.transform;

      // Find a vector that's normal to the axis and in the direction of the
      // camera (from the axis). There's a couple ways to do this:
      //   1. Axis direction × (Axis direction × Camera from axis origin)
      //   2. Camera from axis origin - Camera from axis origin projected onto
      //      Axis direction
      // The easiest way is to take advantage of the fact that the axis's unit
      // vector is (1, 0, 0). The camera from axis origin, projected onto the
      // axis, is just the x component of that vector. The subtraction to find
      // the normal then yields just the y and z components of the vector. Bail
      // if the camera is in line with the axis.
      var normal = transform.InverseTransformPoint(camera.transform.position);
      normal.x = 0;
      if (normal == Vector3.zero)
        return;
      normal = transform.TransformDirection(normal);

      // Define a plane with this vector as its normal that's coplanar with the
      // axis. This plane is the "surface" that the drag appears to be on. Then
      // raycast from the camera onto this plane to find the world position that
      // the drag is on. Bail if the ray doesn't contact the plane, e.g. if the
      // axis is behind the camera).
      var plane = new Plane(normal.normalized, transform.position);
      var cast = camera.ScreenPointToRay((Vector2) Input.mousePosition);
      if (!plane.Raycast(cast, out var distance))
        return;
      var strike = cast.GetPoint(distance);

      // Project the raycast strike onto the axis to find the offset
      var offset = transform.InverseTransformPoint(strike).x;

      offset = axis.RestrainOffset(offset);
      axis.HandleMove.Invoke(this, offset);
    }

    private void OnHandleHold(Handle handle, bool held) {
      if (handle == this)
        return;
      Interactable = !held;
    }

    private void UpdateMaterial() {
      if (!Interactable)
        renderer.material.color = uninteractableColor;
      else if (Held)
        renderer.material.color = heldColor;
      else if (Over)
        renderer.material.color = overColor;
      else
        renderer.material.color = normalColor;
    }
  }
}
