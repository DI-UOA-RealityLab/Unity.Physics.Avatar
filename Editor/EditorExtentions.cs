namespace NKUA.DI.RealityLab.Editor
{
    using System;
    using System.Linq;

    public static class EditorExtentions
    {
        public static int GetIndexFromArrayPropertyPath(this string propertyPath)
        {
            return Convert.ToInt32(new string(propertyPath.Where(c => char.IsDigit(c)).ToArray()));
        }
    }
}
