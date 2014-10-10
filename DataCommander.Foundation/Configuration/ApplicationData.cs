namespace DataCommander.Foundation.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// 
    /// </summary>
    public sealed class ApplicationData
    {
        private String fileName;
        private String sectionName;
        private ConfigurationNode rootNode;

        /// <summary>
        /// 
        /// </summary>
        public ConfigurationNode RootNode
        {
            get
            {
                return this.rootNode;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ConfigurationNode CurrentNamespace
        {
            get
            {
                var trace = new StackTrace( 1 );
                String nodeName = ConfigurationNodeName.FromNamespace( trace, 0 );
                ConfigurationNode node = this.CreateNode( nodeName );
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
                var trace = new StackTrace( 1 );
                String nodeName = ConfigurationNodeName.FromType( trace, 0 );
                ConfigurationNode node = this.CreateNode( nodeName );
                return node;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versioned"></param>
        /// <returns></returns>
        public static String GetApplicationDataFolderPath( Boolean versioned )
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ) );

            String company;
            String product;

            Assembly assembly = Assembly.GetEntryAssembly();
            AssemblyCompanyAttribute companyAttribute = (AssemblyCompanyAttribute) Attribute.GetCustomAttribute( assembly, typeof( AssemblyCompanyAttribute ) );

            if (companyAttribute != null)
            {
                company = companyAttribute.Company;

                if (company.Length == 0)
                {
                    company = null;
                }
            }
            else
            {
                company = null;
            }

            AssemblyProductAttribute productAttribute = (AssemblyProductAttribute) Attribute.GetCustomAttribute( assembly, typeof( AssemblyProductAttribute ) );

            if (productAttribute != null)
            {
                product = productAttribute.Product;

                if (product.Length == 0)
                {
                    product = null;
                }
            }
            else
            {
                product = null;
            }

            AssemblyName name = assembly.GetName();

            if (product == null)
            {
                product = name.Name;
            }

            if (company != null)
            {
                sb.Append( Path.DirectorySeparatorChar );
                sb.Append( company );
            }

            sb.Append( Path.DirectorySeparatorChar );
            sb.Append( product );

            if (versioned)
            {
                sb.Append( " (" );
                sb.Append( name.Version );
                sb.Append( ')' );
            }

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlReader"></param>
        public void Load( XmlReader xmlReader )
        {
            ConfigurationReader reader = new ConfigurationReader();
            this.rootNode = reader.Read( xmlReader, this.sectionName, null, null );

            if (this.rootNode == null)
            {
                this.rootNode = new ConfigurationNode( null );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sectionName"></param>
        public void Load( String fileName, String sectionName )
        {
            this.fileName = fileName;
            this.sectionName = sectionName;

            if (File.Exists( fileName ))
            {
                var reader = new ConfigurationReader();
                StringCollection fileNames = new StringCollection();
                this.rootNode = reader.Read( fileName, sectionName, fileNames );
            }
            else
            {
                this.rootNode = new ConfigurationNode( null );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlWriter"></param>
        /// <param name="sectionName"></param>
        public void Save( XmlWriter xmlWriter, String sectionName )
        {
            Contract.Requires( xmlWriter != null );
            Contract.Requires( sectionName != null );

            xmlWriter.WriteStartElement( sectionName );
            ConfigurationWriter.Write( xmlWriter, this.rootNode.Attributes );

            foreach (ConfigurationNode childNode in this.rootNode.ChildNodes)
            {
                ConfigurationWriter.WriteNode( xmlWriter, childNode );
            }

            xmlWriter.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sectionName"></param>
        public void Save( String fileName, String sectionName )
        {
            String directoryName = Path.GetDirectoryName( fileName );
            Directory.CreateDirectory( directoryName );
            using (var xmlTextWriter = new XmlTextWriter( fileName, Encoding.UTF8 ))
            {
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.Indentation = 2;
                xmlTextWriter.IndentChar = ' ';
                xmlTextWriter.WriteStartDocument();
                this.Save( xmlTextWriter, sectionName );
                xmlTextWriter.WriteEndDocument();
                xmlTextWriter.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            String directoryName = Path.GetDirectoryName( this.fileName );

            if (!Directory.Exists( directoryName ))
            {
                Directory.CreateDirectory( directoryName );
            }

            this.Save( this.fileName, this.sectionName );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public ConfigurationNode CreateNode( String nodeName )
        {
            return this.rootNode.CreateNode( nodeName );
        }
    }
}