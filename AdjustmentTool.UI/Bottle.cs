using UnityEngine;

namespace AdjustmentTool.UI {
  public class Bottle : MonoBehaviour {
    [SerializeField] private Sprite normalSprite;
    public Sprite NormalSprite => normalSprite;

    [SerializeField] private Sprite activeSprite;
    public Sprite ActiveSprite => activeSprite;
  }
}
