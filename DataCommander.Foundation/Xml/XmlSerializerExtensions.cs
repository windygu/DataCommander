﻿namespace DataCommander.Foundation.Xml
{
    using System.Diagnostics.Contracts;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// 
    /// </summary>
    public static class XmlSerializerExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlSerializer"></param>
        /// <param name="xmlWriterSettings"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string SerializeToXmlString( this XmlSerializer xmlSerializer, XmlWriterSettings xmlWriterSettings, object o )
        {
            Contract.Requires( xmlSerializer != null );

            var sb = new StringBuilder();

            using (var xmlWriter = XmlWriter.Create( sb, xmlWriterSettings ))
            {
                xmlSerializer.Serialize( xmlWriter, o );
            }

            return sb.ToString();
        }
    }
}