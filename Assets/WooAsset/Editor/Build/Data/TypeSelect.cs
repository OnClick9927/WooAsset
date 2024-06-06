using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;

namespace WooAsset
{
    [System.Serializable]
    public class TypeSelect
    {
        public string[] types;
        public string[] shortTypes;
        public int typeIndex;
        public Type baseType;
        public static IEnumerable<Type> GetSubTypesInAssemblies(Type self)
        {
            if (self.IsInterface)
            {
                return from item in AppDomain.CurrentDomain.GetAssemblies().SelectMany((item) => item.GetTypes())
                       where item.GetInterfaces().Contains(self)
                       select item;
            }

            return from item in AppDomain.CurrentDomain.GetAssemblies().SelectMany((item) => item.GetTypes())
                   where item.IsSubclassOf(self)
                   select item;
        }
        public void Enable()
        {
            var list = GetSubTypesInAssemblies(baseType)
           .Where(type => !type.IsAbstract);
            types = list.Select(type => type.FullName).ToArray();
            shortTypes = list.Select(type => type.Name).ToArray();
        }
        public Type GetSelectType()
        {
            var type_str = types[typeIndex];
            Type type = GetSubTypesInAssemblies(baseType)
               .Where(type => !type.IsAbstract)
               .ToList()
               .Find(x => x.FullName == type_str);

            return type;
        }

        public bool SetType(Type type)
        {
            string name = type.FullName;
            if (type.IsAbstract || !baseType.IsAssignableFrom(type)) return false;
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == name)
                {
                    typeIndex = i;
                    return true;
                }
            }
            return false;
        }
    }
}
