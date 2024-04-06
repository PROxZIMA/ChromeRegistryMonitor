# ChromeRegistryMonitor

- C# application that watches for changes in `SOFTWARE\Policies\Google` registry key.
- It uses `P/Invoke` to call Windows API functions for interacting with the Windows Registry.
- Then deletes the `Chrome` subkey (and all other subentries) if it is there.

## Installation

```sh
> git clone https://github.com/PROxZIMA/ChromeRegistryMonitor.git
> cd ChromeRegistryMonitor
> dotnet build
```

## Usage

```sh
# Open admin command prompt
> C:\repos\ChromeRegistryMonitor\bin\Debug\net7.0-windows\ChromeRegistryMonitor.exe
```

***OR***

1. Right click `C:\repos\ChromeRegistryMonitor\bin\Debug\net7.0-windows\ChromeRegistryMonitor.exe` and click `Properties`.
2. Go to `Compatibility` tab and check `Run this program as an administrator`.
3. Click `Apply` and `OK`.
4. Right click `C:\repos\ChromeRegistryMonitor\bin\Debug\net7.0-windows\ChromeRegistryMonitor.exe` and click `Create shortcut`.
5. Move the shortcut to Desktop.
6. Double click the shortcut to run the application.

## License
This project is licensed under the [MIT License](./LICENCE).