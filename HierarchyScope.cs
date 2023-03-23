namespace CascadeEventFramework
{
    public class HierarchyScope
    {
        class Template<T, Q> where T : Item { }
        public HierarchyScope(string propertyInfo = null)
        {
            if (propertyInfo != null)
                Stack(propertyInfo);
        }
        public HierarchyScope Stack(string propertyInfo)
        {
            Hierarchy += propertyInfo + "\n";
            return this;
        }
        public IEnumerable<string> HierarchySplit => Hierarchy.Split('\n');
        public string Hierarchy { get; set; } = string.Empty;
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj == this) return true;
            var objHierarchy = obj as HierarchyScope;
            if (objHierarchy is null)
                return false;
            return Hierarchy == objHierarchy.Hierarchy;
        }
        public override int GetHashCode() => Hierarchy.GetHashCode();
    }
}
