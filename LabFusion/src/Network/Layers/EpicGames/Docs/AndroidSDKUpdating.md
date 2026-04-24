# Android SDK Updaiting Guide
This is a pain in the ass

- Download the latest version of the Epic Online Services SDK for Android from the Epic Games Developer Portal.
- Extract "SDK\Bin\Android\static-stdc++\aar\eossdk-StaticSTDC-release.aar" and copy "jni\arm64-v8a\libEOSSDK.so" to "dependencies\resources\lib\arm64"
- Open "Samples\Android\Login" with Android Studio
- Replace the MainActivity.java file with the following code:

```java
package com.epicgames.mobile.login;

import android.app.Activity;

public class MainActivity extends Activity {
}

```
- Delete native-lib.cpp
- Build the project
- We now have an APK file containing all the compiled dex files we need located at "app\build\outputs\apk\debug\app-arm64-v8a-debug.apk"
- Extract the APK file
- Copy all dex files to "dependencies\resources\dex"
- Done

Yes this does introduce more bloat code, but it's better than having to recompile smali code.