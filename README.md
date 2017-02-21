# aws-credential-manager

This command line utility uses two ways of managing aws credentials, to assist those of us who write apps and/or use the AWS CLI with multiple sets of credentials. 

Features: 

1. Keeps a list of "stored credentials" using the AWS API's ProfileManager utilities.   View, add or remove from this list.
1. Allows you to set the default credential, using a wrapper for aws configure.  Default is indicated on list.
1. "Push" any stored credential to a named credential, using a wrapper for aws configure. 
1. Whois function allows you to determine the AWS IAM user for any user. 

## Main menu
```
=== Stored Credetials ===
client1           AKIAIZZZZZZZZZZZZZZZ
anotherclient     AKIAIYYYYYYYYYYYYYYY  (default)
mm sandbox        AKIAIAAAAAAAAAAAAAAA

A: Add    stored credential
R: Remove stored credential
S: Set    default credential
P: Push   to named credential
W: Whois  IAM user associated named credential
X: Exit
```
## Whois function
```
Choose credential (use arrows or type): client1
Account: client1
User:    arn:aws:iam::999999999999:user/iamuser1
```
## Add new profile
```
Profile name: newprofile
Access key: AAAAAAAAAAAAAAAAAAAA
Secret key: BBBBBBBBBBBBBBBBBBBBBBB
```
## Note on default credentials
If one of the credentials listed by the credential manager is called "default", this will be picked up by the AWS API, but NOT by the command line.  

To explain, this app stores credentials in two different places: 
 1. As "named credentials" - You're setting this any time you choose the default or "push to a named credential".  These are plain text (don't shoot the messenger) and are at %USERPROFILE%\.aws\credentials.   A "default" here is seen by both the CLI and the AWS API. 
 1. Using the AWS API's ProfileManager utilities - This is the main list that is displayed, and these are encrypted and stored at %LOCALAPPDATA%/AWSToolkit/RegisteredAccounts.json.   If you set a "default" here, it is only seen by the AWS API, but takes priority over the other one.

The simplest solution is to keep only one default: A "named credential" which will be picked up by both the cli and the api. 
