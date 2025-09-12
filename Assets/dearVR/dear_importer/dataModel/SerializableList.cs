using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialConnect.dearVRAnimations
{
    [Serializable]
    public class SerializableList<T>
    {
        [SerializeField] private List<T> list;
        public List<T> List
        {
            get { return list;}
        }

        public SerializableList()
        {
            this.list = new List<T>();
        }

        public T this[int key]
        {
            get { return list[key]; }
            set { list[key] = value; }
        }

        public int Count
        {
            get { return list.Count; }
        }

        public void Add(T item)
        {
            list.Add(item);
        }
    }
}
