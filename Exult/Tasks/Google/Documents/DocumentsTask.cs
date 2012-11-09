using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Google.Documents;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.Documents;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Win32;
using System.Collections;

namespace Exult.Tasks.Google.Documents
{
    /// <summary>
    /// A task that authenticates with Google Docs
    /// </summary>
    public abstract class DocumentsTask : BaseTask
    {
        [Required]
        public string Username
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public GDataCredentials GetDataCredentials()
        {
            if (string.IsNullOrEmpty(Password))
            {
                //Refer to a registry location so we don't have to check in our API key
                object value = Registry.GetValue(Registry.CurrentUser.Name + "\\exult\\credentials", Username, string.Empty);
                string password = (value ?? string.Empty).ToString();
                //Add the key so we can find and update it
                if (password == string.Empty)
                {
                    Registry.SetValue(Registry.CurrentUser.Name + "\\exult\\credentials", Username, string.Empty);
                    throw new InvalidCredentialsException(string.Format("Expected password at {0}", Registry.CurrentUser.Name + "\\exult\\credentials\\" + Username));
                }
                Password = password;
            }

            return new GDataCredentials(Username, Password);
        }

        public DocumentsService GetDocumentsService()
        {
            DocumentsService service = new DocumentsService("code.google.com/p/exult/");
            service.Credentials = GetDataCredentials();
            return service;
        }
    }
}
