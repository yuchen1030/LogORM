using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Reflection;
using System.Text;

namespace LogORM.ComUtil
{
    public class DtModelConvert<T> //where T : class
    {
        public static Dictionary<string, object> GetPropertity(object model)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            List<string> list = new List<string>();
            Type t = model.GetType();
            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object obj = property.GetValue(model, null);
                dic.Add(property.Name, property.GetValue(model, null));
            }
            return dic;
        }

        public static List<T> DatatableToList(DataTable dt)
        {
            List<T> list = new List<T>();
            if (dt == null || dt.Rows.Count <= 0)
            {
                return list;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                list.Add(DataRowToModel(dt.Rows[i]));
            }
            return list;
        }

        static T DataRowToModel(DataRow dr)
        {
            if (dr == null)
            {
                return default(T);
            }
            return TableRowToModel(dr);
        }

        static T TableRowToModel(DataRow dtRow)
        {
            T objmodel = System.Activator.CreateInstance<T>();// default(T);
            //获取model的类型
            Type modelType = typeof(T);
            //获取model中的属性
            PropertyInfo[] modelpropertys = modelType.GetProperties();
            //遍历model每一个属性并赋值DataRow对应的列
            foreach (PropertyInfo pi in modelpropertys)
            {
                //获取属性名称
                String name = pi.Name;
                if (dtRow.Table.Columns.Contains(name))
                {
                    try
                    {
                        //非泛型
                        if (!pi.PropertyType.IsGenericType)
                        {
                            if (pi.PropertyType.Name == "String")
                            {
                                pi.SetValue(objmodel, string.IsNullOrEmpty(dtRow[name].ToString()) ? null : Convert.ChangeType(dtRow[name].ToString(), pi.PropertyType), null);
                            }
                            else if (pi.PropertyType.BaseType.Name == "Enum")
                            {
                                pi.SetValue(objmodel, Enum.Parse(pi.PropertyType, dtRow[name].ToString()), null);
                                // pi.SetValue(objmodel, Convert.ChangeType(Enum.Parse(pi.PropertyType, dtRow[name].ToString()), pi.PropertyType), null);

                            }
                            else if (pi.PropertyType.Name == "SqlXml")
                            {
                                var str = dtRow[name].ToString();
                                SqlXml temp = null;
                                if (!string.IsNullOrEmpty(str))
                                {
                                    StringReader Reader = new StringReader(str);
                                    byte[] byteArray = Encoding.UTF8.GetBytes(str);
                                    MemoryStream stream = new MemoryStream(byteArray);
                                    temp = new SqlXml(stream);
                                }

                                pi.SetValue(objmodel, temp, null);
                                //  pi.SetValue(objmodel, Convert.ChangeType(temp, pi.PropertyType), null);
                            }
                            else
                            {
                                pi.SetValue(objmodel, string.IsNullOrEmpty(dtRow[name].ToString()) ? null : Convert.ChangeType(dtRow[name], pi.PropertyType), null);
                            }
                        }
                        //泛型Nullable<>
                        else
                        {
                            Type genericTypeDefinition = pi.PropertyType.GetGenericTypeDefinition();
                            //model属性是可为null类型，进行赋null值
                            if (genericTypeDefinition == typeof(Nullable<>))
                            {
                                //返回指定可以为 null 的类型的基础类型参数
                                pi.SetValue(objmodel, string.IsNullOrEmpty(dtRow[name].ToString()) ? null : Convert.ChangeType(dtRow[name], Nullable.GetUnderlyingType(pi.PropertyType)), null);
                            }
                        }

                    }
                    catch
                    //(Exception ex)
                    {
                        //  LogCom.WriteExceptToFile(ex, "TableRowToModel");
                    }


                }
            }
            return objmodel;
        }

        public static DataRow ObjConvertToDr(T obj, System.Data.DataRow drTemp)
        {
            try
            {
                PropertyInfo[] propertyInfos = obj.GetType().GetProperties();
                foreach (PropertyInfo prop in propertyInfos)
                {
                    if (drTemp.Table.Columns.Contains(prop.Name))
                    {
                        object objVal = prop.GetValue(obj, null);
                        drTemp[prop.Name] = objVal == null ? DBNull.Value : objVal;
                    }
                }
            }
            catch (Exception e)
            {
                drTemp = null;
                throw new Exception("Err-ObjConvertToDr:" + e.Message);
            }
            return drTemp;
        }


        public static string SerializeToString(object source)
        {
            var str = JsonConvert.SerializeObject(source);
            return str;
        }

        public static T Deserialize(string str)
        {
            try
            {
                if (str.StartsWith("["))
                {
                    // return JsonConvert.DeserializeObject<List<T>>(str);
                }
                var res = JsonConvert.DeserializeObject<T>(str);
                return res;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        //对象深度复制
        public static T DeepClone(object source)
        {
            //if (!typeof(T).IsSerializable)
            //{
            //    throw new ArgumentException("The type must be serializable.", "source");
            //}

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            try
            {
                var str = SerializeToString(source);
                var res = Deserialize(str);
                return res;
            }
            catch (Exception ex)
            {
                ////深度复制失败，记录日志
                return default(T);
                //return (T)source;
            }

            //IFormatter formatter = new BinaryFormatter();
            //Stream stream = new MemoryStream();
            //using (stream)
            //{
            //    formatter.Serialize(stream, source);
            //    stream.Seek(0, SeekOrigin.Begin);
            //    return (T)formatter.Deserialize(stream);
            //}

        }


    }

}
