﻿using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Serialization;
using YamlDotNet.Serialization;

namespace ysoserial.Helpers
{
    class SerializersHelper
    {
        public static void ShowAll(object myobj)
        {
            ShowAll(myobj, myobj.GetType());
        }

        public static void ShowAll(object myobj, Type type)
        {
            try
            {
                Console.WriteLine("\n~~XmlSerializer:~~\n");
                Console.WriteLine(XmlSerializer_serialize(myobj, myobj.GetType()));
            }
            catch (Exception e)
            {
                Console.WriteLine("\tError in XmlSerializer!");
            }

            try
            {
                Console.WriteLine("\n~~DataContractSerializer:~~\n");
                Console.WriteLine(DataContractSerializer_serialize(myobj, myobj.GetType()));
            }
            catch (Exception e)
            {
                Console.WriteLine("\tError in DataContractSerializer!");
            }

            try
            {
                Console.WriteLine("\n~~Xaml:~~\n");
                Console.WriteLine(Xaml_serialize(myobj));
            }
            catch (Exception e)
            {
                Console.WriteLine("\tError in Xaml!");
            }


            try
            {
                Console.WriteLine("\n~~NetDataContractSerializer:~~\n");
                Console.WriteLine(NetDataContractSerializer_serialize(myobj));
            }
            catch (Exception e)
            {
                Console.WriteLine("\tError in NetDataContractSerializer!");
            }

            try
            {
                Console.WriteLine("\n~~JSON.NET:~~\n");
                Console.WriteLine(JsonNet_serialize(myobj));
            }
            catch (Exception e)
            {
                Console.WriteLine("\tError in JSON.NET!");
            }

            try
            {
                Console.WriteLine("\n~~SoapFormatter:~~\n");
                Console.WriteLine(SoapFormatter_serialize(myobj));
            }
            catch (Exception e)
            {
                Console.WriteLine("\tError in SoapFormatter!");
            }

            try
            {
                Console.WriteLine("\n~~BinaryFormatter:~~\n");
                Console.WriteLine(BinaryFormatter_serialize(myobj));
            }
            catch (Exception e)
            {
                Console.WriteLine("\tError in BinaryFormatter!");
            }

            try
            {
                Console.WriteLine("\n~~LosFormatter:~~\n");
                Console.WriteLine(LosFormatter_serialize(myobj));
            }
            catch (Exception e)
            {
                Console.WriteLine("\tError in LosFormatter!");
            }

            try
            {
                Console.WriteLine("\n~~ObjectStateFormatter:~~\n");
                Console.WriteLine(ObjectStateFormatter_serialize(myobj));
            }
            catch (Exception e)
            {
                Console.WriteLine("\tError in ObjectStateFormatter!");
            }

            try
            {
                Console.WriteLine("\n~~YamlDotNet:~~\n");
                Console.WriteLine(YamlDotNet_serialize(myobj));
            }
            catch (Exception e)
            {
                Console.WriteLine("\tError in YamlDotNet!");
            }

            try
            {
                Console.WriteLine("\n~~JavaScriptSerializer:~~\n");
                Console.WriteLine(JavaScriptSerializer_serialize(myobj));
            }
            catch (Exception e)
            {
                Console.WriteLine("\tError in JavaScriptSerializer!");
            }


        }

        public static void TestAll(object myobj)
        {
            TestAll(myobj, myobj.GetType());
        }

        public static void TestAll(object myobj, Type type)
        {
            XmlSerializer_test(myobj, type);
            DataContractSerializer_test(myobj, type);
            Xaml_test(myobj);
            NetDataContractSerializer_test(myobj);
            JsonNet_test(myobj);
            SoapFormatter_test(myobj);
            BinaryFormatter_test(myobj);
            LosFormatter_test(myobj);
            ObjectStateFormatter_test(myobj);
            YamlDotNet_test(myobj);
            JavaScriptSerializer_test(myobj);
        }

        public static void XmlSerializer_test(object myobj)
        {
            XMLSerializer_deserialize(XmlSerializer_serialize(myobj), myobj.GetType());
        }

