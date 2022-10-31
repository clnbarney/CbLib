using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CbLib
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<(PropertyInfo sourceProperty, PropertyInfo targetProperty)> GetMatchingProperties(object source, object destination)
        {
            // Getting the Types of the objects
            Type srcType = source.GetType();
            Type destType = destination.GetType();

            return from srcProp in srcType.GetProperties()
                   let targetProperty = destType.GetProperty(srcProp.Name)
                   where srcProp.CanRead
                   && targetProperty != null
                   && (targetProperty.GetSetMethod(true) != null && !targetProperty.GetSetMethod(true).IsPrivate)
                   && (targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) == 0
                   && targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)
                   select (sourceProperty: srcProp, targetProperty: targetProperty);
        }

        public static List<(PropertyInfo property, object value)> GetPropertiesWithChangedValues(this object source, object destination)
        {
            var changedPropertyValues = new List<(PropertyInfo property, object value)>();

            // If either are null, return
            if (source == null || destination == null) { return changedPropertyValues; }

            var results = GetMatchingProperties(source, destination);

            foreach (var props in results)
            {
                var srcValue = props.sourceProperty.GetValue(source, null);
                var destValue = props.targetProperty.GetValue(destination, null);

                if (srcValue == null) { continue; }

                if (srcValue != destValue)
                {
                    changedPropertyValues.Add((props.targetProperty, destValue));
                }
            }

            return changedPropertyValues;
        }


        /// <summary>
        /// copies the value of the source object to the destination object, based on the property names being the same
        /// Option 0: Overwrite the destination property only if it is null/empty and the source property is not
        /// Option 1: Overwrite the destination property as long as the source property is not null/empty, regardless of the destination property being not null/empty.
        /// Option 2: Overwrite the destination property even if the source property is null/empty
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="allowNonNullOverwrites"></param>
        /// <param name="allowNullOverwrites"></param>
        public static void CopyProperties(this object source, object destination, int overwriteOption = 0)
        {
            // If either are null, return
            if (source == null || destination == null) { return; }

            // Collect all the valid properties to map
            var results = GetMatchingProperties(source, destination);

            //map the properties
            foreach (var props in results)
            {
                var srcValue = props.sourceProperty.GetValue(source, null);
                var destValue = props.targetProperty.GetValue(destination, null);
                var srcString = srcValue != null ? srcValue.ToString() : "";
                var destString = destValue != null ? destValue.ToString() : "";

                if (srcString == "0" || srcString == DateTime.MinValue.ToString())
                {
                    srcString = "";
                }

                if (destString == "0" || destString == DateTime.MinValue.ToString())
                {
                    destString = "";
                }

                // Any option: Allow overwriting only if the destination property is empty
                bool updateTarget = !srcString.IsEmpty() && destString.IsEmpty();


                // Option 1 or 2: Allow overwriting of the destination property as long as the source property is populated
                if (overwriteOption >= 1 && overwriteOption <= 2 && !updateTarget)
                {
                    updateTarget = !srcString.IsEmpty() && !destString.IsEmpty();
                }


                // Option 2: Allow overwriting even if the source property is null
                if (overwriteOption == 2 && !updateTarget)
                {
                    updateTarget = true;
                }


                // update the property value if the updateTarget is true
                if (updateTarget)
                {
                    props.targetProperty.SetValue(destination, srcValue, null);
                }


            }
        }



        public static void SetPropertyValueByName(this object source, string propName, object propValue)
        {
            if (source == null || propValue == null)
            {
                throw new Exception("Source object or property value not specified!");
            }

            Type srcType = source.GetType();
            PropertyInfo property = srcType.GetProperty(propName);

            if (property != null)
            {
                property.SetValue(source, propValue);
            }


        }


        public static void SetPropertyValueByType<T1, T2>(this T1 destination, T2 source, string propName = "", int overwriteOption = 0)
        {
            if (destination == null || source == null) { return; }

            Type destType = typeof(T1);
            Type srcType = typeof(T2);
            PropertyInfo propertyToSet = null;

            if (srcType != destType)
            {

                var propertiesWithSameType =
                    destType
                        .GetProperties().ToList()
                        .Where(x => x.PropertyType == srcType).ToList();

                if (propName != "")
                {
                    propertyToSet = propertiesWithSameType.Where(x => x.Name == propName).FirstOrDefault();
                }
                else if (propertiesWithSameType.Count >= 1)
                {
                    propertyToSet = propertiesWithSameType.FirstOrDefault();
                }

                if (propertyToSet != null)
                {

                    var destProp = propertyToSet.GetValue(destination);

                    propertyToSet.SetValue(destination, source);


                }
            }

            else if (destType == srcType)
            {
                source.CopyProperties(destination, 1);
            }




        }


        public static int GetCountOfNullProperties(this object source)
        {
            int nullCount = 0;
            Type srcType = source.GetType();
            var properties = srcType.GetProperties();

            if (srcType.Namespace.StartsWith("System"))
            {
                return 0;
            }

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source);
                var typeNameSpace = prop.PropertyType.Namespace.ToUpperInvariant();
                var typeName = prop.PropertyType.Name.ToUpperInvariant();
                var isNull = value == null;

                typeName = Regex.Replace(typeName, @"[\d-]", string.Empty);


                if (!isNull)
                {
                    if (typeNameSpace == "SYSTEM.COLLECTIONS.GENERIC")
                    {
                        var countProp = value.GetType().GetProperty("Count");
                        value = countProp.GetValue(value);
                        typeName = "INT";
                    }
                    else if (value.GetType().IsArray)
                    {
                        var countProp = value.GetType().GetProperty("Length");
                        value = countProp.GetValue(value);
                        typeName = "INT";
                    }


                    switch (typeName)
                    {
                        case "STRING":
                            isNull = value.ToString() == "";
                            break;
                        case "INT":
                            isNull = value.ToString() == "0";
                            break;
                        case "DATETIME":
                            isNull = value.ToString() == DateTime.MinValue.ToString();
                            break;
                        default:
                            if (!typeNameSpace.StartsWith("SYSTEM"))
                            {
                                nullCount += value.GetCountOfNullProperties();
                            }
                            break;

                    }
                }

                nullCount += (isNull ? 1 : 0);

            }

            return nullCount;
        }


        public static List<IGrouping<object, T>> GetDuplicatesByProperty<T>(this List<T> source, string propName)
        {
            Type typeSrc = source.FirstOrDefault().GetType();
            var property = typeSrc.GetProperty(propName);
            var duplicates = new List<IGrouping<object, T>>();

            if (property != null)
            {
                duplicates =
                    source
                    .GroupBy(
                       x => property.GetValue(x)
                    ).Where(x => x.Count() > 1).ToList();
            }

            return duplicates;
        }
    }
}
