Sensible.ImageResizer.Plugins.AzureReader2
==========================================

An ImageResizer plugin for Azure that stores transformed images into Azure blob storage for better performance.

The original implementation (https://github.com/imazen/resizer/tree/master/Plugins/AzureReader2) transforms images on the fly each time they are requested, which can be slow. Sensible.ImageResizer.Plugins.AzureReader2 persists the transformed image in the first request with a vanity path, and subsequent requests are redirected to this copy. 

An example:

http://<<website>>/azure/<<container>>/filename.jpg?width=120&height=200&mode=crop is persisted to the Azure blob container as http://<<account>>.blob.core.windows.net/<<container>>/filename_120x200_crop.jpeg

Only width, height and mode parameters are persisted in the filename in this implementation, but feel free to add more parameters according to your application requirements. 

See http://imageresizing.net/plugins/azurereader2 for instructions on how to install the plugin.

Note: it is highly recommended to use the lazyExistenceCheck="true" parameter in the web.config plugin declaration. This will result in fewer HTTP requests for the file (just a HEAD request to see if the file exists, and a 302 GET request to the transformed file).
