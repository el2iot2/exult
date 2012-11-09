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

namespace Exult.Tasks.Aws
{
    /// <summary>
    /// A task that authenticates with Aws
    /// </summary>
    public abstract class AwsCredentialsTask : BaseTask
    {
        [Required]
        public string AccessKey
        {
            get;
            set;
        }

        public string SecretKey
        {
            get;
            set;
        }

        public void RequireSecretKey()
        {
            if (string.IsNullOrEmpty(SecretKey))
            {
                //Refer to a registry location so we don't have to check in our API key
                object value = Registry.GetValue(Registry.CurrentUser.Name + "\\exult\\credentials\\aws", AccessKey, string.Empty);
                string secretKey = (value ?? string.Empty).ToString();
                //Add the key so we can find and update it
                if (secretKey == string.Empty)
                {
                    Registry.SetValue(Registry.CurrentUser.Name + "\\exult\\credentials\\aws", AccessKey, string.Empty);
                    throw new InvalidCredentialsException(string.Format("Expected secret key at {0}", Registry.CurrentUser.Name + "\\exult\\credentials\\aws\\" + AccessKey));
                }
                SecretKey = secretKey;
            }
        }
    }
}
