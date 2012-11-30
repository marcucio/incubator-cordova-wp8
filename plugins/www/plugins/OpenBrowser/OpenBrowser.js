/* MIT licensed */
// (c) 2010 Jesse MacFadyen, Nitobi

/*global PhoneGap */

function OpenBrowser() {
  // Does nothing
}

/* The interface that you will use to access functionality */

// Show a webpage, will result in a callback to onLocationChange
OpenBrowser.prototype.openWebPage = function(loc,geolocationEnabled)
{
  var success = function(msg)
  {
     console.log("OpenBrowser.openWebPage success :: " + msg);
  };

  var error = function(e)
  {
     console.log("OpenBrowser.openWebPage error :: " + e);
  };

  var options = 
  {
     url:loc
  };

  Cordova.exec(success,error,"OpenBrowserCommand","openWebPage", options);
  //setTimeout(this.close,5000);
};

// Note: this plugin does NOT install itself, call this method some time after deviceready to install it
// it will be returned, and also available globally from window.plugins.openBrowser
OpenBrowser.install = function()
{
  if(!window.plugins) {
    window.plugins = {};
  }

  window.plugins.openBrowser = new OpenBrowser();
  return window.plugins.openBrowser;
};
