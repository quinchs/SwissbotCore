using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore
{
    public class CustomCommandService
    {
        private List<Command> CommandList = new List<Command>();
        private class Command
        { 
            public string CommandName { get; set; }
            public char Prefix { get; set; }
            public System.Reflection.ParameterInfo[] Paramaters { get; set; }
            public MethodInfo Method { get; set; }
            public DiscordCommand attribute { get; set; }
            public CommandClassobj parent { get; set; }
        }
        private class CommandClassobj
        {
            public char Prefix { get; set; }
            public DiscordCommandClass attribute { get; set; }
            public Type type { get; set; }
            public List<Command> ChildCommands { get; set; }
            public object ClassInstance { get; set; }

        }
        public class DiscordCommandClass : Attribute
        {
            internal char prefix { get; set; }
            public DiscordCommandClass(char prefix)
            {
                this.prefix = prefix;
            }
            public DiscordCommandClass()
            {
                
            }
        }
        public class DiscordCommand : Attribute
        {
            internal string commandName { get; set; }
            internal char prefix { get; set; }
            internal string description { get; set; }
            internal string commandHelp { get; set; }

            public DiscordCommand(string commandName)
            {
                this.commandName = commandName;
                prefix = currentSettings.DefaultPrefix;
            }
            public DiscordCommand(string commandName, string description)
            {
                this.commandName = commandName;
                prefix = currentSettings.DefaultPrefix;
                this.description = description;
            }
            public DiscordCommand(string commandName, string description, string commandHelp)
            {
                this.commandName = commandName;
                prefix = currentSettings.DefaultPrefix;
                this.description = description;
                this.commandHelp = commandHelp;
            }
            public DiscordCommand(string commandName, string description, string commandHelp, char prefix)
            {
                this.commandName = commandName;
                this.prefix = prefix;
                this.description = description;
                this.commandHelp = commandHelp;
            }
            public DiscordCommand(string commandName, char prefix)
            {
                this.commandName = commandName;
                this.prefix = prefix;
            }
        }
        public static Settings currentSettings;
        private List<Type> CommandClasses { get; set; }
        public class Settings
        {
            public char DefaultPrefix { get; set; }
        }
        public CustomCommandService(Settings s)
        {
            List<Type> CommandClasses = new List<Type>();
            Dictionary<MethodInfo, Type> CommandMethods = new Dictionary<MethodInfo, Type>();
            currentSettings = s;
            CommandClasses = Assembly.GetEntryAssembly().GetTypes().Where(x => x.GetCustomAttribute(typeof(DiscordCommandClass)) != null).ToList();
            foreach (var t in CommandClasses)
            {
                //check each method for attribute
                var mths = t.GetMethods();
                var cmths = mths.Where(x => x != null).Where(x =>  x.GetCustomAttribute(typeof(DiscordCommand)) != null ).ToList();
                cmths.ForEach(x => CommandMethods.Add(x, t));
            }
            //add to base command list
            foreach(var item in CommandMethods)
            {
                var cmdat = item.Key.GetCustomAttribute<DiscordCommand>();
                var parat = item.Value.GetCustomAttribute<DiscordCommandClass>();
                CommandList.Add(new Command()
                {
                    CommandName = cmdat.commandName,
                    Method = item.Key,
                    Prefix = cmdat.prefix == 0 ? currentSettings.DefaultPrefix : cmdat.prefix,
                    attribute = cmdat,
                    parent = new CommandClassobj()
                    {
                        Prefix = parat.prefix,
                        ClassInstance = Activator.CreateInstance(item.Value)
                    },
                    Paramaters = item.Key.GetParameters(),

                }); ;
            }
            
            
        }
        public enum CommandStatus
        {
            Success,
            Error,
            NotFound,
            NotEnoughParams,
            InvalidParams,
            Unknown
        }
        public class CommandResult
        {
            public CommandStatus Result;
            public Exception Exception;
        }
        public async Task<CommandResult> ExecuteAsync(ICommandContext context)
        {
            string[] param = context.Message.Content.Split(' ');
            param = param.TakeLast(param.Length - 1).ToArray();
            string command = context.Message.Content.Split(' ').First().Remove(0, 1).ToLower();
            char prefix = context.Message.Content.Split(' ').First().ToCharArray().First();
            SwissCommandModule.Context = context;

            //check if the command exists
            if (!CommandList.Any(x => x.CommandName.ToLower() == command))
                return new CommandResult()
                {
                    Result = CommandStatus.NotFound
                };

            //Command exists

            //if more than one command with the same name exists then try to execute both or find one that matches the params
            var commandobj = CommandList.Where(x => x.CommandName.ToLower() == command).Where(x => x.Prefix == prefix || x.parent.Prefix == prefix).ToList();
            bool cnt = false;
            foreach (var cmd in commandobj)
                if (cmd.Prefix == prefix)
                    cnt = true;
            if (!cnt)
                return new CommandResult() { Result = CommandStatus.NotFound };
            //find if we can match the params

            //1 find if param counts match
            foreach (var cmd in commandobj)
            {
                if (cmd.Paramaters.Length == param.Length)
                {
                    List<object> parsedparams = new List<object>();
                    bool check = true;

                    //right command lengths met or params object in it now we gotta parse the params
                    for (int i = 0; i != param.Length; i++)
                    {
                        //PrimitiveParsers.Get(unparsedparam)
                        var p = param[i];
                        var dp = cmd.Paramaters[i];
                        //check if we can parse the value
                        if (TypeDescriptor.GetConverter(dp.ParameterType).IsValid(p))
                        {
                            //we can
                            var pparam = TypeDescriptor.GetConverter(dp.ParameterType).ConvertFromString(p);
                            parsedparams.Add(pparam);
                        }
                        else
                            check = false;
                    }
                    //if we parsed all the params correctly
                    if (check)
                    {
                        //invoke the method
                        try
                        {
                            cmd.Method.Invoke(cmd.parent.ClassInstance, parsedparams.ToArray());
                            return new CommandResult() { Result = CommandStatus.Success };
                        }
                        catch (TargetInvocationException ex)
                        {
                            Console.WriteLine(ex.InnerException);
                            return new CommandResult() { Exception = ex.InnerException, Result = CommandStatus.Error };
                        }

                    }
                    else
                        return new CommandResult() { Result = CommandStatus.InvalidParams };

                }
                else
                {
                    if (cmd.Paramaters.Last().GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                    {

                    }
                    else
                        return new CommandResult() { Result = CommandStatus.NotEnoughParams };
                }
            }
            return new CommandResult() { Result = CommandStatus.Unknown };
        }
        public class SwissCommandModule
        {
            public static ICommandContext Context { get; internal set; }
        }
    }

}
