using System;
using System.Collections.Generic;

namespace SaintCoinach.Ex.Relational {
    public class RelationalDataRow : DataRow, IRelationalDataRow {
        #region Fields
        private Dictionary<string, ExReference<object>> _ValueReferences = new Dictionary<string, ExReference<object>>();
        #endregion

        #region Constructors

        public RelationalDataRow(IRelationalDataSheet sheet, int key, int offset) : base(sheet, key, offset) { }

        #endregion

        public new IRelationalDataSheet Sheet { get { return (IRelationalDataSheet)base.Sheet; } }

        public override string ToString() {
            var defCol = Sheet.Header.DefaultColumn;
            return defCol == null
                       ? string.Format("{0}#{1}", Sheet.Header.Name, Key)
                       : string.Format("{0}", this[defCol.Index]);
        }

        #region IRelationalRow Members

        IRelationalSheet IRelationalRow.Sheet { get { return Sheet; } }

        public object DefaultValue {
            get {
                var defCol = Sheet.Header.DefaultColumn;
                return defCol == null ? null : this[defCol.Index];
            }
        }

        public object this[string columnName] {
            get {
                ExReference<object> valRef;
                object val;
                var hasRef = _ValueReferences.TryGetValue(columnName, out valRef);
                if (hasRef && valRef.TryGetTarget(out val))
                    return val;

                var col = Sheet.Header.FindColumn(columnName);
                if (col == null)
                    throw new KeyNotFoundException();
                val = this[col.Index];

                if (hasRef)
                    valRef.SetTarget(val);
                else
                    _ValueReferences.Add(columnName, new ExReference<object>(Sheet.Collection, val));
                return val;
            }
        }

        object IRelationalRow.GetRaw(string columnName) {
            var column = Sheet.Header.FindColumn(columnName);
            if (column == null)
                throw new KeyNotFoundException();
            return column.ReadRaw(Sheet.GetBuffer(), this);
        }

        #endregion
    }
}
