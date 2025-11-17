using Studio.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.Helpers
{
    class AccountListItemComparer : IComparer
    {
        private string _property;
        private ListSortDirection _direction;

        public AccountListItemComparer(string property, ListSortDirection direction)
        {
            _property = property;
            _direction = direction;
        }

        public int Compare(object x, object y)
        {
            var item1 = (ProfileV2)x;
            var item2 = (ProfileV2)y;

            var val1 = GetNestedPropertyValue(item1, _property) as IComparable;
            var val2 = GetNestedPropertyValue(item2, _property) as IComparable;

            if (val1 == null && val2 == null) return 0;
            if (val1 == null) return 1; // val1 null, put after val2
            if (val2 == null) return -1; // val2 null, put after val1

            int result = val1.CompareTo(val2);
            return _direction == ListSortDirection.Ascending ? -result : result;
        }

        public static object GetNestedPropertyValue(object obj, string propertyPath)
        {
            foreach (var part in propertyPath.Split('.'))
            {
                if (obj == null) return null;

                var type = obj.GetType();
                var propInfo = type.GetProperty(part);
                if (propInfo == null) return null;

                obj = propInfo.GetValue(obj);
            }

            return obj;
        }
    }
}
