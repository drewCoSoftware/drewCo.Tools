//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2019 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace drewCo.Tools.CSV
{
    // ============================================================================================================================
    [DebuggerDisplay("{Name} ({Index})")]
    public class CSVColumnMapping
    {
        public int Index { get; set; }
        public string Name { get; set; }

        // --------------------------------------------------------------------------------------------------------------------------
        public static List<CSVColumnMapping> CreateFromType<T>(params string[] excludeNames)
        {
            var res = new List<CSVColumnMapping>();

            var t = typeof(T);
            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            int index = 0;
            foreach (var p in props)
            {
                if (excludeNames.Contains(p.Name)) { continue; }
                res.Add(new CSVColumnMapping()
                {
                    Index = index,
                    Name = p.Name
                });
                ++index;
            }

            return res;
        }
    }

    // ============================================================================================================================
    /// <summary>
    /// Associates names to indexes, etc. for extracting data from a csv line.
    /// </summary>
    public class CSVColumnMap
    {
        private Dictionary<string, int> NamesToIndexes = null;
        public CSVColumnMap(List<CSVColumnMapping> mappings_)
        {
            NamesToIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in mappings_)
            {
                NamesToIndexes.Add(m.Name, m.Index);
            }
        }

        public int Count { get { return NamesToIndexes.Count; } }

        private List<string> _Names = null;
        public List<string> Names
        {
            get
            {
                return _Names ?? (_Names = NamesToIndexes.Keys.ToList());
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public int GetIndex(string name)
        {
            if (!NamesToIndexes.TryGetValue(name, out int res))
            {
                return -1;
            }
            return res;
        }
    }

}
