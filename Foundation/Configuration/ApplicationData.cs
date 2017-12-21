﻿using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Foundation.Diagnostics.Contracts;

namespace Foundation.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ApplicationData
    {
        private string _fileName;
        private string _sectionName;

        /// <summary>
        /// 
        /// </summary>
        public ConfigurationNode RootNode { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ConfigurationNode CurrentNamespace
        {
            get
            {
                var trace = new StackTrace(1);
                var nodeName = ConfigurationNodeName.FromNamespace(trace, 0);
                var node = CreateNode(nodeName);
                return node;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ConfigurationNode CurrentType
        {
            get
            {
                var trace = new StackTrace(1);
                var nodeName = ConfigurationNodeName.FromType(trace, 0);
                var node = CreateNode(nodeName);
                return node;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versioned"></param>
        /// <returns></returns>
        public static string GetApplicationDataFolderPath(bool versioned)
        {
            var sb = new StringBuilder();
            sb.Append(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            string company;
            string product;

            var assembly = Assembly.GetEntryAssembly();
            var companyAttribute = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute));

            if (companyAttribute != null)
            {
                company = companyAttribute.Company;

                if (company.Length == 0)
                    company = null;
            }
            else
                company = null;

            var productAttribute = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));

            if (productAttribute != null)
            {
                product = productAttribute.Product;

                if (product.Length == 0)
                    product = null;
            }
            else
                product = null;

            var name = assembly.GetName();

            if (product == null)
                product = name.Name;

            if (company != null)
            {
                sb.Append(Path.DirectorySeparatorChar);
                sb.Append(company);
            }

            sb.Append(Path.DirectorySeparatorChar);
            sb.Append(product);

            if (versioned)
            {
                sb.Append(" (");
                sb.Append(name.Version);
                sb.Append(')');
            }

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlReader"></param>
        public void Load(XmlReader xmlReader)
        {
            var reader = new ConfigurationReader();
            RootNode = reader.Read(xmlReader, _sectionName, null, null);

            if (RootNode == null)
                RootNode = new ConfigurationNode(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sectionName"></param>
        public void Load(string fileName, string sectionName)
        {
            _fileName = fileName;
            _sectionName = sectionName;

            if (File.Exists(fileName))
            {
                var reader = new ConfigurationReader();
                var fileNames = new StringCollection();
                RootNode = reader.Read(fileName, sectionName, fileNames);
            }
            else
                RootNode = new ConfigurationNode(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlWriter"></param>
        /// <param name="sectionName"></param>
        public void Save(XmlWriter xmlWriter, string sectionName)
        {
            FoundationContract.Requires<ArgumentNullException>(xmlWriter != null);
            FoundationContract.Requires<ArgumentNullException>(sectionName != null);

            xmlWriter.WriteStartElement(sectionName);
            ConfigurationWriter.Write(xmlWriter, RootNode.Attributes);

            foreach (var childNode in RootNode.ChildNodes)
                ConfigurationWriter.WriteNode(xmlWriter, childNode);

            xmlWriter.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sectionName"></param>
        public void Save(string fileName, string sectionName)
        {
            var directoryName = Path.GetDirectoryName(fileName);
            Directory.CreateDirectory(directoryName);

            using (var xmlTextWriter = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.Indentation = 2;
                xmlTextWriter.IndentChar = ' ';
                xmlTextWriter.WriteStartDocument();
                Save(xmlTextWriter, sectionName);
                xmlTextWriter.WriteEndDocument();
                xmlTextWriter.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            var directoryName = Path.GetDirectoryName(_fileName);

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            Save(_fileName, _sectionName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public ConfigurationNode CreateNode(string nodeName)
        {
            return RootNode.CreateNode(nodeName);
        }
    }
}