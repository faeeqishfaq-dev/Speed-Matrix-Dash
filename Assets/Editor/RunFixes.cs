using UnityEngine;
using UnityEditor;
using GeometryDash.Level;

namespace GeometryDash.Editor
{
    public static class RunFixes
    {
        public static void RunAll()
        {
            PhaseSixBaselineFix.FixLevelBaselines();
            // FixLevelEnds.FixEnds();
            Debug.Log("[RUNFIXES] Complete!");
        }
    }
}
