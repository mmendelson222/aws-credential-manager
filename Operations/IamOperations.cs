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
            try
            {
                GetUserResponse response = iamClient.GetUser();
                meta.UserArn = response.User.Arn;
            }
            catch (AmazonIdentityManagementServiceException e)
            {
                var match = Regex.Match(e.Message, @"arn:aws:iam::(\d{12}):user/(\S*)");
                if (match.Success)
                    //get the ARN anyway, from the error message
                    meta.UserArn = match.ToString();
                else 
                    //show the whole error message
                    meta.UserArn = e.Message;
            }

            try
            {
                var aliasResponse = iamClient.ListAccountAliases();
                meta.AccountAlias = aliasResponse.AccountAliases.Count > 0 ? aliasResponse.AccountAliases[0] : "No account alias";
            }
            catch (AmazonIdentityManagementServiceException e)
            {
                int notAuth = e.Message.IndexOf("not authorized");
                if (notAuth < 0)
                    //show the whole error message
                    meta.AccountAlias = e.Message;
                else
                    //just shorten the string.
                    meta.AccountAlias = e.Message.Substring(notAuth);  
            }

            return meta;
        }

        internal class UserMetadata
        {
            internal string UserArn = string.Empty;
            internal string AccountAlias = string.Empty;
            public override string ToString()
            {
                return string.Format("Account: {0}\nUser:    {1}", AccountAlias, UserArn);
            }
        }
    }
}
