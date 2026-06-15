using System;
using System.Collections.Generic;
using System.Linq;

namespace DevelopmentTools.Editor.ContextMenus {

    [Serializable]
    public class LinkedPropertyGroup {

        public string               Name;
        public List<LinkedProperty> LinkedProperties = new();

        public void Add(LinkedProperty linkedProperty)      => LinkedProperties.Add(linkedProperty);
        public void Remove(LinkedProperty linkedProperty)   => LinkedProperties.RemoveAll(lp => lp.GlobalId.Equals(linkedProperty.GlobalId) && lp.PropertyPath == linkedProperty.PropertyPath);
        public bool Contains(LinkedProperty linkedProperty) => LinkedProperties.Any(lp => lp.GlobalId.Equals(linkedProperty.GlobalId) && lp.PropertyPath == linkedProperty.PropertyPath);

    }

}