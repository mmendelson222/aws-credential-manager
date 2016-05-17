﻿using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace credential_manager.Operations
{
    class IamOperations
    {
        static internal UserMetadata UserInfo()
        {
            UserMetadata meta = new UserMetadata();
            AmazonIdentityManagementServiceClient iamClient = new AmazonIdentityManagementServiceClient();
            try
            {
                GetUserResponse response = iamClient.GetUser();
                meta.UserArn = response.User.Arn;
            }
            catch (AmazonIdentityManagementServiceException e)
            {
                meta.UserArn = e.Message;
            }

            try
            {
                var aliasResponse = iamClient.ListAccountAliases();
                meta.AccountAlias = aliasResponse.AccountAliases.Count > 0 ? aliasResponse.AccountAliases[0] : "No account alias";
            }
            catch (AmazonIdentityManagementServiceException e)
            {
                meta.AccountAlias = e.Message;
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