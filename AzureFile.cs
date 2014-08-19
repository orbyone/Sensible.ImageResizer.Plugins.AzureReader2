/* Copyright (c) 2011 Wouter A. Alberts and Nathanael D. Jones. See license.txt for your rights. */
using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using ImageResizer.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ImageResizer.Plugins.AzureReader2
{

    public class AzureFile : VirtualFile, IVirtualFile
    {

        protected readonly AzureVirtualPathProvider parent;

        public AzureFile(string blobName, AzureVirtualPathProvider parentProvider)
            : base(blobName)
        {
            parent = parentProvider;
        }

        public override Stream Open()
        {
            var baseUri = parent.CloudBlobClient.BaseUri.OriginalString.TrimEnd('/', '\\');
            var context = HttpContext.Current;
            //Get querystring parameters, add more parameters as required for your application
            var width = context.Request.QueryString["width"];
            var height = context.Request.QueryString["height"];
            var mode = context.Request.QueryString["mode"];
            //Parse VirtualFile
            var container = Path.GetDirectoryName(VirtualPath);
            var filenameWithoutExtension = Path.GetFileNameWithoutExtension(VirtualPath);
            var extension = Path.GetExtension(VirtualPath);
            //Poor man's content type calculator
            var contentType = "image/" + extension.ToLower().Replace(".", "");
            //Create a vanity path according to querystring parameters, that will be persistently stored to Azure blob storage
            var vanityVirtualPath = string.Format("{0}/{1}_{2}x{3}_{4}{5}",
                 container, filenameWithoutExtension, width, height, mode, extension);
            var vanityFilename = Path.GetFileName(vanityVirtualPath);
            var blobUri = new Uri(string.Format("{0}/{1}", baseUri, vanityVirtualPath));
            try
            {
                //Check if the transformed image exists
                parent.CloudBlobClient.GetBlobReferenceFromServer(blobUri);
                //If it does, redirect, nothing to do here
                context.Response.Redirect(blobUri.ToString(), true);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 404)
                {
                    //Download original image
                    var imageStream = new MemoryStream(4096);
                    var transformedStream = new MemoryStream(4096);
                    blobUri = new Uri(string.Format("{0}/{1}", baseUri, VirtualPath));
                    ICloudBlob cloudBlob;
                    try
                    {
                        cloudBlob = parent.CloudBlobClient.GetBlobReferenceFromServer(blobUri);
                    }
                    catch (StorageException ex2)
                    {
                        //If original file does not exist, return 404
                        if (ex2.RequestInformation.HttpStatusCode == 404)
                        {
                            context.Response.StatusCode = 404;
                            context.Response.End();
                        }
                        throw;
                    }
                    cloudBlob.DownloadToStream(imageStream);
                    //Seek to beginning of stream
                    imageStream.Seek(0, SeekOrigin.Begin);
                    var config = new Config();
                    //Transform image
                    config.BuildImage(imageStream, transformedStream, context.Request.QueryString.ToString());
                    //Seek to beginning of stream (again)
                    transformedStream.Seek(0, SeekOrigin.Begin);
                    //Upload transformed image to Azure
                    blobUri = new Uri(string.Format("{0}/{1}", baseUri, vanityVirtualPath));
                    cloudBlob = parent.CloudBlobClient.GetContainerReference(container).GetBlockBlobReference(vanityFilename);
                    cloudBlob.Properties.ContentType = contentType;
                    //Completely optional, use it for client-side caching if your images do not change often
                    cloudBlob.Properties.CacheControl = "max-age=31536000"; //1 year
                    cloudBlob.UploadFromStream(transformedStream);
                    //Done, now redirect, our job here is done
                    context.Response.Redirect(blobUri.ToString(), true);
                }
                else
                {
                    throw;
                }
            }
            return null;
        }
    }
}
