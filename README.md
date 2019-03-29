# Update My App
Lightweight library with updates support for your app.

Key options

* Less than 20 KB.
* Async.
* You can define extra data in config file.
* Support download progress (byte and percent).

## Installation
https://www.nuget.org/packages/UpdateMyApp/

```
Install-Package UpdateMyApp
```


## Usage


Make config XML.

```XML
<?xml version="1.0" encoding="UTF-8"?>
<item>
    <version>1.3.0</version>
    <url>https://dl.dropboxusercontent.com/s/regergdfgdbdfb/DownloadTest.zip</url>
    <Whats_new>Performance improvement</Whats_new>
</item>
```

Upload XML to you server and get URL for it. You Can upload e.g. to Dropbox. 

(After you get public URL change it to: https://dl.dropboxusercontent.com/s/[FILE_ID]/[FILENAME]) 

Set URL to your XML file

```CSharp
Update.SetUrlToXml("http://URL_to_XML"); //[bool] return true if url is valid
```
Set current version

```CSharp
Update.SetCurrentVersion("1.2.0"); //[bool] return true if string is valid Version
```

Now you can check if new version is present
```CSharp
await Update.CheckForNewVersionAsync(); //[bool] return true is new version is available
```
 After that you have two option, you can download file define in URL
 ```CSharp
await Update.DownloadFileAsync(@"E:\Shared\My_App\My_App_New.zip"); //[bool] return true is download is complete
 ```
 or open URL in browser
 ```CSharp
 Update.OpenURL();
 ```

 If you download file you can view progress
 ```CSharp
 Update.DownloadedProgress += Update_DownloadedProgress;

 private static void Update_DownloadedProgress(long byteDownloaded, long byteToDownload, double perCentProgress)
    {
        Console.WriteLine($"Downloaded: {byteDownloaded} from: {byteToDownload} | {perCentProgress}");
    }
 ```
To read other value from XML use Dictionary<string, string>.
```CSharp
Dictionary<string, string> AllXMLValue = await Update.ReadAllValueFromXmlAsync();
AllXMLValue.TryGetValue("Whats_new", out string whatsNew);

if (!string.IsNullOrEmpty(whatsNew))
    Console.WriteLine($"New version contein: {whatsNew}");
```


**Error handling**

By default all error are ignored. If you want to catch some, set this at the beginning.
```CSharp
Update.IsEnableError = true;
```
Now you can use try...catch.



## License
Apache License 2.0

https://github.com/m4rcelpl/UpdateMyApp/blob/master/LICENSE
