using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Highlighting;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AdjustmentTool {
  public class PartCollectionChange : UnityEvent<ICollection<Part>> { }
  public delegate bool IsSelectable();

  public class PartCollection : MonoBehaviour, ICollection<Part> {
    public readonly PartCollectionChange Change = new PartCollectionChange();

    // Used to stop part selection when the cursor is over the adjustment tool
    private IsSelectable isSelectable;
    public IsSelectable IsSelectable {
      get => isSelectable;
      set => isSelectable = value ?? (() => true);
    }

    private readonly ICollection<Part> partList = new HashSet<Part>();

    // Make ActiveColor distinct from colorPartRootToolHighlight so that a
    // highlight of the same color as colorPartRootToolHighlight (i.e.
    // colorPartInventoryContainer) is distinguishable from a highlight with
    // ActiveColor. This is so that when this script is disabled, we won't
    // clobber a highlight from another behaviour (i.e. ModuleInventoryPart).
    private static Color ActiveColor =>
      Highlighter.colorPartRootToolHover + new Color32(0, 1, 0, 0);
    private static Color IdleColor => Color.Lerp(
      Highlighter.colorPartRootToolHighlight, ActiveColor, 0.4f);
    private static Color NormalColor => Part.defaultHighlightPart;

    private EditorLogic editor;

    // The default highlight colors are overrideable by GameSettings. Define
    // our colors as static properties and then cache them in Awake().
    private Color activeColor;
    private Color idleColor;
    private Color normalColor;

    public void Add(Part item) {
      if (item == null)
        throw new ArgumentNullException(nameof(item));
      if (isActiveAndEnabled)
        item.SetHighlightColor(activeColor);
      partList.Add(item);
      Change.Invoke(this);
    }

    public bool Remove(Part item) {
      if (item == null)
        throw new ArgumentNullException(nameof(item));
      if (isActiveAndEnabled)
        item.SetHighlightColor(normalColor);
      var result = partList.Remove(item);
      Change.Invoke(this);
      return result;
    }

    public void Clear() {
      if (isActiveAndEnabled)
        ForEach(part => part.SetHighlightColor(normalColor));
      partList.Clear();
      Change.Invoke(this);
    }

    private void Awake() {
      editor = EditorLogic.fetch;
      activeColor = ActiveColor;
      idleColor = IdleColor;
      normalColor = NormalColor;
    }

    private void OnEnable() {
      ForEach(part => part.SetHighlightColor(activeColor));
    }

    private void OnDisable() {
      foreach (var part in this) {
        if (part.highlightColor != activeColor)
          continue;
        part.SetHighlightColor(normalColor);
      }
    }

    private void Update() {
      if (!Input.GetKey(KeyCode.LeftShift) || !Input.GetMouseButtonDown(0))
        return;
      if (EventSystem.current.IsPointerOverGameObject() || !IsSelectable())
        return;

      if (OverPart() is var overPart && overPart == null)
        Clear();
      else if (partList.Contains(overPart))
        Remove(overPart);
      else if (editor.ship.Contains(overPart))
        Add(overPart);
    }

    private void LateUpdate() {
      // If the part's highlight color doesn't need to be changed then this is
      // an inexpensive call. Don't bother trying to optimize this. This does,
      // however, need to be done in LateUpdate() so that a change in highlight
      // color in Update() doesn't result in a flicker.
      foreach (var part in this) {
        if (part.HighlightActive)
          continue;
        part.Highlight(idleColor);
        part.highlighter.ConstantOnImmediate();
      }
    }

    private Part OverPart() {
      var cast = editor.editorCamera.ScreenPointToRay(Input.mousePosition);
      var maxDistance = editor.editorCamera.farClipPlane;
      var layerMask = EditorLogic.LayerMask | (1 << 21);
      if (!Physics.Raycast(cast, out var strike, maxDistance, layerMask))
        return null;
      return FlightGlobals.GetPartUpwardsCached(strike.collider.gameObject);
    }

    [CollectionAccess(CollectionAccessType.Read)]
    private void ForEach([NotNull] [InstantHandle] Action<Part> action) =>
      this.ToList().ForEach(action);

    // Implementation of ICollection<Part>
    public IEnumerator<Part> GetEnumerator() => partList.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() =>
      ((IEnumerable) partList).GetEnumerator();
    public bool Contains(Part item) => partList.Contains(item);
    public void CopyTo(Part[] target, int i) => partList.CopyTo(target, i);
    public int Count => partList.Count;
    public bool IsReadOnly => partList.IsReadOnly;
  }
}
