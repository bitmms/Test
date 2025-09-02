using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Common
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var field = value.GetType().GetField(value.ToString());

            if (field == null)
            {
                throw new ArgumentException(string.Format(
                    "Enum type '{0}' does not contain a definition for the value '{1}'",
                    value.GetType(), value));
            }

            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}

