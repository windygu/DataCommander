namespace DataCommander.Providers.SqlServer2005
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    sealed class UserNode : ITreeNode
    {
        public UserNode(DatabaseNode database,string name)
        {
            this.database = database;
            this.name = name;
        }

        public string Name    
        {
            get
            {
                return name;
            }
        }
    
        public bool IsLeaf    
        {
            get
            {
                return true;
            }
        }

        IEnumerable<ITreeNode> ITreeNode.GetChildren(bool refresh)
        {
            return null;
        }

        public bool Sortable
        {
            get
            {
                return false;
            }
        }
    
        public string Query
        {
            get
            {
                string query = string.Format(@"declare @uid smallint
select @uid = uid from {0}..sysusers where name = '{1}'

select u.name from {0}..sysmembers m
join {0}..sysusers u
on m.groupuid = u.uid
where memberuid = @uid
group by u.name",database.Name,name);

                return query;
            }
        }

        public ContextMenuStrip ContextMenu
        {
            get
            {
                return null;
            }
        }

        DatabaseNode database;
        string       name;
    }
}