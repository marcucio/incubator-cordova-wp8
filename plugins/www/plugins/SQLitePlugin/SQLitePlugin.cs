/*
 * PhoneGap is available under *either* the terms of the modified BSD license *or* the
 * MIT License (2008). See http://opensource.org/licenses/alphabetical for full text.
 *
 * Copyright (c) 2005-2011, Nitobi Software Inc.
 * Copyright (c) 2011, Microsoft Corporation
 */

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using WPCordovaClassLib.Cordova;
using WPCordovaClassLib.Cordova.Commands;
using WPCordovaClassLib.Cordova.JSON;

namespace Cordova.Extension.Commands
{
    /// <summary>
    /// Implementes access to SQLite DB
    /// </summary>
    public class SQLitePlugin : BaseCommand
    {

        #region SQLitePlugin options

        /// <summary>
        /// Represents SQLitePlugin options
        /// </summary>
        [DataContract]
        public class SQLitePluginOptions
        {
            /// <summary>
            /// Tile title
            /// </summary>
            [DataMember(IsRequired=false, Name="title")]
            public string Title { get; set; }

            /// <summary>
            /// Tile count
            /// </summary>
            [DataMember(IsRequired = false, Name = "count")]
            public int Count { get; set; }

            /// <summary>
            /// Tile image
            /// </summary>
            [DataMember(IsRequired = false, Name = "image")]
            public string Image { get; set; }

            /// <summary>
            /// Back tile title
            /// </summary>
            [DataMember(IsRequired = false, Name = "backTitle")]
            public string BackTitle { get; set; }

            /// <summary>
            /// Back tile content
            /// </summary>
            [DataMember(IsRequired = false, Name = "backContent")]
            public string BackContent { get; set; }

            /// <summary>
            /// Back tile image
            /// </summary>
            [DataMember(IsRequired = false, Name = "backImage")]
            public string BackImage { get; set; }

            /// <summary>
            /// Identifier for second tile
            /// </summary>
            [DataMember(IsRequired = false, Name = "secondaryTileUri")]
            public string SecondaryTileUri { get; set; }

        }
        [DataContract]
        public class SQLitePluginOpenCloseOptions
        {
            [DataMember(IsRequired = true, Name = "dbName")]
            public string DBName { get; set; }
        }

        [DataContract]
        public class SQLitePluginExecuteSqlBatchOptions
        {
            [DataMember]
            public TransactionsCollection Transactions {get; set;}
        }

        [CollectionDataContract]
        public class TransactionsCollection : Collection<SQLitePluginTransaction>
        {

        }

        [DataContract]
        public class SQLitePluginTransaction
        {
            /// <summary>
            /// Identifier for transaction
            /// </summary>
            [DataMember(IsRequired = true, Name = "trans_id")]
            public string trans_id { get; set; }

            /// <summary>
            /// Identifier for transaction
            /// </summary>
            [DataMember(IsRequired = true, Name = "query_id")]
            public string query_id { get; set; }

            /// <summary>
            /// Identifier for transaction
            /// </summary>
            [DataMember(IsRequired = true, Name = "query")]
            public string query { get; set; }

            /// <summary>
            /// Identifier for transaction
            /// </summary>
            [DataMember(IsRequired = true, Name = "params")]
            public string[] query_params { get; set; }

        }
        #endregion

        public void open(string options)
        {
            SQLitePluginOpenCloseOptions dbOptions;
            String dbName = "";
            try
            {
                dbOptions = JsonHelper.Deserialize<SQLitePluginOpenCloseOptions>(options);
            }
            catch (Exception)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                return;
            }

            if (!string.IsNullOrEmpty(dbOptions.DBName))
            {
                dbName = dbOptions.DBName;
                System.Diagnostics.Debug.WriteLine("SQLitePlugin.open():" + dbName);
                DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
            }
            else
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "No database name"));
            }
        }
        public void close(string options)
        {
            System.Diagnostics.Debug.WriteLine("SQLitePlugin.close()");
            DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
        }
        public void executeSqlBatch(string options)
        {
            TransactionsCollection transactions;
            try
            {
                transactions = JsonHelper.Deserialize<TransactionsCollection>(options2);
            }
            catch (Exception)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                return;
            }

            foreach (SQLitePluginTransaction transaction in transactions)
            {
                System.Diagnostics.Debug.WriteLine(transaction.query);
            }

            System.Diagnostics.Debug.WriteLine("SQLitePlugin.executeSqlBatch()");
            DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
        }
    }
}