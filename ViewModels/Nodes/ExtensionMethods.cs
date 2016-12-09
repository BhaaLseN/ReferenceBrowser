namespace ReferenceBrowser.ViewModels.Nodes
{
    public static class ExtensionMethods
    {
        public static bool IsNotFalse(this bool? nullable)
        {
            return !nullable.HasValue || nullable.Value;
        }
        public static bool IsNotTrue(this bool? nullable)
        {
            return !nullable.HasValue || !nullable.Value;
        }
        public static bool IsFalse(this bool? nullable)
        {
            return nullable.HasValue && !nullable.Value;
        }
        public static bool IsTrue(this bool? nullable)
        {
            return nullable.HasValue && nullable.Value;
        }
    }
}
