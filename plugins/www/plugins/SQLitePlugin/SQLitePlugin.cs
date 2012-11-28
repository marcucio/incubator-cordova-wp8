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
            public string transId { get; set; }

            /// <summary>
            /// Identifier for transaction
            /// </summary>
            [DataMember(IsRequired = true, Name = "query_id")]
            public string queryId { get; set; }

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
        public class SQLiteTransactionResult
        {
            public string transId;
            public List<SQLiteQueryResult> results;
        }
        public class SQLiteQueryResult
        {
            public string queryId;
            public List<Dictionary<string, object>> result;
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
            System.Diagnostics.Debug.WriteLine("SQLitePlugin.executeSqlBatch()");
            System.Diagnostics.Debug.WriteLine("options: " + options);
            List<string> opt = JsonHelper.Deserialize<List<string>>(options);
            TransactionsCollection transactions;
            SQLiteTransactionResult transResult = new SQLiteTransactionResult();
            try
            {
                transactions = JsonHelper.Deserialize<TransactionsCollection>(opt[0]);
            }
            catch (Exception)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION));
                //SQLitePluginTransaction.txErrorCallback(transId, error)
                //SQLitePluginTransaction.queryErrorCallback = function(transId, queryId, result)
                return;
            }
           var db = new SQLiteConnection("foofoo");

           db.RunInTransaction(() =>
           {
               foreach (SQLitePluginTransaction transaction in transactions)
               {
                   transResult.transId = transaction.transId;
                  
                   var results = db.Query2(transaction.query, transaction.query_params);
                   SQLiteQueryResult queryResult = new SQLiteQueryResult();
                   queryResult.queryId = transaction.queryId;
                   queryResult.result = results;
                   if(transResult.results == null)
                       transResult.results = new List<SQLiteQueryResult>();
                   transResult.results.Add(queryResult);
               }
            });

            db.Close();
            DispatchCommandResult(new PluginResult(PluginResult.Status.OK, JsonHelper.Serialize(transResult)));
        }
    }
}