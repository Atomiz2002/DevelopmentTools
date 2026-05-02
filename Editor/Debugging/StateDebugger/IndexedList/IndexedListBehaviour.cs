#if UNITY_EDITOR && !SIMULATE_BUILD
namespace DevelopmentTools.DevelopmentTools.Editor.Debugging.StateDebugger.IndexedList {

    public enum IndexedListBehaviour {

        Throw,
        Default,
        Loop,
        Clamp,

    }

}
#endif