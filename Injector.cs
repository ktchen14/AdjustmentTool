#if DEBUG
using System.Diagnostics;
using UnityEngine;

namespace AdjustmentTool {
  [KSPAddon(KSPAddon.Startup.EveryScene, false)]
  public class Injector : MonoBehaviour {
    public bool active;

    private void Update() {
      if (!active)
        return;
      Debugger.Break();
    }
  }
}
#endif
