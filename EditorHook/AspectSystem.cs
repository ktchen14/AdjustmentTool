using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdjustmentTool {
  public delegate Quaternion AspectRotation();

  public readonly struct Aspect {
    public readonly string Name;

    private readonly AspectRotation rotation;
    public Quaternion Rotation => rotation();

    public Aspect(string name, AspectRotation rotation) =>
      (Name, this.rotation) = (name, rotation);
  }

  public class AspectSystem : EditorSystem {
    private static readonly List<Aspect> AspectList = new List<Aspect> {
      new Aspect("Absolute", () => Quaternion.identity),
      new Aspect("Local", () => {
        var selectedPart = EditorLogic.SelectedPart;
        var hostRotation = selectedPart.GetReferenceTransform().rotation;
        var partRotation = selectedPart.initRotation;
        return hostRotation * Quaternion.Inverse(partRotation);
      }),
    };

    private Button button;
    private TextMeshProUGUI buttonText;
    private byte i;

    private void OnEnable() {
      EditorHook.st_adjust_active.OnEnter += OnActiveEntrance;
      EditorHook.st_adjust_active.OnLeave += OnActiveExit;
      EditorHook.st_adjust_active.OnUpdate += OnActiveUpdate;
      if (EditorHook.CurrentState == EditorHook.st_adjust_active)
        OnActiveEntrance(null);
    }

    private void OnDisable() {
      EditorHook.st_adjust_active.OnEnter -= OnActiveEntrance;
      EditorHook.st_adjust_active.OnLeave -= OnActiveExit;
      EditorHook.st_adjust_active.OnUpdate -= OnActiveUpdate;
      if (EditorHook.CurrentState == EditorHook.st_adjust_active)
        OnActiveExit(null);
    }

    private void OnActiveEntrance(KFSMState _) {
      var template = Editor.coordSpaceBtn;
      button = Instantiate(template, template.transform.parent);
      button.onClick.RemoveAllListeners();
      button.onClick.AddListener(NextAspect);
      button.gameObject.SetActive(true);

      buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
      buttonText.text = AspectList[i].Name;

      Tool.transform.rotation = AspectList[i].Rotation;
    }

    private void OnActiveExit(KFSMState _) => Destroy(button.gameObject);

    private void OnActiveUpdate() {
      if (!GameSettings.Editor_coordSystem.GetKeyUp())
        return;

      if (Tool.Held)
        return;

      NextAspect();
    }

    private void NextAspect() {
      var aspect = AspectList[i = (byte) ((i + 1) % AspectList.Count)];

      ScreenMessages.PostScreenMessage($"Adjustment: {aspect.Name}",
        EditorHook.ModeMsg);
      buttonText.text = aspect.Name;

      Tool.transform.rotation = aspect.Rotation;
      EditorHook.ReloadTool();
    }
  }
}
