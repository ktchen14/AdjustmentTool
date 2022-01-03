using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdjustmentTool.UI {
  public delegate void OnMove(Vector3 position);
  public delegate void OnMoveStop();

  public class Axes : MonoBehaviour {
    public readonly HandleHold HandleHold = new HandleHold();

    [SerializeField] private Axis axisX;
    public Axis AxisX => axisX;
    [SerializeField] private Axis axisY;
    public Axis AxisY => axisY;
    [SerializeField] private Axis axisZ;
    public Axis AxisZ => axisZ;

    /// Whether a handle in the axes is held at this moment
    public bool Held { get; private set; }

    public OnMove OnMove = delegate { };
    public OnMoveStop OnMoveStop = delegate { };

    /// Whether a handle drag should "snap" to a marker
    [NonSerialized] public bool Quantize;

    /// Initialize the axes so that its axis objects encase each rendered mesh
    /// in the list. This is calculated from the perspective of the axes. Note
    /// that this is an expensive operation and shouldn't be called unless the
    /// orientation of the axes is changed.
    public void Encase(IEnumerable<Renderer> list) {
      var transform = this.transform;

      // Find the oriented bounding box (from the perspective of the axes) that
      // will encapsulate each renderer in the list.
      var domain = list.OfType<MeshRenderer>().Where(
        renderer => renderer.enabled
      ).Select(
        renderer => renderer.GetComponent<MeshFilter>()
      ).Where(
        filter => filter != null
      ).Select(
        filter => (filter.sharedMesh, filter.transform)
      ).Where(
        ((Mesh mesh, Transform transform) item) => item.mesh != null
      ).Select(item => {
        var (mesh, meshTransform) = item;

        // The rotation that, when applied to the untransformed mesh, will make
        // it have the same orientation (as seen by an unrotated observer) as
        // the orientation of the transformed mesh (as seen by the axes). To
        // summarize, this is the rotation of the mesh from the perspective of
        // the axes.
        var y = transform.InverseTransformDirection(meshTransform.up);
        var z = transform.InverseTransformDirection(meshTransform.forward);
        var rotation = Quaternion.LookRotation(z, y);

        // The translation that, when applied to the untranslated (but rotated)
        // mesh, will make it have the same position (as seen by an untranslated
        // observer) as the position of the transformed mesh (as seen by the
        // axes). Note that this translation is in the coordinate system of the
        // mesh as rotated by *rotation*. To summarize, this is the position of
        // the mesh from the (rotated) perspective of the axes.
        var position = transform.InverseTransformPoint(meshTransform.position);

        var matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
        return GeometryUtility.CalculateBounds(mesh.vertices, matrix);
      }).Aggregate((i, j) => { i.Encapsulate(j); return i; });

      AxisX.Show(domain.min.x, domain.max.x);
      AxisY.Show(domain.min.y, domain.max.y);
      AxisZ.Show(domain.min.z, domain.max.z);
    }

    private void Start() {
      HandleHold.AddListener(OnHandleHold);
      EachAxis(axis => axis.HandleMove.AddListener(OnHandleMove));
    }

    private void OnDestroy() {
      HandleHold.RemoveListener(OnHandleHold);
      EachAxis(axis => axis.HandleMove.RemoveListener(OnHandleMove));
    }

    private void OnHandleHold(Handle _, bool held) {
      Held = held;
      if (Held)
        return;
      OnMoveStop();
    }

    private void OnHandleMove(Handle handle, float offset) {
      var transform = this.transform;
      transform.Translate(offset * handle.transform.right, Space.World);
      OnMove(transform.position);
    }

    private void EachAxis(Action<Axis> action)
      => new List<Axis> { AxisX, AxisY, AxisZ }.ForEach(action);
  }
}
