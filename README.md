# aws-credential-manager

This command line utility uses two ways of managing aws credentials, to assist those of us who write apps and/or use the AWS CLI with multiple sets of credentials. 

Features: 
1. On startup, uses IAM api to determine the default credential set currently in use.
1. Maintains a named list of credentials.  View list, add or remove from this list.
1. Allows you to set the default credential, using a wrapper for aws configure. 

[bash]
Current default account:
Account: client1
User:    arn:aws:iam::999999999999:user/mmendelson

A: Add    stored credential
R: Remove stored credential
L: List   stored credentials
D: Dump   stored credentials
S: Set Default Credential
X: Exit

1: client1 AKIAIZZZZZZZZZZZ
2: client1-codedeploy AKIAIYYYYYYYYYYYYY
3: mm sandbox AKIAIXXXXXXXXXXXXXX
[/bash]