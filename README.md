This is my old project and not sure if it still works
Plutonium.Agent is the core of bot and the idea is to run it in other computer like in vps and control the bot(s) from client gui
Last growtopia version this is used in 4.09 you can change the game_version and protocol from Plutonium.Agent/Entities/Config.cs

The agent will host enet server by default on port 1300
to add the agent in ur client gui you write the ip:port, growid and password

Exmaple of lua script

bot = Plutonium:GetBot()
bot:FindPath(10, 10)

![img](https://cdn.discordapp.com/attachments/863369169302716459/1054451613655388230/image.png)
![img](https://cdn.discordapp.com/attachments/863369169302716459/1054451721222504538/image.png)
