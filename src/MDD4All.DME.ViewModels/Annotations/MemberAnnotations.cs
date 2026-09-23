using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MDD4All.DME.ViewModels.Annotations
{
    // What a data model has written onto one place in itself - a property, or a type for the
    // nodes that hang on no property at all.
    //
    // Read once when the node is built. GetCustomAttributes hands back every attribute in one
    // call and builds each of them, so asking twice for two of them means building all of them
    // twice. Sorting the one result is cheaper and says more.
    //
    // The attributes are kept, never what is shown from them. DisplayAttribute holds a resource
    // key and a resource type, both of which outlive any language; the text is worked out on
    // every render and must not be stored here.
    public class MemberAnnotations
    {
        public MemberAnnotations(PropertyInfo property)
            : this(property.GetCustomAttributes(false))
        {
        }

        public MemberAnnotations(Type type)
            : this(type.GetCustomAttributes(false))
        {
        }

        private MemberAnnotations(object[] attributes)
        {
            List<ValidationAttribute> rules = new List<ValidationAttribute>();

            foreach (object attribute in attributes)
            {
                // Not else-if: DataTypeAttribute is a ValidationAttribute as well, so
                // [DataType(DataType.EmailAddress)] both picks the input and checks the value.
                if (attribute is DisplayAttribute display)
                {
                    Display = display;
                }

                if (attribute is DataTypeAttribute dataType)
                {
                    DataType = dataType;
                }

                if (attribute is ValidationAttribute rule)
                {
                    rules.Add(rule);
                }
            }

            Rules = rules;
        }

        // The label, as the model declared it. Never more than one - AllowMultiple is false.
        public DisplayAttribute? Display { get; }

        // Which input control fits. Never more than one, for the same reason.
        public DataTypeAttribute? DataType { get; }

        // Every rule on this member, in the order they were declared.
        public IReadOnlyList<ValidationAttribute> Rules { get; }

        // Nothing to ask means nothing to check - most members have no rules at all, and this
        // saves building a context for them.
        public bool HasRules
        {
            get
            {
                return Rules.Count > 0;
            }
        }

        // Which rules the value breaks - the attributes themselves, not sentences. A sentence
        // is worded in whatever language is set when it is asked for, and that changes while
        // the editor runs; the broken rule does not. Same split as with [Display].
        //
        // Each attribute checks itself: Range knows what a range is, and so does a rule a data
        // model brought along that this editor has never heard of. Empty means the value passes.
        public IReadOnlyList<ValidationAttribute> Validate(object? value)
        {
            List<ValidationAttribute> broken = new List<ValidationAttribute>();

            if (HasRules)
            {
                foreach (ValidationAttribute rule in Rules)
                {
                    if (!rule.IsValid(value))
                    {
                        broken.Add(rule);
                    }
                }
            }

            return broken;
        }
    }
}
