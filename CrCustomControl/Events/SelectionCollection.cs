using System;
using System.Collections.Generic;
using System.Text;

namespace Nixxis.Windows.Controls
{
    /*
     * String collection (V1)   
     */
    public class ItemEventArgsV1
    {
        #region Class Data
        private string _Item;
        private string _MetaData;
        #endregion

        #region Constructor
        public ItemEventArgsV1(string item, string metaData)
        {
            this._Item = item;
            this._MetaData = metaData;
        }
        #endregion

        #region Properties

        public string Item
        {
            get { return this._Item; }
            set { this._Item = value; }
        }

        public string MetaData
        {
            get { return this._MetaData; }
            set { this._MetaData = value; }
        }
        #endregion
    }

    public delegate void StringClickItemEventHandlerV1(ItemEventArgsV1 e);
    public delegate void StringBeforeAddItemEventHandlerV1(ItemEventArgsV1 e);
    /* 
     * Object collection (V2)
     */
    public class ItemInfoEventArgs
    {
        #region Class Data
        private object _Item;
        private string _Id;
        #endregion

        #region Constructor
        public ItemInfoEventArgs(object item, string id)
        {
            this._Item = item;
            this._Id = id;
        }
        #endregion

        #region Properties

        public object Item
        {
            get { return this._Item; }
            set { this._Item = value; }
        }

        public string Id
        {
            get { return this._Id; }
            set { this._Id = value; }
        }
        #endregion
    }
    public class ItemLimintReachedEventArgs
    {
        #region Class Data
        private object _Item;
        private int _MaxItems;
        #endregion

        #region Constructor
        public ItemLimintReachedEventArgs(object item, int maxItems)
        {
            this._Item = item;
            this._MaxItems = maxItems;
        }
        #endregion

        #region Properties

        public object Item
        {
            get { return this._Item; }
            set { this._Item = value; }
        }

        public int MaxItems
        {
            get { return this._MaxItems; }
            set { this._MaxItems = value; }
        }
        #endregion
    }

    public delegate void ObjectClickItemEventHandler(ItemInfoEventArgs e);
    public delegate void ObjectBeforeAddItemEventHandler(ItemInfoEventArgs e);
    public delegate void ObjectAfterAddItemEventHandler(ItemInfoEventArgs e);
    public delegate void ObjectDeleteItemEventHandler(ItemInfoEventArgs e);
    public delegate void MaxItemsReachedEventHandler(ItemLimintReachedEventArgs e);
}
