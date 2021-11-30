using UnityEngine.Events;

namespace AdjustmentTool.UI {
  public class DivisionChange : UnityEvent<Division> { }

  public class Division {
    public static DivisionChange Change { get; } = new DivisionChange();

    public static Division Instance => DivisionList[i];

    private static readonly Division[] DivisionList = {
      new Division(01, 1.0f, 1.0f),
      new Division(02, 1.0f, 1.0f, 1.0f),
      new Division(03, 1.0f, 0.9f, 0.9f, 1.0f),
      new Division(04, 1.0f, 0.9f, 1.0f, 0.9f, 1.0f),
      new Division(06, 1.0f, 0.9f, 0.9f, 1.0f, 0.9f,0.9f, 1.0f),
      new Division(08, 1.0f, 0.8f, 0.9f, 0.8f, 1.0f, 0.8f, 0.9f, 0.8f, 1.0f),
      new Division(12, 1.0f, 0.8f, 0.8f, 0.9f, 0.8f, 0.8f, 1.0f, 0.8f, 0.8f,
                       0.9f, 0.8f, 0.8f, 1.0f),

    };
    private static byte i;

    public byte Number { get; }
    public float[] MarkerSize { get; }

    private Division(byte number, params float[] markerSize) {
      Number = number;
      MarkerSize = markerSize;
    }

    public static void Next() {
      i = (byte) ((i + 1) % DivisionList.Length);
      Change.Invoke(Instance);
    }

    public static void Last() {
      i = (byte) ((i - 1 + DivisionList.Length) % DivisionList.Length);
      Change.Invoke(Instance);
    }

    public static implicit operator byte(Division division) => division.Number;
  }
}
