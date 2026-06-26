using System;

namespace DevelopmentTools.Editor.Helpers {

    public class FieldEntry {
        public string               label;
        public object               value;
        public Func<object, bool>   validation;
        public Func<object, string> error;
        public bool                 errored;

        public FieldEntry(string label, object value) {
            this.label = label;
            this.value = value;
        }

        public FieldEntry(string label, object value, Func<object, bool> validation, Func<object, string> error) : this(label, value) {
            this.validation = validation;
            this.error      = error;
        }

    }

}