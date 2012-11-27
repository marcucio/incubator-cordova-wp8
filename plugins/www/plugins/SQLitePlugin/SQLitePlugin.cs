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
using System.Collections.Generic;
using SQLite;
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
            string options2 = "[{\"trans_id\":\"1353570356172000\",\"query_id\":\"1353570356229000\",\"query\":\"CREATE TABLE IF NOT EXISTS sql_test2 (test_id TEXT NOT NULL, test_name TEXT NOT NULL);\",\"params\":[]},{\"trans_id\":\"1353570356172000\",\"query_id\":\"1353570356265000\",\"query\":\"INSERT INTO sql_test2 (test_id, test_name) VALUES (?, ?);\",\"params\":[\"1\",\"Hi 1\"]},{\"trans_id\":\"1353570356172000\",\"query_id\":\"1353570356348000\",\"query\":\"INSERT INTO sql_test2 (test_id, test_name) VALUES (?, ?);\",\"params\":[\"2\",\"Hi 2\"]},{\"trans_id\":\"1353570356172000\",\"query_id\":\"1353570356388000\",\"query\":\"SELECT * FROM sql_test2;\",\"params\":[]}]";
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
           var db = new SQLiteConnection("foofoo");


           db.RunInTransaction(() =>
           {
               foreach (SQLitePluginTransaction transaction in transactions)
               {
                   int first = transaction.query.IndexOf("SELECT");
                   if (first == -1)
                   {
                       var results = db.Execute(transaction.query, transaction.query_params);
                       //TODO call the callback function if there is a query_id
                   }
                   else
                   {
                        var results = db.Query2(transaction.query, transaction.query_params);

                        System.Diagnostics.Debug.WriteLine("SQLitePlugin result:" + JsonHelper.Serialize(results));
                        foreach (var result in results)
                        {
                            System.Diagnostics.Debug.WriteLine("SQLitePlugin result:::" + JsonHelper.Serialize(result));
                        }
                        //TODO send data to the callback function if there is a query_id
                   }
               }
            });

            db.Close();
            System.Diagnostics.Debug.WriteLine("SQLitePlugin.executeSqlBatch()");
            DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
            //TODO send success callback for the transaction
        }
    }
}