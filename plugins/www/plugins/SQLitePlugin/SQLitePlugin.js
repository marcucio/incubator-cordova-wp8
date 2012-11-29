﻿(function() {
  var SQLitePlugin, SQLitePluginTransaction, get_unique_id, root, transaction_callback_queue, transaction_queue;
  root = this;
  var cdv = window.cordova || window.Cordova || PhoneGap;
  
  SQLitePlugin = function(dbPath, openSuccess, openError) {
    console.log("SQLitePlugin");
    this.dbPath = dbPath;
    this.openSuccess = openSuccess;
    this.openError = openError;
    if (!dbPath) {
      return openError("Cannot create a SQLitePlugin instance without a dbPath");
    }
    this.openSuccess || (this.openSuccess = function() {
      return console.log("DB opened: " + dbPath);
    });
    this.openError || (this.openError = function(e) {
      return console.log(e.message);
    });
    this.open(this.openSuccess, this.openError);
  };
  SQLitePlugin.prototype.openDBs = {};
  SQLitePlugin.prototype.transaction = function(fn, error, success) {
    var t;
    t = new SQLitePluginTransaction(this.dbPath);
    fn(t);
    return t.complete(success, error);
  };
  SQLitePlugin.prototype.open = function(success, error) {
    var opts;
    console.log("SQLitePlugin.prototype.open");
    opts = void 0;
    if (!(this.dbPath in this.openDBs)) {
      this.openDBs[this.dbPath] = true;
      return cdv.exec(success, error, "SQLitePlugin", "open", {dbName: this.dbPath});
    }
    else { // we already have this db open
    	return cdv.exec(success, error, "SQLitePlugin", "open", {dbName: this.dbPath});
    }
  };
  SQLitePlugin.prototype.close = function(success, error) {
    var opts;
    console.log("SQLitePlugin.prototype.close");
    opts = void 0;
    if (this.dbPath in this.openDBs) {
      delete this.openDBs[this.dbPath];
      return cdv.exec(success, error, "SQLitePlugin", "close", [this.dbPath]);
    }
  };
  get_unique_id = function() {
    var id, id2;
    id = new Date().getTime();
    id2 = new Date().getTime();
    while (id === id2) {
      id2 = new Date().getTime();
    }
    return id2 + "000";
  };
  transaction_queue = [];
  transaction_callback_queue = {};
  SQLitePluginTransaction = function(dbPath) {
    this.dbPath = dbPath;
    this.executes = [];
    this.trans_id = get_unique_id();
    this.__completed = false;
    this.__submitted = false;
    console.log("SQLitePluginTransaction - this.trans_id:" + this.trans_id);
    transaction_queue[this.trans_id] = [];
    transaction_callback_queue[this.trans_id] = new Object();
  };
  SQLitePluginTransaction.queryCompleteCallback = function(transId, queryId, result) {
    var query, x;
    console.log("SQLitePluginTransaction.queryCompleteCallback:typeof:"+ typeof transaction_queue[transId]);
    query = null;
    var trans;
    if(queryId)
    {
	    for (x in transaction_queue[transId]) 
	    {
	      	if (transaction_queue[transId][x]["query_id"] === queryId) 
	      	{
	        	query = transaction_queue[transId][x];
	        	if (transaction_queue[transId].length === 1) 
	        	{
	          		transaction_queue[transId] = [];
	        	} 
	        	else 
	        	{
	          		transaction_queue[transId].splice(x, 1);
	        	}
	        	break;
	      }
	    }
	}
    if (queryId && query && query["callback"]) 
    {
    	console.log("SQLitePluginTransaction.queryCompleteCallback -- found callback result:"+JSON.stringify(result));
    	return query["callback"](result);
    }
    else
    {
    	console.log("SQLitePluginTransaction.queryCompleteCallback -- NOT found:"+transId+":"+queryId);
    }
    
  };
  SQLitePluginTransaction.queryErrorCallback = function(transId, queryId, result) {
    var query, x;
    query = null;
    for (x in transaction_queue[transId]) {
      if (transaction_queue[transId][x]["query_id"] === queryId) {
        query = transaction_queue[transId][x];
        if (transaction_queue[transId].length === 1) {
          transaction_queue[transId] = [];
        } else {
          transaction_queue[transId].splice(x, 1);
        }
        break;
      }
    }
    if (query && query["err_callback"]) return query["err_callback"](result);
  };
  //-- makes the reslts that come from cordova into results thar resemble the websql results
  SQLitePluginTransaction.fixQueryResult = function(resultColumns)
  {
  	//make a JSON string then turn it back to an object, since we cannot have dynamic varable names I don't see any other way to do this
	var newResults = "[";
	var columNum = 0;
	for(var x in resultColumns)
	{	
		if(newResults !== "[")
			newResults += ",{";
		else
			newResults += "{";
			
		columNum = 0;
		for(var y in resultColumns[x])
		{
			if(columNum > 0)
				newResults += ",";
		//console.log("typeof resultColumns[x][y].Value:"+typeof resultColumns[x][y].Value)
			if(typeof resultColumns[x][y].Value == 'string')
				newResults += "\""+resultColumns[x][y].Key.replace("\"", "\\\"")+"\":\""+resultColumns[x][y].Value.replace("\"", "\\\"")+"\"";
			else
				newResults += "\""+resultColumns[x][y].Key.replace("\"", "\\\"")+"\":\""+resultColumns[x][y].Value+"\"";
		
			
			columNum++;
		}
		newResults += "}";
	}

	newResults += "]";
	return JSON.parse(newResults);
  }
  SQLitePluginTransaction.txCompleteCallback = function(result, success) 
  {
  	console.log("SQLitePluginTransaction.txCompleteCallback");
    var transId = result.transId;
    var queryId = null;
    var queryResult = null;
    for(var x in result.results)
    {
		queryId = result.results[x].queryId;
    	queryResult = result.results[x].result;
    	SQLitePluginTransaction.queryCompleteCallback(transId, queryId, SQLitePluginTransaction.fixQueryResult(queryResult));
    }
    
    if(success)
    {
    	return success();
    }
    /*
    if (typeof transId !== "undefined") {
      if (transId && success)//transaction_callback_queue[transId] && transaction_callback_queue[transId]["success"]) 
      {
      	console.log("SQLitePluginTransaction.txCompleteCallback---transId = returning");
        return success();
      }
      else
      {
      	console.log("SQLitePluginTransaction.txCompleteCallback---transId = ??????"+transId+":"+JSON.stringify(transaction_callback_queue[transId]));
      }
    } 
    else 
    {
      return console.log("SQLitePluginTransaction.txCompleteCallback---transId = NULL");
    }
    */
  };
  SQLitePluginTransaction.txErrorCallback = function(transId, error) {
    if (typeof transId !== "undefined") {
      console.log("SQLitePluginTransaction.txErrorCallback---transId:" + transId);
      if (transId && transaction_callback_queue[transId]["error"]) {
        transaction_callback_queue[transId]["error"](error);
      }
      delete transaction_queue[transId];
      return delete transaction_callback_queue[transId];
    } else {
      return console.log("SQLitePluginTransaction.txErrorCallback---transId = NULL");
    }
  };
  SQLitePluginTransaction.prototype.add_to_transaction = function(trans_id, query, params, callback, err_callback) {
    var new_query;
    new_query = new Object();
    new_query["trans_id"] = trans_id;
    if (callback) {
      new_query["query_id"] = get_unique_id();
    } else {
      new_query["query_id"] = "";
    }
    new_query["query"] = query;
    if (params) {
      new_query["params"] = params;
    } else {
      new_query["params"] = [];
    }
    new_query["callback"] = callback;
    new_query["err_callback"] = err_callback;
    if (!transaction_queue[trans_id])
    	transaction_queue[trans_id] = [];
    
    return transaction_queue[trans_id].push(new_query);
  };
  SQLitePluginTransaction.prototype.executeSql = function(sql, values, success, error) {
    var errorcb, successcb, txself;
    console.log("SQLitePluginTransaction.prototype.executeSql");
    errorcb = void 0;
    successcb = void 0;
    txself = void 0;
    txself = this;
    successcb = null;
    if (success) {
      console.log("success not null:" + sql);
      successcb = function(execres) {
        var res, saveres;
        console.log("executeSql callback:" + JSON.stringify(execres));
        res = void 0;
        saveres = void 0;
        saveres = execres;
        res = {
          rows: {
            item: function(i) {
              return saveres[i];
            },
            length: saveres.length
          },
          rowsAffected: saveres.rowsAffected,
          insertId: saveres.insertId || null
        };
        return success(txself, res);
      };
    } else {
      console.log("success NULL:" + sql);
    }
    errorcb = null;
    if (error) {
      errorcb = function(res) {
        return error(txself, res);
      };
    }
    this.add_to_transaction(this.trans_id, sql, values, successcb, errorcb);
    return console.log("executeSql - add_to_transaction:" + sql);
  };
  SQLitePluginTransaction.prototype.complete = function(success, error) {
    var begin_opts, commit_opts, errorcb, executes, opts, successcb, txself;
    console.log("SQLitePluginTransaction.prototype.complete");
    begin_opts = void 0;
    commit_opts = void 0;
    errorcb = void 0;
    executes = void 0;
    opts = void 0;
    successcb = void 0;
    txself = void 0;
    if (this.__completed) throw new Error("Transaction already run");
    if (this.__submitted) throw new Error("Transaction already submitted");
    this.__submitted = true;
    txself = this;
    errorcb = function(res) {};
    if (error) {
      errorcb = function(res) {
        return error(txself, res);
      };
    }
    successcb = function(result)
    {
    	console.log('txCallback:'+txself.trans_id+' - ' + JSON.stringify(result));
    	SQLitePluginTransaction.txCompleteCallback(result, success);
    }
    transaction_callback_queue[this.trans_id]["error"] = errorcb;
    return cdv.exec(successcb, function(){console.log('txerror:')}, "SQLitePlugin", "executeSqlBatch", [transaction_queue[this.trans_id]]);
  };
  root.SQLitePluginTransaction = SQLitePluginTransaction;
  return root.sqlitePlugin = {
    openDatabase: function(dbPath, version, displayName, estimatedSize, creationCallback, errorCallback) {
		console.log('Open Database');
      return new SQLitePlugin(dbPath, creationCallback, errorCallback);
    }
  };
})();