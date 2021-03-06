﻿namespace DataCommander.Providers.Tfs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using Foundation.Data;
    using Microsoft.TeamFoundation.VersionControl.Client;
    using Microsoft.TeamFoundation.VersionControl.Common;

    internal class TfsDownloadItemVersionsDataReader : TfsDataReader
    {
        private readonly TfsCommand command;
        private string path;
        private string localPath;
        private bool first = true;
        private IEnumerator<Tuple<Changeset, int>> enumerator;

        public TfsDownloadItemVersionsDataReader(TfsCommand command)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            this.command = command;
        }

        public override DataTable GetSchemaTable()
        {
            var table = CreateSchemaTable();
            AddSchemaRowInt32( table, "ChangesetId", false );
            AddSchemaRowString( table, "Committer", false );
            AddSchemaRowDateTime( table, "CreationDate", false );
            AddSchemaRowString( table, "Comment", false );
            AddSchemaRowString( table, "ChangeType", false );
            AddSchemaRowString( table, "ServerItem", false );
            return table;
        }

        //private static int Count( IEnumerable enumerable )
        //{
        //    Assert.IsNotNull( enumerable, "enumerable" );
        //    IEnumerator enumerator = enumerable.GetEnumerator();
        //    int count = 0;

        //    while (enumerator.MoveNext())
        //    {
        //        count++;
        //    }

        //    return count;
        //}

        public override bool Read()
        {
            bool read;

            if (this.first)
            {
                this.first = false;
                var parameters = this.command.Parameters;
                this.path = (string) parameters[ "path" ].Value;
                VersionSpec version = VersionSpec.Latest;
                var deletionId = 0;
                RecursionType recursion = RecursionType.None;
                string user = Database.GetValueOrDefault<string>( parameters[ "user" ].Value );
                VersionSpec versionFrom = null;
                VersionSpec versionTo = null;
                var parameter = parameters[ "maxCount" ];

                int maxCount = Database.GetValueOrDefault<int>( parameter.Value );

                if (maxCount == 0)
                {
                    maxCount = (int) parameter.DefaultValue;
                }

                const bool includeChanges = true;
                bool slotMode = Database.GetValueOrDefault<bool>( parameters[ "slotMode" ].Value );
                this.localPath = Database.GetValueOrDefault<string>( parameters[ "localPath" ].Value );

                if (string.IsNullOrEmpty(this.localPath ))
                {
                    this.localPath = Path.GetTempPath();
                    this.localPath += Path.DirectorySeparatorChar;
                    this.localPath += $"getversions [{DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff")}]";
                    Directory.CreateDirectory(this.localPath );
                }

                IEnumerable changesets = this.command.Connection.VersionControlServer.QueryHistory(this.path, version, deletionId, recursion, user, versionFrom, versionTo, maxCount, includeChanges, slotMode );
                this.enumerator = this.AsEnumerable( changesets ).GetEnumerator();
            }

            var moveNext = this.enumerator.MoveNext();

            if (moveNext)
            {
                var pair = this.enumerator.Current;
                Changeset changeset = pair.Item1;

                var values = new object[]
                {
                    changeset.ChangesetId,
                    changeset.Committer,
                    changeset.CreationDate,
                    changeset.Comment,
                    null,
                    null
                };

                var changeIndex = pair.Item2;

                if (changeIndex >= 0)
                {
                    Change change = changeset.Changes[ changeIndex ];
                    values[ 4 ] = change.ChangeType;
                    values[ 5 ] = change.Item.ServerItem;
                }

                this.Values = values;
                int changesetId = changeset.ChangesetId;
                var changeType = String.Empty;

                for (var i = 0; i < changeset.Changes.Length; i++)
                {
                    Change change = changeset.Changes[ i ];
                    Trace.WriteLine( change.ChangeType );
                    Trace.WriteLine( change.Item.ServerItem );
                    this.path = change.Item.ServerItem;

                    if (i > 0)
                    {
                        changeType += ',';
                    }

                    changeType += change.ChangeType;
                }

                DateTime creationDate = changeset.CreationDate;
                var deletionId = 0;
                ChangesetVersionSpec versionSpec = new ChangesetVersionSpec( changesetId );
                string fileName = VersionControlPath.GetFileName( this.path );
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension( fileName );
                string extension = VersionControlPath.GetExtension( this.path );
                var localFileName = this.localPath + Path.DirectorySeparatorChar + changesetId.ToString().PadLeft( 5, '0' ) + ';' + changeType + ';' + changeset.Committer.Replace( '\\', ' ' ) + extension;
                this.command.Connection.VersionControlServer.DownloadFile( this.path, deletionId, versionSpec, localFileName );
                File.SetLastWriteTime( localFileName, creationDate );
                File.SetAttributes( localFileName, FileAttributes.ReadOnly );

                read = true;
            }
            else
            {
                read = false;
            }

            return read;
        }

        public override int RecordsAffected => -1;

        public override int FieldCount => 6;

        private IEnumerable<Tuple<Changeset, int>> AsEnumerable( IEnumerable changesets )
        {
            foreach (Changeset changeset in changesets)
            {
                Change[] changes = changeset.Changes;

                if (changes.Length > 0)
                {
                    for (var i = 0; i < changes.Length; i++)
                    {
                        yield return Tuple.Create( changeset, i );
                    }
                }
                else
                {
                    yield return Tuple.Create( changeset, -1 );
                }
            }
        }
    }
}