        public static void XmlSerializer_test(object myobj, Type type)
        {
            try
            {
                XMLSerializer_deserialize(XmlSerializer_serialize(myobj, type), type);
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        public static string XmlSerializer_serialize(object myobj)
        {
            return XmlSerializer_serialize(myobj, myobj.GetType());
        }

        public static string XmlSerializer_serialize(object myobj, Type type)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(type);
            TextWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
            xmlSerializer.Serialize(stringWriter, myobj);
            string text = stringWriter.ToString();
            stringWriter.Close();
            return text;
        }

        public static object XMLSerializer_deserialize(string str, string type)
        {
            return XMLSerializer_deserialize(str, type, "" , "");
        }

        public static object XMLSerializer_deserialize(string str, string type, string rootElement, string typeAttributeName)
        {
            object obj = null; 

            if (!rootElement.Equals(""))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(str);
                XmlElement xmlItem = (XmlElement)xmlDoc.SelectSingleNode(rootElement);
                if (string.IsNullOrEmpty(typeAttributeName))
                {
                    typeAttributeName = "type";
                }
                var s = new XmlSerializer(Type.GetType(xmlItem.GetAttribute(typeAttributeName)));
                obj = s.Deserialize(new XmlTextReader(new StringReader(xmlItem.InnerXml)));
            }
            else
            {
                var s = new XmlSerializer(Type.GetType(type));
                obj = s.Deserialize(new XmlTextReader(new StringReader(str)));
            }
            
            return obj;
        }

        public static object XMLSerializer_deserialize(string str, Type type)
        {
            var s = new XmlSerializer(type);
            object obj = s.Deserialize(new XmlTextReader(new StringReader(str)));
            return obj;
        }

        // This to replace our bespoked marshal objects with the actual object
        // Example: when we use DataContractSerializer_serialize for TextFormattingRunPropertiesMarshal
        // it will add the rootTagName when rootTagName is not empty 
        // default for typeAttributeName is type
        public static string DataContractSerializer_Marshal_2_MainType(string dirtymarshal)
        {
            return DataContractSerializer_Marshal_2_MainType(dirtymarshal, "", "", null);
        }

