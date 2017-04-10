namespace DataCommander.Foundation.Configuration
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// 
    /// </summary>
    public sealed class ConfigurationNode
    {
        /// <summary>
        /// The path delimiter in the nodeName. E.g.: Node1/Node2/Node3.
        /// </summary>
        public const char Delimiter = '/';

        private int index;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ConfigurationNode(string name)
        {
            this.Name = name;
            this.HasName = name != null;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasName { get; }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets/sets the description of the node.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the parent node.
        /// </summary>
        public ConfigurationNode Parent { get; private set; }

        /// <summary>
        /// Gets the full path of the node.
        /// </summary>
        public string FullName
        {
            get
            {
                string fullName;

                if (this.Parent != null)
                {
                    fullName = this.Parent.FullName;

                    if (fullName != null)
                    {
                        fullName += Delimiter;
                    }

                    fullName += this.Name;
                }
                else
                {
                    fullName = null;
                }

                return fullName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childNode"></param>
        public void AddChildNode(ConfigurationNode childNode)
        {
#if CONTRACTS_FULL
            Contract.Requires<ArgumentException>(childNode.Parent == null);
#endif

            if (childNode.Name == null)
            {
                childNode.Name = ConfigurationElementName.Node + "[" + this.index + ']';
                this.index++;
            }

            this.ChildNodes.Add(childNode);
            childNode.Parent = this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="childNode"></param>
        public void InsertChildNode(int index, ConfigurationNode childNode)
        {
#if CONTRACTS_FULL
            Contract.Requires<ArgumentException>(childNode.Parent == null);
#endif

            if (childNode.Name == null)
            {
                childNode.Name = ConfigurationElementName.Node + "[" + index + ']';
                index++;
            }

            this.ChildNodes.Insert(index, childNode);
            childNode.Parent = this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childNode"></param>
        public void RemoveChildNode(ConfigurationNode childNode)
        {
#if CONTRACTS_FULL
            Contract.Requires(childNode != null);
            Contract.Requires(this == childNode.Parent);
#endif

            this.ChildNodes.Remove(childNode);
            childNode.Parent = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ConfigurationNode Clone()
        {
            var clone = new ConfigurationNode(this.Name);

            foreach (var attribute in this.Attributes)
            {
                var attributeClone = attribute.Clone();
                clone.Attributes.Add(attributeClone);
            }

            foreach (var childNode in this.ChildNodes)
            {
                var childNodeClone = childNode.Clone();
                clone.AddChildNode(childNodeClone);
            }

            return clone;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public ConfigurationNode CreateNode(string nodeName)
        {
#if CONTRACTS_FULL
            Contract.Requires(nodeName != null);
#endif
            var node = this;
            var nodeNames = nodeName.Split(Delimiter);

            for (var i = 0; i < nodeNames.Length; i++)
            {
                ConfigurationNode childNode;
                var contains = node.ChildNodes.TryGetValue(nodeNames[i], out childNode);

                if (!contains)
                {
                    childNode = new ConfigurationNode(nodeNames[i]);
                    node.AddChildNode(childNode);
                }

                node = childNode;
            }

            return node;
        }

        /// <summary>
        /// Finds recursively a node under the node.
        /// </summary>
        /// <param name="path">Name of the child node.
        /// The name can contains path delimiters.</param>
        /// <returns>Return the child node is found.
        /// Returns null if no child node found.</returns>
        public ConfigurationNode SelectNode(string path)
        {
            var node = this;

            if (path != null)
            {
                var childNodeNames = path.Split(Delimiter);
                var depth = 0;

                foreach (var childNodeName in childNodeNames)
                {
                    ConfigurationNode childNode;
                    var contains = node.ChildNodes.TryGetValue(childNodeName, out childNode);

                    if (contains)
                    {
                        node = childNode;
                        depth++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (depth != childNodeNames.Length)
                {
                    node = null;
                }
            }

            return node;
        }

        /// <summary>
        /// Gets the attributes stored in this node.
        /// </summary>
        public ConfigurationAttributeCollection Attributes { get; } = new ConfigurationAttributeCollection();

        /// <summary>
        /// Gets the child nodes of this node.
        /// </summary>
        public ConfigurationNodeCollection ChildNodes { get; } = new ConfigurationNodeCollection();

        /// <summary>
        /// Writes the content of this node (attributes and child nodes)
        /// of this node to the specified <paramref name="textWriter"/>.
        /// </summary>
        /// <param name="textWriter"></param>
        public void Write(TextWriter textWriter)
        {
            textWriter.WriteLine("[" + this.FullName + "]");

            foreach (var attribute in this.Attributes)
            {
                attribute.Write(textWriter);
            }

            textWriter.WriteLine();

            foreach (var childNode in this.ChildNodes)
            {
                childNode.Write(textWriter);
            }
        }

        /// <summary>
        /// Writes the documentation of this node to the specified <paramref name="textWriter" />.
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="level">Recursion level</param>
        public void WriteDocumentation(TextWriter textWriter, int level)
        {
#if CONTRACTS_FULL
            Contract.Requires(textWriter != null);
#endif

            var sb = new StringBuilder();
            var indent = new string(' ', level * 2);
            sb.Append(indent);
            sb.Append(this.Name);
            sb.Append("\t\t");
            sb.AppendLine(this.Description);

            if (this.Attributes.Count > 0)
            {
                foreach (var attribute in this.Attributes)
                {
                    sb.Append('\t');
                    sb.Append(attribute.Name);

                    sb.Append('\t');
                    sb.Append(attribute.Description);
                    sb.Append('\t');

                    var value = attribute.Value;
                    var valueString = value != null ? value.ToString() : null;
                    var multiline = valueString.IndexOf('\n') >= 0;

                    if (multiline)
                    {
                        value = valueString.Replace("\r", string.Empty);
                        sb.Append('"');
                    }

                    sb.Append(value);

                    if (multiline)
                    {
                        sb.Append('"');
                    }

                    sb.Append(Environment.NewLine);
                }
            }

            textWriter.Write(sb);

            foreach (var childNode in this.ChildNodes)
            {
                childNode.WriteDocumentation(textWriter, level + 1);
            }
        }
    }
}