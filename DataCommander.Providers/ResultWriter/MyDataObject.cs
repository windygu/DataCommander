﻿namespace DataCommander.Providers
{
    using System;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
    using DataCommander.Foundation.Data;

    internal sealed class MyDataObject : IDataObject
    {
        private DataView dataView;
        private int[] columnIndexes;

        public MyDataObject( DataView dataView, int[] columnIndexes )
        {
            this.dataView = dataView;
            this.columnIndexes = columnIndexes;
        }

        #region IDataObject Members

        object IDataObject.GetData( Type format )
        {
            throw new NotImplementedException();
        }

        object IDataObject.GetData( string format )
        {
            throw new NotImplementedException();
        }

        object IDataObject.GetData( string format, bool autoConvert )
        {
            object data;

            if (format == DataFormats.CommaSeparatedValue)
            {
                StringWriter stringWriter = new StringWriter();
                Database.Write( this.dataView, ',', "\r\n", stringWriter );
                char c = (char) 0;
                stringWriter.Write( c );
                string s = stringWriter.ToString();
                data = new MemoryStream( Encoding.Default.GetBytes( s ) );
            }
            else if (format == DataFormats.Html)
            {
                StringWriter stringWriter = new StringWriter();
                HtmlFormatter.Write( this.dataView, this.columnIndexes, stringWriter );
                string htmlFragment = stringWriter.ToString();
                stringWriter = new StringWriter();
                WriteHtmlFragment( htmlFragment, stringWriter );
                string s = stringWriter.ToString();
                byte[] bytes = Encoding.UTF8.GetBytes( s );
                data = new MemoryStream( bytes );
            }
            else if (format == DataFormats.Text || format == DataFormats.UnicodeText)
            {
                data = this.dataView.ToStringTable().ToString();
            }
            else if (format == "TabSeparatedValues")
            {
                // TODO
                data = "TabSep";
            }
            else
            {
                data = null;
            }

            return data;
        }

        bool IDataObject.GetDataPresent( Type format )
        {
            throw new NotImplementedException();
        }

        bool IDataObject.GetDataPresent( string format )
        {
            throw new NotImplementedException();
        }

        bool IDataObject.GetDataPresent( string format, bool autoConvert )
        {
            bool isDataPresent;

            if (format == DataFormats.CommaSeparatedValue ||
                format == DataFormats.Html ||
                format == DataFormats.Text ||
                format == DataFormats.UnicodeText)
            {
                isDataPresent = true;
            }
            else
            {
                isDataPresent = false;
            }

            return isDataPresent;
        }

        string[] IDataObject.GetFormats()
        {
            throw new NotImplementedException();
        }

        string[] IDataObject.GetFormats( bool autoConvert )
        {
            return new string[]
            {
                DataFormats.CommaSeparatedValue,
                DataFormats.Html,
                //DataFormats.StringFormat,
                DataFormats.Text,
                DataFormats.UnicodeText,
                "TabSeparatedValues" // TODO
            };
        }

        void IDataObject.SetData( object data )
        {
            throw new NotImplementedException();
        }

        void IDataObject.SetData( Type format, object data )
        {
            throw new NotImplementedException();
        }

        void IDataObject.SetData( string format, object data )
        {
            throw new NotImplementedException();
        }

        void IDataObject.SetData( string format, bool autoConvert, object data )
        {
            throw new NotImplementedException();
        }

        #endregion

        private static void WriteHtmlFragment( string htmlFragment, TextWriter textWriter )
        {
            string header = @"Version:0.9
StartHTML:{000000}
EndHTML:{111111}
StartFragment:{222222}
EndFragment:{333333}
";
            string startHtmlString = "<html><body><!--StartFragment-->";
            string endHtmlString = "<!--EndFragment--></body></html>";
            int startHtml = header.Length;
            int startFragment = startHtml + startHtmlString.Length;
            int htmlFragmentLength = Encoding.UTF8.GetByteCount( htmlFragment );
            int endFragment = startFragment + htmlFragmentLength;
            int endHtml = endFragment + endHtmlString.Length;

            header = header.Replace( "{000000}", startHtml.ToString().PadLeft( 8 ) );
            header = header.Replace( "{111111}", endHtml.ToString().PadLeft( 8 ) );
            header = header.Replace( "{222222}", startFragment.ToString().PadLeft( 8 ) );
            header = header.Replace( "{333333}", endFragment.ToString().PadLeft( 8 ) );

            textWriter.Write( header );
            textWriter.Write( startHtmlString );
            textWriter.Write( htmlFragment );
            textWriter.Write( endHtmlString );
        }
    }
}