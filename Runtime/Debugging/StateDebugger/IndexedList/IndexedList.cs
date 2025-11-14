#if UNITY_EDITOR && !SIMULATE_BUILD
using System.Collections.Generic;
using UnityEngine;

namespace DevelopmentTools.Debugging.StateDebugger.IndexedList {

    public class IndexedList<T> : List<T> {

        private int i;

        public int I {
            get => i;
            set => i = Behave(value);
        }

        public T Prev => this[I - 1];
        public T Curr => this[I];
        public T Next => this[I + 1];

        public bool IsFirst    => i == 0;
        public bool IsLast     => i == Count - 1;
        public bool IsNotFirst => !IsFirst;
        public bool IsNotLast  => !IsLast;

        public IndexedListBehaviour Behaviour;

        public IndexedList(IndexedListBehaviour behaviour) => Behaviour = behaviour;

        private int Behave(int i) {
            return Behaviour switch {
                IndexedListBehaviour.Loop  => Count == 0 ? 0 : (i + Count) % Count,
                IndexedListBehaviour.Clamp => Mathf.Clamp(i, 0, Count - 1),
                _                          => i
            };
        }

        public new T this[int i] {
            get {
                if (Count == 0)
                    return default;

                i = Behave(i);

                if (0 <= i && i < Count && Behaviour == IndexedListBehaviour.Default)
                    return default;

                return base[i];
            }
            set {
                if (Count == 0)
                    return;

                base[Behave(i)] = value;
            }
        }

        public static implicit operator T(IndexedList<T> source) => source[source.i];

    }

}
#endif