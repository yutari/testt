using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;
using test.Data;

namespace test.Converters
{
    public static class EntityConverter
    {
        public static List<SelectedEntityData> Convert(List<Entity> entities)
        {
            return entities.Select(e => ConvertOne(e)).ToList();
        }

        private static SelectedEntityData ConvertOne(Entity ent)
        {
            var raw = new Dictionary<string, object>
            {
                { "Handle", ent.Handle.ToString() },
                { "ObjectId", ent.ObjectId.ToString() },
                { "ClassName", ent.GetType().Name },
                { "Layer", ent.Layer },
                { "Color", ent.Color?.ColorNameForDisplay },
                { "Linetype", ent.Linetype },
                { "Lineweight", ent.LineWeight.ToString() }
            };

            return new SelectedEntityData
            {
                RawData = raw,
                Normalized = new Dictionary<string, object>() // để trống, sau này chuẩn hóa
            };
        }
    }
}
