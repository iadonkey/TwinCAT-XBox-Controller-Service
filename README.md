# TwinCAT XBox Controller Service



## Project Description -

TwinCAT XBox Controller Service is designed to be a system service for TwinCAT Windows systems using .Net 6. The service connects to XBox controllers attached to the PC, and then hosts the data for the PLC to access. The standalone ADS router install is all that is needed to start the service properly, and it can be found on Beckhoff's website. 



## How It Works -

Unlike other XBox-to-TwinCAT projects that use an ADS Client, TwinCAT XBox Controller Service is running as an ADS Server and hosting data on customer port 25733. This gives the user significant advantages in not only configuration ease, but also flexibility. Once the service is started, it begins monitoring the PC's XBox controller connections via a C++ based XBox-XInput DLL, and this data is then hosted to ADS clients upon request. The user accesses the data easily via PLC function block that is located inside the XBoxControllerUtilites.library.

Most projects are limited to 4 controllers maximum, due to the maximum count allowed on a PC by the XInput service. However, due to the way this is architected, the amount of controllers is theoretically based on the maximum ADS traffic. You can host 4 controllers per PC, but you can connect many different PCs to the PLC via different ADS routes. This has also been successfully tested with ADS-over-MQTT from 2 different "service hosters" and has demonstrated good results.



## Using This Project -

### Installation:

You can build the project from scratch using Visual Studio 2022, but for ease of use there is a pre-compiled installer located at:

[TC-XBox-Controller-Service-Installer/Release](https://github.com/0w8States/TwinCAT-XBox-Controller-Service/tree/master/TC-XBox-Controller-Service-Installer/Release)



> Note: If building from scratch, be sure to put the projects in Release configuration before building the Installer project. You will receive DLL errors for the service if distributing the setup.exe while in Debug mode.



The PLC library and Example project will be included with the install, and they are located at:

<u>C:\Program Files\TcXBoxServiceProvider\TC XBox Controller Service\TwinCAT Files</u>





### Connecting from PLC:

- Start the service, and keep it running in the background

  <img src="Images\image-20220602081200295.png" alt="AppStart" style="zoom:150%;" />


  <img src="Images\image-20220602081056315.png" alt="Service" style="zoom:100%;" />

  

- Connect via PLC

<img src="Images\image-20220602081433579.png" alt="PLC"  />





## Future Feature List:

- Eventually a Unix based branch for TC/BSD systems using .Net 6
  - The rumble effect will not be functional, but the inputs are readable
- Implement Battery status
- Implement Wireless vs Wire status
- Build into constant Windows System Service, no console launch
- Additional PLC FB features, including easy to use Axis control features