Sensible.ImageResizer.Plugins.AzureReader2
==========================================

An ImageResizer plugin for Azure that stores transformed images into Azure blob storage for better performance.

Only width, height and mode parameters are persisted in the filename in this implementation, but feel free to add more parameters according to your application requirements. For example:

http://<website>/azure/<container>/filename.jpg?width=120&height=200&mode=crop is persisted back to the Azure blob container as http://<account>.blob.core.windows.net/<container>/filename_120x200_crop.jpeg


See http://imageresizing.net/plugins/azurereader2 for instructions on how to install the plugin.

Note: it is highly recommended to use the lazyExistenceCheck="true" parameter in the web.config plugin declaration. This will result in fewer HTTP requests for the final file.
