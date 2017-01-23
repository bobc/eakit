using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using RMC;
using RMC.Classes;


namespace RMC.Dialogs
{
    public delegate void AddButtonEventHandler(object sender, EventArgs e, ref bool bAdd, ref string NewString);
    public delegate void ModifyButtonEventHandler(object sender, EventArgs e, ref bool bModify, ref string CurrentValue);

    public partial class ListDialog : Form
    {
        public event AddButtonEventHandler OnAddButton;
        public event ModifyButtonEventHandler OnModifyButton;
        //public event EventHandler OnDeleteButton;

        //public event EventHandler OnOkButton;
        //public event EventHandler OnCancelButton;

        public ItemList Items;

        public ListDialog()
        {
            InitializeComponent();
        }

        public void ActivateGrid()
        {
            // space columns equally
            int ColWidth = listView1.ClientSize.Width / listView1.Columns.Count;

            foreach (ColumnHeader Col in listView1.Columns)
            {
                Col.Width = ColWidth;
            }
        }

        public void AddColumn(string Name)
        {
            listView1.Columns.Add (Name, 150);
        }

        public ListViewItem NewListViewItem (string DsvString)
        {
            // this assumes 1:1 correspondence between Columns and Fields
            return new ListViewItem(StringUtils.SplitDsvText(DsvString, ","));
        }

        public string GetRow(int RowNum)
        {
            //int ColIndex = 0;
            //string[] strings = new string[listView1.Columns.Count];
            //ListViewItem lvi = listView1.Items[RowNum];
            
            //for (ColIndex = 0; ColIndex < listView1.Columns.Count; ColIndex++ )
            //{
            //    strings[ColIndex] = lvi.SubItems[ColIndex].Text;
            //}

            //return StringUtils.JoinDsvText(strings, ",");

            return Items.Items[RowNum];
        }

        public void SetItems (IEnumerable ItemList)
        {
            Items = new ItemList();
            Items.SetItems(ItemList);

            listView1.Items.Clear();
            foreach (string str in ItemList)
                listView1.Items.Add( NewListViewItem (str));
        }

        //public TStringList GetItems()
        //{
        //    TStringList result = new TStringList();
        //    for (int Index = 0; Index < listView1.Items.Count; Index++ )
        //        result.Add(GetRow(Index));
        //    return result;
        //}

        public void GetItems(IList List)
        {
            List.Clear();
            for (int Index = 0; Index < Items.Items.Count; Index++)
                List.Add(Items.Items[Index]);
        }

        public ItemList GetItemList()
        {
            return Items;
        }

        public int SelectedIndex 
        {
            get 
            { 
                if (listView1.SelectedIndices.Count == 0)
                    return -1;
                else
                    return listView1.SelectedIndices[0];
	        }
            set 
            {
                if (value < 0)
                    listView1.SelectedIndices.Clear();
                else if (listView1.SelectedIndices.Count == 0)
                    listView1.SelectedIndices.Add (value);
                else if (listView1.SelectedIndices[0] != value)
                {
                    listView1.SelectedIndices.Clear();
                    listView1.SelectedIndices.Add(value);
                }
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            bool bAdd = false;
            string NewString = "";

            if (OnAddButton != null)
            {
                OnAddButton(sender, e, ref bAdd, ref NewString);
                if (bAdd)
                {
                    Items.Add(NewString);
                    listView1.Items.Add( NewListViewItem (NewString));
                }
            }
        }

        private void buttonModify_Click(object sender, EventArgs e)
        {
            if (OnModifyButton != null)
            {
                if (SelectedIndex >= 0)
                {
                    bool bModify = false;
                    string CurrentValue = GetRow(SelectedIndex);

                    OnModifyButton(sender, e, ref bModify, ref CurrentValue);

                    if (bModify)
                    {
                        Items.Items[SelectedIndex] = CurrentValue;
                        listView1.Items[SelectedIndex] = NewListViewItem (CurrentValue);
                    }
                }
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (SelectedIndex != -1)
            {
                Items.Items.RemoveAt(SelectedIndex);
                listView1.Items.RemoveAt(SelectedIndex);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndex >= 0)
            {
                if (!buttonModify.Enabled)
                {
                    buttonModify.Enabled = true;
                    buttonDelete.Enabled = true;
                    buttonMoveUp.Enabled = true;
                    buttonMoveDown.Enabled = true;
                }
            }
            else
            {
                if (buttonModify.Enabled)
                {
                    buttonModify.Enabled = false;
                    buttonDelete.Enabled = false;
                    buttonMoveUp.Enabled = false;
                    buttonMoveDown.Enabled = false;
                }
            }
        }

        private void listBox1_Enter(object sender, EventArgs e)
        {

        }

        private void ListDialog_Shown(object sender, EventArgs e)
        {
            listBox1_SelectedIndexChanged(sender, e);
        }

        private void buttonMoveUp_Click(object sender, EventArgs e)
        {
            if (SelectedIndex > 0)
            {
                listView1.BeginUpdate();
                ListViewItem MoveItem = listView1.SelectedItems[0];
                int CurIndex = SelectedIndex;

                Items.MoveUp(CurIndex);

                listView1.Items.RemoveAt(CurIndex);
                listView1.Items.Insert(CurIndex - 1, MoveItem);
                SelectedIndex = CurIndex - 1;
                listView1.EndUpdate();
            }
        }

        private void buttonMoveDown_Click(object sender, EventArgs e)
        {
            if (SelectedIndex < listView1.Items.Count-1)
            {
                ListViewItem MoveItem = listView1.SelectedItems[0];
                int CurIndex = SelectedIndex;

                Items.MoveDown(CurIndex);

                listView1.BeginUpdate();
                listView1.Items.RemoveAt(CurIndex);
                listView1.Items.Insert(CurIndex + 1, MoveItem);
                listView1.EndUpdate();
                SelectedIndex = CurIndex + 1;
            }

        }

        private void stringGrid1_SelectionChanged(object sender, EventArgs e)
        {
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1_SelectedIndexChanged(sender, e);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            buttonModify_Click(sender, e);
        }

    }
}