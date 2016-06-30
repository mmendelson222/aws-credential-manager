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
