This is my old project what was designed to be used for rotation farm and run the bots from agent in vps and control all the bots from client gui.

Plutonium.Agent has mostly all the bot stuff and hosts website on port 8080 to see some basic info on the bots.
Everything should work in latest gt version just by updating the gt ver in Plutonium.Agent->Entities->Config.cs unless ubisoft has changed something major

Plutonium project contains the client gui to control the bots. To add bot you have to put ip:port if its running in your own pc its 127.0.0.1:1300
if youre hosting the bot in vps you have to portforward port 1300 UDP and point to vps public ipv4 example 86.211.92.49:1300

if you have to ask something create issue dont dm me on dc.


(agent must be runned in admin mode)

## How to build?
to build the project you need have .NET 6 SDK!
- open cmd and cd to the location where the .sln file locates and then run 
```
dotnet build
```
this will build the whole solution

goodluck with the messy code...

## Exmaple of lua script
```
bot = Plutonium:GetBot()
bot:FindPath(10, 10)
```
## Screenshots
![img](https://cdn.discordapp.com/attachments/863369169302716459/1054451613655388230/image.png)
![img](https://cdn.discordapp.com/attachments/863369169302716459/1054451721222504538/image.png)
