using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

//using VCLClasses;

namespace RMC.Classes
{
    // XML serializeable list of strings
    [Serializable]
    public class ItemList : Object, IEnumerable
    {
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public int Version;

        public List<string> Items;

        public ItemList()
        {
            Items = new List<string>();
        }

        public void Add(string item)
        {
            Items.Add(item);
        }

        // IEnumerable
        public IEnumerator GetEnumerator()
        {
            return new ItemListEnumerator(this);
        }

        // This is needed for XMLserialize on an Enumerable object
        public void Add(object AObject)
        {
            Add((string)AObject);
        }

        // assign from enumerable source
        public void SetItems(IEnumerable Source)
        {
            Items.Clear();
            foreach (string str in Source)
                Items.Add(str);
        }

        public void MoveUp(int Index)
        {
            if (Index > 0)
            {
                string MoveItem = Items[Index];
                Items.RemoveAt(Index);
                Items.Insert(Index - 1, MoveItem);
            }
        }

        public void MoveDown(int Index)
        {
            if (Index < Items.Count - 1)
            {
                string MoveItem = Items[Index];
                Items.RemoveAt(Index);
                Items.Insert(Index + 1, MoveItem);
            }
        }

        public void LoadFromFile(string FileName)
        {
            //List<string> list = new List<string>();
            string [] data = File.ReadAllLines(FileName);
            //list.LoadFromFile(FileName);
            SetItems(data);
        }

        public void SaveToFile(string FileName)
        {
            string [] data = new string [Items.Count];
            int j = 0;
            foreach (string item in Items)
                data[j++] = item;
            //list.SaveToFile(FileName);
            File.WriteAllLines (FileName, data);
        }
    }

    // Enumerator class for ItemList
    public class ItemListEnumerator : Object, IEnumerator
    {
        private int FIndex = -1;
        private ItemList FItemList;

        internal ItemListEnumerator(ItemList AItemList)
        {
            FItemList = AItemList;
        }

        public Object Current
        {
            get { return FItemList.Items[FIndex]; }
        }

        public bool MoveNext()
        {
            if (FIndex < FItemList.Items.Count)
                ++FIndex;

            return FIndex < FItemList.Items.Count;
        }

        public void Reset()
        {
            FIndex = -1;
        }
    }
}