        public static string DataContractSerializer_Marshal_2_MainType(string dirtymarshal, string rootTagName, string typeAttributeName, Type objectType)
        {
            string result = "";

            // Finding the namespace tag prefix of "http://schemas.microsoft.com/2003/10/Serialization/"
            Regex tagPrefixSerializationRegex = new Regex(@"xmlns:([\w]+)\s*=\s*""http://schemas.microsoft.com/2003/10/Serialization/""", RegexOptions.IgnoreCase);
            Match tagPrefixSerializationMatch = tagPrefixSerializationRegex.Match(dirtymarshal);
            if(tagPrefixSerializationMatch.Groups.Count > 1)
            {
                string tagPrefixSerialization = tagPrefixSerializationMatch.Groups[1].Value;
                if (!string.IsNullOrEmpty(tagPrefixSerialization))
                {
                    // Finding the main type using tagPrefixSerialization:FactoryType
                    Regex regexFactoryType = new Regex(tagPrefixSerialization + @":FactoryType\s*=\s*""([^:]+):([^""]+)""", RegexOptions.IgnoreCase);
                    Match matchFactoryType = regexFactoryType.Match(dirtymarshal);
                    if (matchFactoryType.Groups.Count > 2)
                    {
                        string factoryTypeFullString = matchFactoryType.Groups[0].Value;
                        string mainTypeTagPrefix = matchFactoryType.Groups[1].Value;
                        string mainTypeTagName = matchFactoryType.Groups[2].Value;
                        if(!string.IsNullOrEmpty(mainTypeTagName) && !string.IsNullOrEmpty(mainTypeTagPrefix))
                        {
                            // start replacing the dirty bits!

                            // we need to remove <?xml at the beginning if there is any
                            result = Regex.Replace(dirtymarshal, @"\s*\<\?xml[^\>]+\?\>", "", RegexOptions.IgnoreCase);

                            Regex regexMarshaledTagName = new Regex(@"^\s*<([^\s>]+)");
                            Match matchMarshaledTagName = regexMarshaledTagName.Match(result);
                            string marshaledTagName = matchMarshaledTagName.Groups[1].Value;
                            result = result.Replace(marshaledTagName, mainTypeTagName); // replacing the marshaled tag with the main tag
                            result = result.Replace(factoryTypeFullString, ""); // removing FactoryType bit
                            result = Regex.Replace(result, @"(?<=\<" + mainTypeTagName + @"[^>]+)\s+xmlns=""http://schemas.datacontract.org/[^""]+""", ""); // removing current namespace
                            result = result.Replace(":" + mainTypeTagPrefix, ""); // creating the new namespace

                            if (!string.IsNullOrEmpty(rootTagName) && objectType != null)
                            {
                                // adding the root type
                                if (string.IsNullOrEmpty(typeAttributeName))
                                {
                                    typeAttributeName = "type";
                                }

                                // we need this to make it standard
                                result = XMLMinifier.XmlXSLTMinifier(dirtymarshal);

                                result = "<" + rootTagName + " "+ typeAttributeName + @"=""" + objectType.AssemblyQualifiedName + @""">" + result + "</" + rootTagName + ">";
                            }

                        }
                    }
                        
                }
            }
            
            return result;
        }

        public static void DataContractSerializer_test(object myobj)
        {
            DataContractSerializer_deserialize(DataContractSerializer_serialize(myobj), myobj.GetType());
        }

        public static void DataContractSerializer_test(object myobj, Type type)
        {
            try
            {
                DataContractSerializer_deserialize(DataContractSerializer_serialize(myobj, type), type);
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        public static string DataContractSerializer_serialize(object myobj)
        {
            return DataContractSerializer_serialize(myobj, myobj.GetType());
        }

        public static string DataContractSerializer_serialize(object myobj, Type type)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create("test.xml", settings))
            {
                DataContractSerializer ser = new DataContractSerializer(type);
                ser.WriteObject(writer, myobj);
            }
            string text = File.ReadAllText("test.xml");
            return text;
        }

        public static object DataContractSerializer_deserialize(string str, string type)
        {
            return DataContractSerializer_deserialize(str, type, "", "");
        }

        public static object DataContractSerializer_deserialize(string str, string type, string rootElement, string typeAttributeName)
        {
            object obj = null;
            
            if (!rootElement.Equals(""))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(str);
                XmlElement xmlItem = (XmlElement)xmlDoc.SelectSingleNode(rootElement);
                if (string.IsNullOrEmpty(typeAttributeName))
                {
                    typeAttributeName = "type";
                }
                var s = new DataContractSerializer(Type.GetType(xmlItem.GetAttribute(typeAttributeName)));
                obj = s.ReadObject(new XmlTextReader(new StringReader(xmlItem.InnerXml)));
            }
            else
            {
                var s = new DataContractSerializer(Type.GetType(type));
                obj = s.ReadObject(new XmlTextReader(new StringReader(str)));
            }
            return obj;
        }

        public static object DataContractSerializer_deserialize(string str, Type type)
        {
            var s = new DataContractSerializer(type);
            object obj = s.ReadObject(new XmlTextReader(new StringReader(str)));
            return obj;
        }

        public static void Xaml_test(object myobj)
        {
            try
            {
                Xaml_deserialize(Xaml_serialize(myobj));
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        public static string Xaml_serialize(object myobj)
        {
            // return XamlWriter.Save(myobj); // we lose indentation here so:
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create("test.xaml", settings))
            {
                System.Windows.Markup.XamlWriter.Save(myobj, writer);
            }
            string text = File.ReadAllText("test.xaml");
            return text;
        }

        public static object Xaml_deserialize(string str)
        {
            object obj = XamlReader.Load(new XmlTextReader(new StringReader(str)));
            return obj;
        }

        // This to replace our bespoked marshal objects with the actual object
        // Example: when we use NetDataContractSerializer_serialize for TextFormattingRunPropertiesMarshal
        public static string NetDataContractSerializer_Marshal_2_MainType(string dirtymarshal)
        {
            return DataContractSerializer_Marshal_2_MainType(dirtymarshal);
        }

        public static void NetDataContractSerializer_test(object myobj)
        {
            try
            {
                NetDataContractSerializer_deserialize(NetDataContractSerializer_serialize(myobj));
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        public static string NetDataContractSerializer_serialize(object myobj)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create("testnetdata.xml", settings))
            {
                NetDataContractSerializer ser = new NetDataContractSerializer();
                ser.WriteObject(writer, myobj);
            }
            string text = File.ReadAllText("testnetdata.xml");
            return text;
        }

        public static object NetDataContractSerializer_deserialize(string str)
        {
            return NetDataContractSerializer_deserialize(str, "");
        }

        public static object NetDataContractSerializer_deserialize(string str, string rootElement)
        {
            object obj = null;
            var s = new NetDataContractSerializer();
            if (!rootElement.Equals(""))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(str);
                XmlElement xmlItem = (XmlElement)xmlDoc.SelectSingleNode(rootElement);
                obj = s.ReadObject(new XmlTextReader(new StringReader(xmlItem.InnerXml)));
            }
            else
            {
                byte[] serializedData = Encoding.UTF8.GetBytes(str);
                MemoryStream ms = new MemoryStream(serializedData);
                obj = s.Deserialize(ms);
            }

            return obj;
        }

        public static void JsonNet_test(object myobj)
        {
            try
            {
                JsonNet_deserialize(JsonNet_serialize(myobj));
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        public static string JsonNet_serialize(object myobj)
        {
            string text = JsonConvert.SerializeObject(myobj, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            return text;
        }

        public static object JsonNet_deserialize(string str)
        {
            Object obj = JsonConvert.DeserializeObject<Object>(str, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            return obj;
        }

        public static void SoapFormatter_test(object myobj)
        {
            try
            {
                SoapFormatter_deserialize(SoapFormatter_serialize(myobj));
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        public static string SoapFormatter_serialize(object myobj)
        {
            SoapFormatter sf = new SoapFormatter();
            MemoryStream ms = new MemoryStream();
            sf.Serialize(ms, myobj);
            return Encoding.ASCII.GetString(ms.ToArray());
        }

        public static object SoapFormatter_deserialize(string str)
        {
            byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(str);
            MemoryStream ms = new MemoryStream(byteArray);
            SoapFormatter sf = new SoapFormatter();
            return sf.Deserialize(ms);
        }

        public static void BinaryFormatter_test(object myobj)
        {
            try
            {
                BinaryFormatter_deserialize(BinaryFormatter_serialize(myobj));
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        public static string BinaryFormatter_serialize(object myobj)
        {
            BinaryFormatter sf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            sf.Serialize(ms, myobj);
            return Convert.ToBase64String(ms.ToArray());
        }

        public static object BinaryFormatter_deserialize(string str)
        {
            byte[] byteArray = Convert.FromBase64String(str);
            MemoryStream ms = new MemoryStream(byteArray);
            BinaryFormatter sf = new BinaryFormatter();
            return sf.Deserialize(ms);
        }
        public static void LosFormatter_test(object myobj)
        {
            try
            {
                LosFormatter_deserialize(LosFormatter_serialize(myobj));
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        public static string LosFormatter_serialize(object myobj)
        {
            StringWriter s = new StringWriter(CultureInfo.InvariantCulture);
            new LosFormatter().Serialize(s, myobj);

            return s.ToString();
        }

        public static object LosFormatter_deserialize(string str)
        {
            return new LosFormatter().Deserialize(str);
        }

        public static void ObjectStateFormatter_test(object myobj)
        {
            try
            {
                ObjectStateFormatter_deserialize(ObjectStateFormatter_serialize(myobj));
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        public static string ObjectStateFormatter_serialize(object myobj)
        {
            return new ObjectStateFormatter().Serialize(myobj);
        }

        public static object ObjectStateFormatter_deserialize(string str)
        {
            return new ObjectStateFormatter().Deserialize(str);
        }

        public static void YamlDotNet_test(object myobj)
        {
            try
            {
                YamlDotNet_deserialize(YamlDotNet_serialize(myobj));
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        public static string YamlDotNet_serialize(object myobj)
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(myobj);
            return yaml;
        }

        public static object YamlDotNet_deserialize(string str)
        {
            object result = null;
            //to bypass all of the vulnerable version's type checking, we need to set up a stream
            using (var reader = new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(str))))
            {
                var deserializer = new DeserializerBuilder().Build();
                result = deserializer.Deserialize(reader);
            }
            return result;
        }

        public static void JavaScriptSerializer_test(object myobj)
        {
            try
            {
                JavaScriptSerializer_deserialize(JavaScriptSerializer_serialize(myobj));
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        public static string JavaScriptSerializer_serialize(object myobj)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer(new SimpleTypeResolver());
            return jss.Serialize(myobj);
        }

        public static object JavaScriptSerializer_deserialize(string str)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer(new SimpleTypeResolver());
            return jss.Deserialize<Object>(str);
        }
        
    }
}
