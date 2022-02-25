# Setup MongoDB

1. Download MongoDB from this link https://www.mongodb.com/try/download/community
2. Unzip the files anywhere you want
3. Create a text file and write these lines in it:

@echo off
{MongoDB folder Path}\mongod.exe --dbpath={Path to existing empty folder}

4. Save text file as StartMongo.bat file
5. Run StartMongo.bat