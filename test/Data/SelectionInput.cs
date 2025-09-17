using System.Collections.Generic;

namespace test.Data
{
    public class SelectionInput
    {
        public List<string> EntityTypes { get; set; } = new List<string>();

        // Danh sách filter types (Layer, Colour, Block…)
        public List<string> FilterTypes { get; set; } = new List<string>();

        // Giá trị cụ thể cho từng filter type
        public Dictionary<string, List<string>> FilterValues { get; set; }
            = new Dictionary<string, List<string>>();

        public int Scope { get; set; }
    }
}

