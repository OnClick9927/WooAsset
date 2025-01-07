using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine;

namespace WooAsset
{
    [System.Serializable]
    public class TypeSelect
    {
        [NonSerialized] public string[] types;
        [NonSerialized] public Type[] realTypes;
        [NonSerialized] public string[] shortTypes;
        [NonSerialized] public Type baseType;

        [UnityEngine.SerializeField] private string _select;
        public int typeIndex
        {
            get
            {
                if (string.IsNullOrEmpty(_select))
                {
                    try
                    {
                        if (types != null && types.Length > 0)
                            _select = types[0];
                        return 0;
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
                return Array.IndexOf(types, _select);
            }
            set
            {

                var _index = Mathf.Clamp(value, 0, types.Length);
                if (types != null && types.Length > 0)
                    _select = types[_index];
            }
        }
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
            realTypes = list.ToArray();
            types = list.Select(type => type.FullName).ToArray();
            shortTypes = list.Select(type => type.Name).ToArray();
        }
        public Type GetSelectType()
        {
            return realTypes[typeIndex];
        }

        public bool SetType(Type type)
        {
            string name = type.FullName;
            if (type.IsAbstract || !baseType.IsAssignableFrom(type)) return false;
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == name)
                {
                    _select = name;
                    return true;
                }
            }
            return false;
        }

    }
}
