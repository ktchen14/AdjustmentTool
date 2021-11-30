using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdjustmentTool.UI {
  public class Axis : MonoBehaviour {
    public readonly HandleMove HandleMove = new HandleMove();

    [SerializeField] private Axes axes;
    [SerializeField] private Axle axle;
    [SerializeField] private List<Marker> markerList = new List<Marker>();

    [HideInInspector] public Material material;

    [SerializeField] private Color markerColor;
    [HideInInspector] public Material markerMaterial;

    private float minimum, maximum;

    public void Show(float minimum, float maximum) {
      if (minimum >= maximum)
        throw new ArgumentException("Axis minimum must be less than maximum");

      axle.Show(this.minimum = minimum, this.maximum = maximum);
      UpdateDivision(Division.Instance);
    }

    public float RestrainOffset(float offset) {
      offset = Mathf.Clamp(offset, minimum, maximum);

      // Just return the clamped offset unless we want to quantize the offset
      if (!axes.Quantize)
        return offset;

      var marker = markerList.Where(
        m => m.gameObject.activeSelf
      ).OrderBy(
        m => Mathf.Abs(m.transform.localPosition.x - offset)
      ).First();
      var markerOffset = marker.transform.localPosition.x;

      // Don't change the offset unless we're somewhat near the marker
      var markerRadius = marker.transform.localScale.x * Marker.Radius;
      const float factor = 2f / 3;
      if (Mathf.Abs(offset - markerOffset) > factor * (Handle.Radius + markerRadius))
        return offset;

      return markerOffset;
    }

    private void Awake() {
      markerMaterial = new Material(material) {
        color = markerColor,
        renderQueue = material.renderQueue + 1,
      };
    }

    private void Start() {
      HandleMove.AddListener(OnHandleMove);
      Division.Change.AddListener(UpdateDivision);
    }

    private void OnDestroy() {
      HandleMove.RemoveListener(OnHandleMove);
      Division.Change.RemoveListener(UpdateDivision);
      Destroy(markerMaterial);
    }

    private void OnHandleMove(Handle _, float offset) {
      minimum -= offset;
      maximum -= offset;
    }

    private void UpdateDivision(Division division) {
      var interval = (maximum - minimum) / division;

      for (var i = markerList.Count; i < division + 1; i++)
        markerList.Add(Instantiate(markerList[0], transform));

      for (var i = 0; i < division + 1; i++)
        markerList[i].Show(minimum + i * interval, division.MarkerSize[i]);

      for (var i = division + 1; i < markerList.Count; i++)
        markerList[i].Hide();
    }
  }
}
