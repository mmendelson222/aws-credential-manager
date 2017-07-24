using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace credential_manager.Operations
{
    class IamOperations
    {
        internal static object UserInfo(Amazon.Runtime.AWSCredentials creds)
        {
            UserMetadata meta = new UserMetadata();
            AmazonIdentityManagementServiceClient iamClient = new AmazonIdentityManagementServiceClient(creds);

            //get basic user info. 
            try
            {
                GetUserResponse response = iamClient.GetUser();
                meta.UserArn = response.User.Arn;
                meta.UserName = response.User.UserName;
                meta.AccountID = Regex.Match(meta.UserArn, @"(\d{12})").Groups[1].ToString();
            }
            catch (AmazonIdentityManagementServiceException e)
            {
                var match = Regex.Match(e.Message, @"arn:aws:iam::(\d{12}):user/(\S*)");
                if (match.Success)
                {
                    //get the ARN anyway, from the error message
                    meta.UserArn = match.ToString();
                    meta.AccountID = match.Groups[1].ToString();
                    meta.UserName = match.Groups[2].ToString();
                }
                else
                {
                    //show the whole error message
                    meta.UserArn = e.Message;
                }
            }

            //get key meta.
            try
            {
                var aKeysResponse = iamClient.ListAccessKeys(new ListAccessKeysRequest() { UserName = meta.UserName });
                foreach (var k in aKeysResponse.AccessKeyMetadata)
                    if (k.AccessKeyId == creds.GetCredentials().AccessKey)
                        meta.KeyMeta = string.Format("Age: {0:%d} days, {0:%h} hours, Key Status: {1}", DateTime.Now - k.CreateDate, k.Status);
            }
            catch (Exception ex)
            {
                //need iam:ListAccessKeys.
                meta.KeyMeta = ShortenException(ex);
            }

            //get account info, if proper privs exist.
            try
            {
                var aliasResponse = iamClient.ListAccountAliases();
                meta.AccountAlias = aliasResponse.AccountAliases.Count > 0 ? aliasResponse.AccountAliases[0] : "No account alias";
            }
            catch (Exception e)
            {
                meta.AccountAlias = ShortenException(e);
            }

            return meta;
        }

        private static string ShortenException(Exception e)
        {
            string msg = e.Message;
            int notAuth = msg.IndexOf("not authorized");
            if (notAuth > 0)
                //just shorten the string.
                msg = msg.Substring(notAuth);

            int onResource = msg.IndexOf("on resource");
            if (onResource > 0)
                //just shorten the string.
                msg = msg.Substring(0, onResource);

            return msg;
        }

        internal class UserMetadata
        {
            internal string AccountID;
            internal string UserName;
            internal string UserArn;
            internal string AccountAlias;
            internal  string KeyMeta;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Account:  {0}\nUser:     {1}\nKey Meta: {2}", AccountAlias, UserArn, KeyMeta);
                return sb.ToString();
            }

        }
    }
}
