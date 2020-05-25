using CrossoutLogView.Common;

using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace CrossoutLogView.Database.Reflection
{
    public class MemberDefinitionData
    {
        public MemberDefinitionData(VariableInfo[] variableInfos, string header)
        {
            VariableInfos = variableInfos;
            Header = header;
        }

        public MemberDefinitionData(FieldInfo[] fieldInfos, PropertyInfo[] propInfos, string header)
        {

            VariableInfos = new VariableInfo[fieldInfos.Length + propInfos.Length];
            int i = 0;
            foreach (var fi in fieldInfos)
            {
                VariableInfos[i++] = VariableInfo.FromFieldInfo(fi);
            }
            foreach (var pi in propInfos)
            {
                VariableInfos[i++] = VariableInfo.FromPropertyInfo(pi);
            }
            Header = header;
        }

        public VariableInfo[] VariableInfos { get; }
        public string Header { get; }


        private static ConcurrentDictionary<Guid, MemberDefinitionData> memberDefintionData = new ConcurrentDictionary<Guid, MemberDefinitionData>();
        public static MemberDefinitionData FromType(Type type)
        {
            //try to return cached data
            if (memberDefintionData.TryGetValue(type.GUID, out var data)) return data;
            var varInfos = VariableInfo.FromType(type);
            var columns = new string[varInfos.Length];
            var sortingKeys = new int[columns.Length];
            //prepare header and sortingKeys
            for (int i = 0; i < varInfos.Length; i++)
            {
                columns[i] = varInfos[i].Name.ToLowerInvariant();
                sortingKeys[i] = i;
            }
            Array.Sort(columns, sortingKeys);
            varInfos = varInfos.SortByKeys(sortingKeys);
            data = new MemberDefinitionData(varInfos, String.Join(", ", columns));
            memberDefintionData.AddOrUpdate(type.GUID, data, (guid, memb) => data);
            return data;
        }
    }
}
