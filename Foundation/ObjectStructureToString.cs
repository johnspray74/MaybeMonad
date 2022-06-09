﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Foundation
{

    // This is a dignostic aid for when you are building structure of objects as you d with wiring in ALA or Bind in monads.
    // It allows you to see the each object with its local fields and properties listed with their nmes, types, and values where possible.
    // If a field refers to another object, then that object will be shown indented.
    // So you wll get a tree structure.
    // TBD, detect of we have visted an object before to avoid duplication, or worse an infinite loop


    public static class FoundationExtensionMethods
    {


        public static string ObjectStructureToString<T>(this T instance) where T : class
        {

            if (instance == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append("Class ");

            var type = instance.GetType();

            var typeName = type.Name;
            sb.Append(typeName);
            var instanceProperty = type.GetProperty("instanceName");
            if (instanceProperty != null)
            {
                sb.Append($" \"{instanceProperty.GetValue(instance)}\"");
            }
            if (instance.GetType().IsGenericType) sb.Append(" (Generic)");

            sb.AppendLine();
            sb.AppendLine(new string('=', sb.Length));


            var strListType = typeof(List<string>);
            var strArrType = typeof(string[]);

            var arrayTypes = new[] { strListType, strArrType };
            var handledTypes = new[] { typeof(Int32), typeof(String), typeof(bool), typeof(DateTime), typeof(double), typeof(decimal), strListType, strArrType };



            var propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var methodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            /*
            var max = 0;
            if (propertyInfos.Length >  0)
            {
                max = propertyInfos.Select((p) => p.Name.Length).Max();
            }
            if (fieldInfos.Length > 0)
            {
                var max2 = fieldInfos.Select((p) => p.Name.Length).Max();
                if (max2 > max) max = max2;
            }
            */

            foreach (var fieldInfo in fieldInfos)
            {
                string[] strings = { "_methodBase", "_methodPtr", "_methodPtrAux" };

                var name = fieldInfo.Name;
                if (!strings.Contains(name))
                {
                    sb.Append("Field ");
                    sb.Append(fieldInfo.ToString() + " ");
                    // sb.Append(fieldInfo.FieldType + " ");
                    // Ssb.Append(name + " ");
                    if (handledTypes.Contains(fieldInfo.FieldType))
                    {

                        if (fieldInfo.GetValue(instance) != null)
                        {
                            var s = arrayTypes.Contains(fieldInfo.FieldType)
                                ? string.Join(", ", (IEnumerable<string>)fieldInfo.GetValue(instance))
                                : fieldInfo.GetValue(instance).ToString();
                            sb.AppendLine(s);
                        }
                        else
                        {
                            sb.AppendLine("null");
                        }
                    }
                    else if (typeof(object).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        sb.AppendLine();
                        sb.Append(fieldInfo.GetValue(instance).ObjectStructureToString().Indent());
                    }
                    else
                    {
                        sb.AppendLine("GetValue not supported");
                    }
                }
            }
            foreach (var propertyInfo in propertyInfos)
            {
                sb.Append("Property ");
                sb.Append(propertyInfo.ToString() + " ");
                try
                {

                    if (propertyInfo.GetValue(instance, null) == null)
                    {
                        sb.AppendLine("null");
                    }
                    else
                    {
                        var s = arrayTypes.Contains(propertyInfo.PropertyType)
                                ? string.Join(", ", (IEnumerable<string>)propertyInfo.GetValue(instance, null))
                                : propertyInfo.GetValue(instance, null);
                        sb.AppendLine();
                    }
                }
                catch
                {
                    sb.AppendLine("Exception getting value");
                }
            }
            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.Name[0] == '<')
                {
                    sb.Append("Method ");
                    sb.AppendLine(methodInfo.ToString());
                }
            }
            return sb.ToString();

        }


        public static string Indent(this string s)
        {
            bool first = true;
            StringBuilder sb = new StringBuilder();
            foreach (var line in s.Split(Environment.NewLine))
            {
                if (!first) sb.AppendLine();
                if (line != "")
                {
                    sb.Append("\t" + line);
                    first = false;
                }
            }
            return sb.ToString();
        }

        public static string Join(this IEnumerable<string> s, string separator)
        {
            return s.Aggregate(new StringBuilder(), (sb, s) => { if (sb.Length > 0) sb.Append(separator); sb.Append(s); return sb; }).ToString();
        }

    }
}
