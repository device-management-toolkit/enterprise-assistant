# Enterprise Assistant

[![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/open-amt-cloud-toolkit/enterprise-assistant/badge)](https://api.securityscorecards.dev/projects/github.com/open-amt-cloud-toolkit/enterprise-assistant) [![Discord Shield](https://discordapp.com/api/guilds/1063200098680582154/widget.png?style=shield)](https://discord.gg/yrcMp2kDWh)

> Disclaimer: Production viable releases are tagged and listed under 'Releases'.  All other check-ins should be considered 'in-development' and should not be used in production

Enterprise Assistant is a Windows application that can run as a normal application or as a background Windows service. Once setup to connect to RPS (hosted in either the cloud or enterprise), it can be used to assist with the configuration of AMT devices using TLS. Enterprise Assistant will handle certificate signing requests (CSRs) to Microsoft CA.

Enterprise Assistant is based off the open-source project [MeshCentral Satellite](https://github.com/Ylianst/MeshCentralSatellite).
<br><br>

**For detailed documentation** about Enterprise Assistant or other features of the Open AMT Cloud Toolkit, see the [docs](https://open-amt-cloud-toolkit.github.io/docs).

<br>

<!-- It will automatically create 802.1x profiles in the domain controller for Intel AMT devices and can use a certificate authority in your domain to issue 802.1x certificates to Intel AMT. -->

## Enterprise Assistant Setup

Enterprise Assistant must run on a computer that is joined to your domain and run it with sufficient rights that it can create LDAP computer objects and have access to the domain Certificate Authority so it can request that certificates be signed.

It is suggested to run Enterprise Assistant as a normal Windows application at first to make sure everything works correctly before running it as a background Windows service. You can start by going in the "Settings" option in the menus and setting up the RPS server's hostname and login credentials. You also need to setup that certificate authority to use and certificate template. If a certificate authority is not setup, only PEAPv0/EAP-MSCHAPv2 will be supported.

<!-- You can also indicate what domain security groups a computer must be joined to when a new 802.1x computer is created. -->

You can use the "Testing" menu to create and remove a test computer from the domain. This is useful to make sure everything is working well before getting requests from RPS.

<br>

## Additional Resources

- For detailed documentation and Getting Started, [visit the docs site](https://open-amt-cloud-toolkit.github.io/docs).

- Find a bug? Or have ideas for new features? [Open a new Issue](https://github.com/open-amt-cloud-toolkit/enterprise-assistant/issues).

- Need additional support or want to get the latest news and events about Open AMT? Connect with the team directly through Discord.

    [![Discord Banner 1](https://discordapp.com/api/guilds/1063200098680582154/widget.png?style=banner2)](https://discord.gg/yrcMp2kDWh)

<!-- ## MeshCentral Configuration

This is an example of setting up 802.1x in Intel AMT without MeshCentral Satellite being involved. The MSCHAPv2 username and password is provided in the config.json of MeshCentral.

```
{
  "Settings": {

  },
  "Domains": {
    "": {
      "AmtManager": {
        "802.1x": {
          "AuthenticationProtocol": "PEAPv0/EAP-MSCHAPv2",
          "Username": "authUsername",
          "Password": "authUserPassword"
        },
        "WifiProfiles": [
          {
            "SSID": "AP-SSID-1",
            "Authentication": "wpa2-802.1x",
            "Encryption": "ccmp-aes"
          }
        ]
      }
    }
  }
}
```

The problem with this example is that all Intel AMT devices will be configured with the same 802.1x username and password this is not good for security. You can't revoke individual accounts or monitor what device is connecting since they all use the same account.

Once MeshCentral Satellite is setup, you can have a config.json that looks like this:

```
{
  "Settings": {

  },
  "Domains": {
    "": {
      "AmtManager": {
        "802.1x": {
          "AuthenticationProtocol": "EAP-TLS",
          "SatelliteCredentials": "satelliteUser",
          "AvailableInS0": true
        },
        "WifiProfiles": [
          {
            "SSID": "AP-SSID-1",
            "Authentication": "wpa2-802.1x",
            "Encryption": "ccmp-aes"
          },
          {
            "SSID": "AP-SSID-2",
            "Authentication": "wpa2-802.1x",
            "Encryption": "ccmp-aes"
          },
          {
            "SSID": "AP-SSID-3",
            "Authentication": "wpa2-psk",
            "Encryption": "ccmp-aes",
            "Password": "my-wifi-password"
          }
        ]
      }
    }
  }
}
```

In the example above, MeshCentral will configure 802.1x for the wired interface and for 2 of the 3 WIFI profiles. AP-SSID-1 and AP-SSID-2 are set to authenticate using 802.1x and AP-SSID-3 is setup with regular WPA2 password authentication.

What makes this 802.1x configuration interesting is the line "SatelliteCredentials". This indicates a MeshCentral Satellite will be connected with the user account name "satelliteUser" and to query it to setup a 802.1x profile in the Windows domain controller and issue a 802.1x authentication certificate to Intel AMT.

Another example is this:

```
{
  "Settings": {

  },
  "Domains": {
    "": {
      "AmtManager": {
        "802.1x": {
          "AuthenticationProtocol": "PEAPv0/EAP-MSCHAPv2",
          "SatelliteCredentials": "satelliteUser"
        },
        "WifiProfiles": [
          {
            "SSID": "AP-SSID-1",
            "Authentication": "wpa2-802.1x",
            "Encryption": "ccmp-aes"
          }
        ]
      }
    }
  }
}
```

In this example, the Intel AMT wired interface is configured with 802.1x along with a single WIFI profile. This time, instead of EAP-TLS being used for authentication, PEAPv0/EAP-MSCHAPv2 will be used. MeshCentral Satellite will be queried, but this time, a 802.1x account will be created in the domain with a username and random password. The password will be sent back to MeshCentral and set into Intel AMT.

## Video Tutorials
You can watch many tutorial videos on the [MeshCentral YouTube Channel](https://www.youtube.com/channel/UCJWz607A8EVlkilzcrb-GKg/videos). There is one video on how to setup Intel AMT with 802.1x without MeshCentral Satellite, this is a good way to get started. After that, you can take a look at the full demonstration of MeshCentral Satellite.

Basic Intel AMT 802.1x with JumpCloud.  
[![MeshCentral - Basic Intel AMT 802.1x with JumpCloud](https://img.youtube.com/vi/tKI9UJ1O15M/mqdefault.jpg)](https://www.youtube.com/watch?v=tKI9UJ1O15M)

MeshCentral Satellite & Advanced Intel AMT 802.1x.  
[![MeshCentral - Satellite & Advanced Intel AMT 802.1x](https://img.youtube.com/vi/1otWwjtFBIA/mqdefault.jpg)](https://www.youtube.com/watch?v=1otWwjtFBIA) -->
